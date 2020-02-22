using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Rubinator3000;
using Rubinator3000.CubeScan;
using Rubinator3000.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Rubinator3000.CubeScan {

    class CubeScanFrame {

        #region Properties

        // Image for edge-detection, for finding tiles on input-frame
        public Image<Bgr, byte> Original { get; set; }

        // Stores tile contours
        public List<Contour> TileContours { get; set; }

        public bool Initialized { get; private set; } = false;

        #endregion Properties

        private readonly FastAccessBitmap fastAccessBitmap;

        public CubeScanFrame(Image<Bgr, byte> image = null) {

            fastAccessBitmap = new FastAccessBitmap();
            Reinitialize(image);
        }

        public void Reinitialize(Image<Bgr, byte> image) {

            if (image != null) {
                Original = image;
                TileContours = new List<Contour>();
                FindTiles();
                Initialized = true;
            }
        }

        public List<Contour> FindTiles(double percentage = 0.04) {

            // This method finds all tiles and stores their contours in "tileContours"

            List<Contour> contours = new List<Contour>();
            Image<Gray, byte> edged = Canny();

            // Apply GaussianBlur, gray-scale and binary on image
            Image<Gray, byte> temp = edged.SmoothGaussian(5).Convert<Gray, byte>().ThresholdBinary(new Gray(30), new Gray(255));

            VectorOfVectorOfPoint allContours = new VectorOfVectorOfPoint();
            Mat m = new Mat();

            // Find all contours on temp Image and store them in allContours
            CvInvoke.FindContours(temp, allContours, m, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            for (int i = allContours.Size - 1; i >= 0; i--) {

                double perimeter = CvInvoke.ArcLength(allContours[i], true);
                VectorOfPoint approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(allContours[i], approx, percentage * perimeter, true);

                if (CvInvoke.ContourArea(allContours[i]) > Settings.MinimalContourArea && ReadUtility.IsInBound(perimeter, Settings.MinimalContourLength, Settings.MaximalContourLength) && ReadUtility.IsInBound(approx.Size, 4, 8)) {
                    contours.Add(new Contour(allContours[i]));
                }
            }

            return contours;
        }

        public Image<Gray, byte> Canny() {

            if (Original != null) {

                // Apply Canny-EdgeDetection-Filter
                return Original.Canny(Settings.CannyThresh, Settings.CannyThreshLinking);
            }
            return null;
        }

        public Color ReadColorAtClosestContour(double relativeX, double relativeY) {

            Contour contour = FindClosestContour(relativeX, relativeY);
            return ColorInsideContourEmgu(contour);
        }

        private Contour FindClosestContour(double relativeX, double relativeY) {

            double smallestDistance = double.MaxValue;
            Contour closestContour = null;

            for (int i = 0; i < TileContours.Count; i++) {

                double deltaX = Math.Abs(relativeX - TileContours[i].RelativeCenterX);
                double deltaY = Math.Abs(relativeY - TileContours[i].RelativeCenterY);

                double distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
                if (distance < smallestDistance) {

                    smallestDistance = distance;
                    closestContour = TileContours[i];
                }
            }

            return closestContour;
        }

        private Color ColorInsideContourEmgu(Contour contour) {

            // Retrieve all point-coordinates inside contour
            List<Point> pointsInside = ReadUtility.PointsInContour(contour);
            int pointCount = pointsInside.Count;

            int sumBlue = 0, sumGreen = 0, sumRed = 0;

            for (int i = 0; i < pointCount; i++) {

                int x = pointsInside[i].X;
                int y = pointsInside[i].Y;
                sumBlue += Original.Data[y, x, 0];
                sumGreen += Original.Data[y, x, 1];
                sumRed += Original.Data[y, x, 2];
            }

            // avgs[avgBlue, avgGreen, avgRed]
            int[] avgs = new int[3] { Convert.ToInt32(sumBlue / pointCount), Convert.ToInt32(sumGreen / pointCount), Convert.ToInt32(sumRed / pointCount) };

            int maxValue = avgs.Max();
            int maxIndex = avgs.ToList().IndexOf(maxValue);

            // Give higher values more weight
            for (int i = 0; i < avgs.Length; i++) {
                if (maxIndex != 2) {

                    avgs[i] = Convert.ToInt32(Math.Pow(avgs[i], 1.7));
                }
            }

            double divident = avgs[maxIndex] / (double)255;

            for (int i = 0; i < avgs.Length; i++) {

                avgs[i] = Convert.ToInt32(avgs[i] / divident);
            }

            return Color.FromArgb(Convert.ToByte(avgs[2]), Convert.ToByte(avgs[1]), Convert.ToByte(avgs[0]));
        }

        private System.Windows.Media.Color ColorInsideContourSysDraw(Contour contour) {

            // Retrieve all point-coordinates inside contour
            List<Point> pointsInside = ReadUtility.PointsInContour(contour);
            int pointCount = pointsInside.Count;

            int sumBlue = 0, sumGreen = 0, sumRed = 0;

            for (int i = 0; i < pointCount; i++) {

                int x = pointsInside[i].X;
                int y = pointsInside[i].Y;
                Color c = fastAccessBitmap.ReadPixels(x, y, 1, 1);
                sumBlue += c.B;
                sumGreen += c.G;
                sumRed += c.R;
            }

            // avgs[avgBlue, avgGreen, avgRed]
            int[] avgs = new int[3] { Convert.ToInt32(sumBlue / pointCount), Convert.ToInt32(sumGreen / pointCount), Convert.ToInt32(sumRed / pointCount) };

            int maxValue = avgs.Max();
            int maxIndex = avgs.ToList().IndexOf(maxValue);
            double log = Math.Log(255, maxValue);

            // Give higher values more weight
            for (int i = 0; i < avgs.Length; i++) {

                avgs[i] = Convert.ToInt32(Math.Pow(avgs[i], log));
            }

            return System.Windows.Media.Color.FromArgb(255, Convert.ToByte(avgs[2]), Convert.ToByte(avgs[1]), Convert.ToByte(avgs[0]));
        }

        public VectorOfVectorOfPoint GenerateVectorOfVectorOfPoint() {

            VectorOfVectorOfPoint vovop = new VectorOfVectorOfPoint(TileContours.Count);
            Point[][] points = vovop.ToArrayOfArray();
            for (int i = 0; i < TileContours.Count; i++) {

                points[i] = TileContours[i].PointsModified;
            }

            return new VectorOfVectorOfPoint(points);
        }

    }
}
