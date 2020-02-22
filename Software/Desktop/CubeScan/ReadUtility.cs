using RubinatorCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Rubinator3000.CubeScan {
    public class ReadUtility {

        public enum ReadoutRequested : int {
            DISABLED = 0,
            SINGLE_READOUT = 1,
            AUTO_READOUT = 2
        }

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis() {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        public static System.Windows.Media.SolidColorBrush ColorBrush(CubeColor cubeColor) {

            switch (cubeColor) {

                case CubeColor.ORANGE: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                case CubeColor.WHITE: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                case CubeColor.GREEN: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                case CubeColor.YELLOW: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Yellow);
                case CubeColor.RED: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                case CubeColor.BLUE: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
                default: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        public static bool IsInBound(double value, double bottom, double top) {

            return value >= bottom && value <= top;
        }

        public static bool IsPointInTriangle(Point p, Point a, Point b, Point c) {

            System.Windows.Vector ab = new System.Windows.Vector(b.X - a.X, b.Y - a.Y);
            System.Windows.Vector ac = new System.Windows.Vector(c.X - a.X, c.Y - a.Y);
            double u = (ac.Y * (p.X - a.X) - ac.X * (p.Y - a.Y)) / (ab.X * ac.Y - ac.X * ab.Y);
            double v = (-ab.Y * (p.X - a.X) + ab.X * (p.Y - a.Y)) / (ab.X * ac.Y - ac.X * ab.Y);

            if (IsInBound(u, 0, 1) && IsInBound(v, 0, 1)) {

                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates all points-coordinates of current image, that are inside a specific contour
        /// </summary>
        /// <param name="contour">Contour to check</param>
        /// <returns>All points that are in contour</returns>
        public static List<Point> PointsInContour(Contour contour) {

            // Calculate the gravity-center of contour
            VectorOfPoint vopContour = contour.ToVectorOfPoint();
            var moments = CvInvoke.Moments(vopContour);
            Point center = new Point(Convert.ToInt32(moments.M10 / moments.M00), Convert.ToInt32(moments.M01 / moments.M00));

            // Calculate min- and max-values for points ("bounds to check")
            int minX, minY, maxX, maxY;
            minX = new List<Point>(contour.PointsModified).OrderBy(p => p.X).First().X;
            maxX = new List<Point>(contour.PointsModified).OrderBy(p => p.X).Reverse().First().X;
            minY = new List<Point>(contour.PointsModified).OrderBy(p => p.Y).First().Y;
            maxY = new List<Point>(contour.PointsModified).OrderBy(p => p.Y).Reverse().First().Y;

            List<Point> pointsInside = new List<Point>();

            // Calculate for all points inside the "bounds to check", if they are inside the contour
            for (int i = 0; i < contour.PointsModified.Length - 1; i++) {

                Point point1 = contour.PointsModified[i];
                Point point2 = contour.PointsModified[i + 1];

                for (int x = minX; x <= maxX; x++) {

                    for (int y = minY; y <= maxY; y++) {

                        Point pointToCheck = new Point(x, y);
                        if (ReadUtility.IsPointInTriangle(pointToCheck, center, point1, point2)) {

                            pointsInside.Add(pointToCheck);
                        }
                    }
                }
            }

            return pointsInside;
        }
    }
}
