using System;
using System.IO;
using LibgenServer.Core.Database;
using LibgenServer.Core.Entities;
using static LibgenServer.Core.Common.Constants;

namespace LibgenServer.Cli.Database
{
    internal class DatabaseOperations
    {
        private readonly string databaseFilePath;

        public DatabaseOperations(string databaseFilePath)
        {
            this.databaseFilePath = databaseFilePath;
        }

        public int CreateDatabase()
        {
            try
            {
                if (File.Exists(databaseFilePath))
                {
                    Console.WriteLine($"Error: cannot create database file \"{databaseFilePath}\" because it already exists.");
                    return 2;
                }
                Console.WriteLine("Creating a database file...");
                LocalDatabase localDatabase = LocalDatabase.CreateDatabase(databaseFilePath);
                Console.WriteLine("Creating the metadata table...");
                localDatabase.CreateMetadataTable();
                Console.WriteLine("Creating the files table...");
                localDatabase.CreateFilesTable();
                Console.WriteLine("Creating the non-fiction tables...");
                localDatabase.CreateNonFictionTables();
                Console.WriteLine("Creating the fiction tables...");
                localDatabase.CreateFictionTables();
                Console.WriteLine("Creating the scimag tables...");
                localDatabase.CreateSciMagTables();
                Console.WriteLine("Adding metadata values...");
                DatabaseMetadata databaseMetadata = new DatabaseMetadata
                {
                    AppName = DATABASE_METADATA_APP_NAME,
                    Version = CURRENT_DATABASE_VERSION,
                    NonFictionFirstImportComplete = false,
                    FictionFirstImportComplete = false,
                    SciMagFirstImportComplete = false
                };
                localDatabase.AddMetadata(databaseMetadata);
                Console.WriteLine("Database has been created successfully.");
                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine("There was an error while trying to create a database:");
                Console.WriteLine(exception.ToString());
                return 1;
            }
        }
    }
}
