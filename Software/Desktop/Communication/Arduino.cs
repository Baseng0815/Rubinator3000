using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RubinatorCore;
using RubinatorCore.Solving;

namespace Rubinator3000 {
    public abstract class Arduino : IDisposable {
        public abstract void SendMove(Move move);
        public abstract void SendMultiTurnMove(Move move1, Move move2);

        public abstract void Dispose();

        public abstract void Connect();
        public abstract void Disconnect();
    }
}
