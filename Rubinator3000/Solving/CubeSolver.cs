using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000 {
    public abstract class CubeSolver {

        protected Cube cube;
        protected MoveCollection moves;

        public MoveCollection Moves { get => moves; }

        /// <summary>
        /// Erstellt einen neuen CubeSolver und eine Kopie des aktuellen Cubes
        /// </summary>
        /// <param name="cube">Der zu lösende Cube</param>
        public CubeSolver(Cube cube) {            
#if DEBUG
            this.cube = cube;
#else
            this.cube = new Cube();

            for (int face = 0; face < 6; face++) {
                for (int tile = 0; tile < 9; tile++) {
                    this.cube.SetTile((CubeFace)face, tile, cube.At((CubeFace)face, tile));
                }
            }
#endif
        }

        public abstract void CalcMoves();

        protected void DoMove(Move move, int count = 1) {
            cube.DoMove(move, count);
            moves.Add(move, count);
        }

        protected void DoMove(CubeFace face, bool isPrime = false, int count = 1) => DoMove(new Move(face, isPrime), count);
    }
}
