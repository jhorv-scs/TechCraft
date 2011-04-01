using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model
{
    public class Custom3DArray<T>
    {
        private T[, ,] internalArray;
        public bool dirty;

        public Custom3DArray(int xSize, int ySize, int zSize)
        {
            internalArray = new T[xSize, ySize, zSize];
        }

        public T this[int x, int y, int z]
        {
            get
            {
                return internalArray[x, y, z];
            }
            set
            {
                dirty = true;
                internalArray[x, y, z]= value;
            }
        }
    }
}
