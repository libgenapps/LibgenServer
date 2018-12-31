using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using LibgenServer.Core.Common;
using LibgenServer.Core.Configuration;
using LibgenServer.Core.Database;
using LibgenServer.Core.Entities;
using static LibgenServer.Core.Common.Constants;

namespace LibgenServer.Cli.Scan
{
    internal class ScanOperations
    {
        private readonly ScanLibrary scanLibrary;
        private readonly string scanDirectoryPath;

        public ScanOperations(ScanLibrary scanLibrary, string scanDirectoryPath)
        {
            this.scanLibrary = scanLibrary;
            this.scanDirectoryPath = scanDirectoryPath.Trim().TrimEnd(Path.DirectorySeparatorChar);
        }

        public int Scan()
        {
            ServerConfiguration serverConfiguration = LoadServerConfiguration();
            if (serverConfiguration == null)
            {
                return 1;
            }
            string databaseFilePath = serverConfiguration.DatabaseFilePath;
            if (String.IsNullOrWhiteSpace(databaseFilePath))
            {
                Console.WriteLine("Database file path is not set in the server configuration file.");
                return 2;
            }
            if (!File.Exists(databaseFilePath))
            {
                Console.WriteLine($"Couldn't fine the database file at {databaseFilePath}");
                return 3;
            }
            Console.WriteLine($"Opening database \"{databaseFilePath}\"...");
            LocalDatabase localDatabase = LocalDatabase.OpenDatabase(databaseFilePath);
            List<LibraryFile> foundFiles;
            switch (scanLibrary)
            {
                case ScanLibrary.LIBGEN_NONFICTION:
                    foundFiles = Scan(localDatabase.CountNonFictionBooks(), localDatabase.GetNonFictionBookByMd5Hash);
                    break;
                case ScanLibrary.LIBGEN_FICTION:
                    foundFiles = Scan(localDatabase.CountFictionBooks(), localDatabase.GetFictionBookByMd5Hash);
                    break;
                case ScanLibrary.LIBGEN_SCIMAG:
                    foundFiles = Scan(localDatabase.CountSciMagArticles(), localDatabase.GetSciMagArticleByMd5Hash);
                    break;
                default:
                    throw new ArgumentException($"Unknown library: {scanLibrary}.");
            }
            if (foundFiles.Any())
            {
                Console.WriteLine("Adding files to the library...");
                localDatabase.AddFiles(foundFiles);
                Console.WriteLine("Files have been successfully added to the library.");
            }
            else
            {
                Console.WriteLine("No files to add to the library.");
            }
            return 0;
        }

        private List<LibraryFile> Scan<T>(int objectsInDatabaseCount, Func<string, T> getObjectByMd5HashFunction)
            where T : LibgenObject
        {
            Logger.Debug($"Scan request in directory = {scanDirectoryPath}, object count = {objectsInDatabaseCount}, object type = {typeof(T).Name}.");
            List<LibraryFile> foundFiles = new List<LibraryFile>();
            int found = 0;
            int notFound = 0;
            int errors = 0;
            if (objectsInDatabaseCount > 0)
            {
                ScanDirectory(scanDirectoryPath, scanDirectoryPath, getObjectByMd5HashFunction, foundFiles, ref found, ref notFound, ref errors);
            }
            Console.WriteLine($"Scan complete. Found: {found}, not found: {notFound}, errors: {errors}.");
            return foundFiles;
        }

        private void ScanDirectory<T>(string rootScanDirectory, string scanDirectory, Func<string, T> getObjectByMd5HashFunction,
            List<LibraryFile> foundFiles, ref int found, ref int notFound, ref int errors)
            where T : LibgenObject
        {
            try
            {
                foreach (string filePath in Directory.EnumerateFiles(scanDirectory))
                {
                    string relativeFilePath = filePath;
                    if (relativeFilePath.StartsWith(rootScanDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        relativeFilePath = relativeFilePath.Substring(rootScanDirectory.Length + 1);
                    }
                    string md5Hash;
                    try
                    {
                        using (MD5 md5 = MD5.Create())
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            byte[] md5HashArray = md5.ComputeHash(fileStream);
                            md5Hash = BitConverter.ToString(md5HashArray).Replace("-", String.Empty).ToLowerInvariant();
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Debug($"Couldn't calculate MD5 hash for the file: {filePath}");
                        Console.WriteLine($"Couldn't calculate MD5 hash for the file: {filePath}");
                        Logger.Exception(exception);
                        errors++;
                        continue;
                    }
                    try
                    {
                        T libgenObject = getObjectByMd5HashFunction(md5Hash);
                        if (libgenObject != null)
                        {
                            Console.WriteLine($"Found: {relativeFilePath}");
                            LibraryFile libraryFile = new LibraryFile
                            {
                                FilePath = Path.Combine(rootScanDirectory, relativeFilePath),
                                ArchiveEntry = null,
                                ObjectType = libgenObject.LibgenObjectType,
                                ObjectId = libgenObject.Id
                            };
                            foundFiles.Add(libraryFile);
                            found++;
                        }
                        else
                        {
                            Console.WriteLine($"Not found: {relativeFilePath}");
                            notFound++;
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Debug($"Couldn't lookup the MD5 hash: {md5Hash} in the database for the file: {filePath}");
                        Console.WriteLine($"Error: couldn't lookup the MD5 hash: {md5Hash} in the database for the file: {filePath}");
                        Logger.Exception(exception);
                        errors++;
                        continue;
                    }
                }
                foreach (string directoryPath in Directory.EnumerateDirectories(scanDirectory))
                {
                    ScanDirectory(rootScanDirectory, directoryPath, getObjectByMd5HashFunction, foundFiles, ref found, ref notFound, ref errors);
                }
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
                Console.WriteLine($"Error while scanning the directory: {scanDirectory}");
                Console.WriteLine(exception.ToString());
                errors++;
            }
        }

        private ServerConfiguration LoadServerConfiguration()
        {
            string configurationFilePath = Path.GetFullPath(SERVER_CONFIGURATION_FILE_NAME);
            if (File.Exists(configurationFilePath))
            {
                Console.WriteLine($"Loading server configuration from \"{configurationFilePath}\"...");
                return ServerConfiguration.LoadConfiguration(configurationFilePath);
            }
            else
            {
                Console.WriteLine($"Server configuration file \"{configurationFilePath}\" was not found.");
                return null;
            }
        }
    }
}
