using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {

    public interface IPattern {
        int Number { get; }
        bool IsMatch(Cube cube);
    }

    public struct OllPattern : IPattern {
        private bool[] face;
        private bool[][] sides;

        public int Number { get; }

        public OllPattern(int number, bool[] face, bool[][] sides) {
            if (face.Length != 9)
                throw new ArgumentOutOfRangeException(nameof(face));

            if (sides.Length != 4 || sides.Any(e => e.Length != 3))
                throw new ArgumentOutOfRangeException(nameof(sides));

            this.face = face;
            this.sides = sides;
            this.Number = number;
        }

        public bool IsMatch(Cube cube) {
            for (int t = 0; t < 6; t++) {
                if ((cube.At(DOWN, t) == YELLOW) != face[t])
                    return false;
            }

            for (int f = 0; f < 4; f++) {
                var face = CubeSolver.MiddleLayerFaces[f];
                for (int i = 0; i < 3; i++) {
                    if ((cube.At(face, i + 6) == YELLOW) != sides[f][i])
                        return false;
                }
            }

            return true;
        }
    }

    public struct PllPattern : IPattern {
        public int Number { get; }



        public bool IsMatch(Cube cube) {
            throw new NotImplementedException();
        }
    }
}