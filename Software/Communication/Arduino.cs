using Rubinator3000;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000 {
    public abstract class Arduino : IDisposable {

        public delegate void MoveEventHandler(object sender, MoveEventArgs e);
        public event MoveEventHandler OnMoveDone;

        public abstract void SendMove(Move move);
        public abstract void SendMoves(IEnumerable<Move> moves);

        public abstract void Dispose();

        protected void InvokeMoveDone(object sender, MoveEventArgs e) {
            OnMoveDone?.Invoke(sender, e);
        }
    }
    
}
