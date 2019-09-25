using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;
using static Rubinator3000.Solving.CubeSolver;

namespace Rubinator3000.Solving {

    public abstract class CrossSolvingUtility {
        public static void SwapWhiteEdges(Cube cube, CubeSolver solver) {
            IEnumerable<EdgeStone> whiteEdgesUpFace = cube.Edges.Where(e => e.HasColor(WHITE) && e.GetColorPosition(WHITE).Face == UP);
            EdgeStone pivotEdge = whiteEdgesUpFace.First(e => e.InRightPosition());

            int delta;

            foreach (var edge in whiteEdgesUpFace) {
                if (EdgeInRightPosition(edge, pivotEdge))
                    continue;
            }
        }

        public static int GetOrientationWhiteFace(in Cube cube) {
            int[] count = new int[4];
            IEnumerable<(Position, Position)> upLayerEdges = new List<(Position, Position)>() {
                ((UP, 1), (BACK, 1)), ((UP, 3), (LEFT, 1)), ((UP, 5), (RIGHT, 1)), ((UP, 7), (FRONT, 1))
            };

            Cube c = (Cube)cube.Clone();
            for (int i = 0; i < 4; i++) {
                count[i] = upLayerEdges.Count(e => c.At(e.Item1) == WHITE && c.At(e.Item2) == Cube.GetFaceColor(e.Item2.Face));
                c.DoMove(UP);
            }

            int maxCount = count.Max();
            return Array.IndexOf(count, maxCount);
        }

        public static int GetDelta(CubeColor secondColor, CubeFace face) {
            int colorIndex = Array.IndexOf(MiddleLayerFaces, Cube.GetFace(secondColor));
            int faceIndex = Array.IndexOf(MiddleLayerFaces, face);

            return (colorIndex - faceIndex + 4) % 4;
        }

        public static void RotateWhiteFace(Cube c, ref int rotation, int count = 1) {
            for (int i = 0; i < count; i++)
                c.DoMove(UP);

            rotation += count;
            rotation %= 4;
        }

        private static bool EdgeInRightPosition(EdgeStone edge, EdgeStone pivotEdge) {
            CubeColor secndColor = edge.GetColors().First(e => e != WHITE);

            int realDelta = GetDelta(secndColor, edge.GetColorPosition(secndColor).Face);
            throw new NotImplementedException();
        }
    }
}
