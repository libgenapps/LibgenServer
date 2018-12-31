using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using LibgenServer.Core.Configuration;
using LibgenServer.Core.Database;
using LibgenServer.Core.Entities;
using LibgenServer.Web.ViewModels.Main;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using static LibgenServer.Core.Common.Constants;

namespace LibgenServer.Web.Models
{
    public class MainModel
    {
        private readonly LocalDatabase localDatabase;
        private readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider;
        private readonly NumberFormatInfo numberFormatInfo;
        private readonly string[] fileSizePostfixes;

        public MainModel(IOptionsMonitor<ServerConfiguration> optionsMonitor, FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            ServerConfiguration serverConfiguration = optionsMonitor.CurrentValue;
            if (serverConfiguration == null)
            {
                throw new Exception("Couldn't load configuration.");
            }
            string databaseFilePath = serverConfiguration.DatabaseFilePath;
            if (String.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new Exception("Database file path is not set in the server configuration file.");
            }
            if (!File.Exists(databaseFilePath))
            {
                throw new Exception($"Couldn't find the database file at {databaseFilePath}");
            }
            localDatabase = LocalDatabase.OpenDatabase(databaseFilePath);
            this.fileExtensionContentTypeProvider = fileExtensionContentTypeProvider;
            numberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            numberFormatInfo.NumberDecimalSeparator = ".";
            numberFormatInfo.NumberGroupSeparator = ",";
            fileSizePostfixes = new[] { "bytes", "KB", "MB", "GB", "TB" };
        }

        public List<SearchResultItemViewModel> Search(string searchQuery)
        {
            List<SearchResultItemViewModel> result = new List<SearchResultItemViewModel>();
            foreach (NonFictionBook book in localDatabase.SearchNonFictionBooks(searchQuery, DEFAULT_MAXIMUM_SEARCH_RESULT_COUNT))
            {
                SearchResultItemViewModel resultItem = new SearchResultItemViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Authors = book.Authors,
                    Series = book.Series,
                    Year = book.Year,
                    Publisher = book.Publisher,
                    Format = book.Format,
                    FileSize = FileSizeToString(book.SizeInBytes, false),
                    Ocr = book.Searchable == "1"
                };
                result.Add(resultItem);
            }
            return result;
        }

        public BookViewModel GetBookViewModel(int bookId)
        {
            NonFictionBook book = localDatabase.GetNonFictionBookById(bookId);
            if (book == null)
            {
                return null;
            }
            return new BookViewModel
            {
                Title = book.Title,
                Authors = book.Authors,
                Series = book.Series,
                Publisher = book.Publisher,
                Year = book.Year,
                Language = book.Language,
                Format = book.Format,
                Identifier = book.Identifier,
                AddedDateTime = FormatDateTime(book.AddedDateTime),
                LastModifiedDateTime = FormatDateTime(book.LastModifiedDateTime),
                Library = book.Library,
                FileSize = FileSizeToString(book.SizeInBytes, true),
                Topic = book.Topic,
                Volume = book.VolumeInfo,
                Periodical = book.Periodical,
                City = book.City,
                Edition = book.Edition,
                Pages = GetPagesText(book.Pages, book.PagesInFile),
                Tags = book.Tags,
                Md5Hash = book.Md5Hash,
                Commentary = book.Commentary,
                LibgenId = book.LibgenId.ToString(),
                Issn = book.Issn,
                Udc = book.Udc,
                Lbc = book.Lbc,
                Lcc = book.Lcc,
                Ddc = book.Ddc,
                Doi = book.Doi,
                OpenLibraryId = book.OpenLibraryId,
                GoogleBookId = book.GoogleBookId,
                Asin = book.Asin,
                Dpi = book.Dpi.ToString(),
                Ocr = GetOcrString(book.Searchable),
                Bookmarked = GetBookmarkedString(book.Bookmarked),
                Scanned = GetScannedString(book.Scanned),
                Orientation = GetOrientationString(book.Orientation),
                Paginated = GetPaginatedString(book.Paginated),
                Color = GetColorString(book.Color),
                Cleaned = GetCleanedString(book.Cleaned),
                IsInLibrary = book.FileId.HasValue
            };
        }

        public DownloadBookViewModel GetDownloadBookViewModel(string md5Hash)
        {
            NonFictionBook nonFictionBook = localDatabase.GetNonFictionBookByMd5Hash(md5Hash.ToLowerInvariant());
            if (nonFictionBook == null || !nonFictionBook.FileId.HasValue)
            {
                return null;
            }
            LibraryFile libraryFile = localDatabase.GetFileById(nonFictionBook.FileId.Value);
            string downloadFileName = String.Concat(nonFictionBook.Authors, " - ", nonFictionBook.Title, ".", nonFictionBook.Format);
            if (downloadFileName.Length > 200)
            {
                downloadFileName = String.Concat(nonFictionBook.Md5Hash, ".", nonFictionBook.Format);
            }
            DownloadBookViewModel result = new DownloadBookViewModel
            {
                DownloadFileName = downloadFileName,
                LocalFilePath = libraryFile.FilePath
            };
            if (fileExtensionContentTypeProvider.TryGetContentType(downloadFileName, out string contentTypeFromFormat))
            {
                result.ContentType = contentTypeFromFormat;
            }
            else if (fileExtensionContentTypeProvider.TryGetContentType(downloadFileName, out string contentTypeFromLocalFileExtension))
            {
                result.ContentType = contentTypeFromLocalFileExtension;
            }
            else
            {
                result.ContentType = "application/octet-stream";
            }
            return result;
        }

        private string FileSizeToString(long fileSize, bool showBytes)
        {
            int postfixIndex = fileSize != 0 ? (int)Math.Floor(Math.Log(fileSize) / Math.Log(1024)) : 0;
            StringBuilder resultBuilder = new StringBuilder();
            resultBuilder.Append((fileSize / Math.Pow(1024, postfixIndex)).ToString("N2", numberFormatInfo));
            resultBuilder.Append(" ");
            resultBuilder.Append(fileSizePostfixes[postfixIndex]);
            if (showBytes && postfixIndex != 0)
            {
                resultBuilder.Append(" (");
                resultBuilder.Append(fileSize.ToString("N0", numberFormatInfo));
                resultBuilder.Append(" ");
                resultBuilder.Append(fileSizePostfixes[0]);
                resultBuilder.Append(")");
            }
            return resultBuilder.ToString();
        }

        private string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
        }

        private string GetPagesText(string bodyMatterPages, int totalPages)
        {
            StringBuilder resultBuilder = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(bodyMatterPages))
            {
                resultBuilder.Append(bodyMatterPages);
            }
            else
            {
                resultBuilder.Append("unknown");
            }
            resultBuilder.Append(" (body matter) / ");
            resultBuilder.Append(totalPages.ToString());
            resultBuilder.Append(" (total)");
            return resultBuilder.ToString();
        }

        private string GetOcrString(string value) => StringBooleanToYesNoUnknownString(value);
        private string GetBookmarkedString(string value) => StringBooleanToYesNoUnknownString(value);
        private string GetScannedString(string value) => StringBooleanToYesNoUnknownString(value);
        private string GetOrientationString(string value) => StringBooleanToOrientationString(value);
        private string GetPaginatedString(string value) => StringBooleanToYesNoUnknownString(value);
        private string GetColorString(string value) => StringBooleanToYesNoUnknownString(value);
        private string GetCleanedString(string value) => StringBooleanToYesNoUnknownString(value);
        private string StringBooleanToYesNoUnknownString(string value) => StringBooleanToLabelString(value, "yes", "no", "unknown");
        private string StringBooleanToOrientationString(string value) => StringBooleanToLabelString(value, "portrait", "landscape", "unknown");

        private string StringBooleanToLabelString(string value, string value1Label, string value0Label, string valueUnknownLabel)
        {
            switch (value)
            {
                case "0":
                    return value0Label;
                case "1":
                    return value1Label;
                default:
                    return valueUnknownLabel;
            }
        }
    }
}
