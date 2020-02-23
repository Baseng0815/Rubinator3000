using System.Windows;

namespace Rubinator3000.CubeScan.RelativeElements {
    public abstract class RelativeCanvasElement {

        public abstract UIElement GenerateUIElement(double actualCanvasWidth, double actualCanvasHeight);
    }
}
