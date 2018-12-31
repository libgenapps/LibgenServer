using System;
using System.Collections;
using System.IO;
using LibgenServer.Core.Common;
using LibgenServer.Core.Configuration;
using LibgenServer.Core.Database;
using LibgenServer.Core.Entities;
using LibgenServer.Core.Import;
using LibgenServer.Core.Import.SqlDump;
using static LibgenServer.Core.Common.Constants;

namespace LibgenServer.Cli.Import
{
    internal class ImportOperations
    {
        private readonly ImportFormat importFormat;
        private readonly string importFilePath;

        public ImportOperations(ImportFormat importFormat, string importFilePath)
        {
            this.importFormat = importFormat;
            this.importFilePath = importFilePath;
        }

        public int Import()
        {
            ServerConfiguration serverConfiguration = LoadServerConfiguration();
            if (serverConfiguration == null)
            {
                return 1;
            }
            string databaseFilePath = serverConfiguration.DatabaseFilePath;
            if (String.IsNullOrWhiteSpace(databaseFilePath))
            {
                Console.WriteLine("Database file path is not set in the server configuration file.");
                return 2;
            }
            if (!File.Exists(databaseFilePath))
            {
                Console.WriteLine($"Couldn't find the database file at {databaseFilePath}");
                return 3;
            }
            Console.WriteLine($"Opening database \"{databaseFilePath}\"...");
            LocalDatabase localDatabase = LocalDatabase.OpenDatabase(databaseFilePath);
            using (SqlDumpReader sqlDumpReader = new SqlDumpReader(importFilePath))
            {
                while (true)
                {
                    bool tableFound = false;
                    while (sqlDumpReader.ReadLine())
                    {
                        if (sqlDumpReader.CurrentLineCommand == SqlDumpReader.LineCommand.CREATE_TABLE)
                        {
                            Logger.Debug("CREATE TABLE statement found.");
                            Console.WriteLine("CREATE TABLE statement found.");
                            tableFound = true;
                            break;
                        }
                    }
                    if (!tableFound)
                    {
                        Logger.Debug("CREATE TABLE statement was not found.");
                        Console.WriteLine("CREATE TABLE statement was not found.");
                        return 4;
                    }
                    SqlDumpReader.ParsedTableDefinition parsedTableDefinition = sqlDumpReader.ParseTableDefinition();
                    TableType tableType = DetectImportTableType(parsedTableDefinition);
                    if (tableType == TableType.UNKNOWN)
                    {
                        Console.WriteLine($"Table {parsedTableDefinition.TableName} doesn't match any known supported formats. It will be skipped.");
                        continue;
                    }
                    Logger.Debug($"Table type is {tableType}.");
                    ImportFormat? detectedImportFormat = ConvertTableTypeToImportFormat(tableType);
                    if (!detectedImportFormat.HasValue)
                    {
                        Console.WriteLine("Could not determine the format of the database dump.");
                        return 5;
                    }
                    if (detectedImportFormat.Value == importFormat)
                    {
                        Console.WriteLine($"Found a matching import format: {ImportFormatToString(detectedImportFormat.Value)}.");
                    }
                    else
                    {
                        Console.WriteLine($"Expected the import format {importFormat} but found {ImportFormatToString(detectedImportFormat.Value)}.");
                        return 6;
                    }
                    bool insertFound = false;
                    while (sqlDumpReader.ReadLine())
                    {
                        if (sqlDumpReader.CurrentLineCommand == SqlDumpReader.LineCommand.INSERT)
                        {
                            Logger.Debug("INSERT statement found.");
                            insertFound = true;
                            break;
                        }
                    }
                    if (!insertFound)
                    {
                        Logger.Debug("INSERT statement was not found.");
                        Console.WriteLine("Couldn't find any data to import.");
                        return 7;
                    }
                    Importer importer;
                    BitArray existingLibgenIds = null;
                    int existingBookCount = 0;
                    switch (tableType)
                    {
                        case TableType.NON_FICTION:
                            existingBookCount = localDatabase.CountNonFictionBooks();
                            if (existingBookCount != 0)
                            {
                                existingLibgenIds = localDatabase.GetNonFictionLibgenIdsBitArray();
                            }
                            importer = new NonFictionImporter(localDatabase, existingLibgenIds);
                            break;
                        case TableType.FICTION:
                            existingBookCount = localDatabase.CountFictionBooks();
                            if (existingBookCount != 0)
                            {
                                existingLibgenIds = localDatabase.GetFictionLibgenIdsBitArray();
                            }
                            importer = new FictionImporter(localDatabase, existingLibgenIds);
                            break;
                        case TableType.SCI_MAG:
                            existingBookCount = localDatabase.CountSciMagArticles();
                            if (existingBookCount != 0)
                            {
                                existingLibgenIds = localDatabase.GetSciMagLibgenIdsBitArray();
                            }
                            importer = new SciMagImporter(localDatabase, existingLibgenIds);
                            break;
                        default:
                            throw new Exception($"Unknown table type: {tableType}.");
                    }
                    bool updateProgressLine = false;
                    Importer.ImportProgressReporter importProgressReporter = (int objectsAdded, int objectsUpdated) =>
                    {
                        if (updateProgressLine)
                        {
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                        }
                        else
                        {
                            updateProgressLine = true;
                        }
                        if (objectsUpdated > 0)
                        {
                            Console.WriteLine($"Books added: {objectsAdded}, updated: {objectsUpdated}.");
                        }
                        else
                        {
                            Console.WriteLine($"Books added: {objectsAdded}.");
                        }
                    };
                    Logger.Debug("Importing data.");
                    Console.WriteLine("Importing data.");
                    importer.Import(sqlDumpReader, importProgressReporter, IMPORT_PROGRESS_UPDATE_INTERVAL, parsedTableDefinition);
                    DatabaseMetadata databaseMetadata = localDatabase.GetMetadata();
                    switch (tableType)
                    {
                        case TableType.NON_FICTION:
                            databaseMetadata.NonFictionFirstImportComplete = true;
                            break;
                        case TableType.FICTION:
                            databaseMetadata.FictionFirstImportComplete = true;
                            break;
                        case TableType.SCI_MAG:
                            databaseMetadata.SciMagFirstImportComplete = true;
                            break;
                    }
                    localDatabase.UpdateMetadata(databaseMetadata);
                    Logger.Debug("SQL dump import has been completed successfully.");
                    Console.WriteLine("SQL dump import has been completed successfully.");
                    return 0;
                }
            }
        }

        private TableType DetectImportTableType(SqlDumpReader.ParsedTableDefinition parsedTableDefinition)
        {
            if (TableDefinitions.AllTables.TryGetValue(parsedTableDefinition.TableName, out TableDefinition tableDefinition))
            {
                foreach (SqlDumpReader.ParsedColumnDefinition parsedColumnDefinition in parsedTableDefinition.Columns)
                {
                    if (tableDefinition.Columns.TryGetValue(parsedColumnDefinition.ColumnName.ToLower(), out ColumnDefinition columnDefinition))
                    {
                        if (columnDefinition.ColumnType == parsedColumnDefinition.ColumnType)
                        {
                            continue;
                        }
                    }
                    return TableType.UNKNOWN;
                }
                return tableDefinition.TableType;
            }
            return TableType.UNKNOWN;
        }

        private ServerConfiguration LoadServerConfiguration()
        {
            string configurationFilePath = Path.GetFullPath(SERVER_CONFIGURATION_FILE_NAME);
            if (File.Exists(configurationFilePath))
            {
                Console.WriteLine($"Loading server configuration from \"{configurationFilePath}\"...");
                return ServerConfiguration.LoadConfiguration(configurationFilePath);
            }
            else
            {
                Console.WriteLine($"Server configuration file \"{configurationFilePath}\" was not found.");
                return null;
            }
        }

        private ImportFormat? ConvertTableTypeToImportFormat(TableType tableType)
        {
            switch (tableType)
            {
                case TableType.NON_FICTION:
                    return ImportFormat.LIBGEN_NONFICTION;
                case TableType.FICTION:
                    return ImportFormat.LIBGEN_FICTION;
                case TableType.SCI_MAG:
                    return ImportFormat.LIBGEN_SCIMAG;
                default:
                    return null;
            }
        }

        private string ImportFormatToString(ImportFormat importFormat)
        {
            switch (importFormat)
            {
                case ImportFormat.LIBGEN_NONFICTION:
                    return "libgen-nonfiction";
                case ImportFormat.LIBGEN_FICTION:
                    return "libgen-fiction";
                case ImportFormat.LIBGEN_SCIMAG:
                    return "libgen-scimag";
                default:
                    return null;
            }
        }
    }
}
