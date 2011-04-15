using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model.types
{
    public class Dictionary3<T> : Dictionary<ulong, T>
    {
        const ulong size = UInt32.MaxValue;
        const ulong sizeSquared = (ulong)UInt32.MaxValue * UInt32.MaxValue;
        //and get some oolong tea

        public T this[uint x, uint y, uint z]
        {
            get
            {
                T outVal = default(T);
                TryGetValue((ulong)(x + (y * size) + (z * sizeSquared)), out outVal);
                return outVal;
            }
            set
            {
                ulong key = (ulong)(x + (y * size) + (z * sizeSquared));

                //T outVal = default(T);
                //if (TryGetValue(key, out outVal))
                //{
                    this[key] = value; 
                //}
                //else
                //{
                //    Add(key, value);
                //}
             }
        }
    }
}
