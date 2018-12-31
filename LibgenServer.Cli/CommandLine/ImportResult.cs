using LibgenServer.Cli.Import;
using LibgenServer.Core.Entities;

namespace LibgenServer.Cli.CommandLine
{
    internal class ImportResult : ICommandLineParserResult
    {
        private readonly ImportFormat importFormat;
        private readonly string importFilePath;

        public ImportResult(ImportFormat importFormat, string importFilePath)
        {
            this.importFormat = importFormat;
            this.importFilePath = importFilePath;
        }

        public int Execute()
        {
            ImportOperations importOperations = new ImportOperations(importFormat, importFilePath);
            return importOperations.Import();
        }
    }
}
