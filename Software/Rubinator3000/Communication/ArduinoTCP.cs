using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CubeLibrary;

namespace Rubinator3000 {
    public class ArduinoTCP : Arduino {
        
        
        public override void Dispose() {
            throw new NotImplementedException();
        }

        public override void SendMove(Move move) {
            throw new NotImplementedException();
        }

        public override void SendMoves(IEnumerable<Move> moves) {
            throw new NotImplementedException();
        }
    }
}
