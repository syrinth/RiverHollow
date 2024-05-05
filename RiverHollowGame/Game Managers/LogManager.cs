using RiverHollow.Misc;
using RiverHollow.Utilities;
using System;
using System.IO;

using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    public static class LogManager
    {
        private static StreamWriter _swWriter;

        private static bool DebugEnabled => Constants.WRITE_TO_ERROR_LOG;
        public static void Initialize()
        {
            if (DebugEnabled)
            {
                if (!Directory.Exists(Constants.RIVER_HOLLOW_LOG)) { Directory.CreateDirectory(Constants.RIVER_HOLLOW_LOG); }
                var date = DateTime.Now.ToLocalTime();
                string fileName = string.Format("{0}\\{1}-{2}.txt", Constants.RIVER_HOLLOW_LOG, date.Day, date.Month);
                _swWriter = new StreamWriter(fileName);
                WriteToLog("Opening Log file");
            }
        }

        public static void CloseLogFile()
        {
            WriteToLog("Closing Log File\n\n");
        }

        public static void LogException(Exception ex)
        {
            WriteToLog(ex.ToString(), LogEnum.Error);
        }

        public static void WriteToLog(string text, params object[] args)
        {
            WriteToLog(string.Format(text, args), LogEnum.Warning);
        }

        public static void WriteToLog(string text, LogEnum e = LogEnum.Info)
        {
            if (DebugEnabled)
            {
                var date = DateTime.Now.ToLocalTime();
                var output = string.Format("[{0} - {1}]: {2}", date.ToString("MM/dd/yyyy hh:mm:ss.fff tt"), e.ToString(), text);
                _swWriter?.WriteLine(output);
                _swWriter?.Flush();
            }
        }
    }
}
