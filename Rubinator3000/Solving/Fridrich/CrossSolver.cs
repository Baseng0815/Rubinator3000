#pragma warning disable IDE0039
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

            IEnumerable<Position> whiteEdgePositions = edgePositions.Where(p => cube.At(p) == WHITE);

            // die Steine in die richtige Orientierung bringen
            Func<Position, bool> predicate = (pos) => (pos.Tile == 1 || pos.Tile == 7) && middleLayerFaces.Contains(pos.Face);

            while (whiteEdgePositions.Any(predicate)) {
                // die Steine auf jeder Seite zählen, die sich in der falschen Orientierung befinden
                IEnumerable<(CubeFace face, int count)> whiteFacesCount = from face in middleLayerFaces
                                                                          select (face, whiteEdgePositions.Count(predicate));

                // Steine auf der Seite mit den meißten falschen Steinen richtig orientieren
                CubeFace faceToRot = whiteFacesCount.First(f => f.count == whiteFacesCount.Select(wf => wf.count).Max()).face;

                // die linke Seite drehen, um den richtig orientierten weißen Kantenstein nicht falsch zu orientieren
                if (cube.At(faceToRot, 3) == WHITE) {
                    CubeFace leftFace = middleLayerFaces[((int)faceToRot + 3) % 4];

                    while (cube.At(faceToRot, 3) == WHITE) 
                        DoMove(leftFace);                    
                }

                // die rechte Seite drehen, um den richtig orientierten weißen Kantenstein nicht falsch zu orientieren
                if (cube.At(faceToRot, 5) == WHITE) {
                    CubeFace rightFace = middleLayerFaces[((int)faceToRot + 1) % 4];

                    while (cube.At(faceToRot, 5) == WHITE)
                        DoMove(faceToRot, 5);
                }

                // die vordere Seite drehen, um die Steine richtig zu orientieren
                DoMove(faceToRot);
            }
        }
    }
}
