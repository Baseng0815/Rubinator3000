using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.Solving {
    public partial class CubeSolverFridrich : CubeSolver {

        public CubeSolverFridrich(Cube cube) : base(cube) {

        }

        public override void CalcMoves() {
            try {
                CalcCrossMoves();

                CalcFTL();

                CalcOLL();

                CalcPLL();

                bool solved = GetCubeSolved();

                if (!solved) {
                    moves.Clear();
                }
            }
            catch (Exception) {
                throw;
            }
        }

        partial void CalcCrossMoves();
        partial void CalcFTL();
        partial void CalcOLL();
        partial void CalcPLL();

        private static readonly Position[] edgePositions;
        private static readonly CubeFace[] middleLayerFaces = { CubeFace.LEFT, CubeFace.FRONT, CubeFace.RIGHT, CubeFace.BACK };

        static CubeSolverFridrich() {
            edgePositions = new Position[24];
            for (int face = 0; face < 6; face++) {
                for (int tile = 0; tile < 4; tile++) {
                    edgePositions[4 * face + tile] = new Position((CubeFace)face, tile * 2 + 1);
                }
            }
        }
    }
}
