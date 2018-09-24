using System;

namespace ArgusPaintNet.FFT
{
    internal struct PointInt32 : IEquatable<PointInt32>
    {
        public PointInt32(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is PointInt32 value)
            {
                return this.Equals(value);
            }

            return false;
        }

        public bool Equals(PointInt32 other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override int GetHashCode()
        {
            int hashCode = 1861411795;

            unchecked
            {
                hashCode = (hashCode * -1521134295) + this.X.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.Y.GetHashCode();
            }

            return hashCode;
        }

        public static bool operator ==(PointInt32 int1, PointInt32 int2)
        {
            return int1.Equals(int2);
        }

        public static bool operator !=(PointInt32 int1, PointInt32 int2)
        {
            return !(int1 == int2);
        }
    }
}
