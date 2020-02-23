﻿using Emgu.CV;
using Emgu.CV.Util;
using Rubinator3000.CubeScan.RelativeElements;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Rubinator3000.CubeScan.ColorIdentification {
    public class Contour : ICloneable {

        public Point[] Points { get; private set; }

        public double RelativeCenterX { get; set; }

        public double RelativeCenterY { get; set; }

        private readonly int imageWidth = -1;
        private readonly int imageHeight = -1;

        public Contour(VectorOfPoint vop, int imageWidth, int imageHeight) {

            Points = vop.ToArray();

            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;

            // Calculate RelativeCenter-Coordinates
            var moments = CvInvoke.Moments(vop);
            RelativeCenterX = moments.M10 / moments.M00 / this.imageWidth;
            RelativeCenterY = moments.M01 / moments.M00 / this.imageHeight;
        }

        public VectorOfPoint ToVectorOfPoint() {

            return new VectorOfPoint(Points);
        }

        public RelativePolygon ToRelativeHighlightPolygon() {

            List<RelativePosition> relativePoints = new List<RelativePosition>();
            for (int i = 0; i < Points.Length; i++) {

                relativePoints.Add(new RelativePosition(Points[i].X / (double)imageWidth, Points[i].Y / (double)imageHeight));
            }
            return new RelativePolygon(relativePoints, Settings.HightlightColor, Settings.HightlightThickness);
        }

        public object Clone() {

            return new Contour(ToVectorOfPoint(), imageWidth, imageHeight);
        }
    }
}
