using System;

namespace ArgusPaintNet.Shared
{
    public struct VectorFloat : IEquatable<VectorFloat>
    {
        public VectorFloat(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public float X { get; set; }
        public float Y { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is VectorFloat vector)
            {
                return this.Equals(vector);
            }

            return false;
        }

        public bool Equals(VectorFloat other)
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

        public static bool operator ==(VectorFloat float1, VectorFloat float2)
        {
            return float1.Equals(float2);
        }

        public static bool operator !=(VectorFloat float1, VectorFloat float2)
        {
            return !(float1 == float2);
        }
    }
}
