using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CubeLibrary.CubeScan {
    class Helper {

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis() {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        public static System.Windows.Media.SolidColorBrush ColorBrush(CubeColor cc) {

            switch (cc) {

                case CubeColor.ORANGE: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                case CubeColor.WHITE: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                case CubeColor.GREEN: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                case CubeColor.YELLOW: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Yellow);
                case CubeColor.RED: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                case CubeColor.BLUE: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
                default: return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

    }
}
