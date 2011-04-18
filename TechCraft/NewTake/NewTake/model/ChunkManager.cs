using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using NewTake.model;

namespace NewTake.model.types
{
    public class ChunkManager : Dictionary2<Chunk>
    {
        private IChunkPersitence persistence;
   
        public ChunkManager(IChunkPersitence persistence)
        {
            this.persistence=persistence;
        }
        
        public override void Remove(uint x, uint z)
        {
            Chunk chunk = this[x, z];

            beforeRemove(chunk);

            Remove(KeyFromCoords(x, z));

        }

        private void beforeRemove(Chunk chunk)
        {
            persistence.save(chunk);
        }



        /*
         * The idea of loading directly whenever accessing a null chunk was cool but theres much more to do in the wolrdrenderer.generate method
         * 
         * Needs more thinking and surely some major refactoring. 
         * 
         * For now the load method from this chunkmanager class is called from worldrenderer.generate
         * 
         * public override Chunk this[uint x, uint z]
          {
              get
              {
                  Chunk chunk = base[x, z];
                  if (chunk == null )
                  {
                      Vector3i index = new Vector3i(x,0,z);

                      chunk = whenNull(index);
                      base[x, z] = chunk; //Note for jacoo : this was missing , causing some sort of infinite loop
                  }
                  return chunk;
              }
              set
              {
                  base[x, z] = value;
              }
          }

          private Chunk whenNull(Vector3i index)
          {
              return persistence.load(index);
          }
         */

        public Chunk load(Vector3i index)
        {
            return persistence.load(index);
        }


    }
}
