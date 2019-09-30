﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rubinator3000 {

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

#if DEBUG
        public event Arduino.MoveEventHandler OnMoveDone;
#endif

        private void RotateSide(CubeSide side, bool isPrime) {
            CubeMatrix matrix = data[(int)side.Face];
            matrix.Rotate(isPrime);

            ISubmatrix tmp;
            ISubmatrix submatrix;
            CubeMatrix currentMatrix;
            (int face, int index, bool column, int direction) current, next;
            int nextIndex;            

            if (isPrime) {
                tmp = side.GetSubmatrix(data[side.Submatices[0].Face], 0);

                for (int i = 0; i < 4; i++) {
                    nextIndex = (i + 1) % 4;

                    current = side.Submatices[i];
                    next = side.Submatices[nextIndex];

                    // get the next submatrix                
                    submatrix = i == 3 ? tmp : side.GetSubmatrix(data[next.face], nextIndex);

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
            else {
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
        }

        public void DoMoves(IEnumerable<Move> moves) {
            foreach (var move in moves) {
                DoMove(move);
                
            }
        }
        public void DoMove(CubeFace face, int count = 1) => DoMove(new Move(face, count));

        /// <summary>
        /// does a move
        /// </summary>
        public void DoMove(Move move) {
            CubeSide side = sides.First(e => e.Face == move.Face);

            for (int c = 0; c < move.Count; c++) {
                RotateSide(side, move.IsPrime);

                if (this.isRenderCube) {
                    DrawCube.AddMove(this, move, 1000);
                }

#if DEBUG
                OnMoveDone?.Invoke(this, new MoveEventArgs(move));
#endif
            }

            Log.LogStuff("Move done: " + move.ToString());            
        }
    }

}