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
        protected void RightPaired(EdgeStone edge, CornerStone corner) {
            if (!IsPaired(edge, corner, out bool edgeRight) || !edgeRight)
                throw new InvalidOperationException("Der Kantenstein und der Eckstein müssen richtig verbunden sein");

            // bring pair in right position                
            CubeColor sideColor = cube.At(edge.GetPositions().First(p => MiddleLayerFaces.Any(f => p.Face == f)));
            Position whitePos;
            
            while ((whitePos = corner.GetColorPosition(WHITE)).Face != Cube.GetOpponentFace(sideColor.GetFace())) {
                DoMove(DOWN);
            }

            // insert pair into the slot
            // open the slot
            CubeFace faceToRot = sideColor.GetFace();
            int direction = whitePos.Tile == 6 ? 1 : -1;
            DoMove(faceToRot, direction);

            // insert pair
            DoMove(DOWN, -direction);

            // close the slot
            DoMove(faceToRot, -direction);
        }
        
        protected void Paired_EdgeFalse(EdgeStone edge, CornerStone corner) {
            CubeColor edgeUpColor = cube.At(edge.GetPositions().First(p => p.Face == DOWN));
        }
        #endregion        
    }
}
