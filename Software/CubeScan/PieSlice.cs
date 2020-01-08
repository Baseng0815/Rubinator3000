using System.Windows.Media;

namespace Rubinator3000.CubeScan {

    class PieSlice {

        public string Name { get; set; }

        public double Value { get; set; }

        public SolidColorBrush ColorBrush { get; set; }

        public PieSlice(string name, double value, SolidColorBrush colorBrush) {

            Name = name;
            Value = value;
            ColorBrush = colorBrush;
        }
    }
}
