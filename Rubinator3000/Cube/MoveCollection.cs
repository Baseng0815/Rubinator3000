using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public void Add(CubeFace face, bool isPrime = false, int count = 1) => Add(new Move(face, isPrime), count);

        public void Add(Move move, int count = 1) {
            for (int i = 0; i < count; i++) {
                if (moves.Count > 0) {
                    // optimization by removing unnecessary moves
                    // e.g. Ri R R => R
                    Move lastMove = moves.Last();

                    if (lastMove != move.GetInverted()) {
                        if (move == lastMove)
                            //... [x -3], [x-2], [x-1] + [x]
                            if (moves.Count > 1 && moves[moves.Count - 2] == lastMove) {
                                moves.RemoveRange(moves.Count - 2, 2);
                                moves.Add(move.GetInverted());
                                return;
                            }

                        moves.Add(move);
                    }
                    else moves.RemoveAt(moves.Count - 1);
                }
                else moves.Add(move);
            }
        }

        public void AddRange(IEnumerable<Move> moves) {
            foreach (var move in moves) {
                Add(move);
            }
        }

        public override string ToString() {
            IEnumerable<string> moveStrings = moves.Select(m => m.ToString());

            return string.Join(", ", moveStrings);
        }
    }
}
