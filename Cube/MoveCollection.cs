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
    }
}
