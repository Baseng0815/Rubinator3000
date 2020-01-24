using System;

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

        public override string ToString() {
            return Enum.GetName(typeof(CubeFace), Face)[0] + Tile.ToString();
        }

        public override int GetHashCode() {
            return ((int)Face) ^ Tile;
        }
    }

    public struct CubeOrientation {
        private readonly CubeFace[] orientation;

        public CubeOrientation(CubeFace front, CubeFace up) {
            CubeFace left = GetLeftFace(front, up);
            orientation = new CubeFace[6] {
               left , up, front, Cube.GetOpponentFace(up), Cube.GetOpponentFace(left), Cube.GetOpponentFace(front)
            };

        }

        private static CubeFace GetLeftFace(CubeFace front, CubeFace up) {
            int[] faces = neightbourFaces[(int)front];
            return (CubeFace)faces[(int)up % 4];
        }

        private static readonly int[][] neightbourFaces = new int[6][] {
            new int[6] { -1, 5, 1, 2, -1, 3 },
            new int[6] { 2, -1, 4, -1, 5, 0 },
            new int[6] { 3, 0, -1, 4, 1, -1 },
            new int[6] { 5, -1, 0, -1, 2, 4 },
            new int[6] { -1, 2, 3, 5, -1, 1 },
            new int[6] { 1, 4, -1, 0, 3, -1 }
        };

        public CubeFace TransformFace(CubeFace face) {
            return orientation[(int)face];
        }
    }
}