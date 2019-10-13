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

        public bool IsPaired(EdgeStone edge, CornerStone corner, out bool edgeRight) {
            if (!edge.GetColors().All(c => corner.GetColors().Contains(c)))
                throw new ArgumentException("Die Farben des Kantensteins sind nicht auf dem Eckstein vorhanden");

            // get common edge and corner positions on same face            
            IEnumerable<(Position corner, Position edge)> commonFaces = from ePos in edge.GetPositions()
                                                                        join cPos in corner.GetPositions() on ePos.Face equals cPos.Face
                                                                        select (cPos, ePos);
            edgeRight = false;
            if (commonFaces.Count() == 0)
                return false;

            // check if edge and corner position are side by side 
            if (commonFaces.All(t => {
                int d = Math.Abs(t.edge.Tile - t.corner.Tile);
                return d == 1 || d == 3;
            })) {
                // return if colors are equal on each face
                edgeRight = commonFaces.All(t => cube.At(t.edge) == cube.At(t.corner));
                return true;
            }

            return false;
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

        protected static readonly IEnumerable<CubeColor>[] MiddleLayerEdgesColors = new IEnumerable<CubeColor>[4] {
            new List<CubeColor>() { ORANGE, GREEN },
            new List<CubeColor>() { GREEN, RED },
            new List<CubeColor>() { RED, BLUE },
            new List<CubeColor>() { BLUE, ORANGE }
        };
    }
}
