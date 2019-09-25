using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.Solving {
    public partial class CubeSolverFridrich : CubeSolver {

        public CubeSolverFridrich(Cube cube) : base(cube) {

        }

        public override void CalcMoves() {
            try {
                CalcCrossMoves();

                CalcFTL();

                CalcOLL();

                CalcPLL();

                bool solved = GetCubeSolved();

                if (!solved) {
                    moves.Clear();
                }
            }
            catch (Exception) {
                throw;
            }
        }

        partial void CalcCrossMoves();
        partial void CalcFTL();
        partial void CalcOLL();
        partial void CalcPLL();        
    }
}
