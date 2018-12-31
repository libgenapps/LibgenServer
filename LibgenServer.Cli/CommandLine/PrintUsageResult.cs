using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LibgenServer.Cli.CommandLine
{
    internal class PrintUsageResult : ICommandLineParserResult
    {
        internal enum Usage
        {
            COMMAND_LIST,
            DATABASE,
            DATABASE_CREATE,
            CONFIGURATION,
            CONFIGURATION_DATABASE,
            IMPORT,
            SCAN
        }

        private readonly Usage usage;
        private readonly int returnCode;
        private readonly string executableName;
        private bool needEmptyLine;

        public PrintUsageResult(Usage usage, int returnCode = 0)
        {
            this.usage = usage;
            this.returnCode = returnCode;
            executableName = Assembly.GetExecutingAssembly().GetName().Name;
            needEmptyLine = false;
        }

        public int Execute()
        {
            switch (usage)
            {
                case Usage.COMMAND_LIST:
                    PrintAppName();
                    PrintUsage("<command> [<command arguments>]");
                    PrintCommandsHeader();
                    PrintKeyValuePairs(new[] {
                        KeyValuePair.Create("config", "Alias for configuration command"),
                        KeyValuePair.Create("configuration", "Configuration management"),
                        KeyValuePair.Create("database", "Database operations"),
                        KeyValuePair.Create("db", "Alias for database command"),                                                                            
                        KeyValuePair.Create("import", "Import a database dump"),
                        KeyValuePair.Create("scan", "Scan local files and add them to the library")
                    });
                    break;
                case Usage.DATABASE:
                    PrintAppName();
                    PrintUsage("database <command> [<command arguments>]");
                    PrintCommandsHeader();
                    PrintKeyValuePairs(new[] {
                        KeyValuePair.Create("create", "Create a new database")
                    });
                    break;
                case Usage.DATABASE_CREATE:
                    PrintUsage("database create <path to a new database file>");
                    break;
                case Usage.CONFIGURATION:
                    PrintAppName();
                    PrintUsage("configuration <option> <value>");
                    PrintOptionsHeader();
                    PrintKeyValuePairs(new[] {
                        KeyValuePair.Create("database", "Path to the database file"),
                        KeyValuePair.Create("db", "Alias for database option")
                    });
                    break;
                case Usage.CONFIGURATION_DATABASE:
                    PrintUsage("database configuration database <path to the database file>");
                    break;
                case Usage.IMPORT:
                    Console.WriteLine();
                    PrintUsage("import <format> <path to the file to import>");
                    Console.WriteLine();
                    Console.WriteLine("Supported formats:");
                    PrintKeyValuePairs(new[] {
                        KeyValuePair.Create("libgen-nonfiction", "Library Genesis MySQL dump for the non-fiction books (unpacked sql or a zip/rar/gz/7z archive)")
                    });
                    break;
                case Usage.SCAN:
                    Console.WriteLine();
                    PrintUsage("scan <library> <path to the directory to scan>");
                    Console.WriteLine();
                    Console.WriteLine("Supported libraries:");
                    PrintKeyValuePairs(new[] {
                        KeyValuePair.Create("libgen-nonfiction", "Library Genesis non-fiction books")
                    });
                    break;
            }
            return returnCode;
        }

        private void PrintAppName()
        {
            Console.WriteLine("LibgenServer command-line tools 0.1 alpha");
            needEmptyLine = true;
        }

        private void PrintUsage(string commandFormat)
        {
            PrintEmptyLineIfNeeded();
            Console.WriteLine($"Usage: {executableName} {commandFormat}");
            needEmptyLine = true;
        }

        private void PrintCommandsHeader()
        {
            PrintEmptyLineIfNeeded();
            Console.WriteLine("Available commands:");
            needEmptyLine = false;
        }

        private void PrintOptionsHeader()
        {
            PrintEmptyLineIfNeeded();
            Console.WriteLine("Available options:");
            needEmptyLine = false;
        }

        private void PrintKeyValuePairs(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            int maxKeyNameLength = keyValuePairs.Max(command => command.Key.Length);
            foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
            {
                Console.WriteLine($"    {keyValuePair.Key}{new string(' ', maxKeyNameLength - keyValuePair.Key.Length + 2)}{keyValuePair.Value}");
            }
        }

        private void PrintEmptyLineIfNeeded()
        {
            if (needEmptyLine)
            {
                Console.WriteLine();
            }
        }
    }
}
