﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000
{
    public class Move {
        private int count;

        public CubeFace Face { get; internal set; }
        public int Count {
            get => count;
            set {
                count = value;
                while (count < 0) count += 4;
                count %= 4;
            }
        }
        public bool IsPrime => Count == 3;

        // used for mapping Faces to strings (e.g. 0 to L)
        private static readonly string[] mappings = new string[]
        {
            "L", "U", "F", "D", "R", "B"
        };

        public Move(CubeFace Face, int count = 1)
        {
            this.Face = Face;

            while (count < 0) count += 4;            
            this.Count = count % 4;

            if (Count == 0)
                throw new ArgumentOutOfRangeException();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Move))
                return false;

            var move = (Move)obj;
            return Face == move.Face && IsPrime == move.IsPrime;
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
            if (IsPrime) str += "i";

            return str;
        }

        public static bool operator ==(Move left, Move right) => left?.Face == right?.Face && left?.Count == right?.Count;

        public static bool operator !=(Move left, Move right) => !(left == right);

        public Move GetInverted() => new Move(Face, -Count);

        internal void Print()
        {
            // not implemented
            System.Diagnostics.Debug.WriteLine(this.ToString());
        }
    }
}