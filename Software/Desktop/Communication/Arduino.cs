using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RubinatorCore;
using RubinatorCore.Solving;

namespace Rubinator3000 {
    public enum ArduinoLEDs { UP = 0x01, DOWN = 0x02, STRIPES = 0x04, ALL = UP | DOWN | STRIPES }
    public abstract class Arduino : IDisposable {

        public Task SendMoveAsync(Move move) => Task.Factory.StartNew(() => SendMove(move));
        public abstract void SendMove(Move move);

        public Task SendMultiTurnMoveAsync(Move move1, Move move2) => Task.Factory.StartNew(() => SendMultiTurnMove(move1, move2));
        public abstract void SendMultiTurnMove(Move move1, Move move2);

        public abstract void Dispose();

        public abstract void Connect();
        public abstract void Disconnect();

        public abstract void SendLedCommand(ArduinoLEDs leds, int state);
    }
}
