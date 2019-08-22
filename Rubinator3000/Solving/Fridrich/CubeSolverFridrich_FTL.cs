using System;
using System.Collections.Generic;
using System.Linq;
using IStone = Rubinator3000.Cube.IStone;
using Corner = Rubinator3000.Cube.CornerStone;
using Edge = Rubinator3000.Cube.EdgeStone;

namespace Rubinator3000{

    partial class CubeSolverFridrich {
        private void CalcMovesFTL() {
            if (!CheckCross())
                throw new InvalidOperationException();

            // Ecksteine einsetzen

        }

        private (Corner corner, Edge edge)? GetFTLStones() {
            IEnumerable<(Corner, Edge)> stones = from corner in Cube.CornerStones
                                                 from edge in Cube.EdgeStones
                                                 where cube.GetColors(corner).Contains(CubeColor.WHITE) && !cube.InRightPosition(corner)
                                                    && cube.GetColors(corner).SequenceEqual(cube.GetColors(edge)) && !cube.InRightPosition(edge)
                                                 select (corner, edge);

            if (stones.Count() > 0)
                return stones.First();
            else
                return null;
        }
    }
}