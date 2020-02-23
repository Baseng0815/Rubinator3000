using System;
using System.Windows;

namespace Rubinator3000.CubeScan.RelativeElements {

    public abstract class RelativeCanvasElement : ICloneable {
        public abstract object Clone();

        public abstract UIElement GenerateUIElement(double actualCanvasWidth, double actualCanvasHeight);
    }
}
