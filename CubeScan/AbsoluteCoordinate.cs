using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.CubeScan
{
    public struct AbsoluteCoordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        public AbsoluteCoordinate(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", X, Y);
        }

        public override bool Equals(object obj)
        {
            return obj is AbsoluteCoordinate && Equals((AbsoluteCoordinate) obj);
        }

        private bool Equals(AbsoluteCoordinate obj)
        {
            return X == obj.X && Y == obj.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}
