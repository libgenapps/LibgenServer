using System;
using System.Diagnostics;
using LibgenServer.Cli.CommandLine;

namespace LibgenServer.Cli
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            CommandLineParser commandLineParser = new CommandLineParser();
            ICommandLineParserResult commandLineParserResult = commandLineParser.Parse(args);
            int result = commandLineParserResult.Execute();
            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
            return result;
        }
    }
}
