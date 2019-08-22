using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000
{ 
    public class Move
    {
        public CubeFace Face;
        public bool IsPrime;

        // used for mapping Faces to strings (e.g. 0 to L)
        private static readonly string[] mappings = new string[]
        {
            "L", "U", "F", "D", "R", "B"
        };

        public Move(CubeFace Face, bool IsPrime)
        {
            this.Face = Face;
            this.IsPrime = IsPrime;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Move))
                return false;

            var move = (Move)obj;
            return this.Face == move.Face && this.IsPrime == move.IsPrime;
        }

        internal static bool TryParse(string str, out Move move)
        {
            throw new NotImplementedException();
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
            if (IsPrime) str.Append('i');

            return str;
        }

        public static bool operator ==(Move left, Move right) => left.Face == right.Face && left.IsPrime == right.IsPrime;

        public static bool operator !=(Move left, Move right) => !(left == right);

        public Move GetInverted() => new Move(Face, !IsPrime);

        internal void Print()
        {
            // not implemented
            System.Diagnostics.Debug.WriteLine(this.ToString());
        }
    }
}
