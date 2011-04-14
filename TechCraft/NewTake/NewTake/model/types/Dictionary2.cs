using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model.types
{
    public class Dictionary2<T> : Dictionary<ulong, T>
    {
        const ulong size = UInt32.MaxValue;

        public T this[uint x, uint z]
        {
            get
            {
                T outVal = default( T); 
                TryGetValue((ulong)(x + (z * size)),out outVal);
                return outVal;
            }
            set
            {
                ulong key = (ulong)(x + (z * size));
                T outVal = default(T);
                if (TryGetValue(key, out outVal))
                {
                    this[key] = value; 
                }
                else
                {
                    Add(key, value);
                }
             }
        }
    }
}
