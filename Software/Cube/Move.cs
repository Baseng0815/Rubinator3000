using Rubinator3000.Solving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000
{
    public class Move {
        public CubeFace Face;
        public int Count;
        public bool IsPrime;

        // used for mapping Faces to strings (e.g. 0 to L)
        private static readonly string[] mappings = new string[]
        {
            "L", "U", "F", "D", "R", "B"
        };

        public Move(CubeFace Face, int count = 1, bool isPrime = false)
        {
            this.Face = Face;
            this.IsPrime = isPrime;

            // @TODO: handle count == 0 cases (handle normally? throw exception?)

            // in case count is 3, optimize it to use an inverted turn
            if (Count == 3 || Count == -1) {
                Count = 1;
                IsPrime = true;
            } else
                this.Count = count % 4;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Move))
                return false;

            var move = (Move)obj;
            return Face == move.Face && IsPrime == move.IsPrime;
        }

        public override int GetHashCode()
        {
            var hashCode = 1804491660;
            hashCode = hashCode * -1521134295 + Face.GetHashCode();
            hashCode = hashCode * -1521134295 + IsPrime.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            string str = mappings[(int)Face];
            if (IsPrime) str += "i";
            else if (Count == 2)
                str += "2";

            return str;
        }

        public static bool operator ==(Move left, Move right) => left?.Face == right?.Face && left?.Count == right?.Count;

        public static bool operator !=(Move left, Move right) => !(left == right);

        public Move GetInverted() => new Move(Face, Count, !IsPrime);
    }
}
