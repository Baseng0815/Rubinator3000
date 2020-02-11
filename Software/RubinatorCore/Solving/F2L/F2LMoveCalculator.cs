using System;
using System.Collections.Generic;
using System.Linq;
using static RubinatorCore.CubeColor;
using static RubinatorCore.CubeFace;

namespace RubinatorCore.Solving {
    /// <summary>
    /// Berechnet die Züge die zum Lösen eines F2L-Paares nötig sind
    /// </summary>
    internal partial class F2LMoveCalculator {

        /// <summary>
        /// Eine Kopie des Würfels der gelöst werden soll
        /// </summary>
        private Cube cube;

        /// <summary>
        /// Die F2L-Paare
        /// </summary>
        private IEnumerable<F2LPair> pairs;

        /// <summary>
        /// Das aktuelle F2L-Paar
        /// </summary>
        private F2LPair pair;

        /// <summary>
        /// Ein EventHandler um die Züge zu speichern
        /// </summary>
        /// <param name="face">Die Seite, die gedreht wird</param>
        /// <param name="count">Die Anzahl der Drehungen im Uhrzeigersinn</param>
        private delegate void DoMoveEventHandler(CubeFace face, int count = 1);

        /// <summary>
        /// Ein Event, das ausgelöst wird, wenn der Würfel gedreht wird
        /// </summary>
        private event DoMoveEventHandler DoMove;

        /// <summary>
        /// Erstellt einen neuen <see cref="F2LMoveCalculator"/> mit einem Wüfel und einem F2L-Paar
        /// /// </summary>
        /// <param name="pair">Das Paar, für welches die Züge berechnet werden sollen</param>
        /// <param name="cube">Der Würfel</param>
        public F2LMoveCalculator(F2LPair pair, Cube cube) {
            this.cube = (Cube)cube.Clone();

            // die Position der F2L-Paare bestimmen
            pairs = from corner in this.cube.Corners
                    where corner.HasColor(WHITE)
                    select F2LPair.GetPair(corner, this.cube);

            // das zu lösende Paar festlegen
            this.pair = pairs.First(p => p == pair);
        }

        /// <summary>
        /// Gibt die Züge zurück, die nötig sind um das Paar zu lösen
        /// </summary>
        /// <returns>Eine <see cref="MoveCollection"/>, die die Züge zum Lösen enthält</returns>
        public MoveCollection CalcMoves() {
            MoveCollection moves = new MoveCollection();

            // eine lokale Methode zum Hinzufügen und Ausführen der Züge
            void DoMove(CubeFace face, int count) {
                if (count == 0)
                    return;

                Move m = new Move(face, count);
                cube.DoMove(m);
                moves.Add(m);
            }

            // das DoMove Event abonnieren
            this.DoMove += DoMove;

            // die Züge für das Paar berechnen
            CalcPairMoves();

            // das DoMove Event deabonnieren
            this.DoMove -= DoMove;

            // die Züge zum Lösen zurückgeben
            return moves;
        }

        /// <summary>
        /// Berechnet die Züge, die nötig sind um das aktuelle Paar zu lösen
        /// </summary>
        private void CalcPairMoves() {
            // Eckstein im Slot und weiße Fläche auf der weißen Seite
            if (pair.CornerWhitePosition.Face == UP) {
                // im richtigen Slot
                if (pair.Corner.InRightPosition()) {
                    // Kantenstein in einem Slot
                    if (pair.EdgeInSlot) {
                        // F2L-Paar breits gelöst
                        if (pair.Edge.InRightPosition()) {
                            return;
                        }
                        // Kantenstein falsch orientiert im falschen Slot
                        else if (EdgeFalseInRightSlot(pair.Edge)) {
                            // Paar auf gelbe Ebene bewegen
                            MoveSlotUp(pair.Corner);

                            // Falsch gepaart auf gelber Ebene lösen
                            FalsePairedDownLayer();
                        }
                        // Kantenstein in einem falschen Slot
                        else {
                            // Kantentstein auf gelbe Ebene bewegen
                            MoveSlotUp(pair.Edge);

                            // Konstellation lösen
                            CornerInSlot_WhiteUp_EdgeDown();
                        }
                    }
                    // Kantenstein auf gelber Ebene
                    else {
                        CornerInSlot_WhiteUp_EdgeDown();
                    }
                }
                // Eckstein im falschen Slot
                else {
                    // Kantenstein in einem Slot
                    if (pair.EdgeInSlot) {
                        IEnumerable<CubeFace> cornerSideFaces = from pos in pair.Corner.GetPositions()
                                                                where pos.Face != UP
                                                                select pos.Face;
                        // Kanten- und Eckstein im gleichen Slot
                        if (pair.Edge.GetPositions().All(p => cornerSideFaces.Contains(p.Face))) {
                            // richtig gepaart
                            if (pair.Paired) {
                                // auf gelbe Ebene bewegen
                                MoveSlotUp(pair.Corner);

                                // in richtigen Slot einsetzen
                                RightPairedDownLayer();
                            }
                            else {
                                // Kantenstein auf gelbe Ebene bewegen
                                MoveSlotUp(pair.Edge);

                                // als falsch gepaart lösen
                                FalsePairedDownLayer();
                            }
                        }
                        // Kantenstein in anderem Slot
                        else {
                            // Kantenstein auf gelbe Ebene bringen
                            MoveSlotUp(pair.Edge);

                            // Konstellation lösen
                            CornerInSlot_WhiteUp_EdgeDown();
                        }
                    }
                    else {
                        // Eckstein richtig im Slot und Kantenstein auf gelber Ebene lösen
                        CornerInSlot_WhiteUp_EdgeDown();
                    }
                }
            }
            // Eckstein falsch orientiert in einem Slot
            else if (pair.Corner.GetPositions().Any(p => p.Face == UP)) {
                // die andere seitliche Farbe bestimmen, die nicht Weiß ist
                Position cornerSidePos = pair.Corner.GetPosition(p => p.Face != UP && p != pair.CornerWhitePosition);
                CubeColor cornerSideColor = pair.Corner.GetColor(cornerSidePos);

                // Kantenstein in einem Slot
                if (pair.EdgeInSlot) {
                    // beide Steine im gleichen Slot
                    if (pair.IsPaired()) {
                        // beide Steine uf gelbe Ebene bewegen und neu lösen
                        MoveSlotUp(pair.Edge);

                        CalcPairMoves();
                    }
                    // Kantenstein in einem anderen Slot
                    else {
                        // Kantenstein auf gelbe Ebene bewegen und neu lösen
                        MoveSlotUp(pair.Edge);

                        CalcPairMoves();
                    }
                }
                // Kantenstein auf gelber Ebene
                else {
                    // Farbe des Kantensteins auf gelber Seite bestimmen
                    CubeColor edgeDownColor = pair.Edge.GetColor(p => p.Face == DOWN);

                    // Kantenstein richtig orientiert
                    if (cornerSideColor == edgeDownColor) {
                        // "Krokodil"-Konstellation lösen
                        CornerInSlot_WhiteSide_EdgeRightDown();
                    }
                    else {
                        CornerInSlot_WhiteSide_EdgeFalseDown();
                    }
                }
            }
            // Eckstein auf gelber Ebene und weiße Fläche nach oben
            else if (pair.CornerWhitePosition.Face == DOWN) {
                // Kantenstein in einem Slot
                if (pair.EdgeInSlot) {
                    CubeColor color = pair.Corner.GetColors().First(c => c != WHITE);
                    int cornerTile = pair.Corner.GetColorPosition(color).Tile;
                    int edgeTile = pair.Edge.GetColorPosition(color).Tile;

                    // Kantenstein richtig orientiert
                    if (edgeTile + 3 == cornerTile) {
                        EaglePosition();
                    }
                    // Kantenstein falsch orientiert
                    else {
                        CornerDown_YellowSide_EdgeFalse();
                    }
                }
                // Kantenstein auf gelber Ebene
                else {
                    if (pair.IsPaired()) {
                        CornerDown_YellowSide_Paired();
                    }
                    else {
                        CornerDown_YellowSide_EdgeDown();
                    }
                }
            }
            // Eckstein auf gelber Ebene und weiße Fläche zur Seite
            else {
                // Kantenstein in einem Slot
                if (pair.EdgeInSlot) {
                    CornerDown_Side_EdgeSlot();
                }
                // Kantenstein auf gelber Ebene
                else {
                    // Steine gepaart
                    if (pair.IsPaired(out bool edgeRight, out bool cornerRight)) {
                        // Eckstein falsch orientiert
                        if (!cornerRight) {
                            PairedDown_CornerFalse();
                        }
                        // Eckstein richtig orientiert
                        else {
                            // Kantenstein richtig
                            if (edgeRight) {
                                RightPairedDownLayer();
                            }
                            // Kantenstein falsch
                            else {
                                FalsePairedDownLayer();
                            }
                        }
                    }
                    // nicht gepaart
                    else {
                        CornerDown_Side_EdgeDown();
                    }
                }
            }
        }

        /// <summary>
        /// Bestimmt, ob ein Kantenstein falsch orientiert im richtigen Slot ist
        /// </summary>
        /// <param name="edge">Der Kantenstein, der überprüft werden soll</param>
        /// <returns>Einen Wert, der angibt, ob der Kantentstein falsch orientiert im richtigen Slot ist</returns>
        private static bool EdgeFalseInRightSlot(EdgeStone edge) {
            // überprüfen, ob der Kantenstein in einem Slot ist
            if (edge.GetPositions().All(p => p.Face != DOWN)) {
                // überprüfen ob die Orientierung falsch ist und der Slot richig ist
                var color1Face = Cube.GetFace(edge.Colors.Item1);
                var color2Face = Cube.GetFace(edge.Colors.Item2);

                return edge.GetColorPosition(edge.Colors.Item1).Face == color2Face
                    && edge.GetColorPosition(edge.Colors.Item2).Face == color1Face;
            }

            return false;
        }
    }
}