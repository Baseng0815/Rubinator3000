using System;
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
            for(int f = 0; f < 4; f++) {
                CubeColor faceColor = Cube.GetFaceColor(MiddleLayerFaces[f]);
                for(int t = 3; t < 9; t++) {
                    if (cube.At(MiddleLayerFaces[f], t) != faceColor)
                        return false;
                }
            }

            return true;
        }

        private static (OllPattern pattern, MoveCollection algorithm)[] OllPatterns;
        private static (PllPattern pattern, MoveCollection algorithm)[] PllPatterns;

        private static void LoadOllPatterns() {
            XDocument doc = XDocument.Parse(Properties.Resources.ollSolving);

            Func<XElement, (OllPattern, MoveCollection)> getPatternData = e => {
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
                }
                catch (FormatException ex) {
                    string message = $"Error:\tParsing Oll algorithm {ollNumber}";
                    Log.LogStuff(message);
                }


                return (new OllPattern(faceData, sidesData), moves);
            };

            IEnumerable<(OllPattern, MoveCollection)> patterns = from element in doc.Root.Elements("ollPattern")
                                                                 select getPatternData(element);
        }


    }
}
