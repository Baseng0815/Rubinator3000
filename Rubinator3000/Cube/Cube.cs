using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;

namespace Rubinator3000 {
    public enum CubeColor : int {
        ORANGE,
        WHITE,
        GREEN,
        YELLOW,
        RED,
        BLUE,
        
        NONE = -1
    };

    public enum CubeFace : int {
        LEFT,
        UP,
        FRONT,
        DOWN,
        RIGHT,
        BACK,
        
        NONE = -1
    };

    /// <summary>
    /// Represent a tile position on a cube
    /// </summary>
    public struct Position {
        public CubeFace Face { get; set; }
        public int Tile { get; set; }

        public Position(CubeFace face, int tile) {
            Face = face;
            Tile = tile;
        }



        public static implicit operator Position((CubeFace, int) tuple) {
            return new Position(tuple.Item1, tuple.Item2);
        }

        public static bool operator ==(Position left, Position right) {
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right) {
            return !(left == right);
        }

        public override bool Equals(object obj) {
            return obj is Position position &&
                   Face == position.Face &&
                   Tile == position.Tile;
        }
    }

    [Serializable]
    public partial class Cube {
        private CubeMatrix[] data = new CubeMatrix[6];
        private readonly bool isRenderCube;   

        internal CubeMatrix[] Data {
            get => data;
        }            

        internal Cube(CubeMatrix[] matrices = null, bool isRenderCube = false) {
            if(matrices == null) {
                data = new CubeMatrix[6];
                for (int face = 0; face < 6; face++)
                    data[face] = new CubeMatrix((CubeColor)face);
            }
            else {           
                if (matrices.Length != 6)
                    throw new ArgumentOutOfRangeException(nameof(matrices));

                data = matrices;
            }

            this.isRenderCube = isRenderCube;
        }
 
        /// <summary>
        /// Set a whole face
        /// </summary>
        public void SetFace(CubeFace face, CubeColor[] colors) {
            for (int tile = 0; tile < 9; tile++)
                data[(int)face][tile] = colors[tile];
        }

        /// <summary>
        /// Set only a tile of one face
        /// </summary>
        public void SetTile(CubeFace face, int tile, CubeColor color) {
            data[(int)face][tile] = color;
        }

        /// <summary>
        /// Randomly shuffle the cube numberOfMoves amount of times
        /// </summary>
        public void Shuffle(int numberOfMoves) {
            Random rand = new Random();
            MoveCollection moves = new MoveCollection();

            while (moves.Count() < numberOfMoves)
                moves.Add(new Move((CubeFace)rand.Next(5), rand.Next(1, 3)));

            Log.LogStuff(string.Format("Shuffle Cube {0} Times: {1}", numberOfMoves, moves.ToString()));
#if DEBUG
            foreach (var move in moves)
                move.Print();
#endif
            DoMoves(moves);
        }

        internal CubeColor At(int face, int tile) {
            return data[face][tile];
        }

        public CubeColor At(CubeFace face, int tile) {
            return data[(int)face][tile];
        }

        public CubeColor At(Position position) {
            return data[(int)position.Face][position.Tile];
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <returns></returns>
        public CubeMatrix[] GetData() {
            return data;
        }

        public static bool IsOpponentColor(CubeColor color1, CubeColor color2) {            

            switch (color1) {
                case CubeColor.ORANGE:
                    return color2 == CubeColor.RED;
                case CubeColor.WHITE:
                    return color2 == CubeColor.YELLOW;
                case CubeColor.GREEN:
                    return color2 == CubeColor.BLUE;
                case CubeColor.YELLOW:
                    return color2 == CubeColor.WHITE;
                case CubeColor.RED:
                    return color2 == CubeColor.ORANGE;
                case CubeColor.BLUE:
                    return color2 == CubeColor.GREEN;
                default:
                    throw new ArgumentException(color1 == CubeColor.NONE ? nameof(color1) : nameof(color2));
            }
        }

        public static CubeColor GetFaceColor(CubeFace face) {
            return (CubeColor)(int)face;
        }

        public static CubeFace GetFace(CubeColor color) {
            return (CubeFace)(int)color;
        }

        public static readonly Tuple<Position, Position>[] EdgeStonePositions = new Tuple<Position, Position>[] {
            new Tuple<Position, Position>((UP, 1), (BACK, 1)), new Tuple<Position, Position>((UP, 3), (LEFT, 1)), new Tuple<Position, Position>((UP, 5), (RIGHT, 1)), new Tuple<Position, Position>((UP, 7), (FRONT, 1)),

            new Tuple<Position, Position>((LEFT, 3), (BACK, 5)), new Tuple<Position, Position>((FRONT, 3), (LEFT, 5)), new Tuple<Position, Position>((RIGHT, 3), (FRONT, 5)), new Tuple<Position, Position>((BACK, 3), (RIGHT, 5)),

            new Tuple<Position, Position>((DOWN, 1), (FRONT, 7)), new Tuple<Position, Position>((DOWN, 3), (LEFT, 7)), new Tuple<Position, Position>((DOWN, 5), (RIGHT, 7)), new Tuple<Position, Position>((DOWN, 7), (BACK, 7)),
        };
    }
}
