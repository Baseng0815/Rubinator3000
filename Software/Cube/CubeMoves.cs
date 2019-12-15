using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rubinator3000 {
    public class MoveEventArgs : EventArgs {
        public Move Move { get; }

        public MoveEventArgs(Move move) {
            Move = move ?? throw new ArgumentNullException(nameof(move));
        }
    }

    [DebuggerNonUserCode]
    partial class Cube {
        private static readonly IEnumerable<CubeSide> sides = new CubeSide[] {
            new CubeSide(CubeFace.LEFT, (1, 0, true, 1), (2, 0, true, 1), (3, 0, true, 1), (5, 2, true, -1)),
            new CubeSide(CubeFace.UP, (0, 0, false, -1), (5, 0, false, -1), (4, 0, false, -1), (2, 0, false, -1)),
            new CubeSide(CubeFace.FRONT, (0, 2, true, -1), (1, 2, false, 1), (4, 0, true, 1), (3, 0, false, -1)),
            new CubeSide(CubeFace.DOWN, (0, 2, false, 1), (2, 2, false, 1), (4, 2, false, 1), (5, 2, false, 1)),
            new CubeSide(CubeFace.RIGHT, (1, 2, true, -1), (5, 0, true, 1), (3, 2, true, -1), (2, 2, true, -1)),
            new CubeSide(CubeFace.BACK, (0, 0, true, 1), (3, 2, false, 1), (4, 2, true, -1), (1, 0, false, -1))
        };

        public delegate void MoveEventHandler(object sender, MoveEventArgs e);
        public event MoveEventHandler OnMoveDone;

        protected void RotateSide(CubeSide side) {
            CubeMatrix matrix = data[(int)side.Face];
            matrix.Rotate();

            ISubmatrix tmp;
            ISubmatrix submatrix;
            CubeMatrix currentMatrix;
            (int face, int index, bool column, int direction) current, next;
            int nextIndex;

            tmp = side.GetSubmatrix(data[side.Submatices[3].Face], 3);

            for (int i = 4 - 1; i >= 0; i--) {
                nextIndex = (i + 3) % 4;

                current = side.Submatices[i];
                next = side.Submatices[nextIndex];

                // get the next submatrix                
                submatrix = i == 0 ? tmp : side.GetSubmatrix(data[next.face], nextIndex);

                // transform the matrix
                if (current.column ^ next.column)
                    submatrix = submatrix.GetTranspose();

                if (current.direction != next.direction)
                    submatrix = submatrix.GetReverse();

                // set the matrix to the current face
                currentMatrix = data[current.face];
                if (submatrix is RowMatrix)
                    currentMatrix.SetRow(current.index, (RowMatrix)submatrix);
                else if (submatrix is ColumnMatrix)
                    currentMatrix.SetColumn(current.index, (ColumnMatrix)submatrix);
            }
        }

        public void DoMoves(IEnumerable<Move> moves, bool renderMoves = true) {
            foreach (var move in moves) {
                DoMove(move, renderMoves);
            }
        }
        public void DoMove(CubeFace face, int count = 1, bool renderMove = true) => DoMove(new Move(face, count), renderMove);

        /// <summary>
        /// does a move
        /// </summary>
        public virtual void DoMove(Move move, bool renderMove = true) {
            CubeSide side = sides.First(e => e.Face == move.Face);

            for (int c = 0; c < move.Count; c++) {
                RotateSide(side);                                
            }

            OnMoveDone?.Invoke(this, new MoveEventArgs(move));

            if (this.isRenderCube && renderMove) {
                DrawCube.AddMove(this, move, 1000);
            }

            Log.LogStuff("Move done: " + move.ToString());
        }
    }

}
