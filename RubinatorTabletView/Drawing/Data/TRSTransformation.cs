using OpenTK;
using OpenTK.Graphics.ES31;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubinatorTabletView
{
    /// <summary>
    /// Translation - rotation - scale - transformation
    /// </summary>
    public class TRSTransformation
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        public TRSTransformation(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
        }

        public TRSTransformation(Vector3 position, Vector3 rotation)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = new Vector3(1);
        }

        public TRSTransformation()
        {
            this.Scale = new Vector3(1);
        }

        public Matrix4 GetMatrix()
        {
            // rotation quaternion
            // better than multiplying each euler angle
            Matrix4 mX = Matrix4.CreateRotationX(Utility.ToRad(Rotation.X));
            Matrix4 mY = Matrix4.CreateRotationY(Utility.ToRad(Rotation.Y));
            Matrix4 mZ = Matrix4.CreateRotationZ(Utility.ToRad(Rotation.Z));
            Matrix4 rotation = mX * mY * mZ;

            // only rotations around the origin are required
            // so the rotation will be multiplied last (left-to-right mm (!OpenTK reverse to GLSL!))
            return Matrix4.Scale(Scale) * Matrix4.CreateTranslation(Position) * rotation;
        }
    }
}
