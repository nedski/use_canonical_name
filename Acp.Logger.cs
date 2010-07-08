using System;
using System.Diagnostics;

/// <summary>
/// Summary description for Acp.Logger
/// </summary>
namespace Acp
{
    public class Logger : ILogger
    {

        private string _sourceName = String.Empty;

        public Logger(string SourceName)
        {
           _sourceName = SourceName;

        }
        public void Debug(string text)
        {
            EventLog.WriteEntry(_sourceName, text, EventLogEntryType.Information);
        }

        public void Warn(string text)
        {
            EventLog.WriteEntry(_sourceName, text, EventLogEntryType.Warning);

        }

        public void Error(string text)
        {
            EventLog.WriteEntry(_sourceName, text, EventLogEntryType.Error );
        }

        public void Error(string text, Exception ex)
        {
            Error(text);
            Error(ex.StackTrace);
        }
    }
}