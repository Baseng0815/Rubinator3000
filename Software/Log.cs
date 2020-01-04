using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;

namespace Rubinator3000 {
    //[System.Diagnostics.DebuggerNonUserCode]
    public static class Log {

        public delegate void OnLoggingEventHandler(LoggingEventArgs e);
        public static event OnLoggingEventHandler OnLogging;

#if DEBUG_MOVES
        private static StreamWriter writer;
        private static volatile bool enableMoveLogging = false;
#endif

        public static void LogMessage(string message) {
            TimeSpan time = DateTime.Now.TimeOfDay;

            string logMessage = string.Format("{0}:\t{1}", time.ToString(@"hh\:mm\:ss\.ff"), message);

            OnLogging?.Invoke(new LoggingEventArgs(logMessage));
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Log:\t{logMessage}");
#endif
        }

#if DEBUG_MOVES        
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
#endif
    }

    public class LoggingEventArgs : EventArgs {
        public string Message { get; }

        public LoggingEventArgs(string message) {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
