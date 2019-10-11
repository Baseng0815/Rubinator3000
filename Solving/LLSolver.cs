using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rubinator3000.Solving {
    public class LLSolver : CubeSolver {
        public LLSolver(Cube cube) : base(cube) {

        }

        protected override bool CheckCube(Cube cube) {
            // check white face is solved
            for (int t = 0; t < 9; t++) {
                if (cube.At(CubeFace.UP, t) != CubeColor.WHITE)
                    return false;
            }

            // check FTL is solved
            for (int f = 0; f < 4; f++) {
                CubeColor faceColor = Cube.GetFaceColor(MiddleLayerFaces[f]);
                for (int t = 3; t < 9; t++) {
                    if (cube.At(MiddleLayerFaces[f], t) != faceColor)
                        return false;
                }
            }

            return true;
        }

        protected override void CalcMoves() {
            int count = 0;
            CubeOrientation orientation;
            MoveCollection moves;
            if (!OllSolved()) {
                
                for (count = 0; count < 4; count++) {
                    if (OllPatterns.Any(e => e.pattern.IsMatch(cube)))
                        break;

                    DoMove(CubeFace.DOWN, addMove: false);
                }
                moves = OllPatterns.First(e => e.pattern.IsMatch(cube)).algorithm;
                DoMove(CubeFace.DOWN, 4 - count, false);

                orientation = new CubeOrientation(MiddleLayerFaces[count], CubeFace.UP);

                this.moves.AddRange(moves.TransformMoves(orientation));
            }
            
            for(count = 0; count < 4; count++) {
                if (PllPatterns.Any(p => p.pattern.IsMatch(cube)))
                    break;

                DoMove(CubeFace.DOWN, addMove: false);
            }
            moves = OllPatterns.First(p => p.pattern.IsMatch(cube)).algorithm;
            DoMove(CubeFace.DOWN, 4 - count, false);

            orientation = new CubeOrientation(MiddleLayerFaces[count], CubeFace.UP);

            this.moves.AddRange(moves.TransformMoves(orientation));

            if (!GetCubeSolved()) {
                throw new InvalidProgramException();
            }
        }

        protected bool OllSolved() {
            for (int t = 0; t < 9; t++) {
                if (cube.At(CubeFace.DOWN, t) != CubeColor.YELLOW)
                    return false;
            }

            return true;
        }

        #region static members

        private static (OllPattern pattern, MoveCollection algorithm)[] OllPatterns;
        private static (PllPattern pattern, MoveCollection algorithm)[] PllPatterns;

        public override bool Solved => throw new NotImplementedException();

        private static void LoadOllPatterns() {
            XDocument doc = XDocument.Parse(Properties.Resources.ollSolving);

            Func<XElement, (OllPattern, MoveCollection)> getPattern = e => {
                int ollNumber = int.Parse(e.Attribute("number").Value);

                // read faceData from xml
                int face = int.Parse(e.Attribute("face").Value);

                bool[] faceData = new bool[9];
                for (int i = 0; i < faceData.Length; i++) {
                    int exp = (int)Math.Pow(2, i);
                    faceData[i] = (face & exp) == exp;
                }

                // read sidesData
                bool[][] sidesData = new bool[4][];
                for (int s = 0; s < 4; s++) {
                    sidesData[s] = new bool[3];
                    int sideValue = int.Parse(e.Attribute($"face{s}").Value);

                    for (int i = 0; i < 3; i++) {
                        int exp = (int)Math.Pow(2, i);
                        sidesData[s][i] = (sideValue & exp) == exp;
                    }
                }

                try {
                    MoveCollection moves = MoveCollection.Parse(e.Attribute("algorithm").Value);
                    return (new OllPattern(ollNumber, faceData, sidesData), moves);
                }
                catch (FormatException ex) {
                    string message = $"Error:\tParsing Oll algorithm {ollNumber}";
                    Log.LogStuff(message);
                }

                return (new OllPattern(), null);
            };

            IEnumerable<(OllPattern, MoveCollection)> patterns = from element in doc.Root.Elements("ollPattern")
                                                                 select getPattern(element);

            OllPatterns = patterns.ToArray();
        }

        private static void LoadPllPatterns() {
            XDocument doc = XDocument.Parse(Properties.Resources.pllSolving);
            CubeOrientation orientation = new CubeOrientation(CubeFace.LEFT, CubeFace.DOWN);

            Func<XElement, (PllPattern, MoveCollection)> getPattern = e => {
                int number = int.Parse(e.Attribute("number").Value);

                byte[][] patternData = new byte[4][];
                // read data from xml
                for (int f = 0; f < 4; f++) {
                    patternData[f] = new byte[3];
                    int value = int.Parse(e.Attribute($"face{f}").Value);

                    for (int i = 0; i < 3; i++) {
                        patternData[f][i] = (byte)(value >> (i * 2) & 0x3);
                    }
                }

                try {
                    MoveCollection moves = MoveCollection.Parse(e.Attribute("algorithm").Value);
                    return (new PllPattern(number, patternData), moves.TransformMoves(orientation));
                }
                catch (FormatException ex) {
                    string message = $"Error:\tParsing Pll algorithm {number}";
                    Log.LogStuff(message);
                }

                return (new PllPattern(), null);
            };

            IEnumerable<(PllPattern, MoveCollection)> patterns = from element in doc.Root.Elements("pllPattern")
                                                                 select getPattern(element);

            PllPatterns = patterns.ToArray();
        }

        static LLSolver() {
            LoadOllPatterns();
            LoadPllPatterns();
        }

        #endregion
    }
}
