using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using NewTake.model;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using NewTake.view.blocks;

namespace NewTake.view
{
     class ChunkRenderer
    {
        public List<VertexPositionTextureShade> _vertexList;

        public VertexBuffer vertexBuffer;

        public Chunk chunk;
        public readonly World world;
        protected readonly VertexBlockRenderer blocksRenderer;
        public readonly GraphicsDevice graphicsDevice;

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

        public virtual void BuildVertexList()
        {
            //Debug.WriteLine("building vertexlist ...");
            _vertexList.Clear();
            for (uint x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (uint y = 0; y < Chunk.CHUNK_YMAX; y++)
                {
                    for (uint z = 0; z < Chunk.CHUNK_ZMAX; z++)
                    {
                        Block block = chunk.Blocks[x, y, z];
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

                chunk.dirty = false;
                //Debug.WriteLine("............building Vertexlist done");
            }
        }

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

        public virtual void draw(GameTime gameTime)
        {
            if (chunk.dirty)
            {
                //_buildingThread = new Thread(new ThreadStart(BuildVertexList));
                ////_threadManager.Add(_buildingThread);
                //_buildingThread.Start();
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
    }
}
