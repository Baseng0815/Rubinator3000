using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RubinatorCore.Solving {
    /// <summary>
    /// Ein Last Layer Solver, der die letzte Seite löst
    /// </summary>
    public class LLSolver : CubeSolver {
        /// <summary>
        /// Gibt an, ob der Würfel gelöst ist
        /// </summary>
        public override bool Solved => GetCubeSolved();

        /// <summary>
        /// Erstellt einen neuen Last Layer Solver mit einer Kopie des aktuellen Würfels
        /// </summary>
        /// <param name="cube">Der zu lösende Cube</param>
        public LLSolver(Cube cube) : base(cube) {

        }

        /// <summary>
        /// Überprüft, ob die vorherigen beiden Seiten gelöst sind
        /// </summary>
        /// <param name="cube">Der zu lösende Würfel</param>
        /// <returns>Einen Wert, der angibt, ob der Würfel den Anforderungen entspricht</returns>
        protected override bool CheckCube(Cube cube) {
            // überprüfen, ob die weiße Seite gelöst ist
            for (int t = 0; t < 9; t++) {
                if (cube.At(CubeFace.UP, t) != CubeColor.WHITE)
                    return false;
            }

            // überprüfen, ob die obere und mittlere Ebene gelöst sind
            for (int f = 0; f < 4; f++) {
                CubeColor faceColor = Cube.GetFaceColor(MiddleLayerFaces[f]);
                for (int t = 0; t < 6; t++) {
                    if (cube.At(MiddleLayerFaces[f], t) != faceColor)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Bestimmt die Algorithmen zum Lösen der letzten Ebene
        /// </summary>
        public override void SolveCube() {
            try {
                // überprüfen, ob das OLL gelöst ist
                if (!OllSolved()) {
                    // das richtige OLL Pattern finden                    
                    while (!OllPatterns.Any(p => p.pattern.IsMatch(cube))) {
                        DoMove(new Move(CubeFace.DOWN));
                    }

                    // den Algorithmus ausführen
                    (OllPattern p, MoveCollection a) pattern = OllPatterns.First(p => p.pattern.IsMatch(cube));
                    Log.LogMessage($"OLL Pattern {pattern.p.Number}");
                    DoMoves(pattern.a);
                }

                // überprüfen, ob der Würfel glöst ist
                if (!GetCubeSolved()) {
                    // das richtige PLL Pattern finden                    
                    while (!PllPatterns.Any(p => p.pattern.IsMatch(cube))) {
                        DoMove(new Move(CubeFace.DOWN));
                    }

                    // den Algorithmus ausführen
                    (PllPattern p, MoveCollection a) pattern = PllPatterns.First(p => p.pattern.IsMatch(cube));
                    Log.LogMessage($"PLL Pattern {pattern.p.Number}");
                    DoMoves(pattern.a);
                }

                // die gelbe Seite in die richtige Position drehen
                while (!GetCubeSolved()) {
                    DoMove(new Move(CubeFace.DOWN));
                }
            }
            catch (Exception e) {
                Log.LogMessage("Last Layer Solver:\t" + e.ToString());
            }
        }

        /// <summary>
        /// Überprüft, ob das Oll gelöst ist
        /// </summary>
        /// <returns></returns>
        protected bool OllSolved() {
            // die gelbe Seite überprüfen
            for (int t = 0; t < 9; t++) {
                if (cube.At(CubeFace.DOWN, t) != CubeColor.YELLOW)
                    return false;
            }

            return true;
        }

        #region static members        
        /// <summary>
        /// Die OLL Patterns und Algorithmen
        /// </summary>
        public static (OllPattern pattern, MoveCollection algorithm)[] OllPatterns;

        /// <summary>
        /// Die PLL Patterns und Algorithmen
        /// </summary>
        public static (PllPattern pattern, MoveCollection algorithm)[] PllPatterns;

        /// <summary>
        /// Lädt die OLL Patterns und Algorithmen aus der xml-Datei
        /// </summary>
        internal static void LoadOllPatterns() {
            // die xml-Datei öffnen
            XDocument doc = XDocument.Parse(Resources.ollSolving);

            Func<XElement, (OllPattern, MoveCollection)> getPattern = e => {
                // die Nummer des Patterns aus dem xml-Element lesen
                int ollNumber = int.Parse(e.Attribute("number").Value);

                // die Konstellation der gelben Seite aus dem xml-Element lesen
                int face = int.Parse(e.Attribute("face").Value);

                bool[] faceData = new bool[9];
                for (int i = 0; i < faceData.Length; i++) {
                    int exp = (int)Math.Pow(2, i);
                    faceData[i] = (face & exp) == exp;
                }

                // die Orientierung der anderen Seiten aus dem xml-Element lesen
                bool[][] sidesData = new bool[4][];
                for (int s = 0; s < 4; s++) {
                    sidesData[s] = new bool[3];
                    int sideValue = int.Parse(e.Attribute($"side{s}").Value);

                    for (int i = 0; i < 3; i++) {                        
                        sidesData[s][i] = (sideValue & (1 >> i)) == (1 >> i);
                    }
                }

                try {
                    // den Algorithmus aus dem xml-Element lesen
                    MoveCollection moves = MoveCollection.Parse(e.Attribute("algorithm").Value);

                    // das Pattern und den Algorithmus zurückgeben
                    return (new OllPattern(ollNumber, faceData, sidesData), moves);
                }
                catch (FormatException ex) {
                    string message = $"Error:\tParsing Oll algorithm {ollNumber}";
                    Log.LogMessage(message);
                }

                return (new OllPattern(), null);
            };

            // alle Elemente aus der Datei lesen und speichern
            IEnumerable<(OllPattern, MoveCollection)> patterns = from element in doc.Root.Elements("ollPattern")
                                                                 select getPattern(element);

            OllPatterns = patterns.ToArray();
        }
        internal static void LoadPllPatterns() {
            XDocument doc = XDocument.Parse(Resources.pllSolving);
            CubeOrientation orientation = new CubeOrientation(CubeFace.LEFT, CubeFace.DOWN);

            Func<XElement, (PllPattern, MoveCollection)> getPattern = e => {
                // die Nummer des Patterns aus dem xml-Element lesen
                int number = int.Parse(e.Attribute("number").Value);

                byte[][] patternData = new byte[4][];
                // die Farbdifferenzen aus der xml-Datei lesen
                for (int f = 0; f < 4; f++) {
                    patternData[f] = new byte[3];
                    int value = int.Parse(e.Attribute($"face{f}").Value);

                    for (int i = 0; i < 3; i++) {
                        patternData[f][i] = (byte)(value >> (i * 2) & 0x3);
                    }
                }

                try {
                    // den Algorithmus aus dem xml-Element lesen
                    MoveCollection moves = MoveCollection.Parse(e.Attribute("algorithm").Value);

                    // das Pattern und den transformierten Algorithmus zurückgeben
                    return (new PllPattern(number, patternData), moves.TransformMoves(orientation));
                }
                catch (FormatException) {
                    string message = $"Error:\tParsing Pll algorithm {number}";
                    Log.LogMessage(message);
                }

                return (new PllPattern(), null);
            };

            // alle Elemente aus der Datei lesen und speichern
            IEnumerable<(PllPattern, MoveCollection)> patterns = from element in doc.Root.Elements("pllPattern")
                                                                 select getPattern(element);
            PllPatterns = patterns.ToArray();
        }

        /// <summary>
        /// Ein statischer Konstruktor, der die Patterns läd
        /// </summary>
        static LLSolver() {
            LoadOllPatterns();
            LoadPllPatterns();
        }

        #endregion
    }
}
