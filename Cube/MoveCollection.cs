using Rubinator3000.Solving;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rubinator3000 {
    public class MoveCollection : IEnumerable<Move> {
        private List<Move> moves;

        public MoveCollection() {
            moves = new List<Move>();
        }

        public MoveCollection(IEnumerable<Move> moves) {
            moves = new List<Move>(moves);
        }

        // string constructor
        public MoveCollection(params string[] movesStr) {
            moves = new List<Move>();

            foreach (var str in movesStr)
                if (Move.TryParse(str, out Move move))
                    Add(move);
        }

        public IEnumerator<Move> GetEnumerator() {
            return moves.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return moves.GetEnumerator();
        }

        public void Add(CubeFace face, int count = 1) => Add(new Move(face, count));

        public void Add(Move move) {
            if (moves.Count > 0) {
                Move last = moves.Last();

                // wenn der vorherige Move die gleiche Seite hatte, wird die Anzahl der Vierteldrehugen zum vorherigen Addiert
                if (last.Face == move.Face) {
                    last.Count += move.Count;

                    // sollten sich der neue und der vorherige Move aufheben, so wird der vorherige entfernt
                    if (last.Count == 0) {
                        moves.RemoveAt(moves.Count - 1);
                    }
                }
                else moves.Add(move);
            }
            else moves.Add(move);
        }

        public void AddRange(IEnumerable<Move> moves) {
            foreach (var move in moves) {
                Add(move);
            }
        }

        public void Clear() {
            moves.Clear();
        }

        public override string ToString() {
            IEnumerable<string> moveStrings = moves.Select(m => m.ToString());

            return string.Join(", ", moveStrings);
        }

        public static MoveCollection Parse(string s) {
            MoveCollectionParser parser = new MoveCollectionParser();
            return parser.Parse(s);
        }

        public MoveCollection TransformMoves(CubeOrientation orientation) {            

            MoveCollection newMoves = new MoveCollection();
            for (IEnumerator<Move> e = GetEnumerator(); e.MoveNext(); ) {
                Move m = e.Current;
                newMoves.Add(orientation.TransformFace(m.Face), m.Count);
            }

            return newMoves;
        }
    }    

    public enum OrientationMove { X, Y, Z }

    /// <summary>
    /// Eine Hilfsklasse, um eine MoveCollection zu parsen, die M,E,S oder x,y,z Moves enthält
    /// </summary>
    public class MoveCollectionParser {        
        private CubeFace[] cubeOrientation;
        private static readonly char[] faceMappings = { 'L', 'U', 'F', 'D', 'R', 'B' };
        private static readonly char[] orientationMoveChars = { 'x', 'y', 'z' };

        public MoveCollectionParser() {
            cubeOrientation = new CubeFace[6] { CubeFace.LEFT, CubeFace.UP, CubeFace.FRONT, CubeFace.DOWN, CubeFace.RIGHT, CubeFace.BACK };
        }

        public MoveCollection Parse(string moveString) {
            MoveCollection moves = new MoveCollection();

            for (int i = 0; i < moveString.Length; i++) {
                char moveChar = moveString[i];
                string postfix = new string(moveString.Skip(i + 1).TakeWhile(c => 
                    !faceMappings.Contains(c) && !orientationMoveChars.Contains(c)).ToArray());
                i += postfix.Length;

                // get rotations count ('i' = -1, '2' = 2, '' = 1)
                int count = postfix.EndsWith("i") ? -1 : (postfix.EndsWith("2") ? 2 : 1);

                // normal face move
                if (faceMappings.Contains(moveChar)) {
                    CubeFace face = cubeOrientation[Array.IndexOf(faceMappings, moveChar)];                    

                    // W move
                    if (postfix.StartsWith("w")) {
                        switch (face) {
                            // Lw
                            case CubeFace.LEFT:     ChangeOrientation(OrientationMove.X, -count); break;
                            // Uw
                            case CubeFace.UP:       ChangeOrientation(OrientationMove.Y, count); break;
                            // Fw
                            case CubeFace.FRONT:    ChangeOrientation(OrientationMove.Z, count); break;
                            // Dw
                            case CubeFace.DOWN:     ChangeOrientation(OrientationMove.Y, -count); break;
                            // Rw
                            case CubeFace.RIGHT:    ChangeOrientation(OrientationMove.X, count); break;
                            // Bw
                            case CubeFace.BACK:     ChangeOrientation(OrientationMove.Z, -count); break;
                            default:
                                throw new InvalidProgramException();
                        }
                        face = GetOpponentFace(face);                        
                    }

                    moves.Add(face, count);
                }
                // orientation move
                else if(orientationMoveChars.Contains(moveChar)) {
                    switch (char.ToLower(moveChar)) {
                        case 'x': ChangeOrientation(OrientationMove.X, count); break;
                        case 'y': ChangeOrientation(OrientationMove.Y, count); break;
                        case 'z': ChangeOrientation(OrientationMove.Z, count); break;
                        default: throw new InvalidProgramException();
                    }
                }
                else {
                    throw new FormatException($"Cannot parse orientation move \'{moveChar + postfix}\'");
                }
            }

            return moves;
        }

        private CubeFace GetOpponentFace(CubeFace face) {
            switch (face) {
                case CubeFace.LEFT:
                    return CubeFace.RIGHT;
                case CubeFace.UP:
                    return CubeFace.DOWN;
                case CubeFace.FRONT:
                    return CubeFace.BACK;
                case CubeFace.DOWN:
                    return CubeFace.UP;
                case CubeFace.RIGHT:
                    return CubeFace.LEFT;
                case CubeFace.BACK:
                    return CubeFace.FRONT;
                default:
                    throw new ArgumentOutOfRangeException(nameof(face));
            }
        }

        private void ChangeOrientation(OrientationMove move, int count = 1) {
            count = SolvingUtility.NormalizeCount(count);

            switch (move) {
                case OrientationMove.X:
                    // x
                    for(int i = 0; i < count; i++) {
                        CubeFace tmp = cubeOrientation[(int)CubeFace.FRONT];
                        cubeOrientation[(int)CubeFace.FRONT] = cubeOrientation[(int)CubeFace.DOWN];
                        cubeOrientation[(int)CubeFace.DOWN] = cubeOrientation[(int)CubeFace.BACK];
                        cubeOrientation[(int)CubeFace.BACK] = cubeOrientation[(int)CubeFace.UP];
                        cubeOrientation[(int)CubeFace.UP] = tmp;
                    }
                    break;
                case OrientationMove.Y:
                    // y
                    for (int i = 0; i < count; i++) {
                        CubeFace tmp = cubeOrientation[(int)CubeFace.FRONT];
                        cubeOrientation[(int)CubeFace.FRONT] = cubeOrientation[(int)CubeFace.RIGHT];
                        cubeOrientation[(int)CubeFace.RIGHT] = cubeOrientation[(int)CubeFace.BACK];
                        cubeOrientation[(int)CubeFace.BACK] = cubeOrientation[(int)CubeFace.LEFT];
                        cubeOrientation[(int)CubeFace.LEFT] = tmp;
                    }
                    break;
                case OrientationMove.Z:
                    // z
                    for (int i = 0; i < count; i++) {
                        CubeFace tmp = cubeOrientation[(int)CubeFace.LEFT];
                        cubeOrientation[(int)CubeFace.LEFT] = cubeOrientation[(int)CubeFace.DOWN];
                        cubeOrientation[(int)CubeFace.DOWN] = cubeOrientation[(int)CubeFace.RIGHT];
                        cubeOrientation[(int)CubeFace.RIGHT] = cubeOrientation[(int)CubeFace.UP];
                        cubeOrientation[(int)CubeFace.UP] = tmp;
                    }
                    break;
            }
        }
    }    
}