using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace Rubinator3000
{
    class PlaneTransformations
    {
        // a transformation matrix for each face
        private readonly TRSTransformation[] Transformations;

        /// <summary>
        /// Properly construct transformations for faces of a cuboid
        /// </summary>
        public PlaneTransformations()
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

            for (CubeFace face = 0; face < CubeFace.NUMBER_FACES; face++)
                Transformations[(int)face] = new TRSTransformation(position,
                    faceRotations[(int)face]);
        }

        public TRSTransformation this[int i]
        {
            get { return this.Transformations[i]; }

        }
    }
}
