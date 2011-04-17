using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace NewTake.model
{
    public interface IChunkPersitence
    {

        void save(Chunk chunk);


        Chunk load(Vector3i index);

    }
}
