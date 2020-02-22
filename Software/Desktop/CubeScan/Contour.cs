using Emgu.CV;
using Emgu.CV.Util;
using Rubinator3000;
using Rubinator3000.CubeScan;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Rubinator3000.CubeScan {
    public class Contour {

        public Point[] Points { get; private set; }

        public double RelativeCenterX { get; set; }

        public double RelativeCenterY { get; set; }

        public Contour(VectorOfPoint vop, int imageWidth = 640, int imageHeight = 480) {

            Points = vop.ToArray();

            // Calculate RelativeCenter-Coordinates
            var moments = CvInvoke.Moments(vop);
            RelativeCenterX = moments.M10 / moments.M00 / imageWidth;
            RelativeCenterY = moments.M01 / moments.M00 / imageHeight;
        }

        public VectorOfPoint ToVectorOfPoint() {

            return new VectorOfPoint(Points);
        }
    }
}
