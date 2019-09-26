using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.CubeScan
{
    class Helper
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        private static double[] percentages;

        public static CubeColor WhichColor(Color color)
        {
            percentages = new double[6];
            percentages[0] = OrangePercentage(color);
            percentages[1] = WhitePercentage(color);
            percentages[2] = GreenPercentage(color);
            percentages[3] = YellowPercentage(color);
            percentages[4] = RedPercentage(color);
            percentages[5] = BluePercentage(color);

            double max = 0;
            int maxIndex = -1;
            
            for (int i = 0; i < percentages.Length; i++)
            {
                if (percentages[i] > max)
                {
                    max = percentages[i];
                    maxIndex = i;
                }
            }

            GC.Collect();

            switch (maxIndex)
            {
                case 0: return CubeColor.ORANGE;
                case 1: return CubeColor.WHITE;
                case 2: return CubeColor.GREEN;
                case 3: return CubeColor.YELLOW;
                case 4: return CubeColor.RED;
                case 5: return CubeColor.BLUE;
                default: return CubeColor.NONE;
            }
        }

        private static double OrangePercentage(Color color)
        {
            double percentageSum = 0;

            // Very high if r is far away from b and r*(11/17) is close to g
            percentageSum += 1 - (color.B / (double)color.R);
            percentageSum += 1 - (color.B / ((double)color.R) * (11/(double)17));

            double percentage = percentageSum / 2;

            return percentage;
        }

        private static double WhitePercentage(Color color)
        {
            double percentageSum = 0;

            // Very high if r, g and b are very close together
            percentageSum += (color.R > color.G) ? color.G / (double)color.R : color.R / (double)color.G;
            percentageSum += (color.R > color.B) ? color.B / (double)color.R : color.R / (double)color.B;
            percentageSum += (color.G > color.B) ? color.B / (double)color.G : color.G / (double)color.B;

            double percentage = percentageSum / 3;

            return percentage;
        }

        private static double GreenPercentage(Color color)
        {
            double percentageSum = 0;

            // Very if difference between "b and g" and "r and g" is very big
            percentageSum += 1 - (color.B / (double)color.G);
            percentageSum += 1 - (color.R / (double)color.G);

            double percentage = percentageSum / 2;

            return percentage;
        }

        private static double YellowPercentage(Color color)
        {
            double percentageSum = 0;

            // Very high when r and g are close together and far away from b
            percentageSum += (color.R > color.G) ? color.G / (double)color.R : color.R / (double)color.G;
            percentageSum += 1 - (color.B / (double)color.R);
            percentageSum += 1 - (color.B / (double)color.G);

            double percentage = percentageSum / 3;

            return percentage;
        }

        private static double RedPercentage(Color color)
        {
            double percentageSum = 0;

            // Very high when r is very far away from g and b
            percentageSum += 1 - (color.G / (double)color.R);
            percentageSum += 1 - (color.B / (double)color.R);

            double percentage = percentageSum / 2;

            return percentage;
        }

        private static double BluePercentage(Color color)
        {
            double percentageSum = 0;

            // Very high when b is far away from r and b
            percentageSum += 1 - (color.R / (double)color.B);
            percentageSum += 1 - (color.G / (double)color.B);

            double percentage = percentageSum / 2;

            return percentage;
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
