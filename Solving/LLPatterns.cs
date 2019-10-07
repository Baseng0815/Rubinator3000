using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {

    public interface IPattern {
        bool IsMatch(Cube cube);
    }

    public class OllPattern : IPattern {
        private bool[] face;
        private bool[][] sides;

        public bool IsMatch(Cube cube) {
            for (int t = 0; t < 6; t++) {
                if ((cube.At(DOWN, t) == YELLOW) != face[t])
                    return false;
            }

            for (int f = 0; f < 4; f++) {
                var face = CubeSolver.MiddleLayerFaces[f];

            }
        }
    }
}
