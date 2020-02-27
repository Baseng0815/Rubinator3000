using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.CubeScan {
    class Tunnel {

        public int Input { get; set; }

        public int Output { get; set; }

        public Tunnel(int input, int output) {
            Input = input;
            Output = output;
        }
    }
}
