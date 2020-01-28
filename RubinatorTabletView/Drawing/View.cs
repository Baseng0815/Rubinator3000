using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace RubinatorTabletView
{
    public class View
    {
        public Matrix4 ViewMatrix;
        public Matrix4 ProjectionMatrix;

        private float width, height, fov;


        private void recalculateProjectionMatrix()
        {
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Utility.ToRad(fov), width / height, .1f, 100f);
        }

        public View(int width, int height, float fov, float distance)
        {
            ViewMatrix = Matrix4.LookAt(new Vector3(0, 0, distance), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

            this.width = width;
            this.height = height;
            this.fov = fov;

            recalculateProjectionMatrix();
        }

        public void SetSize(float width, float height)
        {
            this.width = width;
            this.height = height;

            this.recalculateProjectionMatrix();
        }

        public void ChangeFov(float dFov)
        {
            this.fov += dFov;
            if (fov < 10) fov = 10;
            else if (fov > 170) fov = 170;

            this.recalculateProjectionMatrix();
        }

        internal interface IOnClickListener {
        }
    }
}
