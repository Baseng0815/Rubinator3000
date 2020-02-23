using System;
using System.Collections.Generic;
using System.Threading;

namespace RubinatorCore {
    //[System.Diagnostics.DebuggerNonUserCode]
    public static class Log {
        private static bool stopRequested = false;

        private static bool logging;
        private static Thread loggingThread;

        private static Queue<string> messages = new Queue<string>();

        public static Action<string> LogCallback { get; set; }

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
                string message = null;
                try {
                    message = messages.Dequeue();
                }
                catch (InvalidOperationException) {
                    break;
                }

                LogCallback?.Invoke(message);
                System.Diagnostics.Debug.WriteLine(message);
            }

            logging = false;
        }
    }
}
