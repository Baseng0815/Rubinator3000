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

            CubeFace faceToRot;
            int direction;
            Position cornerPos;
            // in slot white face up
            if (pair.CornerWhitePosition.Face == DOWN) {
                cornerPos = pair.Corner.GetPositions().First(p => p.Face != DOWN);
                faceToRot = cornerPos.Face;
                direction = cornerPos.Tile == 8 ? 1 : -1;

                // remove pair from slot
                DoMove(faceToRot, direction);
                DoMove(DOWN, -direction);
                DoMove(faceToRot, -direction);
            }
            // in false slot
            else if (pair.CornerWhitePosition.Face == UP) {
                cornerPos = pair.Corner.GetPositions().First(p => p.Face != UP);
                faceToRot = cornerPos.Face;
                direction = cornerPos.Tile == 2 ? 1 : -1;

                // remove pair from slot
                DoMove(faceToRot, direction);
                DoMove(DOWN, direction);
                DoMove(faceToRot, -direction);
            }


            // get the opponent face of the edge color on middle layer face
            faceToRot = cube.At(pair.Edge.GetPositions().First(p => p.Face != DOWN)).GetFace();
            CubeFace targetFace = Cube.GetOpponentFace(faceToRot);
            // bring the pair to right position
            while (pair.CornerWhitePosition.Face != targetFace)
                DoMove(DOWN);

            // open side 
            direction = pair.CornerWhitePosition.Tile == 6 ? 1 : -1;
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

            // bring pair in correct position
            CubeFace targetFace = Cube.GetOpponentFace(pair.Corner.GetPositions().First(p => p.Face == DOWN).Face);

            while (pair.CornerWhitePosition.Face != targetFace)
                DoMove(DOWN);

            CubeFace faceToRot = pair.CornerWhitePosition.Face;

            int direction = pair.CornerWhitePosition.Tile == 6 ? 1 : -1;
            DoMove(faceToRot, direction);

            DoMove(DOWN, 2);
            DoMove(faceToRot, 2);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, 2);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, -direction);
        }

        protected void CornerInSlot(FTLPair pair) {
            if (pair.CornerWhitePosition.Face != UP)
                throw new InvalidOperationException("Der Eckstein muss sich in einem Slot befinden!");

            CubeFace targetFace, faceToRot;
            int direction;
            // pair right paired in false slot  
            if (pair.Paired && pair.CornerWhitePosition.Face == UP) {
                Position cornerSidePos = pair.Corner.GetPositions().First(p => p.Face != UP);

                faceToRot = cornerSidePos.Face;
                direction = cornerSidePos.Tile == 2 ? 1 : -1;

                DoMove(faceToRot, direction);
                DoMove(DOWN, direction);
                DoMove(faceToRot, -direction);

                // handle as right paired
                RightPaired(pair);
            }
            // edge on yellow layer
            else if (pair.Edge.GetPositions().Any(p => p.Face == DOWN)) {
                CubeColor upColor = cube.At(pair.Edge.GetPositions().First(p => p.Face == DOWN));
                targetFace = Cube.GetOpponentFace(Cube.GetFace(upColor));
                CubeColor sideColor = cube.At(pair.Edge.GetPositions().First(p => p.Face != DOWN));

                // rotate the edge to opponent face of up color
                while (pair.Edge.GetColorPosition(sideColor).Face != targetFace)
                    DoMove(DOWN);

                // pair corner and edge
                faceToRot = Cube.GetFace(upColor);
                direction = pair.Edge.GetColorPosition(upColor).Tile == 2 ? 1 : -1;
                DoMove(faceToRot, direction);
                DoMove(DOWN, -direction);
                DoMove(faceToRot, -direction);

                // handle as paired stones
                RightPaired(pair);
            }                      
        }
        #endregion
    }
}
