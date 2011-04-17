using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace NewTake.model
{
    public class MockChunkPersitence : IChunkPersitence
    {

        private readonly World world;

        public MockChunkPersitence(World world)
        {
            this.world = world;
        }

        public void save(Chunk chunk)
        {
            Debug.WriteLine("would be saving " + GetFilename(chunk.Position));

        }

        public Chunk load(Vector3i index)
        {
            Vector3i position = new Vector3i(index.X * Chunk.CHUNK_XMAX, index.Y * Chunk.CHUNK_YMAX, index.Z * Chunk.CHUNK_ZMAX);

            string filename = GetFilename(position);
            Debug.WriteLine("Would be loading " + filename);
            return null;
        }

        private string GetFilename(Vector3i position)
        {
            return string.Format("{0}\\{1}-{2}-{3}","LEVELFOLDER", position.X, position.Y, position.Z);
        }
    }
}
