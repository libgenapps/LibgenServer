using System;
using System.IO;
using Newtonsoft.Json;

namespace LibgenServer.Core.Configuration
{
    public class ServerConfiguration
    {
        public static ServerConfiguration Default
        {
            get
            {
                return new ServerConfiguration
                {
                    DatabaseFilePath = null
                };
            }
        }

        public string DatabaseFilePath { get; set; }

        public static ServerConfiguration LoadConfiguration(string configurationFilePath)
        {
            ServerConfiguration result;
            try
            {
                if (File.Exists(configurationFilePath))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    using (StreamReader streamReader = new StreamReader(configurationFilePath))
                    using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                    {
                        result = jsonSerializer.Deserialize<ServerConfiguration>(jsonTextReader);
                    }
                    result = ValidateAndCorrect(result);
                }
                else
                {
                    result = Default;
                }
            }
            catch
            {
                result = Default;
            }
            return result;
        }

        public static void SaveConfiguration(string configurationFilePath, ServerConfiguration serverConfiguration)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            using (StreamWriter streamWriter = new StreamWriter(configurationFilePath))
            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
            {
                jsonTextWriter.Formatting = Formatting.Indented;
                jsonTextWriter.Indentation = 4;
                jsonSerializer.Serialize(jsonTextWriter, serverConfiguration);
            }
        }

        public static ServerConfiguration ValidateAndCorrect(ServerConfiguration configuration)
        {
            if (configuration == null)
            {
                return Default;
            }
            else
            {
                configuration.ValidateAndCorrectDatabaseFilePath();
                return configuration;
            }
        }

        private void ValidateAndCorrectDatabaseFilePath()
        {
            if (DatabaseFilePath == null)
            {
                DatabaseFilePath = String.Empty;
            }
        }
    }
}
