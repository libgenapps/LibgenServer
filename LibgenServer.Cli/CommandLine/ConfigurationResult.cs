using LibgenServer.Cli.Configuration;

namespace LibgenServer.Cli.CommandLine
{
    internal class ConfigurationResult : ICommandLineParserResult
    {
        private readonly string databaseFilePath;

        public ConfigurationResult(string databaseFilePath)
        {
            this.databaseFilePath = databaseFilePath;
        }

        public int Execute()
        {
            ConfigurationOperations configurationOperations = new ConfigurationOperations();
            return configurationOperations.SetDatabaseFilePath(databaseFilePath);
        }
    }
}
