using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CubeLibrary;

namespace Rubinator3000 {
    public class ArduinoUdp : Arduino {
        private UdpClient client;
        private const int port = 3000;

        public ArduinoUdp() {
            client = new UdpClient(port);            
        }

        public override void SendMove(Move move) {
            throw new NotImplementedException();
        }

        public override void SendMoves(IEnumerable<Move> moves) {
            throw new NotImplementedException();
        }

        public override void Dispose() {
            throw new NotImplementedException();
        }
    }
}
