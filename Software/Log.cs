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
        public static void LogStuff(string message) {
            TimeSpan time = DateTime.Now.TimeOfDay;

            string logMessage = string.Format("{0}:\t{1}", time.ToString(@"hh\:mm\:ss\.ff"), message);
            Application.Current.Dispatcher.Invoke(() => {
                MainWindow window = (MainWindow)Application.Current.MainWindow;
                window.LogStuff(logMessage);
            });

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Log:\t{logMessage}");
#endif            
        }
    }
}
