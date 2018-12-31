using System;
using System.Collections.Generic;
using System.Text;
using LibgenServer.Cli.Database;

namespace LibgenServer.Cli.CommandLine
{
    internal class DatabaseCreateResult : ICommandLineParserResult
    {
        private readonly DatabaseOperations databaseOperations;

        public DatabaseCreateResult(string databaseFilePath)
        {
            databaseOperations = new DatabaseOperations(databaseFilePath);
        }

        public int Execute()
        {
            return databaseOperations.CreateDatabase();
        }
    }
}
