using LibgenServer.Cli.Scan;
using LibgenServer.Core.Entities;

namespace LibgenServer.Cli.CommandLine
{
    internal class ScanResult : ICommandLineParserResult
    {
        private readonly ScanLibrary scanLibrary;
        private readonly string scanDirectoryPath;

        public ScanResult(ScanLibrary scanLibrary, string scanDirectoryPath)
        {
            this.scanLibrary = scanLibrary;
            this.scanDirectoryPath = scanDirectoryPath;
        }

        public int Execute()
        {
            ScanOperations scanOperations = new ScanOperations(scanLibrary, scanDirectoryPath);
            return scanOperations.Scan();
        }
    }
}
