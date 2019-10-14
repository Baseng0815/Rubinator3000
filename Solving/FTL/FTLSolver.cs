using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {
    public class FTLSolver : CubeSolver {
        public override bool Solved {
            get {
                for (int f = 0; f < 4; f++) {
                    CubeColor faceColor = MiddleLayerFaces[f].GetFaceColor();
                    for (int t = 0; t < 6; t++) {
                        if (!(cube.At(f, t) == faceColor)) {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        public FTLSolver(Cube cube) : base(cube) {

        }

        protected override void CalcMoves() {
            throw new NotImplementedException();
        }

        protected override bool CheckCube(Cube cube) {
            for (int t = 0; t < 9; t++) {
                if (!(cube.At(UP, t) == WHITE)) {
                    return false;
                }
            }

            return true;
        }

        #region FTL Case Handling
        protected void RightPaired(FTLPair pair) {
            if (!pair.Paired)
                throw new ArgumentOutOfRangeException(nameof(pair), pair, "Die Steine sind nicht korrekt gepaart");

            // get the opponent face of the edge color on middle layer face
            CubeFace faceToRot = cube.At(pair.Edge.GetPositions().First(p => p.Face != DOWN)).GetFace();
            CubeFace targetFace = Cube.GetOpponentFace(faceToRot);
            // bring the pair to right position
            while (pair.CornerWhitePosition.Face != targetFace)
                DoMove(DOWN);

            // open side 
            int direction = pair.CornerWhitePosition.Tile == 6 ? 1 : -1;
            DoMove(faceToRot, direction);

            // insert pair
            DoMove(DOWN, -direction);

            // close side
            DoMove(faceToRot, -direction);
        }

        protected void FalsePaired(FTLPair pair) {
            if (!(pair.IsPaired(out bool edgeRight) && edgeRight))
                throw new ArgumentOutOfRangeException(nameof(pair), pair, "Die Steine haben nicht die richtige Verbindung für diesen Fall");

            if (!pair.OnDownLayer)
                throw new ArgumentOutOfRangeException(nameof(pair), pair, "Die Steine müssen sich auf der gelben Ebene befinden");



            CubeFace faceToRot = pair.CornerWhitePosition.Face;
        }
        #endregion        
    }
}
