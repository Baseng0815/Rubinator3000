using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.Solving {
    public static class SolvingUtility {
        private static readonly CubeColor[][] layerColors = new CubeColor[6][] {
            // LEFT
            new CubeColor[4] { CubeColor.WHITE, CubeColor.BLUE, CubeColor.YELLOW, CubeColor.GREEN },
            // UP
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.GREEN, CubeColor.RED, CubeColor.BLUE },
            // FRONT
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.YELLOW, CubeColor.RED, CubeColor.WHITE },
            // DOWN
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.BLUE, CubeColor.RED, CubeColor.GREEN },
            // RIGHT
            new CubeColor[4] { CubeColor.WHITE, CubeColor.GREEN, CubeColor.YELLOW, CubeColor.BLUE },
            // BACK
            new CubeColor[4] { CubeColor.ORANGE, CubeColor.WHITE, CubeColor.RED, CubeColor.YELLOW }
        };

        public static int NormalizeCount(int count) {
            return NormalizeNumber(count, 0, 3);
        }

        public static int NormalizeCount(int count, int minCount) {
            return NormalizeNumber(count, minCount, minCount + 3);
        }

        public static int NormalizeNumber(int number, int minValue, int maxValue) {
            if (minValue >= maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue), "Die untere Grenze muss kleiner als die obere sein");

            int l = maxValue - minValue + 1;
            while (number < minValue) number += l;
            if (number < 0)
                return -(Math.Abs(number) % l);
            else
                return number % l;
        }

        #region Extension Methodes
        /// <summary>
        /// Gibt den Unterschied der Farben zurück. Der Unterschied ist die Anzahl der Seite im Uhrzeigersinn, um die Farbe auf 
        /// die richtige Seite zu bringen. Er wird durch die Differenz von der Seitenfarbe und der Farbe bestimmt.
        /// </summary>
        /// <param name="color">Die Farbe</param>
        /// <param name="face">Die Seite auf der sich die Farbe befindet</param>
        /// <param name="faceToRot">Die Seite, die gedreht werden soll, um die Farbe auf die richtige Seite zu bringen</param>
        /// <returns></returns>
        public static int GetDelta(this CubeColor color, CubeFace face, CubeFace faceToRot) {
            CubeColor[] colors = layerColors[(int)faceToRot];
            CubeColor faceColor = Cube.GetFaceColor(face);

            if (Cube.IsOpponentColor(faceColor, Cube.GetFaceColor(faceToRot)) || faceColor == Cube.GetFaceColor(faceToRot))
                throw new ArgumentOutOfRangeException();

            if (!colors.Contains(faceColor))
                throw new ArgumentOutOfRangeException(nameof(faceColor));

            if (!colors.Contains(color))
                throw new ArgumentOutOfRangeException(nameof(faceToRot));

            int delta = Array.IndexOf(colors, color) - Array.IndexOf(colors, faceColor);
            return SolvingUtility.NormalizeCount(delta);
        }

        public static CubeColor GetFaceColor(this CubeFace face) {
            return Cube.GetFaceColor(face);
        }

        public static CubeFace GetFace(this CubeColor color) {
            return Cube.GetFace(color);
        }

        public static bool ValuesEqual<T>(this Tuple<T, T> source, Tuple<T, T> tuple) {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            return (comparer.Equals(source.Item1, tuple.Item1) && comparer.Equals(source.Item2, tuple.Item2))
                || (comparer.Equals(source.Item1, tuple.Item2) && comparer.Equals(source.Item2, tuple.Item1));

        }

        public static Tuple<T, T> Swap<T>(this Tuple<T, T> tuple) {
            return new Tuple<T, T>(tuple.Item2, tuple.Item1);
        }

        public static Tuple<T, T, T> Swap<T>(this Tuple<T, T, T> tuple, int firstItem, int secondItem) {
            if (firstItem < 0 || firstItem > 2)
                throw new ArgumentOutOfRangeException(nameof(firstItem));

            if (secondItem < 0 || secondItem > 2)
                throw new ArgumentOutOfRangeException(nameof(secondItem));

            if (firstItem == secondItem)
                return tuple;

            T[] values = { tuple.Item1, tuple.Item2, tuple.Item3 };

            T tmp = values[firstItem];
            values[firstItem] = values[secondItem];
            values[secondItem] = tmp;

            return new Tuple<T, T, T>(values[0], values[1], values[2]);
        }

        public static bool ValuesEqual<T>(this IEnumerable<T> first, IEnumerable<T> second) {
            return ValuesEqual(first, second, EqualityComparer<T>.Default);
        }

        public static bool ValuesEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, EqualityComparer<T> comparer) {
            return first.All(e => second.Any(f => comparer.Equals(e, f)));
        }
        #endregion
    }
}
