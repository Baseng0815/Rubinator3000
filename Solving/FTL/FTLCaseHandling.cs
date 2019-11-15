using System;
using System.Linq;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {
    partial class FTLSolver {
        protected void MoveSlotUp(CornerStone slotCorner, Action<CubeFace, int> doMoveAction) {
            var cornerPos = slotCorner.GetPositions().First(p => p.Face != UP);
            CubeFace faceToRot = cornerPos.Face;
            int direction = cornerPos.Tile == 2 ? 1 : -1;

            doMoveAction(faceToRot, direction);
            doMoveAction(DOWN, direction);
            doMoveAction(faceToRot, -direction);
        }

        protected void MoveSlotUp(EdgeStone slotEdge, Action<CubeFace, int> doMoveAction) {
            var edgePos = slotEdge.Positions.Item1;
            CubeFace faceToRot = edgePos.Face;
            int direction = edgePos.Tile == 5 ? 1 : -1;

            doMoveAction(faceToRot, direction);
            doMoveAction(DOWN, direction);
            doMoveAction(faceToRot, -direction);
        }

        protected void RightPairedDownLayer(FTLPair pair, Action<CubeFace, int> doMoveAction) {
            // rotate pair in right position
            // 0-2 Moves
            (Position pos, CubeColor color) = pair.Corner.GetPositions().Select(p => (p, cube.At(p))).First(p => p.p.Face != UP && p.Item2 != WHITE);
            int delta = SolvingUtility.GetDelta(color, pos.Face, DOWN) - 1;

            doMoveAction(DOWN, delta);

            // insert the pair in right slot
            // 3 Moves
            var faceToRot = Cube.GetOpponentFace(pair.CornerWhitePosition.Face);
            int direction = pair.CornerWhitePosition.Tile == 6 ? 1 : -1;
            doMoveAction(faceToRot, direction);
            doMoveAction(DOWN, -direction);
            doMoveAction(faceToRot, -direction);

            // sum 3-5 Moves
        }

        protected void FalsePairedDownLayer(FTLPair pair, Action<CubeFace, int> doMoveAction) {
            // move corner above right slot
            // 0-2 moves
            Position pos = pair.Corner.GetPositions().First(p => p.Face == DOWN);
            int delta = SolvingUtility.GetDelta(cube.At(pos), pair.CornerWhitePosition.Face, DOWN);
            doMoveAction(DOWN, delta);

            // do algorithm     7-10 moves            
            var faceToRot = Cube.GetFace(cube.At(pair.Corner.GetPositions().First(p => p.Face == DOWN)));

            FTLPair secondPair = pairs.First(p => p.Edge.HasColor(Cube.GetFaceColor(faceToRot)) && p != pair);
            bool secondPairSolved = secondPair.Solved;

            int direction = pair.CornerWhitePosition.Tile == 6 ? 1 : -1;
            doMoveAction(faceToRot, direction);
            doMoveAction(DOWN, 2);
            doMoveAction(faceToRot, 2);
            doMoveAction(DOWN, -direction);
            doMoveAction(faceToRot, -direction);

            if (secondPairSolved) {
                doMoveAction(faceToRot, -direction);
                doMoveAction(DOWN, -direction);
                doMoveAction(faceToRot, direction);
            }
        }

        protected void CornerInSlotEdgeDown(FTLPair pair, Action<CubeFace, int> doMoveAction) {
            // move edge to right orientation
            var edgeUpColorFace = Cube.GetFace(pair.Edge.GetColor(p => p.Face == DOWN));
            CubeFace opponentFace = Cube.GetOpponentFace(edgeUpColorFace);

            while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != opponentFace)
                doMoveAction(DOWN, 1);

            // pair the stones
            var cornerPos = pair.Corner.GetPosition(p => p.Face == edgeUpColorFace);
            CubeFace faceToRot = cornerPos.Face;
            int direction = cornerPos.Tile == 2 ? 1 : -1;

            doMoveAction(faceToRot, direction);
            doMoveAction(DOWN, -direction);
            doMoveAction(faceToRot, -direction);

            // handle right paired down layer
            RightPairedDownLayer(pair, doMoveAction);
        }
    }
}
