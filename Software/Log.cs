using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Rubinator3000 {
    [System.Diagnostics.DebuggerNonUserCode]
    public static class Log {

        public delegate void OnLoggingEventHandler(LoggingEventArgs e);
        public static event OnLoggingEventHandler OnLogging;

        public static void LogStuff(string message) {
            TimeSpan time = DateTime.Now.TimeOfDay;

            string logMessage = string.Format("{0}:\t{1}", time.ToString(@"hh\:mm\:ss\.ff"), message);

            OnLogging?.Invoke(new LoggingEventArgs(logMessage));
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Log:\t{logMessage}");
#endif            
        }
    }

    public class LoggingEventArgs : EventArgs {
        public string Message { get; }

        public LoggingEventArgs(string message) {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
