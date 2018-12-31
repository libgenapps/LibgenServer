using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LibgenServer.Core.Common
{
    internal static class Environment
    {
        static Environment()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string logFileName = $"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.log";
            LogFilePath = Path.Combine(executingAssembly.Location, "Logs", logFileName);
            OsVersion = RuntimeInformation.OSDescription;
            TargetFramework = executingAssembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
            IsIn64BitProcess = System.Environment.Is64BitProcess;
        }

        public static string LogFilePath { get; }
        public static string OsVersion { get; }
        public static string TargetFramework { get; }
        public static bool IsIn64BitProcess { get; }
    }

}
