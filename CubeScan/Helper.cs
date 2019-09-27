using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Rubinator3000.CubeScan
{
    class Helper
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        public static BitmapSource EmptyBitmapSource(int width, int height)
        {
            var bmptmp = BitmapSource.Create(1, 1, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null, new byte[3] { 0, 0, 0 }, 3);

            return new TransformedBitmap(bmptmp, new System.Windows.Media.ScaleTransform(width, height));
        }

        public static List<Color> ColorsAsList(Color[,,] array)
        {
            List<Color> list = new List<Color>();
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    for (int k = 0; k < array.GetLength(2); k++)
                    {
                        list.Add(array[i, j, k]);
                    }
                }
            }
            return list;
        }

    }
}
