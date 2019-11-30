using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CubeLibrary.CubeScan {
    static class ColorIdentification {

        // "color" is the rgb color, that should be identified
        public static double[] CalculateColorPercentages(Color color) {

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

            double orangeValue = (color.R * (106 / (double)255));
            percentageSum += orangeValue > color.G ? color.G / orangeValue : orangeValue / color.G;

            double percentage = percentageSum / 2;

            return percentage;
        }

        private static double WhitePercentage(Color color) {

            double percentageSum = 0;

            // Very high if r, g and b are very close together

            percentageSum += color.R > color.G ? (color.G / (double)color.R) : (color.R / (double)color.G);
            percentageSum += color.R > color.B ? (color.B / (double)color.R) : (color.R / (double)color.B);
            percentageSum += color.G > color.B ? (color.B / (double)color.G) : (color.G / (double)color.B);

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

        public static int[] Max8Indicies(CubeColor cubeColor, List<ReadPosition> ctc) {

            if (ctc.Count == 8) {

                return new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            }

            // <index, percentage of cubeColor>
            Dictionary<int, double> probableIndicies = new Dictionary<int, double>();

            for (int i = 0; i < ctc.Count; i++) {

                probableIndicies.Add(i, ctc[i].Percentages[(int)cubeColor]);
            }

            probableIndicies = probableIndicies.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            List<ReadPosition> lrp = ctc;

            return probableIndicies.Keys.ToList().GetRange(probableIndicies.Count - 8, 8).ToArray();
        }   
    }
}
