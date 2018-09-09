using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FilebotApi.Result;

namespace FilebotApi
{
    public static class FileBotLogParser
    {
        private const string LOCK = "Locking";
        private const string RUN = "Run script";
        private const string PARAMETER = "Parameter:";
        private const string ARGUMENT = "Argument:";
        private const string USING_EXCLUDES = "Using excludes:";
        private const string INGNORE_HIDDEN = "Ignore hidden folder:";
        private const string EXCLUDE = "Exclude:";
        private const string GROUP = "Group:";
        private const string AUTO_DETECTED = "Auto-detected query:";
        private const string TEST_RENAME = "[TEST] From";
        private const string MOVE_RENAME = "[MOVE] From";
        private const string SKIPPED = "Skipped";
        private const string PROCESSED = "Processed";
        private const string DONE = "Done ãƒ¾(ï¼ âŒ’ãƒ¼âŒ’ï¼ )ãƒŽ";

        private static Dictionary<string, Func<string, DateTime, FileBotResult>> Actions { get; } = new Dictionary<string, Func<string, DateTime, FileBotResult>>
        {
            {TEST_RENAME, Rename},
            {MOVE_RENAME, Rename},

            {SKIPPED, Skip}

            //{LOCK, Log },
            //{RUN, Log },
            //{PARAMETER, Log },
            //{ARGUMENT, Log },
            //{USING_EXCLUDES, Log },
            //{INGNORE_HIDDEN, Log },
            //{EXCLUDE, Log },
            //{GROUP, Log },
            //{AUTO_DETECTED, Log },
            //{PROCESSED, Log },
            //{DONE, Log },
        };

        internal static bool TryParse(string _line, out FileBotResult _result)
        {
            Debug.Assert(_line != null);

            List<string> keys = Actions.Keys.Where(_line.StartsWith).ToList();
            Debug.Assert(keys.Count <= 1);
            if (keys.Count == 1)
            {
                string key = keys.First();

                Func<string, DateTime, FileBotResult> action = Actions[key];
                _result = action(_line, DateTime.Now);
            }
            else
            {
                _result = new FileBotResult(_line, DateTime.Now);
            }

            return _result != null;
        }

        public static RenameResult Rename(string _line, DateTime _dateTime)
        {
            const string testPrefix = TEST_RENAME + " ";
            const string movePrefix = MOVE_RENAME + " ";

            string trimmed = "";
            if (_line.StartsWith(testPrefix))
            {
                trimmed = _line.Replace(testPrefix, "");
            }
            else if (_line.StartsWith(movePrefix))
            {
                trimmed = _line.Replace(movePrefix, "");
            }
            else
            {
                Debug.Fail("rename prefix not supported!");
            }

            string[] split = trimmed.Split(new[] {"] to ["}, StringSplitOptions.None);
            Debug.Assert(split.Length == 2);

            string source = split[0].TrimStart('[');
            string destination = split[1].TrimEnd(']');

            RenameResult result = new RenameResult(Filebot.ActionType.TEST, source, destination, _line, _dateTime);
            return result;
        }

        public static SkipResult Skip(string _line, DateTime _dateTime)
        {
            const string prefix = SKIPPED + " ";

            string trimmed = "";
            if (_line.StartsWith(prefix))
            {
                trimmed = _line.Replace(prefix, "");
            }
            else
            {
                Debug.Fail("skip prefix not supported!");
            }

            string[] split = trimmed.Split(new[] {"] because ["}, StringSplitOptions.None);
            Debug.Assert(split.Length == 2);

            string source = split[0].TrimStart('[');
            //var destination = split[1].TrimEnd(']');

            SkipResult result = new SkipResult(source, _line, _dateTime);
            return result;
        }

        public static FileBotResult Log(string _line, DateTime _dateTime)
        {
            string message = $"[LOG] {_line}";
            return new FileBotResult(message, _dateTime);
        }
    }
}
