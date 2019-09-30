using System;
using System.Collections.Generic;
using System.Drawing;

namespace Rubinator3000.CubeScan {
    static class ColorIdentification {

        // "color" is the rgb color, that should be identified
        private static double[] CalculateColor(Color color) {

            double[] percentages = new double[6];
            percentages[0] = OrangePercentage(color);
            percentages[1] = WhitePercentage(color);
            percentages[2] = GreenPercentage(color);
            percentages[3] = YellowPercentage(color);
            percentages[4] = RedPercentage(color);
            percentages[5] = BluePercentage(color);

            return percentages;
        }

        private static double OrangePercentage(Color color) {

            double percentageSum = 0;

            // Very high if r is far away from b and r*(11/17) is close to g
            percentageSum += 1 - (color.B / (double)color.R);
            percentageSum += 1 - (color.B / ((double)color.R) * (11 / (double)17));

            double percentage = percentageSum / 2;

            return percentage;
        }

        private static double WhitePercentage(Color color) {

            double percentageSum = 0;

            // Very high if r, g and b are very close together
            percentageSum += 1 - Math.Abs(color.R - color.G);
            percentageSum += 1 - Math.Abs(color.R - color.B);
            percentageSum += 1 - Math.Abs(color.G - color.B);

            double percentage = percentageSum / 3;

            return percentage;
        }

        private static double GreenPercentage(Color color) {

            double percentageSum = 0;

            // Very if difference between "b and g" and "r and g" is very big
            percentageSum += 1 - (color.B / (double)color.G);
            percentageSum += 1 - (color.R / (double)color.G);

            double percentage = percentageSum / 2;

            return percentage;
        }

        private static double YellowPercentage(Color color) {

            double percentageSum = 0;

            // Very high when r and g are close together and far away from b
            percentageSum += (color.R > color.G) ? color.G / (double)color.R : color.R / (double)color.G;
            percentageSum += 1 - (color.B / (double)color.R);
            percentageSum += 1 - (color.B / (double)color.G);

            double percentage = percentageSum / 3;

            return percentage;
        }

        private static double RedPercentage(Color color) {

            double percentageSum = 0;

            // Very high when r is very far away from g and b
            percentageSum += 1 - (color.G / (double)color.R);
            percentageSum += 1 - (color.B / (double)color.R);

            double percentage = percentageSum / 2;

            return percentage;
        }

        private static double BluePercentage(Color color) {

            double percentageSum = 0;

            // Very high when b is far away from r and b
            percentageSum += 1 - (color.R / (double)color.B);
            percentageSum += 1 - (color.G / (double)color.B);

            double percentage = percentageSum / 2;

            return percentage;
        }

        public static int MaxIndex(int cubeColorIndex, List<ReadPosition> colorsToCompare) {
            
            // cubeColorIndex tells, for which cube-color the percentages should be calculated

<<<<<<< HEAD
            if (colorsToCompare.Count == 50) {
                int i = 0;
            }

            int maxIndex = -1;
            double maxValue = double.MinValue;

            double[] pcts = new double[6];
=======
            int max = int.MinValue;
>>>>>>> 785bd1543ec0bcf0ba0f4e69754e7d1ae0bc0d97

            for (int i = 0; i < colorsToCompare.Count; i++) {

                pcts = CalculateColor(colorsToCompare[i].Color);

                if (pcts[cubeColorIndex] > maxValue) {

                    maxIndex = i;
                }
            }

            if (maxIndex == -1) {
                int cci = cubeColorIndex;
                List<ReadPosition> ctc = colorsToCompare;
                double[] ds = pcts;
                return maxIndex;
            }

            return maxIndex;
        }
    }
}
