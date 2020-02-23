using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.CubeScan {
    public class RelativePosition : ICloneable {

        public double RelativeX { get; set; }

        public double RelativeY { get; set; }

        public RelativePosition(double relativeX, double relativeY) {
            RelativeX = relativeX;
            RelativeY = relativeY;
        }

        public override string ToString() {

            return string.Format("[{0}, {1}]", RelativeX, RelativeY);
        }

        public object Clone() {

            return new RelativePosition(RelativeX, RelativeY);
        }
    }
}
