using System.Linq;
using static Rubinator3000.CubeColor;

namespace Rubinator3000.Solving {
    
    internal struct FTLSlot {
        private Tuple<CubeColor, CubeColor> colors;   
        private readonly Cube cube;

        public Slot(Tuple<CubeColor, CubeColor> colors, Cube cube) {
            if(!CubeSolver.MiddleLayerEdgeColors.Any(t => t.ValuesEqual(colors)))
                throw new ArgumentOutOfRangeException();

            this.colors = colors;
            this.cube = cube;
        }

        public Slot(EdgeStone edge, CornerStone corner) {
            Tuple<CubeColor, CubeColor> edgeColors = edge.Colors;
            if(!(corner.HasColor(edgeColors.Item1) && corner.HasColor(Item2) && corner.HasColor(WHITE)))
                throw new ArgumentException();

            this.colors = edgeColors;
            this.cube = edge.GetCube();
        }

        public EdgeStone Edge {
            get {
                return cube.Edges.First(e => e.Colors.ValuesEqual(colors));
            }
        }

        public CornerStone Corner {
            get {
                return cube.Corners.Where(c => c.HasColor(colors.Item1) && c.HasColor(colors.Item2) && c.HasColor(WHITE)).First();
            } 
        }

        public bool IsPaired {
            get => IsPaired();
        }

        private bool IsPaired(out bool edgeRight) {
            // get common edge and corner positions on same face            
            IEnumerable<(Position corner, Position edge)> commonFaces = from ePos in edge.GetPositions()
                                                                        join cPos in corner.GetPositions() on ePos.Face equals cPos.Face
                                                                        select (cPos, ePos);

            edgeRight = false;
            if (commonFaces.Count() == 0)
                return false;

            // check if edge and corner position are side by side 
            if (commonFaces.All(t => {
                int d = Math.Abs(t.edge.Tile - t.corner.Tile);
                return d == 1 || d == 3;
            })) {
                // return if colors are equal on each face
                edgeRight = commonFaces.All(t => cube.At(t.edge) == cube.At(t.corner));
                return true;
            }

            return false;
        }
    }
}