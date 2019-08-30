using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000 {
    public enum CubeColor : int {
        ORANGE,
        WHITE,
        GREEN,
        YELLOW,
        RED,
        BLUE,

        NUMBER_COLORS,
        NONE = -1
    };

    public enum CubeFace : int {
        LEFT,
        UP,
        FRONT,
        DOWN,
        RIGHT,
        BACK,

        NUMBER_FACES,
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
    }

    [Serializable]
    public partial class Cube {
        private CubeMatrix[] data = new CubeMatrix[6];        

        public Cube() {
            for (int face = 0; face < (int)CubeFace.NUMBER_FACES; face++) {
                data[face] = new CubeMatrix();

                for (int tile = 0; tile < 9; tile++)
                    data[face][tile] = (CubeColor)face;
            }            
        }

        internal Cube(CubeMatrix[] matrices) {
            if (matrices.Length != 6)
                throw new ArgumentOutOfRangeException(nameof(matrices));

            data = matrices;
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
            if (color2 == CubeColor.NUMBER_COLORS) {
                throw new ArgumentException();
            }

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
                    throw new ArgumentException();
            }
        }
    }
}
