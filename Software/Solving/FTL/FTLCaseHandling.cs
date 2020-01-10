using System;
using System.Linq;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {
    partial class FTLMoveCalculator {
        private void MoveSlotUp(CornerStone slotCorner) {
            var cornerPos = slotCorner.GetPositions().First(p => p.Face != UP);
            CubeFace faceToRot = cornerPos.Face;
            int direction = cornerPos.Tile == 2 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, direction);
            DoMove(faceToRot, -direction);
        }

        private void MoveSlotUp(EdgeStone slotEdge) {
            var edgePos = slotEdge.Positions.Item1;
            CubeFace faceToRot = edgePos.Face;
            int direction = edgePos.Tile == 5 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, direction);
            DoMove(faceToRot, -direction);
        }

        private void MoveSlotUp(EdgeStone slotEdge, CubeColor downFacingColor) {
            Position edgePos = slotEdge.GetColorPosition(c => c != downFacingColor);
            CubeFace faceToRot = edgePos.Face;
            int direction = edgePos.Tile == 5 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, direction);
            DoMove(faceToRot, -direction);
        }
        
        private void RightPairedDownLayer() {
            // rotate pair to right position
            // 0-2 Moves            
            Log.LogMessage($"Right Paired Down Layer {pair}");

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
        
        private void FalsePairedDownLayer() {
            // move corner above right slot
            // 0-2 moves
            Log.LogMessage($"False Paired Down Layer {pair}");

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
        
        //TODO: Check CornerInSlot_WhiteUp_EdgeDown
        private void CornerInSlot_WhiteUp_EdgeDown() {
            // move edge to right orientation
            Log.LogMessage($"Corner In Slot White Up Edge Down {pair}");

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
            RightPairedDownLayer();
        }

        // a. k. a. crocodile        
        private void CornerInSlot_WhiteSide_EdgeRightDown() {
            Log.LogMessage($"Corner in Slot White Side Edge Right Down {pair}");

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

            RightPairedDownLayer();
        }
        
        private void CornerInSlot_WhiteSide_EdgeFalseDown() {
            // move edge to right orientation
            Log.LogMessage($"Corner in Slot White Side Edge False Down {pair}");

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

            TigerPosition();
        }
        
        private void TigerPosition() {
            Log.LogMessage($"Tiger {pair}");

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
        
        private void EaglePosition() {
            Log.LogMessage($"Eagle {pair}");

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
            RightPairedDownLayer();
        }
        
        private void CornerDown_YellowSide_EdgeFalse() {
            Log.LogMessage($"Corner Down Yellow Side Edge False {pair}");

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

            CornerInSlot_WhiteSide_EdgeRightDown();
        }
        
        private void CornerDown_YellowSide_EdgeDown() {
            Log.LogMessage($"Corner Down Yellow Side Edge Down {pair}");

            CubeColor color = pair.Edge.GetColor(p => p.Face != DOWN);
            CubeFace targetFace = Cube.GetFace(color);

            while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != targetFace)
                DoMove(DOWN);

            CubeFace faceToRot = pair.Edge.GetPosition(p => p.Face != DOWN).Face;
            int colorSide = Array.IndexOf(CubeSolver.MiddleLayerFaces, (CubeFace)pair.Edge.GetColor(p => p.Face != DOWN));
            int colorDown = Array.IndexOf(CubeSolver.MiddleLayerFaces, (CubeFace)pair.Edge.GetColor(p => p.Face == DOWN));
            int direction = 2 - (colorDown - colorSide + 4) % 4;

            // move edge in right position            
            DoMove(faceToRot, direction);

            // pair the stones
            while (pair.Corner.GetColorPosition(color).Face != targetFace)
                DoMove(DOWN);

            DoMove(faceToRot, -direction);

            // handle as right paired
            RightPairedDownLayer();
        }

        
        private void CornerDown_Side_EdgeSlot() {
            Log.LogMessage($"Corner Down Side Edge Slot {pair}");
            // transform to tiger
            CubeColor sideColor = pair.Corner.GetColor(p => p.Face != DOWN && p != pair.CornerWhitePosition);
            CubeFace targetFace = Cube.GetOpponentFace(pair.Edge.GetColorPosition(sideColor).Face);

            while (pair.CornerWhitePosition.Face != targetFace)
                DoMove(DOWN);

            CubeFace faceToRot = pair.Edge.GetColorPosition(c => c != sideColor).Face;
            int direction = pair.Edge.GetColorPosition(c => c != sideColor).Tile == 5 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, -direction);

            TigerPosition();
        }

        
        private void PairedDown_CornerFalse() {
            Log.LogMessage($"Paired Down Corner False {pair}");
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

            CornerInSlot_WhiteSide_EdgeRightDown();
        }

        
        private void CornerDown_YellowSide_Paired() {
            Log.LogMessage($"Corner Down Yellow Side Paired");

            Position edgeSidePos = pair.Edge.GetPosition(p => p.Face != DOWN);
            CubeColor edgeSideColor = pair.Edge.GetColor(edgeSidePos);
            CubeColor cornerSideColor = pair.Corner.GetColor(p => p.Face == edgeSidePos.Face);

            // move corner above slot
            CubeFace targetFace = Cube.GetFace(pair.Edge.GetColor(p => p.Face != DOWN));

            while (pair.Corner.GetColorPosition(cornerSideColor).Face != targetFace)
                DoMove(DOWN);

            // side colors equal
            if (cornerSideColor == edgeSideColor) {
                // do algorithm "F2L 19"/"F2L 21"
                CubeFace face1 = pair.Corner.GetColorPosition(c => !(c == WHITE || c == edgeSideColor)).Face;
                CubeFace face2 = pair.Corner.GetColorPosition(edgeSideColor).Face;
                int direction = pair.Corner.GetColorPosition(edgeSideColor).Tile == 8 ? 1 : -1;

                DoMove(face1, direction);
                DoMove(DOWN, direction);
                DoMove(face2, direction);
                DoMove(DOWN, -direction);
                DoMove(face2, -direction);
                DoMove(face1, -direction);
                DoMove(face2, direction);
                DoMove(DOWN, -direction);
                DoMove(face2, -direction);
            }
            else {
                // do algorithm "F2L 18"/"F2L 20"
                CubeFace faceToRot = pair.Edge.GetColorPosition(edgeSideColor).Face;
                int direction = pair.Corner.GetColorPosition(c => !(c == WHITE || c == edgeSideColor)).Tile == 8 ? 1 : -1;

                DoMove(faceToRot, direction);
                DoMove(DOWN, 2);
                DoMove(faceToRot, -direction);
                DoMove(DOWN, -direction);
                DoMove(faceToRot, direction);
                DoMove(DOWN, direction);
                DoMove(faceToRot, -direction);
            }
        }

        private void CornerDown_Side_EdgeDown() {
            Log.LogMessage($"Corner Down Side Edge Down {pair}");

            CubeColor edgeUpColor = pair.Edge.GetColor(p => p.Face == DOWN);
            CubeColor cornerUpColor = pair.Corner.GetColor(p => p.Face == DOWN);

            if (edgeUpColor == cornerUpColor) {
                // transform to crocodile
                CubeFace targetFace = Cube.GetFace(pair.Corner.GetColors().First(c => !(c == WHITE || c == cornerUpColor)));

                while (pair.CornerWhitePosition.Face != targetFace)
                    DoMove(DOWN);

                CubeFace faceToRot = Cube.GetFace(cornerUpColor);
                int direction = pair.CornerWhitePosition.Tile == 8 ? 1 : -1;

                DoMove(faceToRot, direction);
                DoMove(DOWN, -direction);
                DoMove(faceToRot, -direction);

                // handle crocodile
                CornerInSlot_WhiteSide_EdgeRightDown();
            }
            else {
                // check if stones are in right position
                bool isTiger = pair.Corner.GetColorPosition(c => !(c == WHITE || c == cornerUpColor)).Face
                    == Cube.GetOpponentFace(pair.Edge.GetPosition(p => p.Face != DOWN).Face);

                if (!isTiger) {
                    // move corner to slot
                    CubeFace targetFace = Cube.GetFace(pair.Corner.GetColor(p => p.Face != DOWN && p != pair.CornerWhitePosition));

                    while (pair.CornerWhitePosition.Face != targetFace)
                        DoMove(DOWN);

                    CubeFace faceToRot = Cube.GetFace(cornerUpColor);
                    int direction = pair.CornerWhitePosition.Tile == 8 ? 1 : -1;

                    DoMove(faceToRot, direction);
                    DoMove(DOWN, -direction);
                    DoMove(faceToRot, -direction);

                    CornerInSlot_WhiteSide_EdgeFalseDown();
                }
                else {
                    TigerPosition();
                }
            }
        }
    }
}
