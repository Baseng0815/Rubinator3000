using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.Solving {
    using static CubeFace;
    using static CubeColor;

    partial class CubeSolverFridrich {

        partial void CalcCrossMoves() {
            IEnumerable<EdgeStone> whiteStones = CrossSolver.GetWhiteEdges(cube);

            int rotation = Cross_CountAndSortStones(whiteStones);

            // sort white edges

            while (whiteStones.Any(e => !e.InRightPosition())) {
                EdgeStone edge = whiteStones.First(e => !e.InRightPosition());
            }
        }

        int Cross_CountAndSortStones(IEnumerable<EdgeStone> stones) {
            int[] stonesCount = new int[4];
            for (int i = 0; i < 4; i++) {
                stonesCount[i] = stones.Count(s => s.InRightPosition());

                DoMove(UP, addMove: false);
            }

            int maxCount = stonesCount.Max();
            int delta = Array.IndexOf(stonesCount, maxCount);

            int rotation = 0;
            RotWhiteFace(ref rotation, delta);
            rotation = 0;

            // sort stones
            while (stones.Any(s => !s.InRightPosition() && s.GetColorPosition(WHITE).Face == UP)) {

            }
        }

        void RotWhiteFace(ref int rotation, int count = 1, bool addMove = true) {
            DoMove(UP, count, addMove);
            rotation += count;

            if (rotation > 3)
                rotation %= 4;
        }

        private abstract class CrossSolver {
            public static IEnumerable<EdgeStone> GetWhiteEdges(Cube cube) {
                var stones = from edgeStone in Cube.EdgeStonePositions
                             select EdgeStone.FromPosition(cube, edgeStone.Item1);

                return stones.Where(s => s.HasColor(WHITE));
            }
        }
    }
}
