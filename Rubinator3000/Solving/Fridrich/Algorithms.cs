using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;

namespace Rubinator3000
{
    public partial class CubeSolverFridrich
    {        
        static CubeSolver()
        {
            AlgorithmsOLL = LoadOllAlgorithms();
        }

        private static Dictionary<int, MoveCollection> LoadOllAlgorithms()
        {
            Dictionary<int, MoveCollection> pairs = new Dictionary<int, MoveCollection>();
            pairs.Add(0, new MoveCollection());
            string algorithms = Properties.Resources.Algorithms_Oll;

            int line = 1;
            string algorithm;
            using (StringReader reader = new StringReader(algorithms))
            {
                while ((algorithm = reader.ReadLine()) != null)
                {
                    if (MoveCollection.TryParse(algorithm, out MoveCollection moves))
                    {
                        pairs.Add(line, moves);
                        line++;
                    }
                    else goto Exception;
                }
            }

            return pairs;

        Exception:
            throw new FormatException();
        }

        private static readonly Dictionary<int, MoveCollection> AlgorithmsOLL;

        public static MoveCollection GetOllAlogrithm(int pattern)
        {
            if (AlgorithmsOLL.ContainsKey(pattern))
            {
                return AlgorithmsOLL[pattern];
            }
            else throw new ArgumentException();
        }

        
        private static readonly Dictionary<PllPattern, MoveCollection> AlgorithmsPLL = new List<KeyValuePair<PllPattern, MoveCollection>>() {
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_01, new MoveCollection("Li", "F", "Li", "B", "B", "L", "Fi", "Li", "B", "B", "L", "L")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_02, new MoveCollection("L", "L", "B", "B", "L", "F", "Li", "B", "B", "L", "Fi", "L")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_03, new MoveCollection("L", "Bi", "Li", "F", "L", "B", "Li", "Fi", "L", "B", "Li", "F", "L", "Bi", "Li", "Fi")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_04, new MoveCollection("L", "Di", "L", "D", "L", "D", "L", "Di", "Li", "Di", "L", "L")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_05, new MoveCollection("Ri", "D", "Ri", "Di", "Ri", "Di", "Ri", "D", "R", "D", "R", "R")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_06, new MoveCollection("L", "L", "D", "D", "L", "D", "D", "L", "L", "D", "D", "L", "L", "D", "D", "L", "D", "D", "L", "L")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_07, new MoveCollection("Li", "Di", "L", "L", "D", "L", "D", "Li", "Di", "L", "D", "L", "Di", "L", "Di", "Li", "D", "D")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_08, new MoveCollection("L", "D", "Li", "Di", "Li", "F", "L", "L", "Di", "Li", "Di", "L", "D", "Li", "Fi")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_09, new MoveCollection("L", "D", "Li", "Fi", "L", "D", "Li", "Di", "Li", "F", "L", "L", "Di", "Li", "Di")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_10, new MoveCollection("Ri", "Di", "R", "F", "Ri", "Di", "R", "D", "R", "Fi", "R", "R", "D", "R", "D")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_11, new MoveCollection("Li", "D", "D", "L", "D", "D", "Li", "F", "L", "D", "Li", "Di", "Li", "Fi", "L", "L", "Di")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_12, new MoveCollection("R", "D", "D", "Ri", "D", "D", "R", "Fi", "Ri", "Di", "R", "D", "R", "F", "R", "R", "D")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_13, new MoveCollection("Li", "Di", "Fi", "L", "D", "Li", "Di", "Li", "F", "L","L", "Di", "Li", "Di", "L", "D", "Li", "D", "L")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_14, new MoveCollection("U", "U", "L", "D", "D", "Li", "U", "L", "Di", "L", "Di", "L", "D", "L", "L", "U", "Li", "Di", "L")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_15, new MoveCollection("F", "L", "Di", "Li", "Di", "L", "D", "Li", "Fi", "L", "D", "Li", "Di", "Li", "F", "L", "Fi")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_16, new MoveCollection("Ri", "F", "R", "D", "D", "Ri", "F", "R", "D", "D", "Ri", "F", "R", "D", "D", "Ri", "F", "R", "D", "D", "Ri", "F", "R", "D", "D")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_17, new MoveCollection("L", "F", "Li", "D", "D", "L", "F", "Li", "D", "D", "L", "F", "Li", "D", "D", "L", "F", "Li", "D", "D", "L", "F", "Li", "D", "D")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_18, new MoveCollection("L", "L", "D", "Li", "D", "Li", "Di", "L", "Di", "L", "L", "U", "Di", "Li", "D", "L", "Ui")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_19, new MoveCollection("U", "Li", "Di", "L", "D", "Ui", "L", "L", "D", "Li", "D", "L", "Di", "L", "Di", "L", "L")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_20, new MoveCollection("L", "L", "Di", "L", "Di", "L", "D", "Li", "D", "L", "L", "Ui", "D", "L", "Di", "Li", "U")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_21, new MoveCollection("Ui", "L", "D", "Li", "Di", "U", "L", "L", "Di", "L", "Di", "Li", "D", "Li", "D", "L", "L")),
            new KeyValuePair<PllPattern, MoveCollection>(PllPattern.Pll_Complete, new MoveCollection())
        }.ToDictionary(e => e.Key, f => f.Value);
        
        public static MoveCollection GetPllAlgorithm(PllPattern pattern)
        {
            if (PllPattern.PllPatterns.Contains(pattern))
            {
                return AlgorithmsPLL[pattern];                
            }
            else throw new ArgumentException();
        }
    }
}