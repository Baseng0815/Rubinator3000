using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000
{
    public class PllPattern
    {
        protected int[,] m_pattern;
        protected Position? m_anchor;

        protected PllPattern(int[,] pattern, Position? anchor = null)
        {
            if (pattern.GetLength(0) != 4 && pattern.GetLength(1) != 3) throw new ArgumentException();

            m_pattern = pattern;
            m_anchor = anchor;
        }

        public bool IsMatch(Cube cube)
        {
            CubeFace[] faces = { CubeFace.FRONT, CubeFace.LEFT, CubeFace.BACK, CubeFace.RIGHT };

            for (int i = 0; i < 4; i++)
            {
                CubeFace face = faces[i];
                for (int j = 0; j < 3; j++)
                {
                    int t1 = (2 - j) / 2 + 6;
                    int t2 = (int)(Math.Sqrt(2 - j) + 1) + 6;

                    CubeColor c1 = cube.At(face, t1);
                    CubeColor c2 = cube.At(face, t2);

                    if (GetColorRelation(c1, c2) != m_pattern[i, j]) return false;
                }
            }

            return m_anchor == null ? true : cube.At(m_anchor.Value) == m_anchor.Value.Face;
        }

        private int GetColorRelation(CubeColor color1, CubeColor color2)
        {
            if (color1 == color2) return 1;

            if (Cube.OpponentColors[color1] == color2) return 2;

            return 0;
        }

        #region Static Patterns
        public static readonly PllPattern Pll_Empty = new PllPattern(new int[4, 3] {
            { -1, -1, -1 },
            { -1, -1, -1 },
            { -1, -1, -1 },
            { -1, -1, -1 }
        });

        public static readonly PllPattern Pll_Complete = new PllPattern(new int[4, 3] {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 }
        });

        /// <summary>
        /// A-Perm 1
        /// </summary>
        public static readonly PllPattern Pll_01 = new PllPattern(new int[4, 3] {
            { 1, 2, 2 },
            { 2, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 1 }
        });

        /// <summary>
        /// A-Perm 2
        /// </summary>
        public static readonly PllPattern Pll_02 = new PllPattern(new int[4, 3] {
            { 1, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 2 },
            { 2, 2, 1 }
        });

        /// <summary>
        /// E-Perm
        /// </summary>
        public static readonly PllPattern Pll_03 = new PllPattern(new int[4, 3] {
            { 0, 2, 0 },
            { 0, 2, 0 },
            { 0, 2, 0 },
            { 0, 2, 0 }
        });

        /// <summary>
        /// U-Perm 1
        /// </summary>
        public static readonly PllPattern Pll_04 = new PllPattern(new int[4, 3]{
            { 0, 1, 0 },
            { 2, 1, 2 },
            { 1, 1, 1 },
            { 0, 1, 0 }
        });

        /// <summary>
        /// U-Perm 2
        /// </summary>
        public static readonly PllPattern Pll_05 = new PllPattern(new int[4, 3] {
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 2, 1, 2 }
        });

        /// <summary>
        /// H-Perm
        /// </summary>
        public static readonly PllPattern Pll_06 = new PllPattern(new int[4, 3] {
            { 2, 1, 2 },
            { 2, 1, 2 },
            { 2, 1, 2 },
            { 2, 1, 2 }
        });

        /// <summary>
        /// Z-Perm
        /// </summary>
        public static readonly PllPattern Pll_07 = new PllPattern(new int[4, 3] {
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 }
        }, new Position(2, 6));

        /// <summary>
        /// T-Perm
        /// </summary>
        public static readonly PllPattern Pll_08 = new PllPattern(new int[4, 3] {
            { 1, 0, 0 },
            { 0, 2, 0 },
            { 0, 0, 1 },
            { 2, 1, 2 }
        });

        /// <summary>
        /// J-Perm
        /// </summary>
        public static readonly PllPattern Pll_09 = new PllPattern(new int[4, 3] {
            { 0, 0, 1 },
            { 2, 2, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 }
        });

        /// <summary>
        /// L-Perm
        /// </summary>
        public static readonly PllPattern Pll_10 = new PllPattern(new int[4, 3] {
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 2, 2 }
        });

        /// <summary>
        /// R-Perm 1
        /// </summary>
        public static readonly PllPattern Pll_11 = new PllPattern(new int[4, 3] {
            { 0, 1, 0 },
            { 0, 0, 2 },
            { 0, 2, 0 },
            { 0, 0, 1 }
        });

        /// <summary>
        /// R-Perm 2
        /// </summary>
        public static readonly PllPattern Pll_12 = new PllPattern(new int[4, 3] {
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 0, 2, 0 },
            { 2, 0, 0 }
        });

        /// <summary>
        /// F-Perm
        /// </summary>
        public static readonly PllPattern Pll_13 = new PllPattern(new int[4, 3] {
            { 2, 0, 0 },
            { 0, 2, 0 },
            { 0, 0, 2 },
            { 1, 1, 1 }
        });

        /// <summary>
        /// V-Perm
        /// </summary>
        public static readonly PllPattern Pll_14 = new PllPattern(new int[4, 3] {
            { 1, 2, 2 },
            { 0, 2, 0 },
            { 0, 2, 0 },
            { 2, 2, 1 }
        });

        /// <summary>
        /// Y-Perm
        /// </summary>
        public static readonly PllPattern Pll_15 = new PllPattern(new int[4, 3] {
            { 1, 2, 2 },
            { 2, 2, 1 },
            { 0, 2, 0 },
            { 0, 2, 0 }
        });

        /// <summary>
        /// N-Perm 1
        /// </summary>
        public static readonly PllPattern Pll_16 = new PllPattern(new int[4, 3]{
            { 2, 2, 1 },
            { 2, 2, 1 },
            { 2, 2, 1 },
            { 2, 2, 1 }
        });

        /// <summary>
        /// N-Perm 2
        /// </summary>
        public static readonly PllPattern Pll_17 = new PllPattern(new int[4, 3]{
            { 1, 2, 2 },
            { 1, 2, 2 },
            { 1, 2, 2 },
            { 1, 2, 2 }
        });

        /// <summary>
        /// G-Perm A
        /// </summary>
        public static readonly PllPattern Pll_18 = new PllPattern(new int[4, 3] {
            { 0, 0, 1 },
            { 0, 2, 0 },
            { 0, 0, 2 },
            { 0, 1, 0 }
        });

        /// <summary>
        /// G-Perm B
        /// </summary>
        public static readonly PllPattern Pll_19 = new PllPattern(new int[4, 3] {
            { 2, 0, 0 },
            { 2, 2, 1 },
            { 2, 0, 0 },
            { 2, 1, 2 }
        });

        /// <summary>
        /// G-Perm C
        /// </summary>
        public static readonly PllPattern Pll_20 = new PllPattern(new int[4, 3] {
            { 2, 0, 0 },
            { 0, 2, 0 },
            { 1, 0, 0 },
            { 0, 1, 0 }
        });

        /// <summary>
        /// G-Perm D
        /// </summary>
        public static readonly PllPattern Pll_21 = new PllPattern(new int[4, 3] {
            { 0, 0, 2 },
            { 1, 2, 2 },
            { 0, 0, 2 },
            { 2, 1, 2 }
        });

        public static readonly PllPattern[] PllPatterns = new PllPattern[] {
            Pll_Complete, Pll_01, Pll_02, Pll_03, Pll_04, Pll_05, Pll_06, Pll_07, Pll_08, Pll_09, Pll_10,
            Pll_11, Pll_12, Pll_13, Pll_14, Pll_15, Pll_16, Pll_17, Pll_18, Pll_19, Pll_20, Pll_21
        };
        #endregion
    }
}
