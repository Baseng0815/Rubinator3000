using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000 {
    public interface IStone {
        Position GetColorPosition(CubeColor color);
        Position GetColorPosition(Func<CubeColor, bool> colorPredicate);
        CubeColor GetColor(Position position);
        CubeColor GetColor(Func<Position, bool> positionPredicate);
        IEnumerable<CubeColor> GetColors();
        IEnumerable<Position> GetPositions();
        Position GetPosition(Func<Position, bool> predicate);
        bool HasColor(CubeColor color);
        bool InRightPosition();
    }

    public struct EdgeStone : IStone {
        private readonly Tuple<CubeColor, CubeColor> colors;
        private readonly Cube cube;

        public Tuple<CubeColor, CubeColor> Colors {
            get => colors;
        }

        public Tuple<Position, Position> Positions {
            get {
                if (cube == null || colors == null)
                    throw new InvalidOperationException();


                foreach (var edgePos in Cube.EdgeStonePositions) {
                    IEnumerable<CubeColor> colors = GetColors();

                    if (colors.Contains(cube.At(edgePos.Item1))
                        && colors.Contains(cube.At(edgePos.Item2))) {
                        return edgePos;
                    }
                }

                throw new InvalidProgramException();
            }
        }

        public EdgeStone(Tuple<CubeColor, CubeColor> colors, Cube cube) {
            if (Cube.IsOpponentColor(colors.Item1, colors.Item2) || colors.Item1 == colors.Item2)
                throw new ArgumentException("Ein Kantenstein kann keine Gegenfarben enthalten", nameof(colors));

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

        public CubeColor GetColor(Position position) {
            if (!GetPositions().Contains(position))
                throw new ArgumentOutOfRangeException("Die Position gehört nicht zu dem Stein");

            return cube.At(position);
        }

        public CubeColor GetColor(Func<Position, bool> positionPredicate) {
            if (!GetPositions().Any(positionPredicate))
                throw new ArgumentException();

            Position pos = GetPositions().First(positionPredicate);
            return GetColor(pos);
        }

        public Position GetColorPosition(CubeColor color) {
            if (!(colors.Item1 == color || colors.Item2 == color))
                throw new ArgumentOutOfRangeException(nameof(color));

            var pos = Positions;
            return cube.At(pos.Item1) == color ? pos.Item1 : pos.Item2;
        }

        public Position GetColorPosition(Func<CubeColor, bool> colorPredicate) {
            return GetColorPosition(GetColors().First(colorPredicate));
        }

        public IEnumerable<Position> GetPositions() {
            var pos = Positions;
            yield return pos.Item1;
            yield return pos.Item2;
        }

        public Position GetPosition(Func<Position, bool> predicate) {
            if (!GetPositions().Any(predicate))
                throw new ArgumentException();

            return GetPositions().First(predicate);
        }

        public bool InRightPosition() {
            var pos = GetPositions();
            Cube cube = this.cube;

            Func<Position, bool> onRightFace = s => {
                CubeColor color = cube.At(s);

                return (int)color == (int)s.Face;
            };

            return pos.All(onRightFace);
        }

        internal Cube GetCube() {
            return cube;
        }

        public static EdgeStone FromPosition(Cube cube, Position position) {
            if (cube == null) throw new ArgumentNullException(nameof(cube));

            if (!Cube.EdgeStonePositions.Any(e => e.Item1 == position || e.Item2 == position))
                throw new ArgumentOutOfRangeException(nameof(position));

            Tuple<Position, Position> pos = Cube.EdgeStonePositions.First(p => p.Item1 == position || p.Item2 == position);
            Tuple<CubeColor, CubeColor> colors = new Tuple<CubeColor, CubeColor>(cube.At(pos.Item1), cube.At(pos.Item2));

            return new EdgeStone(colors, cube);
        }

        public override string ToString() {
            return string.Format("edge({0}, {1})", colors.Item1, colors.Item2);
        }

        public static bool operator ==(EdgeStone left, EdgeStone right) {
            // compare colors
            IEnumerable<CubeColor> leftColors = left.GetColors();
            IEnumerable<CubeColor> rightColors = right.GetColors();

            return leftColors.All(c => rightColors.Contains(c));
        }

        public static bool operator !=(EdgeStone left, EdgeStone right) => !(left == right);
    }

    public struct CornerStone : IStone {
        private readonly Tuple<CubeColor, CubeColor, CubeColor> colors;
        private readonly Cube cube;

        public Tuple<CubeColor, CubeColor, CubeColor> Colors {
            get => colors;
        }

        public Tuple<Position, Position, Position> Positions {
            get {
                if (cube == null || colors == null)
                    throw new InvalidOperationException();

                foreach (var corner in Cube.CornerStonePositions) {
                    IEnumerable<CubeColor> colors = GetColors();

                    if (colors.Contains(cube.At(corner.Item1))
                        && colors.Contains(cube.At(corner.Item2))
                        && colors.Contains(cube.At(corner.Item3))) {
                        return corner;
                    }
                }

                throw new InvalidProgramException();
            }
        }

        public CornerStone(Tuple<CubeColor, CubeColor, CubeColor> colors, Cube cube) {
            if (Cube.IsOpponentColor(colors.Item1, colors.Item2) || Cube.IsOpponentColor(colors.Item1, colors.Item3) || Cube.IsOpponentColor(colors.Item2, colors.Item3))
                throw new ArgumentException("Ein Eckstein kann keine Gegenfarben enthalten", nameof(colors));

            this.colors = colors ?? throw new ArgumentNullException(nameof(colors));
            this.cube = cube ?? throw new ArgumentNullException(nameof(cube));
        }

        public Position GetColorPosition(CubeColor color) {
            if (colors.Item1 != color && colors.Item2 != color && colors.Item3 != color)
                throw new ArgumentOutOfRangeException(nameof(color));

            var pos = Positions;
            if (cube.At(pos.Item1) == color)
                return pos.Item1;
            else if (cube.At(pos.Item2) == color)
                return pos.Item2;
            else if (cube.At(pos.Item3) == color)
                return pos.Item3;

            throw new InvalidProgramException();
        }

        public Position GetColorPosition(Func<CubeColor, bool> colorPredicate) {
            return GetColorPosition(GetColors().First(colorPredicate));
        }

        public IEnumerable<CubeColor> GetColors() {
            yield return colors.Item1;
            yield return colors.Item2;
            yield return colors.Item3;
        }

        public CubeColor GetColor(Position position) {
            if (!GetPositions().Contains(position))
                throw new ArgumentOutOfRangeException("Die Position gehört nicht zu dem Stein");

            return cube.At(position);
        }

        public CubeColor GetColor(Func<Position, bool> positionPredicate) {
            if (!GetPositions().Any(positionPredicate))
                throw new ArgumentException();

            Position pos = GetPositions().First(positionPredicate);
            return GetColor(pos);
        }

        public IEnumerable<Position> GetPositions() {
            var pos = Positions;

            yield return pos.Item1;
            yield return pos.Item2;
            yield return pos.Item3;
        }

        public Position GetPosition(Func<Position, bool> predicate) {
            if (!GetPositions().Any(predicate))
                throw new ArgumentException();

            return GetPositions().First(predicate);
        }

        public bool HasColor(CubeColor color) {
            return colors.Item1 == color
                || colors.Item2 == color
                || colors.Item3 == color;
        }

        public bool InRightPosition() {
            var pos = GetPositions();
            Cube cube = this.cube;

            Func<Position, bool> onRightFace = p => {
                CubeColor color = cube.At(p);

                return (int)color == (int)p.Face;
            };

            return pos.All(onRightFace);
        }

        public static CornerStone FromPosition(Cube cube, Position position) {
            if (cube == null)
                throw new ArgumentNullException(nameof(cube));

            if (!Cube.CornerStonePositions.Any(c => c.Item1 == position || c.Item2 == position || c.Item3 == position))
                throw new ArgumentOutOfRangeException(nameof(position));

            Tuple<Position, Position, Position> positions = Cube.CornerStonePositions.First(c => c.Item1 == position || c.Item2 == position || c.Item3 == position);
            Tuple<CubeColor, CubeColor, CubeColor> colors = new Tuple<CubeColor, CubeColor, CubeColor>(
                cube.At(positions.Item1), cube.At(positions.Item2), cube.At(positions.Item3));

            return new CornerStone(colors, cube);
        }

        public static bool operator ==(CornerStone left, CornerStone right) {
            IEnumerable<CubeColor> leftColors = left.GetColors();
            IEnumerable<CubeColor> rightColors = right.GetColors();

            return leftColors.All(c => rightColors.Contains(c));
        }

        public static bool operator !=(CornerStone left, CornerStone right) => !(left == right);
    }
}
