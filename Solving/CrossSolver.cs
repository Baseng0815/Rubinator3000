using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.CubeFace;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {

    public class CrossSolver : CubeSolver {
        protected EdgeStone pivotStone;
		protected readonly IEnumerable<EdgeStone> whiteEdges;		

        public CrossSolver(Cube cube) : base(cube) {
			whiteEdges = cube.Edges.Where(e => e.HasColor(WHITE));			
        }

        public override bool Solved {
			get {
				for (int t = 1; t < 9; t += 2) {
					if(cube.At(UP, t) != WHITE)
						return false;
				}

				return MiddleLayerFaces.All(f => cube.At(f, 1) == Cube.GetFaceColor(f));
			}
		}

        protected override void CalcMoves() {			
            // check white cross solved
			if (Solved) 
				return;

			// check if any white edge is on white face
			if (whiteEdges.Any(e => e.GetColorPosition(WHITE).Face == UP)) {
				// select pivot stone
				int orientation = GetWhiteFaceOrientation();

				DoMove(UP, orientation);
				pivotStone = whiteEdges.First(e => e.InRightPosition());
			}
			else {

			}
        }		

        protected override bool CheckCube(Cube cube) {
            return true;
        }

		protected int GetWhiteFaceOrientation() {
			int[] count = new int[4];
			for (int i = 0; i < 4; i++) {
				count[i] = whiteEdges.Count(e => e.InRightPosition());
				DoMove(UP, addMove: false);
			}

			int maxCount = count.Max();
			return Array.IndexOf(count , maxCount);
		}

		protected int GetDelta(EdgeStone edge) {
			if (!edge.HasColor(WHITE) || edge.GetColorPosition(WHITE).Face != UP)
				throw new ArgumentException("Der Kantenstein muss eine weiße Fläche haben und diese muss sich auf der weißen Seite befinden");

			CubeColor edgeColor = edge.GetColors().First(c => c != WHITE);
			CubeFace face = edge.GetColorPosition(c => c != WHITE).Face;

			return SolvingUtility.GetDelta(edgeColor, Cube.GetFaceColor(face), UP);
		}

		protected bool IsEdgeRight(EdgeStone edge) {
			if (pivotStone.Colors == null) {
				return edge.InRightPosition();
			}

			if (edge.GetColorPosition(WHITE).Face != UP)
				return false;

			int pivotDelta = GetDelta(pivotStone);
			int stoneDelta = GetDelta(edge);
			
			return pivotDelta == stoneDelta;
		}

		protected int GetStoneRating(EdgeStone edge) {			
			if ()
				return 0;

			Position whitePosition = edge.GetColorPosition(WHITE);
			if (whitePosition.Face == UP) {
				return 1;
			}
			else if (whitePosition.Face == DOWN) {
				return 2;
			}
			else {
				// on middle layer
				if(whitePosition.Tile == 1 || whitePosition.Tile == 7) {
					return 3;
				}
				else return 2;
			}
		}
    }
}
