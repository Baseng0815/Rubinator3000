using RubinatorCore.CubeRepresentation;
using System;
using System.Threading.Tasks;

namespace Rubinator3000.Communication {
    public enum ArduinoLEDs : byte { DOWN = 0x01, UP = 0x02, STRIPES = 0x04, ALL = UP | DOWN | STRIPES }
    public abstract class Arduino : IDisposable {
        public abstract bool Connected { get; }

        public Task SendMoveAsync(Move move) => Task.Factory.StartNew(() => SendMove(move));
        public abstract void SendMove(Move move);

        public Task SendMultiTurnMoveAsync(Move move1, Move move2) => Task.Factory.StartNew(() => SendMultiTurnMove(move1, move2));
        public abstract void SendMultiTurnMove(Move move1, Move move2);

        public abstract void Dispose();

        public abstract void Connect();
        public abstract void Disconnect();

        public abstract void SetSolvedState(bool state);

        protected abstract byte[] SendCommand(params byte[] command);
        public abstract void SendLedCommand(ArduinoLEDs leds, byte brightness);
    }
}
