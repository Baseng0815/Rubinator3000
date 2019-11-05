using System;
using System.Linq;
using static Rubinator3000.CubeColor;
using static Rubinator3000.CubeFace;

namespace Rubinator3000.Solving {
    partial class FTLSolver {
        private static readonly Func<FTLPair, bool> rightPairedCase = p => p.Paired && OnDownLayer(p.Edge);

        private static readonly Func<FTLPair, bool> falsePairedCase = p => p.IsPaired(out bool edgeRight) && !edgeRight && OnDownLayer(p.Edge);

        private static readonly Func<FTLPair, bool> eagleCase = p => {
            if (!(OnDownLayer(p.Corner) && OnDownLayer(p.Edge)))
                return false;

            var pairColors = p.Edge.GetColors();
            Position edgeSidePos = p.Edge.GetPositions().First(pos => pos.Face != DOWN);
            CubeColor edgeSideColor = p.Edge.GetColor(edgeSidePos);
            Position cornerSidePos = p.Corner.GetPositions().First(pos => pos.Face != DOWN && pos != p.CornerWhitePosition);
            CubeColor cornerSideColor = p.Corner.GetColor(cornerSidePos);

            return p.CornerWhitePosition.Face != DOWN                           // weiße Fläche muss zur seite zeigen
                && Cube.IsOpponentFace(edgeSidePos.Face, cornerSidePos.Face)    // der Kantenstein muss auf der entgegengesetzten Seite liegen wie der Eckstein
                && edgeSideColor != cornerSideColor;                            // die seitlichen Farben müssen unterschiedlich sein
        };
    
        private static readonly Func<FTLPair, bool> crocodileCase = p => {

        }
    }
}