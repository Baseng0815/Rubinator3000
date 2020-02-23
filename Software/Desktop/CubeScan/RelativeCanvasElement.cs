using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Rubinator3000.CubeScan {
    public abstract class RelativeCanvasElement {

        public abstract UIElement GenerateUIElement(double actualCanvasWidth, double actualCanvasHeight);
    }
}
