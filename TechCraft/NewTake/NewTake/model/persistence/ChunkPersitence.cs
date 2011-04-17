using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace NewTake.model
{
    public class ChunkPersitence : IChunkPersitence
    {
        private const String LEVELFOLDER = "c:\\techcraft";

        private readonly World world;

        public ChunkPersitence(World world ) {
            this.world = world;
            
            if (! Directory.Exists(LEVELFOLDER)){
                Directory.CreateDirectory(LEVELFOLDER);
            }
        }

        public void save(Chunk chunk)
        {
            Debug.WriteLine("saving " + GetFilename(chunk.Position));
            FileStream fs = File.Open(GetFilename(chunk.Position), FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fs);
            Save(chunk,writer);
            writer.Flush();
            writer.Close();
            fs.Close();
        }

        public Chunk load(Vector3i index)
        {
            Vector3i position = new Vector3i(index.X * Chunk.CHUNK_XMAX, index.Y * Chunk.CHUNK_YMAX, index.Z * Chunk.CHUNK_ZMAX);
            string filename = GetFilename(position);

            if (File.Exists(filename))
            {
                Debug.WriteLine("Loading " + filename);
                FileStream fs = File.Open(filename, FileMode.Open);

                BinaryReader reader = new BinaryReader(fs);
                Chunk chunk = Load(position, reader);
                reader.Close();
                fs.Close();
                return chunk;
            }
            else
            {
                Debug.WriteLine("New " + filename);
                return null;
            }
        }

        //TODO write all bytes in one shot !
        private void Save(Chunk chunk, BinaryWriter writer)
        {

            byte[] array = new byte[chunk.Blocks.Length];
            
            for (int i = 0; i < chunk.Blocks.Length; i++)
            {
                  array[i]=(byte)chunk.Blocks[i].Type;                                
            }
            writer.Write(array);
        }

        private Chunk Load(Vector3i worldPosition, BinaryReader reader)
        {
            //index from position
            Vector3i index = new Vector3i(worldPosition.X / Chunk.CHUNK_XMAX, worldPosition.Y / Chunk.CHUNK_YMAX, worldPosition.Z / Chunk.CHUNK_ZMAX);

            Chunk chunk = new Chunk(world,index);

            //TODO read all bytes in one shot !
            for (int x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (int z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX;
                    for (int y = 0; y < Chunk.CHUNK_YMAX; y++)
                    {
                        chunk.Blocks[offset + y].Type = (BlockType)reader.ReadByte();
                    }
                }
            }

            return chunk;
        }


        private string GetFilename(Vector3i position)
        {
            return string.Format("{0}\\{1}-{2}-{3}", LEVELFOLDER, position.X, position.Y, position.Z);
        }
    }
}
