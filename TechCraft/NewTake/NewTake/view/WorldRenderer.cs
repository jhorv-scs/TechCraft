using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading;
using System.Diagnostics;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;

namespace NewTake.view
{
    class WorldRenderer
    {
        #region inits

        public const float FARPLANE = 140 * 2;
        public const int FOGNEAR = 90 * 2;
        public const int FOGFAR = 140 * 2;

        protected World world;
        protected readonly GraphicsDevice GraphicsDevice;
        protected Dictionary<Vector3i, ChunkRenderer> ChunkRenderers;
        protected Effect _solidBlockEffect;
        protected Texture2D _textureAtlas;
        public readonly FirstPersonCamera camera;
        protected Vector3i previousChunkIndex;

        //public TimeSpan removeTime = TimeSpan.Zero;
        //public Queue<Chunk> _toBuild;
        //public bool _running = true;
        //public Thread _buildingThread;

        protected readonly RasterizerState _wireframedRaster = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        protected readonly RasterizerState _normalRaster = new RasterizerState() { CullMode = CullMode.CullCounterClockwiseFace, FillMode = FillMode.Solid };
        protected bool _wireframed = false;

        #endregion

        public void ToggleRasterMode()
        {
            this._wireframed = !this._wireframed;
        }

        public WorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
        {
            this.world = world;
            this.GraphicsDevice = graphicsDevice;
            ChunkRenderers = new Dictionary<Vector3i, ChunkRenderer>();
            // Generate the initial chunks
            Debug.WriteLine("Generating initial chunks");
            world.visitChunks(DoGenerate);
            // Build the initial chunks
            Debug.WriteLine("Building initial chunks");
            world.visitChunks(DoBuild);
            // Generate
            this.camera = camera;

            this.previousChunkIndex = new Vector3i();
         }

        public virtual void DoBuild(Vector3i vector)
        {
            throw new Exception("Must override Build");
        }

        public virtual void DoGenerate(Vector3i vector)
        {
            throw new Exception("Must override Generate");
        }

        public virtual void loadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks");
            _solidBlockEffect = content.Load<Effect>("Effects\\SolidBlockEffect");
        }

        #region RemoveChunks
        public void RemoveChunks()
        {
            uint x = (uint)camera.Position.X;
            uint z = (uint)camera.Position.Z;

            uint cx = x / Chunk.CHUNK_XMAX;
            uint cz = z / Chunk.CHUNK_ZMAX;

            uint lx = x % Chunk.CHUNK_XMAX;
            uint lz = z % Chunk.CHUNK_ZMAX;

            Vector3i currentChunkIndex = world.viewableChunks[cx, cz].Index;

            for (uint j = cx - (World.VIEW_DISTANCE_FAR_X); j < cx + (World.VIEW_DISTANCE_FAR_X); j++)
            {
                for (uint l = cz - (World.VIEW_DISTANCE_FAR_Z); l < cz + (World.VIEW_DISTANCE_FAR_Z); l++)
                {
                    int distancecx = (int)(cx - j);
                    int distancecz = (int)(cz - l);

                    if (distancecx < 0) distancecx = 0 - distancecx;
                    if (distancecz < 0) distancecz = 0 - distancecz;

                    if ((distancecx > World.VIEW_DISTANCE_NEAR_X) || (distancecz > World.VIEW_DISTANCE_NEAR_Z))
                    {
                        if ((world.viewableChunks[j, l] != null))
                        {
                            Chunk chunk = world.viewableChunks[j, l];
                            chunk.visible = false;
                            world.viewableChunks[j, l] = null;
                            ChunkRenderers.Remove(chunk.Index);
                            //Debug.WriteLine(string.Format("Removed Chunk {0}-{1}-{2}", (int)chunk.Position.X, (int)chunk.Position.Y, (int)chunk.Position.Z));
                        }
                    }
                }
            }
        }
        #endregion

        #region Update
        public virtual void Update(GameTime gameTime)
        { 
            uint x = (uint)camera.Position.X;
            uint z = (uint)camera.Position.Z;

            uint cx = x / Chunk.CHUNK_XMAX;
            uint cz = z / Chunk.CHUNK_ZMAX;

            uint lx = x % Chunk.CHUNK_XMAX;
            uint lz = z % Chunk.CHUNK_ZMAX;

            Vector3i currentChunkIndex = world.viewableChunks[cx, cz].Index;    // This is the chunk in which the camera currently resides

            if (currentChunkIndex != previousChunkIndex)
            {
                previousChunkIndex = currentChunkIndex;

                // Loop through all possible chunks around the camera in both X and Z directions
                for (uint j = cx - (World.VIEW_DISTANCE_FAR_X + 1); j < cx + (World.VIEW_DISTANCE_FAR_X + 1); j++)
                {
                    for (uint l = cz - (World.VIEW_DISTANCE_FAR_Z + 1); l < cz + (World.VIEW_DISTANCE_FAR_Z + 1); l++)
                    {
                        int distancecx = (int)(cx - j);        // The distance from the camera to the chunk in the X direction
                        int distancecz = (int)(cz - l);        // The distance from the camera to the chunk in the Z direction

                        if (distancecx < 0) distancecx = 0 - distancecx;        // If the distance is negative (behind the camera) make it positive
                        if (distancecz < 0) distancecz = 0 - distancecz;        // If the distance is negative (behind the camera) make it positive

                        // Remove Chunks
                        if ((distancecx > World.VIEW_DISTANCE_NEAR_X) || (distancecz > World.VIEW_DISTANCE_NEAR_Z))
                        {
                            if ((world.viewableChunks[j, l] != null)) // Chunk is created, therefore remove
                            {
                                Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));    // This is the chunk in the loop, offset from the camera
                                Chunk chunk = world.viewableChunks[j, l];
                                chunk.visible = false;
                                world.viewableChunks[j, l] = null;
                                ChunkRenderers.Remove(newIndex);
                                //Debug.WriteLine("Removed chunk at {0},{1},{2}", chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
                            }
                            else
                            {
                                //Debug.WriteLine("[Remove] chunk not found at at {0},0,{1}", j, l);
                            }
                        }
                        // Generate Chunks
                        else if ((distancecx > World.VIEW_CHUNKS_X) || (distancecz > World.VIEW_CHUNKS_Z))
                        {
                            // A new chunk is coming into view - we need to generate it
                            if (world.viewableChunks[j, l] == null) // Chunk is not created, therefore create
                            {
                                Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));    // This is the chunk in the loop, offset from the camera
                                DoGenerate(newIndex);
                                //Debug.WriteLine("Built chunk at {0},{1},{2}", newIndex.X, newIndex.Y, newIndex.Z);
                            }
                        }
                        // Build Chunks
                        else
                        {
                            Chunk chunk = world.viewableChunks[j, l];
                            if ((!chunk.built) && (chunk.generated))
                            {
                                // We have a chunk in view - it has been generated but we haven't built a vertex buffer for it
                                Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));    // This is the chunk in the loop, offset from the camera
                                DoBuild(newIndex);
                                //Debug.WriteLine("Vertices built at {0},{1},{2}", newIndex.X, newIndex.Y, newIndex.Z);
                            }
                        }
                    }
                }
            }// end if

            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);

            foreach (ChunkRenderer chunkRenderer in ChunkRenderers.Values)
            {
                if (chunkRenderer == null) return;

                if (chunkRenderer.isInView(viewFrustum))
                {
                    // Only update chunks which have a valid vertex buffer
                    if (chunkRenderer.chunk.built)
                    {
                        chunkRenderer.update(gameTime);
                    }
                }
            }
        }

        #endregion

        #region Draw
        public virtual void Draw(GameTime gameTime)
        {
            // A renderer must implement it's own draw method
            throw new Exception("Must override draw");
        }
        #endregion

    }
}
