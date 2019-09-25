#pragma warning disable IDE0039
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rubinator3000.Solving.CrossSolvingUtility;

namespace Rubinator3000.Solving {
    using static CubeFace;
    using static CubeColor;

    partial class CubeSolverFridrich {

        partial void CalcCrossMoves() {

            IEnumerable<Position> whiteEdgePositions = EdgePositions.Where(p => cube.At(p) == WHITE);

            // die Steine in die richtige Orientierung bringen
            Func<Position, bool> predicate = (pos) => (pos.Tile == 1 || pos.Tile == 7) && MiddleLayerFaces.Contains(pos.Face);

            while (whiteEdgePositions.Any(predicate)) {
                // die Steine auf jeder Seite zählen, die sich in der falschen Orientierung befinden
                IEnumerable<(CubeFace face, int count)> whiteFacesCount = from face in MiddleLayerFaces
                                                                          select (face, whiteEdgePositions.Count(predicate));

                // Steine auf der Seite mit den meißten falschen Steinen richtig orientieren
                CubeFace faceToRot = whiteFacesCount.First(f => f.count == whiteFacesCount.Select(wf => wf.count).Max()).face;

                // die linke Seite drehen, um den richtig orientierten weißen Kantenstein nicht falsch zu orientieren
                if (cube.At(faceToRot, 3) == WHITE) {
                    CubeFace leftFace = MiddleLayerFaces[((int)faceToRot + 3) % 4];

                    while (cube.At(faceToRot, 3) == WHITE)
                        DoMove(leftFace);
                }

                // die rechte Seite drehen, um den richtig orientierten weißen Kantenstein nicht falsch zu orientieren
                if (cube.At(faceToRot, 5) == WHITE) {
                    CubeFace rightFace = MiddleLayerFaces[((int)faceToRot + 1) % 4];

                    while (cube.At(faceToRot, 5) == WHITE)
                        DoMove(faceToRot, 5);
                }

                // die vordere Seite drehen, um die Steine richtig zu orientieren
                DoMove(faceToRot);

                // die weißen Kantensteine richtig auf die weiße Ebene bringen
                // die Drehung der weißen Seite bestimmen, in der die meisten Kantensteine stimmen
                int delta = GetOrientationWhiteFace(in cube);

                // die weiße Seite in die richtige Position bringen
                DoMove(UP, delta);

                // falsche weißen Kantensteine auf der weißen Seite tauschen
                SwapWhiteEdges(cube, this);
            }
        }
    }
}
