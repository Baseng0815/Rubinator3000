using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Rubinator3000.CubeScan {
    static class ColorIdentification {

        public static void ChangeReferenceColor(CubeColor cubeColor, Color newColor) {
            ReferenceColors[(int)cubeColor] = newColor;
        }

        public static Color[] ReferenceColors = new Color[6];

        // "color" is the rgb color, that should be identified
        public static double[] CalculateColorPercentages(Color color) {

            double[] percentages = new double[6];

            if (Settings.UseReferenceColors) {

                for (int i = 0; i < 6; i++) {

                    percentages[i] = ReferencePercentage(i, color);
                }
            }
            else {

                percentages[0] = OrangePercentage(color);
                percentages[1] = WhitePercentage(color);
                percentages[2] = GreenPercentage(color);
                percentages[3] = YellowPercentage(color);
                percentages[4] = RedPercentage(color);
                percentages[5] = BluePercentage(color);
            }

            return percentages;
        }

        private static double ReferencePercentage(int colorIndex, Color color) {

            double percentageSum = 0;

            percentageSum += (double)ReferenceColors[colorIndex].R < color.R ? (double)ReferenceColors[colorIndex].R / color.R : (double)color.R / ReferenceColors[colorIndex].R;
            percentageSum += (double)ReferenceColors[colorIndex].G < color.G ? (double)ReferenceColors[colorIndex].G / color.G : (double)color.G / ReferenceColors[colorIndex].G;
            percentageSum += (double)ReferenceColors[colorIndex].B < color.B ? (double)ReferenceColors[colorIndex].B / color.B : (double)color.B / ReferenceColors[colorIndex].B;

            double percentage = percentageSum / 3;
            return percentage;
        }

        #region Hardcoded Color Percentages

        private static double OrangePercentage(Color color) {

            double percentageSum = 0;

            // Very high if r is far away from b and r*(165/255) is close to g
            percentageSum += 1 - (color.B / (double)color.R);

            // Calculating the ideal green value for an orange-color according to the red value
            double orangeGValue = (color.R * (165 / (double)255));

            percentageSum += orangeGValue > color.G ? color.G / orangeGValue : orangeGValue / color.G;

            double percentage = percentageSum / 2;

            return percentage;
        }

        private static double WhitePercentage(Color color) {

            double percentageSum = 0;

            // Very high if r, g and b are very close together (This would not distinguish between black, grey and white)
            percentageSum += color.R > color.G ? (color.G / (double)color.R) : (color.R / (double)color.G);
            percentageSum += color.R > color.B ? (color.B / (double)color.R) : (color.R / (double)color.B);
            percentageSum += color.G > color.B ? (color.B / (double)color.G) : (color.G / (double)color.B);
            percentageSum += ((color.R + color.G + color.B) / (255 * 3)) * 10;

            double percentage = percentageSum / 4;

            return percentage;
        }

        private static double GreenPercentage(Color color) {

            double percentageSum = 0;

            // Very high if difference between "b and g" and "r and g" is very big
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

        #endregion

        public static int[] Max8Indicies(CubeColor cubeColor, List<ReadPosition> ctc) {

            // cubeColor = max 8 indicies of this color
            // ctc = colors to compare
            if (ctc.Count == 8) {

                return new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            }

            // <index, percentage of cubeColor>
            Dictionary<int, double> probableIndicies = new Dictionary<int, double>();

            for (int i = 0; i < ctc.Count; i++) {

                // write the indicies with the according color-percentage of the "cubeColor" with indicies in a dictionary
                probableIndicies.Add(i, ctc[i].Percentages[(int)cubeColor]);
            }

            // sort by highest percentage (highest percentage is at the highest index)
            probableIndicies = probableIndicies.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // return a list of the last 8 indicies (indicies of the highest percentages)
            return probableIndicies.Keys.ToList().GetRange(probableIndicies.Count - 8, 8).ToArray();
        }

    }
}
