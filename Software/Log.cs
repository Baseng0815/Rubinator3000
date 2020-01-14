using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using System.Threading;

namespace Rubinator3000 {
    //[System.Diagnostics.DebuggerNonUserCode]
    public static class Log {
        private static bool stopRequested = false;

        private static bool logging;
        private static Thread loggingThread;

        private static Queue<string> messages = new Queue<string>();

        public static void StopLogging() {
            stopRequested = true;
            if (logging && loggingThread != null)
                loggingThread.Join();
        }

        public static void LogMessage(string message) {
            TimeSpan time = DateTime.Now.TimeOfDay;

            string logMessage = string.Format("{0}:\t{1}", time.ToString(@"hh\:mm\:ss\.ff"), message);

            messages.Enqueue(logMessage);

            //invoke logging thread
            if (!logging && !stopRequested) {
                loggingThread = new Thread(LoggingThread);
                loggingThread.Start();
            }
        }

        private static void LoggingThread() {
            logging = true;
            while (messages.Count > 0 && !stopRequested) {
                string message = messages.Dequeue();
                
                    Application.Current.Dispatcher.Invoke(() => {
                        MainWindow window = (MainWindow)Application.Current.MainWindow;
                        if (window.TextBox_Log != null)
                            window.TextBox_Log.Text += $"{message}\r\n";

                        // Auto Scroll Implementation
                        if (window.WindowsFormsHost_CubePreview.Child != null) {
                            window.TextBox_Log.Focus();
                            window.TextBox_Log.CaretIndex = window.TextBox_Log.Text.Length;
                            window.TextBox_Log.ScrollToEnd();
                        }
                    });
            }

            logging = false;
        }
    }
}
