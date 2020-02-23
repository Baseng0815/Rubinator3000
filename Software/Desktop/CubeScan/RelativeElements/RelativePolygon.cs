using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Rubinator3000.CubeScan.RelativeElements {
    public class RelativePolygon : RelativeCanvasElement {

        public List<RelativePosition> RelativePoints { get; set; }

        public Color StrokeColor { get; set; }

        public int HighlightThickness { get; set; }

        public RelativePolygon(List<RelativePosition> relativePoints, Color strokeColor, int highlightThickness) {

            RelativePoints = relativePoints;
            StrokeColor = strokeColor;
            HighlightThickness = highlightThickness;
        }

        public Polygon GeneratePolygon(double actualCanvasWidth, double actualCanvasHeight) {

            PointCollection points = new PointCollection();
            for (int i = 0; i < RelativePoints.Count; i++) {

                points.Add(new Point(RelativePoints[i].RelativeX * actualCanvasWidth, RelativePoints[i].RelativeY * actualCanvasHeight));
            }

            Polygon polygon = new Polygon {
                Points = points,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = HighlightThickness
            };
            Canvas.SetLeft(polygon, 0);
            Canvas.SetTop(polygon, 0);

            return polygon;
        }

        public override UIElement GenerateUIElement(double actualCanvasWidth, double actualCanvasHeight) {

            return GeneratePolygon(actualCanvasWidth, actualCanvasHeight);
        }

        public override object Clone() {

            List<RelativePosition> clonedRelativePoints = new List<RelativePosition>();
            for (int i = 0; i < RelativePoints.Count; i++) {

                clonedRelativePoints.Add((RelativePosition)RelativePoints[i].Clone());
            }
            return new RelativePolygon(clonedRelativePoints, StrokeColor, HighlightThickness);
        }
    }
}
