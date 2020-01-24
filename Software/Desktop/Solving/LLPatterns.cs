using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {

    public interface IPattern {
        int Number { get; }
        bool IsMatch(Cube cube);
    }

    public struct OllPattern : IPattern {
        private bool[] face;
        private bool[][] sides;

        public int Number { get; }

        public bool[] Face {
            get => face;
        }

        public bool[][] Sides {
            get => sides;
        }

        public OllPattern(int number, bool[] face, bool[][] sides) {
            if (face.Length != 9)
                throw new ArgumentOutOfRangeException(nameof(face));

            if (sides.Length != 4 || sides.Any(e => e.Length != 3))
                throw new ArgumentOutOfRangeException(nameof(sides));

            this.face = face;
            this.sides = sides;
            this.Number = number;
        }

        public bool IsMatch(Cube cube) {
            for (int t = 0; t < 9; t++) {
                if ((cube.At(DOWN, t) == YELLOW) != face[t])
                    return false;
            }

            for (int f = 0; f < 4; f++) {
                var face = CubeSolver.MiddleLayerFaces[f];
                for (int i = 0; i < 3; i++) {
                    if ((cube.At(face, i + 6) == YELLOW) != sides[f][i])
                        return false;
                }
            }

            return true;
        }
    }

    public struct PllPattern : IPattern {
        public int Number { get; }

        private byte[][] patternData;

        public byte[][] PatternData {
            get => patternData;
        }

        private static readonly Dictionary<CubeColor, int> middleLayerColors;

        static PllPattern() {
            middleLayerColors = new Dictionary<CubeColor, int>();
            middleLayerColors.Add(ORANGE, 0);
            middleLayerColors.Add(GREEN, 1);
            middleLayerColors.Add(RED, 2);
            middleLayerColors.Add(BLUE, 3);
        }

        public PllPattern(int number, byte[][] patternData) {
            if (patternData.Length != 4 || patternData.Any(e => e.Length != 3))
                throw new ArgumentOutOfRangeException(nameof(patternData));

            this.patternData = patternData;
            this.Number = number;
        }

        public bool IsMatch(Cube cube) {
            for (int f = 0; f < 4; f++) {
                CubeFace face = CubeSolver.MiddleLayerFaces[f];

                CubeColor[] tiles = {
                    cube.At(face, 6),
                    cube.At(face, 7),
                    cube.At(face, 8)
                };

                int delta0 = (SolvingUtility.NormalizeCount(middleLayerColors[tiles[0]] - middleLayerColors[tiles[1]]) + 4) % 4;
                int delta1 = (SolvingUtility.NormalizeCount(middleLayerColors[tiles[0]] - middleLayerColors[tiles[2]]) + 4) % 4;
                int delta2 = (SolvingUtility.NormalizeCount(middleLayerColors[tiles[1]] - middleLayerColors[tiles[2]]) + 4) % 4;

                if (delta0 != patternData[f][0] || delta1 != patternData[f][1] || delta2 != patternData[f][2])
                    return false;
            }

            return true;
        }
    }
}