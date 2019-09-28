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

        public EdgeStone(Tuple<CubeColor, CubeColor> colors, Cube cube) {
            if (Cube.IsOpponentColor(colors.Item1, colors.Item2) || colors.Item1 == colors.Item2)
                throw new ArgumentException();

            this.colors = colors ?? throw new ArgumentNullException(nameof(colors));
            this.cube = cube ?? throw new ArgumentNullException(nameof(cube));
        }

        public bool HasColor(CubeColor color) {
            return colors.Item1 == color || colors.Item2 == color;
        }

        public IEnumerable<CubeColor> GetColors() {
            yield return colors.Item1;
            yield return colors.Item2;
        }

        public Position GetColorPosition(CubeColor color) {
            if (!(colors.Item1 == color || colors.Item2 == color))
                throw new ArgumentOutOfRangeException(nameof(color));

            var pos = Positions;
            return cube.At(pos.Item1) == color ? pos.Item1 : pos.Item2;
        }

        public Position GetColorPosition(Func<CubeColor, bool> colorPredicate) {
            CubeColor[] stoneColors = { colors.Item1, colors.Item2 };

            return GetColorPosition(stoneColors.First(colorPredicate));
        }

        public IEnumerable<Position> GetPositions() {
            var pos = Positions;
            yield return pos.Item1;
            yield return pos.Item2;
        }

        public bool InRightPosition() {
            var pos = Positions;
            Cube cube = this.cube;

            bool OnRightFace(Position s) {
                CubeColor color = cube.At(s);

                return (int)color == (int)s.Face;
            }

            return OnRightFace(pos.Item1) && OnRightFace(pos.Item2);
        }

        public static EdgeStone FromPosition(Cube cube, Position position) {
            if (cube == null) throw new ArgumentNullException(nameof(cube));

            if (!Cube.EdgeStonePositions.Any(e => e.Item1 == position || e.Item2 == position)) throw new ArgumentOutOfRangeException(nameof(position));

            Tuple<Position, Position> pos = Cube.EdgeStonePositions.First(p => p.Item1 == position || p.Item2 == position);
            Tuple<CubeColor, CubeColor> colors = new Tuple<CubeColor, CubeColor>(cube.At(pos.Item1), cube.At(pos.Item2));

            return new EdgeStone(colors, cube);
        }
    }
}
