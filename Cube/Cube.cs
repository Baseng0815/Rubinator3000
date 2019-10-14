using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;

namespace Rubinator3000 {    

    [Serializable]
    public partial class Cube : ICloneable {
        private CubeMatrix[] data = new CubeMatrix[6];
        internal readonly bool isRenderCube;   

        internal CubeMatrix[] Data {
            get => data;
        }            

        public EdgeStone[] Edges { get; }
        public CornerStone[] Corners { get; }

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

            Edges = EdgeStonePositions.Select(p => new EdgeStone(new Tuple<CubeColor, CubeColor>(At(p.Item1), At(p.Item2)), this)).ToArray();            
            Corners = CornerStonePositions.Select(p => new CornerStone(new Tuple<CubeColor, CubeColor, CubeColor>(At(p.Item1), At(p.Item2), At(p.Item3)), this)).ToArray();
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
            if (isRenderCube) {
                DrawCube.AddMove(this);
            }
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

        public IEnumerable<T> GetStonesOnFace<T>(CubeFace face) where T : IStone {
            if(typeof(T) == typeof(EdgeStone)) {
                return Edges.Where(e => e.GetPositions().Any(p => p.Face == face)).Cast<T>();
            }
            else {
                // comming soon ...
                throw new NotImplementedException();
            }
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

        public static CubeFace GetOpponentFace(CubeFace face) {
            switch (face) {
                case CubeFace.LEFT:
                    return RIGHT;
                case CubeFace.UP:
                    return DOWN;
                case CubeFace.FRONT:
                    return BACK;
                case CubeFace.DOWN:
                    return UP;
                case CubeFace.RIGHT:
                    return LEFT;
                case CubeFace.BACK:
                    return FRONT;
                default:
                    return NONE;                
            }
        }

        public static CubeColor GetFaceColor(CubeFace face) {
            return (CubeColor)(int)face;
        }

        public static CubeFace GetFace(CubeColor color) {
            return (CubeFace)(int)color;
        }

        public object Clone() {            
            CubeMatrix[] newData = new CubeMatrix[6];

            for (int i = 0; i < 6; i++) {
                newData[i] = new CubeMatrix();
                for (int x = 0; x < 3; x++) {
                    for (int y = 0; y < 3; y++) {
                        int tile = 3 * x + y;

                        newData[i][tile] = data[i][tile];
                    }
                }
            }

            return new Cube(newData);
        }

        public static readonly Tuple<Position, Position>[] EdgeStonePositions = new Tuple<Position, Position>[] {
            new Tuple<Position, Position>((UP, 1), (BACK, 1)), new Tuple<Position, Position>((UP, 3), (LEFT, 1)), new Tuple<Position, Position>((UP, 5), (RIGHT, 1)), new Tuple<Position, Position>((UP, 7), (FRONT, 1)),

            new Tuple<Position, Position>((LEFT, 3), (BACK, 5)), new Tuple<Position, Position>((FRONT, 3), (LEFT, 5)), new Tuple<Position, Position>((RIGHT, 3), (FRONT, 5)), new Tuple<Position, Position>((BACK, 3), (RIGHT, 5)),

            new Tuple<Position, Position>((DOWN, 1), (FRONT, 7)), new Tuple<Position, Position>((DOWN, 3), (LEFT, 7)), new Tuple<Position, Position>((DOWN, 5), (RIGHT, 7)), new Tuple<Position, Position>((DOWN, 7), (BACK, 7)),
        };

        public static readonly Tuple<Position, Position, Position>[] CornerStonePositions = new Tuple<Position, Position, Position>[] {
            new Tuple<Position, Position, Position>((UP, 0), (LEFT, 0), (BACK, 2)), new Tuple<Position, Position, Position>((UP, 2), (RIGHT, 2), (BACK, 0)),
            new Tuple<Position, Position, Position>((UP, 6), (LEFT, 2), (FRONT, 0)), new Tuple<Position, Position, Position>((UP, 8), (RIGHT, 0), (FRONT, 2)),

            new Tuple<Position, Position, Position>((DOWN, 0), (LEFT, 8), (FRONT, 6)), new Tuple<Position, Position, Position>((DOWN, 2), (RIGHT, 6), (FRONT, 8)),
            new Tuple<Position, Position, Position>((DOWN, 6), (LEFT, 6), (BACK, 8)), new Tuple<Position, Position, Position>((DOWN, 8), (RIGHT, 8), (BACK, 6)),
        };
    }
}
