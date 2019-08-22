//#define TwoLookOll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000
{
    partial class CubeSolverFridrich
    {
#if TwoLookOll
        public MoveCollection OLL()
        {
            MoveCollection moves = new MoveCollection();

            //Yellow Cross
            FacePattern crossPattern = FacePattern.Face_CrossPattern;

            FacePattern[] patterns = { FacePattern.Face_JPattern, FacePattern.Face_LinePattern, FacePattern.Face_NoEdges };

            MoveCollection crossMoves = new MoveCollection("F", "L", "D", "Li", "Di", "Fi");

            while (!crossPattern.IsMatch(Cube, 3))
            {
                while (!patterns.Any(e => e.IsMatch(Cube, 3)))
                {
                    Cube.DoMove(Move.Parse("D"));
                    moves.Add(Move.Parse("D"));
                }

                Cube.DoMoves(crossMoves);
                moves.AddRange(crossMoves);
            }

            //Oll            
            while (!OllPattern.OllPatterns.Any(e => e.IsMatch(Cube)))
            {
                Cube.DoMove(Move.Parse("D"));
                moves.Add(Move.Parse("D"));
            }

            MoveCollection ollMoves = GetOllAlogrithm(OllPattern.OllPatterns.First(e => e.IsMatch(Cube)));

            Cube.DoMoves(ollMoves);
            moves.AddRange(ollMoves);

            return moves;
        }
#else
        private MoveCollection GetOllAlgorithm()
        {
            MoveCollection moves = new MoveCollection();
            int count = 0;
            while (!OllPattern.OllPatterns.Any(e => e.IsMatch(cube)) && count < 4)
            {
                cube.DoMove(CubeFace.DOWN, false);
                count++;
            }

            OllPattern pattern;
            try
            {
                 pattern = OllPattern.OllPatterns.First(e => e.IsMatch(Cube));
            }
            catch (InvalidOperationException) { throw; }

            MoveCollection ollMoves = MoveConverter.Convert(AlgorithmsOLL[pattern.Number]);
            Cube.DoMoves(ollMoves);

            moves.AddRange(ollMoves);
            return moves;
        }

        private void CalcOLLMoves()
        {
            try
            {
                MoveCollection m = GetOllAlgorithm();                

                return m;
            } catch (InvalidOperationException) { throw; }            
        }
#endif
    }
}
