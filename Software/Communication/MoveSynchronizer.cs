using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Rubinator3000.Communication {
    // synchronizes sending moves to the arduino and redrawing the cube
    public class MoveSynchronizer {
        private MoveCollection moves;
        private Arduino arduino;
        private TextBox moveHistory;

        public MoveSynchronizer(MoveCollection moves, Arduino arduino, TextBox moveHistory) {
            this.moves = moves;
            this.arduino = arduino;
            this.moveHistory = moveHistory;
        }

        public void Run() {
            Application.Current.Dispatcher.Invoke(moveHistory.Clear);
            foreach (Move move in moves) {
                //MessageBox.Show("Next move: " + move.ToString());
                //arduino.SendMove(move);
                DrawCube.AddMove(move: move);
                Application.Current.Dispatcher.Invoke( delegate { moveHistory.AppendText(move.ToString()); });
            }
        }
    }
}
