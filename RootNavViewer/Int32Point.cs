using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace RootNav.Viewer
{
    public struct Int32Point
    {
        public Int32Point(Int32 X, Int32 Y)
        {
            this.x = X;
            this.y = Y;
        }
        Int32 x;

        public Int32 X
        {
            get { return x; }
        }

        Int32 y;

        public Int32 Y
        {
            get { return y; }
        }

        public static explicit operator Point(Int32Point p)
        {
            return new Point(p.X, p.Y);
        }

        public static explicit operator Int32Point(Point p)
        {
            return new Int32Point((int)p.X, (int)p.Y);
        }

        public override bool Equals(object obj)
        {
            if ((null == obj) || !(obj is Int32Point))
            {
                return false;
            }

            Int32Point value = (Int32Point)obj;
            return this.x == value.x && this.y == value.y;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode();
        }
    }
}
