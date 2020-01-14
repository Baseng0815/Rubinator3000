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

#if DEBUG_MOVES
        private static StreamWriter writer;
        private static volatile bool enableMoveLogging = false;
#endif

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
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Log:\t{logMessage}");
#endif
        }

#if DEBUG_MOVES
        #region DebugMoves
        public static void EnableMoveLogging() {
            writer = new StreamWriter("moveLog.txt", false);
            enableMoveLogging = true;
        }

        public static void DisableMoveLogging() {
            enableMoveLogging = false;
            writer.Close();
        }

        public static void MoveLogLogging(string message) {
            if (!enableMoveLogging)
                return;

            TimeSpan time = DateTime.Now.TimeOfDay;

            string logMessage = string.Format("{0}:\t{1}", time.ToString(@"hh\:mm\:ss\.ff"), message);
            writer.WriteLine(logMessage);
        }

        public static void LogMove(Move move, in Cube cubeState) {
            if (!enableMoveLogging)
                return;

            CubeColor[][] data = cubeState.GetData();

            string text = $"Move: {move.ToString()}\r\n" +
                $"\tCubeState:\r\n";

            for (int l = 0; l < 9; l++) {
                if (l < 3) {
                    text += "\t\t";
                    text += string.Join(" ", (int)data[1][3 * l], (int)data[1][3 * l + 1], (int)data[1][3 * l + 2]);
                    text += "\r\n";
                }
                else if (l < 6) {
                    int[] faces = { 0, 2, 4, 5 };
                    for (int f = 0; f < 4; f++) {
                        text += ("\t" + string.Join(" ", (int)data[faces[f]][3 * (l - 3)], (int)data[faces[f]][3 * (l - 3) + 1], (int)data[faces[f]][3 * (l - 3) + 2]));
                    }

                    text += "\r\n";
                }
                else {
                    text += "\t\t";
                    text += string.Join(" ", (int)data[3][3 * (l - 6)], (int)data[3][3 * (l - 6) + 1], (int)data[3][3 * (l - 6) + 2]);
                    text += "\r\n";
                }
            }

            writer.Write(text);
            writer.Flush();
        }
        #endregion
#endif

        private static void LoggingThread() {
            logging = true;
            if (messages.Count > 0 && !stopRequested) {
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
