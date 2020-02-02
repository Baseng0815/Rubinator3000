using OpenTK;
using OpenTK.Graphics.ES31;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubinatorTabletView {
    /// <summary>
    /// Translation - rotation - scale - transformation
    /// </summary>
    public class TRSTransformation {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        public TRSTransformation(Vector3 position, Vector3 rotation, Vector3 scale) {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
        }

        public TRSTransformation(Vector3 position, Vector3 rotation) {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = new Vector3(1);
        }

        public TRSTransformation() {
            this.Scale = new Vector3(1);
        }

        public Matrix4 GetMatrix() {
            // create Rotationmatrix
            // https://de.wikipedia.org/wiki/Quaternion#Bezug_zu_orthogonalen_Matrizen

            float sx = Utility.Sin(Rotation.X / 2);
            float sy = Utility.Sin(Rotation.Y / 2);
            float sz = Utility.Sin(Rotation.Z / 2);

            float cx = Utility.Cos(Rotation.X / 2);
            float cy = Utility.Cos(Rotation.Y / 2);
            float cz = Utility.Cos(Rotation.Z / 2);

            float a = (sx * sy * sz) - (cx * cy * cz);
            float b = (sx * cy * cz) + (cx * sy * sz);
            float c = (cx * sy * cz) - (sx * cy * sz);
            float d = (cx * cy * sz) + (sx * sy * cz);

            Matrix4 rotation = new Matrix4(
                1 - 2 * (c * c + d * d), 2 * (b * c - a * d), 2 * (b * d + a * c), 0,
                2 * (b * c + a * d), 1 - 2 * (d * d + b * b), 2 * (c * d - a * b), 0,
                2 * (b * d - a * c), 2 * (c * d + a * b), 1 - 2 * (b * b + c * c), 0,
                0, 0, 0, 1);

            // only rotations around the origin are required
            // so the rotation will be multiplied last (left-to-right mm (!OpenTK reverse to GLSL!))
            return Matrix4.Scale(Scale) * Matrix4.CreateTranslation(Position) * rotation;
        }
    }
}
