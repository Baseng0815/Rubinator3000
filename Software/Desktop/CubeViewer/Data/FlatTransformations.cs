using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000
{
    /// <summary>
    /// Transformations used for flat drawing
    /// </summary>
    public static class FlatTransformations
    {
        // transformations for each piece ([Face, Tile])
        public static readonly TRSTransformation[,] Transformations;

        static FlatTransformations()
        {
            // 1 ^= a whole face (3 tiles)
            Vector3[] faceOffsets = new Vector3[6]
            {
                new Vector3(-1, 1 / 3f, 0),
                new Vector3(-0.5f, 1, 0),
                new Vector3(-0.5f, 1 / 3f, 0),
                new Vector3(-0.5f, - 1 / 3f, 0),
                new Vector3(0, 1 / 3f, 0),
                new Vector3(0.5f, 1 / 3f, 0)
            };

            Transformations = new TRSTransformation[6, 9];
            for (CubeFace face = 0; (int)face < 6; face++)
                for (int tile = 0; tile < 9; tile++)
                {
                    TRSTransformation transform = new TRSTransformation(faceOffsets[(int)face], new Vector3(0));
                    transform.Position += new Vector3(1 / 6f * (tile % 3), -1 / 4.5f * (tile / 3), 0);
                    transform.Scale = new Vector3(1 / 6f, 1 / 4.5f, 1);
                    Transformations[(int)face, tile] = transform;
                }
        }
    }
}
