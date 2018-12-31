using System;
using System.Linq;
using LibgenServer.Core.Entities;

namespace LibgenServer.Cli.CommandLine
{
    internal class CommandLineParser
    {
        public ICommandLineParserResult Parse(string[] commandLineArguments)
        {
            if (commandLineArguments.Any())
            {
                switch (commandLineArguments[0].ToLowerInvariant())
                {
                    case "db":
                    case "database":
                        return ParseDatabaseCommand(commandLineArguments.Skip(1).ToArray());
                    case "configuration":
                    case "config":
                        return ParseConfigurationCommand(commandLineArguments.Skip(1).ToArray());
                    case "import":
                        return ParseImportCommand(commandLineArguments.Skip(1).ToArray());
                    case "scan":
                        return ParseScanCommand(commandLineArguments.Skip(1).ToArray());
                }
            }
            return new PrintUsageResult(PrintUsageResult.Usage.COMMAND_LIST);
        }

        private ICommandLineParserResult ParseDatabaseCommand(string[] databaseCommandLineArguments)
        {
            if (databaseCommandLineArguments.Any())
            {
                switch (databaseCommandLineArguments[0].ToLowerInvariant())
                {
                    case "create":
                        return ParseDatabaseCreateCommand(databaseCommandLineArguments.Skip(1).ToArray());
                }
            }
            return new PrintUsageResult(PrintUsageResult.Usage.DATABASE);
        }

        private ICommandLineParserResult ParseDatabaseCreateCommand(string[] databaseCreateCommandLineArguments)
        {
            if (databaseCreateCommandLineArguments.Length != 1)
            {
                int returnCode;
                if (databaseCreateCommandLineArguments.Length < 1)
                {
                    Console.WriteLine("Error: path to the database file is not specified.");
                    returnCode = 1;
                }
                else
                {
                    Console.WriteLine("Error: too many arguments.");
                    returnCode = 2;
                }
                return new PrintUsageResult(PrintUsageResult.Usage.DATABASE_CREATE, returnCode);
            }
            return new DatabaseCreateResult(databaseCreateCommandLineArguments[0]);
        }

        private ICommandLineParserResult ParseConfigurationCommand(string[] configurationCommandLineArguments)
        {
            if (configurationCommandLineArguments.Any())
            {
                switch (configurationCommandLineArguments[0].ToLowerInvariant())
                {
                    case "database":
                    case "db":
                        return ParseConfigurationDatabaseCommand(configurationCommandLineArguments.Skip(1).ToArray());
                }
            }
            return new PrintUsageResult(PrintUsageResult.Usage.CONFIGURATION);
        }

        private ICommandLineParserResult ParseConfigurationDatabaseCommand(string[] configurationDatabaseCommandLineArguments)
        {
            if (configurationDatabaseCommandLineArguments.Length != 1)
            {
                int returnCode;
                if (configurationDatabaseCommandLineArguments.Length < 1)
                {
                    Console.WriteLine("Error: path to the database file is not specified.");
                    returnCode = 1;
                }
                else
                {
                    Console.WriteLine("Error: too many arguments.");
                    returnCode = 2;
                }
                return new PrintUsageResult(PrintUsageResult.Usage.CONFIGURATION_DATABASE, returnCode);
            }
            return new ConfigurationResult(configurationDatabaseCommandLineArguments[0]);
        }

        private ICommandLineParserResult ParseImportCommand(string[] importCommandLineArguments)
        {
            if (importCommandLineArguments.Length != 2)
            {
                int returnCode;
                if (importCommandLineArguments.Length < 1)
                {
                    Console.WriteLine("Error: import format is not specified.");
                    returnCode = 1;
                }
                else if (importCommandLineArguments.Length < 2)
                {
                    Console.WriteLine("Error: path to the file to import is not specified.");
                    returnCode = 2;
                }
                else
                {
                    Console.WriteLine("Error: too many arguments.");
                    returnCode = 3;
                }
                return new PrintUsageResult(PrintUsageResult.Usage.IMPORT, returnCode);
            }
            string importFormatString = importCommandLineArguments[0];
            string importFilePath = importCommandLineArguments[1];
            ImportFormat? importFormat;
            switch (importFormatString.ToLowerInvariant())
            {
                case "libgen-nonfiction":
                    importFormat = ImportFormat.LIBGEN_NONFICTION;
                    break;
                default:
                    importFormat = null;
                    break;
            }
            if (!importFormat.HasValue)
            {
                Console.WriteLine($"Error: import format {importFormatString} is not supported.");
                return new PrintUsageResult(PrintUsageResult.Usage.IMPORT, 4);
            }
            return new ImportResult(importFormat.Value, importFilePath);
        }

        private ICommandLineParserResult ParseScanCommand(string[] scanCommandLineArguments)
        {
            if (scanCommandLineArguments.Length != 2)
            {
                int returnCode;
                if (scanCommandLineArguments.Length < 1)
                {
                    Console.WriteLine("Error: library is not specified.");
                    returnCode = 1;
                }
                else if (scanCommandLineArguments.Length < 2)
                {
                    Console.WriteLine("Error: path to the directory to scan is not specified.");
                    returnCode = 2;
                }
                else
                {
                    Console.WriteLine("Error: too many arguments.");
                    returnCode = 3;
                }
                return new PrintUsageResult(PrintUsageResult.Usage.SCAN, returnCode);
            }
            string scanLibraryString = scanCommandLineArguments[0];
            string scanDirectoryPath = scanCommandLineArguments[1];
            ScanLibrary? scanLibrary;
            switch (scanLibraryString.ToLowerInvariant())
            {
                case "libgen-nonfiction":
                    scanLibrary = ScanLibrary.LIBGEN_NONFICTION;
                    break;
                default:
                    scanLibrary = null;
                    break;
            }
            if (!scanLibrary.HasValue)
            {
                Console.WriteLine($"Error: library {scanLibraryString} is not supported.");
                return new PrintUsageResult(PrintUsageResult.Usage.SCAN, 4);
            }
            return new ScanResult(scanLibrary.Value, scanDirectoryPath);
        }
    }
}
