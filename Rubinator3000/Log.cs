using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000 {
    public static class Log {
        public static void LogStuff(string message) {
            TimeSpan time = DateTime.Now.TimeOfDay;

            string logMessage = string.Format("{0}:\t{1}", time.ToString("HH:mm:ss.ff"), message);
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Log:\t{logMessage}");
#endif
            mainWindow.LogStuff(logMessage);
        }
    }
}
