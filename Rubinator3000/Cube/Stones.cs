using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000 {
    public struct EdgeStone {
        private Tuple<CubeColor, CubeColor> colors;
        private readonly Cube cube;

        public Tuple<CubeColor, CubeColor> Colors {
            get => colors;
            set {
                if (Cube.IsOpponentColor(value.Item1, value.Item2) || value.Item1 == value.Item2)
                    throw new ArgumentException();

                colors = value;
            }
        }

        public Tuple<Position, Position> Positions {
            get {
                if (this.cube == null || this.colors == null)
                    throw new InvalidOperationException();

                Cube cube = this.cube;
                var colors = this.colors;

                return (from edge in Cube.EdgeStonePositions
                        where cube.At(edge.Item1) == colors.Item1 || cube.At(edge.Item2) == colors.Item1
                        select edge).First(edge => cube.At(edge.Item2) == colors.Item2 || cube.At(edge.Item2) == colors.Item2);

            }
        }

        public bool HasColor(CubeColor color) {
            return colors.Item1 == color || colors.Item2 == color;
        }
    }
}
