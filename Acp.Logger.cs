using System;
using System.Diagnostics;

/// <summary>
/// Summary description for Acp.Logger
/// </summary>
namespace Acp
{
    public class Logger
    {

        public static int LEVEL_DEBUG = 0;
        public static int LEVEL_WARN  = 1;
        public static int LEVEL_ERROR = 2;

        private string _sourceName = "Application";
        private int    _logLevel   = LEVEL_WARN;

        public Logger()
        {

        }
        public Logger(string SourceName)
        {
           _sourceName = SourceName;

        }
        
        public Logger(int LogLevel)
        {
            _logLevel = LogLevel;

        }

        public Logger(string SourceName, int LogLevel)
        {
            _sourceName = SourceName;
            _logLevel   = LogLevel;

        }
        public void Debug(string text)
        {
            if (LEVEL_DEBUG >= _logLevel)
                EventLog.WriteEntry(_sourceName, text, EventLogEntryType.Information);
        }

        public void Warn(string text)
        {
            if (LEVEL_WARN >= _logLevel) 
                EventLog.WriteEntry(_sourceName, text, EventLogEntryType.Warning);

        }

        public void Error(string text)
        {
            if (LEVEL_ERROR >= _logLevel) 
                EventLog.WriteEntry(_sourceName, text, EventLogEntryType.Error );
        }

        public void Error(string text, Exception ex)
        {
            if (LEVEL_ERROR >= _logLevel)
            {
                Error(text);
                Error(ex.StackTrace);
            }
        }
    }
}