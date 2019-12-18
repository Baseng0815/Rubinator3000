using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rubinator3000.Solving;

namespace Rubinator3000 {
    public abstract class Arduino : IDisposable {
        public abstract void SendMove(Move move);
        public abstract void SendMoves(IEnumerable<Move> moves);

        public abstract void Dispose();

        public abstract void Connect();
        public abstract void Disconnect();

        protected byte[] MoveToByte(Move move) {
            //bool isPrime = move.IsPrime;
            int face = (int)move.Face;
                        
            byte moveData = (byte)((face + 1) << 1);

            //if(isPrime){
            //    moveData |= 0x01;
            //}

            return Enumerable.Repeat(moveData, Math.Abs(move.Count)).ToArray();
        }
    }
    
}
