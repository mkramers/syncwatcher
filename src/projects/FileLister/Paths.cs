using System.IO;

namespace FileLister
{
    internal static class Paths
    {
        public static string AppData
        {
            get
            {
                if (!string.IsNullOrEmpty(RELATIVE_PATH))
                    return Path.Combine(RELATIVE_PATH, APP_DATA);
                return APP_DATA;
            }
        }

        public static string SettingsFile => Path.Combine(AppData, SETTINGS_FILENAME);
        public static string FileBotSettings => Path.Combine(AppData, FILEBOT_SETTINGS_FILENAME);
        public static string FilebotRecords => Path.Combine(AppData, FILEBOT_RECORDS_FILENAME);
        public static string ConfigFilePath => Path.Combine(AppData, CONFIG_FILENAME);

        private static readonly string RELATIVE_PATH = "..\\..\\";
        private static readonly string APP_DATA = "Data";
        private static readonly string SETTINGS_FILENAME = "mutlifilelister.settings.xml";
        private static readonly string FILEBOT_SETTINGS_FILENAME = "fileBot.settings.xml";
        private static readonly string FILEBOT_RECORDS_FILENAME = "fileBot.records.xml";
        private static readonly string CONFIG_FILENAME = "config.txt";
    }
}