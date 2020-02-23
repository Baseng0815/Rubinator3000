using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Rubinator3000.CubeScan {
    public class RelativePolygon : RelativeCanvasElement {

        public List<RelativePosition> RelativePoints { get; set; }

        public Color StrokeColor { get; set; }

        public RelativePolygon(List<RelativePosition> relativePoints, Color strokeColor) {

            RelativePoints = relativePoints;
            StrokeColor = strokeColor;
        }

        public Polygon GeneratePolygon(double actualCanvasWidth, double actualCanvasHeight) {

            double minRelativeX = double.MaxValue, minRelativeY = double.MaxValue;
            PointCollection points = new PointCollection();
            for (int i = 0; i < RelativePoints.Count; i++) {

                if (RelativePoints[i].RelativeX < minRelativeX) {
                    minRelativeX = RelativePoints[i].RelativeX;
                }
                if (RelativePoints[i].RelativeY < minRelativeX) {
                    minRelativeX = RelativePoints[i].RelativeY;
                }
                points.Add(new Point(RelativePoints[i].RelativeX * actualCanvasWidth, RelativePoints[i].RelativeY * actualCanvasHeight));
            }

            Polygon polygon = new Polygon {
                Points = points,
                Stroke = new SolidColorBrush(StrokeColor)
            };

            Canvas.SetLeft(polygon, minRelativeX * actualCanvasWidth);
            Canvas.SetLeft(polygon, minRelativeY * actualCanvasHeight);

            return polygon;
        }

        public override UIElement GenerateUIElement(double actualCanvasWidth, double actualCanvasHeight) {

            return GeneratePolygon(actualCanvasWidth, actualCanvasHeight);
        }
    }
}
