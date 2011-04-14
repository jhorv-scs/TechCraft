using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using NewTake.view.blocks;
using NewTake.model;
using NewTake;

namespace NewTake.view
{
            
    public class ChunkRenderer
    {
        #region inits

        public List<VertexPositionTextureShade> _vertexList;
        
        public VertexBuffer vertexBuffer;

        public Chunk chunk;
        public readonly World world;
        protected readonly VertexBlockRenderer blocksRenderer;
        public readonly GraphicsDevice graphicsDevice;

        #endregion

        public ChunkRenderer(GraphicsDevice graphicsDevice, World world, Chunk chunk)
        {
            this.chunk = chunk;
            this.world = world;
            this.graphicsDevice = graphicsDevice;
            _vertexList = new List<VertexPositionTextureShade>();

            blocksRenderer = new VertexBlockRenderer(world);
        }

        public virtual bool isInView(BoundingFrustum viewFrustum)
        {
            return chunk.BoundingBox.Intersects(viewFrustum);
        }

        public Block BlockAt(Vector3i chunkRelativePositon)
        {
            if (chunkRelativePositon.Y < 0 || chunkRelativePositon.Y > Chunk.CHUNK_YMAX - 1)
            {
                return new Block(BlockType.Rock, false);
            }
            else if (chunkRelativePositon.X < 0 || chunkRelativePositon.Z < 0 ||
                chunkRelativePositon.X > Chunk.CHUNK_XMAX - 1 || chunkRelativePositon.Z > Chunk.CHUNK_ZMAX - 1)
            {
                Vector3i worldPosition = new Vector3i(chunk.Position.X + chunkRelativePositon.X, chunk.Position.Y + chunkRelativePositon.Y, chunk.Position.Z + chunkRelativePositon.Z);
                Chunk nChunk = world.viewableChunks[worldPosition.X / Chunk.CHUNK_XMAX, worldPosition.Z / Chunk.CHUNK_ZMAX];
                if (nChunk != null)
                {
                    Vector3i chunkBlockPosition = new Vector3i(worldPosition.X - nChunk.Position.X, worldPosition.Y - nChunk.Position.Y, worldPosition.Z - nChunk.Position.Z);
                    return nChunk.Renderer.BlockAt(chunkBlockPosition);
                }
                else
                {
                    return new Block(BlockType.Rock, false);
                }
            }
            else
            {
                //return chunk.Blocks[chunkRelativePositon.X, chunkRelativePositon.Y, chunkRelativePositon.Z];
                return chunk.Blocks[chunkRelativePositon.X * Chunk.FlattenOffset + chunkRelativePositon.Z * Chunk.CHUNK_YMAX + chunkRelativePositon.Y];
            }
        }

        #region BuildVertexList
        public virtual void BuildVertexList()
        {
            //Debug.WriteLine("building vertexlist ...");
            _vertexList.Clear();
            for (uint x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (uint z = 0; z < Chunk.CHUNK_ZMAX; z++)
                {
                    uint offset = x * (uint)Chunk.FlattenOffset + z * Chunk.CHUNK_YMAX;
                    for (uint y = 0; y < Chunk.CHUNK_YMAX; y++)
                    {
                        //Block block = chunk.Blocks[x, y, z];
                        Block block = chunk.Blocks[offset + y];
                        if (block.Type != BlockType.None)
                        {
                            // Vector3i blockPosition = chunk.Position + new Vector3i(x, y, z);

                            blocksRenderer.BuildBlockVertices(ref _vertexList, block, chunk, new Vector3i(x, y, z));
                        }
                    }
                }
            }
            VertexPositionTextureShade[] a = _vertexList.ToArray();

            if (a.Length != 0)
            {
                vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTextureShade), a.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(a);
            }
        }
        #endregion

        #region update
        //TODO currently only used by single thread impl 
        public virtual void update(GameTime gameTime)
        {
            if (chunk.dirty)
            {
                //_buildingThread = new Thread(new ThreadStart(BuildVertexList));
                ////_threadManager.Add(_buildingThread);
                //_buildingThread.Start();
                BuildVertexList();
            }
        }
        #endregion

        #region draw
        public virtual void draw(GameTime gameTime)
        {

            if (!chunk.generated) return;
            if (chunk.dirty)
            {
                BuildVertexList();
            }

            if (!chunk.visible)
            {
                _vertexList.Clear();
                vertexBuffer.Dispose();
                chunk.dirty = false;
            }

            if (vertexBuffer != null)
            {
                if (vertexBuffer.IsDisposed)
                {
                    return;
                }

                if (vertexBuffer.VertexCount > 0)
                {
                    graphicsDevice.SetVertexBuffer(vertexBuffer);
                    graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.VertexCount / 3);
                    // graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertexList.ToArray(), 0, _vertexList.Count / 3);
                }
                else
                {
                    Debug.WriteLine("no vertices");
                }
            }
        }
        #endregion

    }
}
