using System;
using System.Linq;
using static RubinatorCore.CubeFace;
using static RubinatorCore.CubeColor;

namespace RubinatorCore.Solving {
    partial class FTLMoveCalculator {
        /// <summary>
        /// Löst einen Eckstein aus dem Slot und bringt ihn auf die gelbe Ebene
        /// </summary>
        /// <param name="slotCorner">Der weiße Eckstein, der auf die gelbe Ebene gebracht werden soll</param>
        private void MoveSlotUp(CornerStone slotCorner) {
            // den Zug bestimmen, der den Slot öffnet
            var cornerPos = slotCorner.GetPositions().First(p => p.Face != UP);
            CubeFace faceToRot = cornerPos.Face;
            int direction = cornerPos.Tile == 2 ? 1 : -1;

            // Slot öffnen
            DoMove(faceToRot, direction);
            // Eckstein aus Slot bewegen
            DoMove(DOWN, direction);
            // Slot schließen
            DoMove(faceToRot, -direction);
        }

        /// <summary>
        /// Löst einen Kantenstein aus dem Slot und bringt ihn auf die gelbe Ebene
        /// </summary>
        /// <param name="slotEdge">Der weiße Kantenstein, der auf die gelbe Ebene gebracht werden soll</param>
        private void MoveSlotUp(EdgeStone slotEdge) {
            // den Zug bestimmen, der den Slot öffnet
            var edgePos = slotEdge.Positions.Item1;
            CubeFace faceToRot = edgePos.Face;
            int direction = edgePos.Tile == 5 ? 1 : -1;

            // Slot öffnen
            DoMove(faceToRot, direction);
            // Kantenstein aus Slot bewegen
            DoMove(DOWN, direction);
            // Slot schließen
            DoMove(faceToRot, -direction);
        }

        /// <summary>
        /// Bewegt ein richtig gepaartes FTL-Paar auf der gelben Ebene in den richtigen Slot
        /// </summary>
        private void RightPairedDownLayer() {
            // Paar in richtige Position bringen
            CubeColor sideColor = pair.Corner.GetColor(p => p.Face != DOWN && p != pair.CornerWhitePosition);
            CubeFace targetFace = Cube.GetOpponentFace(Cube.GetFace(sideColor));

            while (pair.CornerWhitePosition.Face != targetFace)
                DoMove(DOWN);

            // Paar in den Slot einsetzen
            var faceToRot = Cube.GetOpponentFace(pair.CornerWhitePosition.Face);
            int direction = pair.CornerWhitePosition.Tile == 6 ? 1 : -1;

            // Slot öffnen
            DoMove(faceToRot, direction);
            // Paar einsetzen
            DoMove(DOWN, -direction);
            // Slot schließen
            DoMove(faceToRot, -direction);
        }

        /// <summary>
        /// Paart ein Paar mit einem falsch orientierten Kantenstein und bewegt es in den richtigen Slot
        /// </summary>
        private void FalsePairedDownLayer() {
            // den Eckstein über den richtigen Slot bewegen
            Log.LogMessage($"False Paired Down Layer {pair}");

            Position pos = pair.Corner.GetPositions().First(p => p.Face == DOWN);
            int delta = SolvingUtility.GetDelta(cube.At(pos), pair.CornerWhitePosition.Face, DOWN);
            DoMove(DOWN, -delta);

            // führt einen Algorithmus aus der diese Konstellation löst
            // F2L 25/ F2L 40 
            // https://speedcube.de/fridrich_f2l.php
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

        /// <summary>
        /// Bringt einen Eckstein im Slot mit der weißen Fläche nach oben und den Kantenstein auf der gelben Ebene in die richtige Position
        /// </summary>
        private void CornerInSlot_WhiteUp_EdgeDown() {
            // den Kantenstein in die richtige Position bringen
            var edgeUpColor = pair.Edge.GetColor(p => p.Face == DOWN);
            CubeFace opponentFace = Cube.GetOpponentFace(pair.Corner.GetColorPosition(c => c == edgeUpColor).Face);

            while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != opponentFace)
                DoMove(DOWN, 1);

            // die beiden Steine paaren
            var cornerPos = pair.Corner.GetColorPosition(c => c == edgeUpColor);
            CubeFace faceToRot = cornerPos.Face;
            int direction = cornerPos.Tile == 2 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, -direction);

            // Paar in den richtigen Slot einsetzen
            RightPairedDownLayer();
        }

        /// <summary>
        /// "Krokodil"-Konstellation
        /// Bringt einen weißen Eckstein im Slot mit weiß zur Seite und den richtig orientierten Kantenstein auf der gelben Ebene in die richtige Position
        /// </summary>
        private void CornerInSlot_WhiteSide_EdgeRightDown() {

            Position cornerSidePos = pair.Corner.GetPosition(p => p.Face != UP && p != pair.CornerWhitePosition);
            while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != cornerSidePos.Face)
                DoMove(DOWN);

            CubeFace faceToRot = pair.CornerWhitePosition.Face;
            int direction = pair.CornerWhitePosition.Tile == 2 ? 1 : -1;
            // Slot öffnen und Steine paaren
            DoMove(faceToRot, direction);
            // Paar zur Seite bewegen
            DoMove(DOWN, direction);
            // Slot schließen
            DoMove(faceToRot, -direction);

            // Paar in richtigen Slot einsetzen
            RightPairedDownLayer();
        }

        /// <summary>
        /// Bringt einen weißen Eckstein im Slot mit der weißen Fläche zur Seite und den falsch orientierten Kantenstein auf der gelben Ebene in die richtige Position
        /// </summary>
        private void CornerInSlot_WhiteSide_EdgeFalseDown() {
            // Kantenstein in die richtige Position bringen
            while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != pair.CornerWhitePosition.Face)
                DoMove(DOWN);

            CubeFace faceToRot = pair.CornerWhitePosition.Face;
            int direction = pair.CornerWhitePosition.Tile == 2 ? 1 : -1;

            // Slot öffnen
            DoMove(faceToRot, direction);
            // Eckstein aus dem Slot bewegen
            DoMove(DOWN, direction);
            // Slot schließen
            DoMove(faceToRot, -direction);

            // "Tiger"-Konstellation lösen
            TigerPosition();
        }

        /// <summary>
        /// Löst die "Tiger"-Konstellation
        /// </summary>
        private void TigerPosition() {
            // Eckstein über richtigen Slot bewegen
            CubeFace targetFace = Cube.GetFace(pair.Corner.GetColor(p => p.Face == DOWN));
            while (pair.CornerWhitePosition.Face != targetFace)
                DoMove(DOWN);

            CubeFace faceToRot = pair.CornerWhitePosition.Face;
            int direction = pair.CornerWhitePosition.Tile == 8 ? 1 : -1;

            // Slot öffnen und Steine paaren
            DoMove(faceToRot, direction);
            // Paar in Slot einsetzen
            DoMove(DOWN, direction);
            // Slot schließen
            DoMove(faceToRot, -direction);
        }

        /// <summary>
        /// Löst die "Adler"-Konstellation
        /// Weiße Seite des Ecksteins auf der gelben Seite und Kantenstein richtig orientiert im Slot
        /// </summary>
        private void EaglePosition() {
            // die Steine paaren
            CubeColor color = pair.Corner.GetColors().First(c => c != WHITE);
            CubeFace edgeFace = pair.Edge.GetColorPosition(color).Face;
            
            while (pair.Corner.GetColorPosition(color).Face != edgeFace)
                DoMove(DOWN);

            CubeFace faceToRot = edgeFace;
            int direction = pair.Corner.GetColorPosition(color).Tile == 8 ? 1 : -1;

            // Slot öffnen
            DoMove(faceToRot, direction);
            // Paar auf gelbe Ebene bewegen
            DoMove(DOWN, -direction);
            // Slot schließen
            DoMove(faceToRot, -direction);

            // Paar in richtigen Slot einsetzen
            RightPairedDownLayer();
        }

        /// <summary>
        /// Bringt einen Eckstein mit der weißen Fläche auf der geben Seite und einen Kantenstein falsch orientiert im Slot in die richtige Position
        /// </summary>
        private void CornerDown_YellowSide_EdgeFalse() {
            // Eckstein in die richtige Position bringen
            CubeColor color = pair.Corner.GetColors().First(c => c != WHITE);
            CubeFace edgeFace = pair.Edge.GetColorPosition(color).Face;

            while (pair.Corner.GetColorPosition(color).Face != edgeFace)
                DoMove(DOWN);

            Position edgePos = pair.Edge.GetColorPosition(c => c != color);
            CubeFace faceToRot = edgePos.Face;
            int direction = edgePos.Tile == 5 ? 1 : -1;

            // Slot öffnen
            DoMove(faceToRot, direction);
            // Eckstein in Slot einsetzen und Kantenstein aus Slot bewegen
            DoMove(DOWN, -direction);
            // Slot schließen
            DoMove(faceToRot, -direction);

            // "Krokodil"-Konstellation lösen
            CornerInSlot_WhiteSide_EdgeRightDown();
        }

        /// <summary>
        /// Bringt einen Eckstein mit der weißen Fläche auf der gelben Seite und einen Kantenstein auf der gelben Ebene in die richtige Position
        /// </summary>
        private void CornerDown_YellowSide_EdgeDown() {
            // Kantenstein in die richtige Position bewegen
            CubeColor color = pair.Edge.GetColor(p => p.Face != DOWN);
            CubeFace targetFace = Cube.GetFace(color);

            while (pair.Edge.GetPosition(p => p.Face != DOWN).Face != targetFace)
                DoMove(DOWN);

            CubeFace faceToRot = pair.Edge.GetPosition(p => p.Face != DOWN).Face;
            int colorSide = Array.IndexOf(CubeSolver.MiddleLayerFaces, (CubeFace)pair.Edge.GetColor(p => p.Face != DOWN));
            int colorDown = Array.IndexOf(CubeSolver.MiddleLayerFaces, (CubeFace)pair.Edge.GetColor(p => p.Face == DOWN));
            int direction = 2 - (colorDown - colorSide + 4) % 4;

            // Kantenstein falsch herum in Slot einsetzen
            DoMove(faceToRot, direction);

            // Steine richtig Paaren
            while (pair.Corner.GetColorPosition(color).Face != targetFace)
                DoMove(DOWN);

            DoMove(faceToRot, -direction);

            // Paar in den richtigen Slot einsetzen
            RightPairedDownLayer();
        }

        /// <summary>
        /// Bringt einen Eckstein auf der gelben Ebene mit der weißen Fläche zur Seite und einen Kantenstein in einem Slot in die richtige Position
        /// </summary>
        private void CornerDown_Side_EdgeSlot() {
            // in die "Tiger"-Konstellation umformen
            CubeColor sideColor = pair.Corner.GetColor(p => p.Face != DOWN && p != pair.CornerWhitePosition);
            CubeFace targetFace = Cube.GetOpponentFace(pair.Edge.GetColorPosition(sideColor).Face);

            while (pair.CornerWhitePosition.Face != targetFace)
                DoMove(DOWN);

            CubeFace faceToRot = pair.Edge.GetColorPosition(c => c != sideColor).Face;
            int direction = pair.Edge.GetColorPosition(c => c != sideColor).Tile == 5 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, -direction);
            DoMove(faceToRot, -direction);

            // "Tiger"-Konstellation lösen
            TigerPosition();
        }

        /// <summary>
        /// Bringt ein Paar mit einem falsch orientierten Eckstein auf der gelben Ebene in die richtige Position
        /// </summary>
        private void PairedDown_CornerFalse() {
            // in die "Krokodil"-Konstellation umformen
            CubeColor sideColor = pair.Corner.GetColor(p => p.Face != DOWN && p != pair.CornerWhitePosition);
            CubeFace targetFace = Cube.GetFace(sideColor);

            while (pair.Corner.GetColorPosition(sideColor).Face != targetFace)
                DoMove(DOWN);

            CubeFace faceToRot = targetFace;
            int direction = pair.Corner.GetColorPosition(sideColor).Tile == 8 ? 1 : -1;

            DoMove(faceToRot, direction);
            DoMove(DOWN, direction);
            DoMove(faceToRot, -direction);

            // "Krokodil"-Konstellation lösen
            CornerInSlot_WhiteSide_EdgeRightDown();
        }

        /// <summary>
        /// Bringt ein falsch gepaartes Paar auf der gelben Ebene mit der weißen Fläche des Ecksteins auf der gelben Seite in die richtige Position
        /// </summary>
        private void CornerDown_YellowSide_Paired() {            
            Position edgeSidePos = pair.Edge.GetPosition(p => p.Face != DOWN);
            CubeColor edgeSideColor = pair.Edge.GetColor(edgeSidePos);
            CubeColor cornerSideColor = pair.Corner.GetColor(p => p.Face == edgeSidePos.Face);

            // Eckstein über den Slot bewegen
            CubeFace targetFace = Cube.GetFace(pair.Edge.GetColor(p => p.Face != DOWN));

            while (pair.Corner.GetColorPosition(cornerSideColor).Face != targetFace)
                DoMove(DOWN);

            // seitlichen Farben des Paares gleich
            if (cornerSideColor == edgeSideColor) {
                // "F2L 19"/"F2L 21"
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
                // "F2L 18"/"F2L 20"
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

        /// <summary>
        /// Bringt einen Kantenstein auf der gelben Ebene und den Eckstein auf der gelben Ebene mit der weißen Fläche zur Seite in die richtige Position
        /// </summary>
        private void CornerDown_Side_EdgeDown() {
            // überprüfen, ob die Farben der beiden Steine auf der gelben Seite gleich sind
            CubeColor edgeUpColor = pair.Edge.GetColor(p => p.Face == DOWN);
            CubeColor cornerUpColor = pair.Corner.GetColor(p => p.Face == DOWN);

            if (edgeUpColor == cornerUpColor) {
                // in die "Krokodil"-Konstellation umformen
                CubeFace targetFace = Cube.GetFace(pair.Corner.GetColors().First(c => !(c == WHITE || c == cornerUpColor)));

                while (pair.CornerWhitePosition.Face != targetFace)
                    DoMove(DOWN);

                CubeFace faceToRot = Cube.GetFace(cornerUpColor);
                int direction = pair.CornerWhitePosition.Tile == 8 ? 1 : -1;

                DoMove(faceToRot, direction);
                DoMove(DOWN, -direction);
                DoMove(faceToRot, -direction);

                // "Krokodil"-Konstellation lösen
                CornerInSlot_WhiteSide_EdgeRightDown();
            }
            else {
                // überprüfen, ob die Steine in "Tiger"-Konstellation sind
                bool isTiger = pair.Corner.GetColorPosition(c => !(c == WHITE || c == cornerUpColor)).Face
                    == Cube.GetOpponentFace(pair.Edge.GetPosition(p => p.Face != DOWN).Face);

                if (!isTiger) {
                    // Eckstein in Slot bewegen
                    CubeFace targetFace = Cube.GetFace(pair.Corner.GetColor(p => p.Face != DOWN && p != pair.CornerWhitePosition));

                    while (pair.CornerWhitePosition.Face != targetFace)
                        DoMove(DOWN);

                    CubeFace faceToRot = Cube.GetFace(cornerUpColor);
                    int direction = pair.CornerWhitePosition.Tile == 8 ? 1 : -1;

                    DoMove(faceToRot, direction);
                    DoMove(DOWN, -direction);
                    DoMove(faceToRot, -direction);

                    // die resultierende Konstellation lösen
                    CornerInSlot_WhiteSide_EdgeFalseDown();
                }
                else {
                    // "Tiger"-Konstellation lösen
                    TigerPosition();
                }
            }
        }
    }
}
