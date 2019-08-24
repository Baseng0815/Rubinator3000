using OpenTK;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000
{
    public static class CuboidTransformations
    {
        // a transformation matrix for each cuboid
        public static TRSTransformation[] Transformations;

        // map cuboids to tiles
        public static Dictionary<Vector3, Position[]> CuboidMappings;

        // map face to cuboids
        public static Dictionary<CubeFace, int[]> FaceMappings;

        /// <summary>
        /// Properly construct transformations for faces of a cuboid
        /// <para>
        /// Access by [layer(up-down),layer(front-back), layer(left-right)]
        /// </para>
        /// </summary>
        static CuboidTransformations()
        {
            // order from top to bottom
            // start on white 0 going to white[-1]

            Transformations = new TRSTransformation[27];

            CuboidMappings = new Dictionary<Vector3, Position[]>();

            string[] lines = File.ReadAllLines("Resources/CuboidTileMappings.txt").Where(val => !val.Contains("//")).ToArray();
            int lineIndex = 0;

            FaceMappings = new Dictionary<CubeFace, int[]>()
            {
                { CubeFace.LEFT, new int[] { 0, 3, 6, 9, 12, 15, 18, 21, 24 } },
                { CubeFace.UP, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 } },
                { CubeFace.FRONT, new int[] { 0, 1, 2, 9, 10, 11, 18, 19, 20 } },
                { CubeFace.DOWN, new int[] { 18, 19, 20, 21, 22, 23, 24, 25, 26 } },
                { CubeFace.RIGHT, new int[] { 2, 5, 8, 11, 14, 17, 20, 23, 26 } },
                { CubeFace.BACK, new int[] { 6, 7, 8,  15, 16, 17, 24, 25, 26 } },
            };

            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        // mapping
                        string line = lines[lineIndex];
                        List<Position> positions = new List<Position>();

                        while (!line.Contains("end"))
                        {
                            Console.WriteLine(line);
                            string[] split = line.Split(',');

                            Position pos = new Position((CubeFace)int.Parse(split[0]), int.Parse(split[1]));
                            positions.Add(pos);

                            line = lines[++lineIndex];
                        }

                        // x - 1 ... necessary to center cube
                        CuboidMappings.Add(new Vector3(x - 1, 1 - y, 1 - z), positions.ToArray());

                        // transformations
                        Transformations[y * 9 + z * 3 + x] = new TRSTransformation(
                            new Vector3(x - 1, 1 - y, 1 - z),
                            new Vector3(0, 0, 0));

                        Console.WriteLine(x + "  " + y + "  " + z);
                        Console.WriteLine(y * 9 + z * 3 + x);

                        lineIndex++;
                    }
                }
            }
        }
    }
}
