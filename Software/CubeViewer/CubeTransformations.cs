using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace Rubinator3000
{
    /// <summary>
    /// Transformations used for cubedrawing
    /// </summary>
    public static class CubeTransformations
    {
        // a transformation matrix for each face
        public static readonly TRSTransformation[] Transformations;

        static CubeTransformations()
        {
            // rotations for faces
            Vector3[] faceRotations =
            {
                new Vector3(0, 0, 90),
                new Vector3(0, 0, 0),
                new Vector3(90, 0, 0),
                new Vector3(180, 0, 0),
                new Vector3(0, 0, -90),
                new Vector3(-90, 0, 0),
            };


            // default tile is placed in the "up"-position
            // all other tiles are rotated with the origin as pivot
            Vector3 position = new Vector3(0, .5f, 0);

            Transformations = new TRSTransformation[6];

            for (CubeFace face = 0; (int)face < 6; face++)
                Transformations[(int)face] = new TRSTransformation(position,
                    faceRotations[(int)face]);
        }
    }
}
