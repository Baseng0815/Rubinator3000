using System;
using System.Linq;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {
    partial class FTLMoveCalculator {
        private static void MoveSlotUp(CornerStone slotCorner) {
            var cornerPos = slotCorner.GetPositions().First(p => p.Face != UP);
            CubeFace faceToRot = cornerPos.Face;
            int direction = cornerPos.Tile == 2 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, direction);
            DoMove(faceToRot, -direction);
        }

        private static void MoveSlotUp(EdgeStone slotEdge) {
            var edgePos = slotEdge.Positions.Item1;
            CubeFace faceToRot = edgePos.Face;
            int direction = edgePos.Tile == 5 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, direction);
            DoMove(faceToRot, -direction);
        }

        private static void MoveSlotUp(EdgeStone slotEdge, CubeColor downFacingColor) {
            Position edgePos = slotEdge.GetColorPosition(c => c != downFacingColor);
            CubeFace faceToRot = edgePos.Face;
            int direction = edgePos.Tile == 5 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, direction);
            DoMove(faceToRot, -direction);
        }

        // works fine
        private static void RightPairedDownLayer(FTLPair pair) {    
            // rotate pair to right position
            // 0-2 Moves            
            CubeColor sideColor = pair.Corner.GetColor(p => p.Face != DOWN && p != pair.CornerWhitePosition);
            CubeFace targetFace = Cube.GetOpponentFace(Cube.GetFace(sideColor));

            while (pair.CornerWhitePosition.Face != targetFace)
                DoMove(DOWN);

            // insert the pair in right slot
            // 3 Moves
            var faceToRot = Cube.GetOpponentFace(pair.CornerWhitePosition.Face);
            int direction = pair.CornerWhitePosition.Tile == 6 ? 1 : -1;
            DoMove(faceToRot, direction);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, -direction);

            // sum 3-5 Moves
        }

        // works fine
        private static void FalsePairedDownLayer(FTLPair pair) {
            // move corner above right slot
            // 0-2 moves
            Position pos = pair.Corner.GetPositions().First(p => p.Face == DOWN);
            int delta = SolvingUtility.GetDelta(cube.At(pos), pair.CornerWhitePosition.Face, DOWN);
            DoMove(DOWN, -delta);

            // do algorithm     7-10 moves            
            var faceToRot = pair.CornerWhitePosition.Face;

            FTLPair secondPair = pairs.First(p => p.Edge.HasColor(Cube.GetFaceColor(faceToRot)) && p != pair);
            bool secondPairSolved = secondPair.Solved;

            int direction = pair.CornerWhitePosition.Tile == 6 ? 1 : -1;
            DoMove(faceToRot, direction);
            DoMove(DOWN, 2);
            DoMove(faceToRot, 2);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, direction);

            if (secondPairSolved) {
                DoMove(faceToRot, direction);
                DoMove(DOWN, -direction);
                DoMove(faceToRot, -direction);
            }
        }

        // works fine
        private static void CornerInSlot_WhiteUp_EdgeDown(FTLPair pair) {
            // move edge to right orientation
            var edgeUpColor = pair.Edge.GetColor(p => p.Face == DOWN);
            CubeFace opponentFace = Cube.GetOpponentFace(pair.Corner.GetColorPosition(c => c == edgeUpColor).Face);

            while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != opponentFace)
                DoMove(DOWN, 1);

            // pair the stones
            var cornerPos = pair.Corner.GetColorPosition(c => c == edgeUpColor);
            CubeFace faceToRot = cornerPos.Face;
            int direction = cornerPos.Tile == 2 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, -direction);

            // handle right paired down layer
            RightPairedDownLayer(pair);
        }

        // a. k. a. crocodile
        // works fine
        private static void CornerInSlot_WhiteSide_EdgeRightDown(FTLPair pair) {
            Position cornerSidePos = pair.Corner.GetPosition(p => p.Face != UP && p != pair.CornerWhitePosition);
            while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != cornerSidePos.Face)
                DoMove(DOWN);

            CubeFace faceToRot = pair.CornerWhitePosition.Face;
            int direction = pair.CornerWhitePosition.Tile == 2 ? 1 : -1;
            // open slot and pair
            DoMove(faceToRot, direction);
            // move pair away
            DoMove(DOWN, direction);
            // close the slot
            DoMove(faceToRot, -direction);

            RightPairedDownLayer(pair);
        }

        // works fine
        private static void CornerInSlot_WhiteSide_EdgeFalseDown(FTLPair pair) {
            // move edge to right orientation
            while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != pair.CornerWhitePosition.Face)
                DoMove(DOWN);

            CubeFace faceToRot = pair.CornerWhitePosition.Face;
            int direction = pair.CornerWhitePosition.Tile == 2 ? 1 : -1;

            // open slot
            DoMove(faceToRot, direction);
            // transform to "tiger" position
            DoMove(DOWN, direction);
            // close slot
            DoMove(faceToRot, -direction);

            TigerPosition(pair);
        }

        // works fine
        private static void TigerPosition(FTLPair pair) {
            CubeFace targetFace = Cube.GetFace(pair.Corner.GetColor(p => p.Face == DOWN));
            while (pair.CornerWhitePosition.Face != targetFace)
                DoMove(DOWN);

            CubeFace faceToRot = pair.CornerWhitePosition.Face;
            int direction = pair.CornerWhitePosition.Tile == 8 ? 1 : -1;

            // open slot and pair
            DoMove(faceToRot, direction);
            // move pair to slot
            DoMove(DOWN, direction);
            // close slot
            DoMove(faceToRot, -direction);
        }

        // works fine
        private static void EaglePosition(FTLPair pair) {
            CubeColor color = pair.Corner.GetColors().First(c => c != WHITE);
            CubeFace edgeFace = pair.Edge.GetColorPosition(color).Face;

            // pair the stones
            while (pair.Corner.GetColorPosition(color).Face != edgeFace)
                DoMove(DOWN);

            CubeFace faceToRot = edgeFace;
            int direction = pair.Corner.GetColorPosition(color).Tile == 8 ? 1 : -1;

            // move paired stones away from slot
            DoMove(faceToRot, direction);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, -direction);

            // handle as right paired
            RightPairedDownLayer(pair);
        }

        // works fine
        private static void CornerDown_YellowSide_EdgeFalse(FTLPair pair) {
            CubeColor color = pair.Corner.GetColors().First(c => c != WHITE);
            CubeFace edgeFace = pair.Edge.GetColorPosition(color).Face;

            while (pair.Corner.GetColorPosition(color).Face != edgeFace)
                DoMove(DOWN);

            Position edgePos = pair.Edge.GetColorPosition(c => c != color);
            CubeFace faceToRot = edgePos.Face;
            int direction = edgePos.Tile == 5 ? 1 : -1;

            // transform to corner false in slot and edge right
            DoMove(faceToRot, direction);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, -direction);

            CornerInSlot_WhiteSide_EdgeRightDown(pair);
        }

        private static void CornerDown_YellowSide_EdgeDown(FTLPair pair) {
            CubeColor color = pair.Edge.GetColor(p => p.Face != DOWN);
            CubeFace targetFace = Cube.GetFace(color);

            while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != targetFace)
                DoMove(DOWN);

            CubeFace faceToRot = pair.Edge.GetPosition(p => p.Face != DOWN).Face;
            int direction = SolvingUtility.GetDelta(pair.Edge.GetColor(p => p.Face != DOWN),
                                                        faceToRot, DOWN);
            // move edge in right position            
            DoMove(faceToRot, direction);
            // pair the stones
            while (pair.Corner.GetColorPosition(color).Face != targetFace)
                DoMove(DOWN);

            DoMove(faceToRot, -direction);

            // handle as right paired
            RightPairedDownLayer(pair);
        }

        private static void CornerDown_Side_EdgeSlot(FTLPair pair) {
            // transform to tiger
            CubeColor sideColor = pair.Corner.GetColor(p => p.Face != DOWN && p != pair.CornerWhitePosition);
            CubeFace targetFace = pair.Edge.GetColorPosition(sideColor).Face;

            while (pair.CornerWhitePosition.Face != targetFace)
                DoMove(DOWN);

            CubeFace faceToRot = pair.Edge.GetColorPosition(c => c != sideColor).Face;
            int direction = pair.Edge.GetColorPosition(c => c != sideColor).Tile == 5 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, -direction);

            TigerPosition(pair);
        }

        private static void PairedDown_CornerFalse(FTLPair pair) {
            // transform to crocodile
            CubeColor sideColor = pair.Corner.GetColor(p => p.Face != DOWN && p != pair.CornerWhitePosition);
            CubeFace targetFace = Cube.GetFace(sideColor);

            while (pair.Corner.GetColorPosition(sideColor).Face != targetFace)
                DoMove(DOWN);

            CubeFace faceToRot = targetFace;
            int direction = pair.Corner.GetColorPosition(sideColor).Tile == 8 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, direction);
            DoMove(faceToRot, -direction);

            CornerInSlot_WhiteSide_EdgeRightDown(pair);
        }
    }
}
