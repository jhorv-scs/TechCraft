using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NewTake
{

    public struct Vector3i
    {
        public Vector3i(uint x, uint y, uint z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3i(Vector3 vector3)
        {
            X = (uint)vector3.X;
            Y = (uint)vector3.Y;
            Z = (uint)vector3.Z;
        }

        public readonly uint X;
        public readonly uint Y;
        public readonly uint Z;
      

        public override bool Equals(object obj)
        {
            if (obj is Vector3i)
            {
                Vector3i other = (Vector3i)obj;
                return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
            }
            return base.Equals(obj);
        }
        public static bool operator ==(Vector3i a, Vector3i b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }
        public static bool operator !=(Vector3i a, Vector3i b)
        {
            return !(a.X == b.X && a.Y == b.Y && a.Z == b.Z);
        }
        public static Vector3i operator +(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X + b.X ,a.Y +b.Y ,a.Z + b.Z);
        }

        public static uint DistanceSquared(Vector3i value1, Vector3i value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;
            uint z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }

        public override int GetHashCode()
        {
            //TODO this hashcode impl is wrong
            uint hash = 23;
            unchecked
            {
                hash = hash * 37 + X;
                hash = hash * 37 + Y;
                hash = hash * 37 + Z;
            }
            return (int)hash;
        }

        public override string ToString()
        {
            return ("vector3i (" + X + "," + Y + "," + Z + ")");
        }

        public Vector3 asVector3()
        {
            return new Vector3(X, Y, Z);
        }

    }
}
