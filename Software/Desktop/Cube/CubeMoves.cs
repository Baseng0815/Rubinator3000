using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;

namespace Rubinator3000 {    

    [DebuggerNonUserCode]
    partial class Cube {        
        private static readonly FaceRelation[][] faceRelations = new FaceRelation[6][];

        protected void RotateFace(CubeFace face) {
            // rotate face
            CubeColor[] newFaceColors = new CubeColor[9];

            for(int t = 0; t < 9; t++) {                
                newFaceColors[2 + 3 * t - 10 * (t / 3)] = data[(int)face][t];
            }
            data[(int)face] = newFaceColors;

            // swap side faces
            // load last destination in buffer to save data
            FaceRelation lastRelation = faceRelations[(int)face][3];
            (int tile, CubeColor color)[] buffer = new (int, CubeColor)[3];
            for(int i = 0;  i < 3; i++) {
                int tile = lastRelation.relation[i].destination; 
                CubeColor color = data[(int)lastRelation.destination][lastRelation.relation[i].destination];
                buffer[i] = (tile, color);
            }
            

            for(int f = 3; f >= 0; f--) {
                FaceRelation relation = faceRelations[(int)face][f];

                for (int i = 0; i < 3; i++) {
                    CubeColor color;
                    if (f == 0) {
                        color = buffer.First(e => e.tile == relation.relation[i].departure).color;
                    }
                    else {
                        color = data[(int)relation.departure][relation.relation[i].departure];
                    }

                    data[(int)relation.destination][relation.relation[i].destination] = color;
                }
            }
        }

        /// <summary>
        /// Execute a collection of moves
        /// </summary>
        /// <param name="moves"></param>
        public void DoMoves(IEnumerable<Move> moves) {
            foreach (var move in moves) {
                DoMove(move);
            }
        }

        /// <summary>
        /// Execute a move
        /// </summary>
        public virtual void DoMove(Move move) {
            for (int c = 0; c < move.CountPositive; c++) {
                RotateFace(move.Face);
            }
        }

        static Cube() {
            faceRelations[(int)LEFT] = new FaceRelation[4] {
                new FaceRelation(UP,    FRONT,  (0, 0), (3, 3), (6, 6)),
                new FaceRelation(FRONT, DOWN,   (0, 0), (3, 3), (6, 6)),
                new FaceRelation(DOWN,  BACK,   (0, 8), (3, 5), (6, 2)),
                new FaceRelation(BACK,  UP,     (2, 6), (5, 3), (8, 0))
            };

            faceRelations[(int)UP] = new FaceRelation[4] {
                new FaceRelation(LEFT,  BACK,   (0, 0), (1, 1), (2, 2)),
                new FaceRelation(BACK,  RIGHT,  (0, 0), (1, 1), (2, 2)),
                new FaceRelation(RIGHT, FRONT,  (0, 0), (1, 1), (2, 2)),
                new FaceRelation(FRONT, LEFT,   (0, 0), (1, 1), (2, 2))
            };

            faceRelations[(int)FRONT] = new FaceRelation[4] {
                new FaceRelation(LEFT,  UP,     (8, 6), (5, 7), (2, 8)),
                new FaceRelation(UP,    RIGHT,  (6, 0), (7, 3), (8, 6)),
                new FaceRelation(RIGHT, DOWN,   (0, 2), (3, 1), (6, 0)),
                new FaceRelation(DOWN,  LEFT,   (2, 8), (1, 5), (0, 2))
            };

            faceRelations[(int)DOWN] = new FaceRelation[4] {
                new FaceRelation(LEFT,  FRONT,  (6, 6), (7, 7), (8, 8)),
                new FaceRelation(FRONT, RIGHT,  (6, 6), (7, 7), (8, 8)),
                new FaceRelation(RIGHT, BACK,   (6, 6), (7, 7), (8, 8)),
                new FaceRelation(BACK,  LEFT,   (6, 6), (7, 7), (8, 8))
            };

            faceRelations[(int)RIGHT] = new FaceRelation[4] {
                new FaceRelation(UP,    BACK,   (2, 6), (5, 3), (8, 0)),
                new FaceRelation(BACK,  DOWN,   (0, 8), (3, 5), (6, 2)),
                new FaceRelation(DOWN,  FRONT,  (2, 2), (5, 5), (8, 8)),
                new FaceRelation(FRONT, UP,     (2, 2), (5, 5), (8, 8))
            };

            faceRelations[(int)BACK] = new FaceRelation[4] {
                new FaceRelation(LEFT,  DOWN,   (0, 6), (3, 7), (6, 8)),
                new FaceRelation(DOWN,  RIGHT,  (6, 8), (7, 5), (8, 2)),
                new FaceRelation(RIGHT, UP,     (2, 0), (5, 1), (8, 2)),
                new FaceRelation(UP,    LEFT,   (0, 6), (1, 3), (2, 0))
            };
        }
    }
}
