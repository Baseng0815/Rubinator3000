using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000
{
    partial class CubeSolverFridrich
    {
        private void CalcOLLMoves()
        {
            MoveCollection moves = new MoveCollection();
            while (!PllPattern.PllPatterns.Any(e => e.IsMatch(Cube)))
            {
                Cube.DoMove(Move.Parse("D"));
                moves.Add(Move.Parse("D"));
            }

            MoveCollection pllMoves = GetPllAlgorithm(PllPattern.PllPatterns.First(e => e.IsMatch(Cube)));

            Cube.DoMoves(pllMoves);
            moves.AddRange(pllMoves);

            while (!IsSolved(Cube))
            {
                Cube.DoMove(Move.Parse("D"));
                moves.Add(Move.Parse("D"));
            }

            return moves;
        }
    }
}
