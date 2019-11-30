using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace CubeLibrary {
    public static class Log {
        private static Queue<string> messages = new Queue<string>();
        private static bool logging = false;
        private static Action<string> logCallback;
        private static Task logTask;

        public static void Init(Action<string> logCallback) {
            Log.logCallback = logCallback;

            logging = true;
        }

        private static void Run() {
            while (messages.Count > 0 && logCallback != null) {
                string message = messages.Dequeue();

                logCallback(message);
            }           
        }

        internal static void LogStuff(string message) {            

            TimeSpan time = DateTime.Now.TimeOfDay;

            string logMessage = string.Format("{0}:\t{1}", time.ToString(@"hh\:mm\:ss\.ff"), message);

            messages.Enqueue(logMessage);

            if ((logTask == null || logTask.Status != TaskStatus.Running) && logging) {                
                logTask = Task.Factory.StartNew(() => Run());
            }

            System.Diagnostics.Debug.WriteLine(logMessage);
        }

        public static void StopLogging() {
            logging = false;
            logTask.Wait();
        }        
    }
}
