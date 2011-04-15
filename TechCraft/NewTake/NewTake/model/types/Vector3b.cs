using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace NewTake
{

    public struct Vector3b
    {
        public readonly byte X;
        public readonly byte Y;
        public readonly byte Z;
    

        public Vector3b(byte x, byte y, byte z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }


        public override bool Equals(object obj)
        {
            if (obj is Vector3b)
            {
                Vector3b other = (Vector3b)obj;
                return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
            }
            return base.Equals(obj);
        }

        public static bool operator ==(Vector3b a, Vector3b b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(Vector3b a, Vector3b b)
        {
            return !(a.X == b.X && a.Y == b.Y && a.Z == b.Z);
        }

        public static Vector3b operator +(Vector3b a, Vector3b b)
        {
            return new Vector3b((byte)(a.X + b.X), (byte)(a.Y + b.Y), (byte)(a.Z + b.Z));
        }

       
        public override int GetHashCode()
        {
            //TODO check this hashcode impl - here shoul be ok, no overflow problem           
            return (int)(X ^ Y ^ Z);
        }

        public override string ToString()
        {
            return ("vector3b (" + X + "," + Y + "," + Z + ")");
        }

    }
}
