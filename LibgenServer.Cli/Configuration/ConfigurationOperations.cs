using System;
using System.IO;
using LibgenServer.Core.Configuration;
using static LibgenServer.Core.Common.Constants;

namespace LibgenServer.Cli.Configuration
{
    internal class ConfigurationOperations
    {
        public int SetDatabaseFilePath(string databaseFilePath)
        {
            string configurationFilePath = Path.GetFullPath(SERVER_CONFIGURATION_FILE_NAME);
            ServerConfiguration serverConfiguration;
            if (File.Exists(configurationFilePath))
            {
                Console.WriteLine($"Loading server configuration from \"{configurationFilePath}\"...");
                serverConfiguration = ServerConfiguration.LoadConfiguration(configurationFilePath);
            }
            else
            {
                Console.WriteLine($"Server configuration file \"{configurationFilePath}\" was not found. A new one will be created.");
                serverConfiguration = ServerConfiguration.Default;
            }
            Console.WriteLine($"Setting database file path to \"{databaseFilePath}\"...");
            serverConfiguration.DatabaseFilePath = databaseFilePath;
            Console.WriteLine($"Saving server configuration file...");
            ServerConfiguration.SaveConfiguration(configurationFilePath, serverConfiguration);
            Console.WriteLine("Database file path has been set successfully.");
            return 0;
        }
    }
}
