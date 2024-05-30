using System;

namespace AreaRoomsAPI
{
    public struct PointD
    {
        public double X;

        public double Y;

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        public PointD(PointD pointD)
        {
            X = pointD.X;
            Y = pointD.Y;
        }

        public static bool operator ==(PointD left, PointD right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(PointD left, PointD right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PointD))
            {
                return false;
            }

            PointD pointD = (PointD)obj;
            if (this == pointD)
            {
                return Math.Abs(this.X - pointD.X) < 10e-6 && Math.Abs(this.Y - pointD.Y) < 10e-6;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 257;

                hash = hash * 101 + X.GetHashCode().GetHashCode();
                hash = hash * 101 + Y.GetHashCode().GetHashCode();

                return hash;
            }
        }
    }
}