using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubinatorCore {
    public class Move {
        public CubeFace Face;

        private int count;
        public int Count {
            get => count;

            // guarantee that count always stays in the [-2;2] interval
            set {
                count = value % 4;
                if (count == 3) count = -1;
                else if (count == -3) count = 1;
            }
        }

        // returns count in positive only range (e.g. -1 becomes 3)
        public int CountPositive {
            get {
                return (count + 4) % 4;
            }
        }

        // -1 on prime, 1 on non-prime moves
        public int Direction {
            get {
                return Count == 0 ? 0 : Count / Math.Abs(Count);
            }
        }

        // used for mapping Faces to strings (e.g. 0 to L)
        private static readonly string[] mappings = new string[]
        {
            "L", "U", "F", "D", "R", "B"
        };

        public Move(CubeFace Face, int Count = 1) {
            this.Face = Face;
            this.Count = Count;
        }

        public override bool Equals(object obj) {
            if (!(obj is Move))
                return false;

            var move = (Move)obj;
            return Face == move.Face && Count == move.Count;
        }

        public override int GetHashCode() {
            var hashCode = 1804491660;
            hashCode = hashCode * -1521134295 + Face.GetHashCode();
            hashCode = hashCode * -1521134295 + Count.GetHashCode();
            return hashCode;
        }

        public override string ToString() {
            string str = mappings[(int)Face];
            if (Direction == -1) str += "i";
            else if (Count == 2)
                str += "2";

            return str;
        }

        public static bool operator ==(Move left, Move right) => left?.Face == right?.Face && left?.Count == right?.Count;

        public static bool operator !=(Move left, Move right) => !(left == right);

        public Move GetInverted() => new Move(Face, -Count);
    }
}
