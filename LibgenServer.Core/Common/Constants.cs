using System;

namespace LibgenServer.Core.Common
{
    public static class Constants
    {
        public const string DATABASE_METADATA_APP_NAME = "LibgenServer";
        public const string CURRENT_VERSION = "0.1 alpha";
        public const string CURRENT_GITHUB_RELEASE_NAME = "0.1";
        public static readonly DateTime CURRENT_GITHUB_RELEASE_DATE = new DateTime(2018, 12, 31);
        public const string CURRENT_DATABASE_VERSION = "0.1";
        public const string SERVER_CONFIGURATION_FILE_NAME = "libgen.config";

        public const double SEARCH_PROGRESS_REPORT_INTERVAL = 0.1;
        public const double IMPORT_PROGRESS_UPDATE_INTERVAL = 0.5;
        public const double SYNCHRONIZATION_PROGRESS_UPDATE_INTERVAL = 0.1;
        public const int DATABASE_TRANSACTION_BATCH = 500;
        public const int DEFAULT_MAXIMUM_SEARCH_RESULT_COUNT = 50000;
    }
}
