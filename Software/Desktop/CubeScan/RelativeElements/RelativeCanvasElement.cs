using System;
using System.Windows;

namespace Rubinator3000.CubeScan.RelativeElements {

    public abstract class RelativeCanvasElement : ICloneable {

        public UIElement CanvasElement { get; set; }

        public abstract object Clone();

        public abstract UIElement GenerateUIElement(double actualCanvasWidth, double actualCanvasHeight);
    }
}
