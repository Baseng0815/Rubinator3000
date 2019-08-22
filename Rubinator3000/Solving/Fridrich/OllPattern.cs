using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Rubinator3000
{
    public class OllPattern : FacePattern {
        public static OllPattern[] OllPatterns { get; internal set; }        

        private int m_number = -1;
        public int Number {
            get => m_number;
            set {
                if (value > 255 && value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, "Der Wert muss zwischen 0 und 255 liegen");
                else m_number = value;
            }
        }

        protected bool[][] m_sidePattern;

        protected OllPattern(bool[] facePattern, bool[][] sidePattern) : base(facePattern) {
            if (sidePattern.Length != 4) throw new ArgumentException();

            m_sidePattern = sidePattern;
        }

        protected OllPattern(FacePattern facePattern, bool[][] sidePattern) : base(facePattern) {
            if (sidePattern.Length != 4) throw new ArgumentException();

            m_sidePattern = sidePattern;
        }

        static OllPattern() {
            OllPatterns = LoadPatterns();
        }

        public bool IsMatch(Cube cube) {
            if (base.IsMatch(cube, 3)) {
                int[] faces = { 2, 0, 5, 4 };
                for (int i = 0; i < 4; i++) {
                    for (int j = 0; j < 3; j++) {
                        int face = faces[i];
                        int tile = 8 - j;

                        if (m_sidePattern[i][j] != (cube.At(face, tile) == 3)) return false;
                    }
                }
                return true;
            }
            else return false;
        }

        public static OllPattern GetOllPattern(Cube cube) {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj) {
            return obj is OllPattern pattern &&
                     EqualityComparer<bool[]>.Default.Equals(this.m_facePattern, pattern.m_facePattern) &&
                     EqualityComparer<bool[][]>.Default.Equals(this.m_sidePattern, pattern.m_sidePattern);
        }

        public static OllPattern GetPattern(Cube cube) {
            FacePattern facePattern = FacePattern.GetFacePattern(cube, 3);

            bool[][] sides = new bool[4][];
            int[] faces = { 2, 0, 5, 4 };
            for (int i = 0; i < 4; i++) {
                sides[i] = new bool[3];
                int face = faces[i];
                for (int j = 0; j < 3; j++) {
                    sides[i][j] = cube.At(face, 8 - j) == 3;
                }
            }

            return new OllPattern(facePattern, sides);
        }

        private static OllPattern[] LoadPatterns()
        {
            List<OllPattern> patterns = new List<OllPattern>();

            XDocument doc = XDocument.Parse(Properties.Resources.ollPatterns);

            IEnumerable<XElement> patternElements =
                from element in doc.Element("patterns").Elements()
                where element.Name == "ollPattern"
                select element;

            for (IEnumerator<XElement> e = patternElements.GetEnumerator(); e.MoveNext();)
            {
                XElement element = e.Current;
                bool[] facePattern = new bool[9];
                byte[] bytes = BitConverter.GetBytes(int.Parse(element.Attribute("face").Value));
                BitArray bits = new BitArray(bytes);

                for (int i = 0; i < 9; i++) facePattern[i] = bits[i];

                bool[][] sidePattern = new bool[4][];
                for (int i = 0; i < 4; i++)
                {
                    bytes = BitConverter.GetBytes(int.Parse(element.Attribute("side" + i).Value));
                    bits = new BitArray(bytes);
                    sidePattern[i] = new bool[3];
                    for (int j = 0; j < 3; j++) sidePattern[i][j] = bits[j];
                }

                patterns.Add(new OllPattern(facePattern, sidePattern) {
                    Number = int.Parse(element.Attribute("number").Value)
                });
            }

            return patterns.ToArray();
        }

        internal static OllPattern[] LoadPatternsFromFile()
        {
            List<OllPattern> patterns = new List<OllPattern>();

            XDocument doc = XDocument.Load(@"C:\Users\Phili\Source\Repos\Cube\Cube\Solving\ollPatterns.xml");

            IEnumerable<XElement> patternElements =
                from element in doc.Element("patterns").Elements()
                where element.Name == "ollPattern"
                select element;

            for (IEnumerator<XElement> e = patternElements.GetEnumerator(); e.MoveNext();)
            {
                XElement element = e.Current;
                bool[] facePattern = new bool[9];
                byte[] bytes = BitConverter.GetBytes(int.Parse(element.Attribute("face").Value));
                BitArray bits = new BitArray(bytes);

                for (int i = 0; i < 9; i++) facePattern[i] = bits[i];

                bool[][] sidePattern = new bool[4][];
                for (int i = 0; i < 4; i++)
                {
                    bytes = BitConverter.GetBytes(int.Parse(element.Attribute("side" + i).Value));
                    bits = new BitArray(bytes);
                    sidePattern[i] = new bool[3];
                    for (int j = 0; j < 3; j++) sidePattern[i][j] = bits[j];
                }

                patterns.Add(new OllPattern(facePattern, sidePattern)
                {
                    Number = int.Parse(element.Attribute("number").Value)
                });
            }

            return patterns.ToArray();
        }
    }
}