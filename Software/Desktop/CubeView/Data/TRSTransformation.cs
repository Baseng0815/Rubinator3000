using OpenTK;

namespace Rubinator3000.CubeView.Data {
    /// <summary>
    /// Translation - rotation - scale - transformation
    /// </summary>
    public class TRSTransformation {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        public TRSTransformation(Vector3 position, Vector3 rotation, Vector3 scale) {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public TRSTransformation(Vector3 position, Vector3 rotation) {
            Position = position;
            Rotation = rotation;
            Scale = new Vector3(1);
        }

        public TRSTransformation() {
            Scale = new Vector3(1);
        }

        public Matrix4 GetMatrix() {
            // rotation quaternion
            // better than multiplying each euler angle
            Quaternion rotationQuat = new Quaternion(Utility.ToRad(Rotation));

            // only rotations around the origin are required
            // so the rotation will be multiplied last (left-to-right mm (!OpenTK reverse to GLSL!))
            return Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Position) * Matrix4.CreateFromQuaternion(rotationQuat);
        }
    }
}
