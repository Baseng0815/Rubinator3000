using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000{
    public partial class CubeSolverFridrich : CubeSolver {

        public CubeSolverFridrich(Cube cube) : base(cube) { }

        public override void CalcMoves() {
            moves = new MoveCollection();

            //CalcCrossMoves();
        }
    }
}
