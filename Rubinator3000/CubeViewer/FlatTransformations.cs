﻿using OpenTK;
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
                new Vector3(4, 1, 0),
                new Vector3(3, 0, 0),
                new Vector3(3, 1, 0),
                new Vector3(3, 2, 0),
                new Vector3(2, 1, 0),
                new Vector3(1, 1, 0)
            };

            Transformations = new TRSTransformation[6, 9];
            for (CubeFace face = 0; face < CubeFace.NUMBER_FACES; face++)
                for (int tile = 0; tile < 9; tile++)
                    Transformations[(int)face, tile].Position = faceOffsets[(int)face] + new Vector3(tile / 3, tile % 3, 0);
        }
    }
}
