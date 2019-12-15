using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000
{
    class Utility
    {
        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        public static float ToRad(float deg)
        {
            return (float)(Math.PI * deg / 180f);
        }
        /// <summary>
        /// Convert degrees to radians for a whole vector
        /// </summary>
        public static Vector3 ToRad(Vector3 vec)
        {
            float x = vec.X * (float)Math.PI / 180f;
            float y = vec.Y * (float)Math.PI / 180f;
            float z = vec.Z * (float)Math.PI / 180f;

            return new Vector3(x, y, z);
        }        

        /// <summary>
        /// Convert radians to degrees
        /// </summary>
        public static double ToDeg(double rad)
        {
            return Math.PI / 180f * rad;
        }

        /// <summary>
        /// Cosine of angle, given in degrees
        /// </summary>
        public static float Cos(float angle)
        {
            return (float)Math.Cos(ToRad(angle));
        }

        /// <summary>
        /// Sine of angle, given in degrees
        /// </summary>
        public static float Sin(float angle)
        {
            return (float)Math.Sin(ToRad(angle));
        }
    }
}
