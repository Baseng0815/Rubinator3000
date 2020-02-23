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
    public class RelativeCircle : RelativeCanvasElement, ICloneable {

        public RelativePosition RelativePosition { get; set; }

        public int Radius { get; set; }

        public Color FillColor { get; set; }

        public RelativeCircle(RelativePosition relativePosition, int radius, Color color) {

            RelativePosition = relativePosition;
            Radius = radius;
            FillColor = color;
        }

        public Ellipse GenerateCircle(double actualCanvasWidth, double actualCanvasHeight) {

            Ellipse circle = new Ellipse {
                Width = Radius * 2 + 1,
                Height = Radius * 2 + 1,
                Fill = new SolidColorBrush(FillColor)
            };

            Canvas.SetLeft(circle, RelativePosition.RelativeX * actualCanvasWidth - Radius);
            Canvas.SetTop(circle, RelativePosition.RelativeY * actualCanvasHeight - Radius);

            return circle;
        }

        public override UIElement GenerateUIElement(double actualCanvasWidth, double actualCanvasHeight) {

            return GenerateCircle(actualCanvasWidth, actualCanvasHeight);
        }

        public object Clone() {

            return new RelativeCircle((RelativePosition)RelativePosition.Clone(), Radius, FillColor);
        }
    }
}
