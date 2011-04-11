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

        public const float FARPLANE = 140*8;
        public const int FOGNEAR = 90*8;
        public const int FOGFAR = 140*8;

        protected World world;
        protected readonly GraphicsDevice GraphicsDevice;
        //private IList<ChunkRenderer> ChunkRenderers;
        protected Dictionary<Vector3i, ChunkRenderer> ChunkRenderers;
        protected Effect _solidBlockEffect;
        protected Texture2D _textureAtlas;
        public readonly FirstPersonCamera camera;

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
            world.visitChunks(initRendererAction);
            this.camera = camera;
            //postConstruct();
         }

        protected virtual void postConstruct(){
            //_toBuild = new Queue<Chunk>();
            //_buildingThread = new Thread(new ThreadStart(BuildingThread));
            //_buildingThread.Start();
        }

        public virtual void initRendererAction(Vector3i vector)
        {
            Chunk chunk = world.viewableChunks[vector.X, vector.Z];
            //ChunkRenderer cRenderer = new ChunkRenderer(GraphicsDevice, world, chunk);
            ChunkRenderer cRenderer = new BoundariesChunkRenderer(GraphicsDevice, world, chunk, camera);
            ChunkRenderers.Add(chunk.Index, cRenderer);
        }

        public void loadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks");
            _solidBlockEffect = content.Load<Effect>("Effects\\SolidBlockEffect");
        }


        #region AddChunks
        public void AddChunks()
        {
            uint x = (uint)camera.Position.X;
            uint z = (uint)camera.Position.Z;

            uint cx = x / Chunk.CHUNK_XMAX;
            uint cz = z / Chunk.CHUNK_ZMAX;

            uint lx = x % Chunk.CHUNK_XMAX;
            uint lz = z % Chunk.CHUNK_ZMAX;

            Vector3i currentChunkIndex = world.viewableChunks[cx, cz].Index;

            for (uint j = cx - (World.VIEW_DISTANCE_NEAR_X); j < cx + (World.VIEW_DISTANCE_NEAR_X); j++)
            {
                for (uint l = cz - (World.VIEW_DISTANCE_NEAR_Z); l < cz + (World.VIEW_DISTANCE_NEAR_Z); l++)
                {
                    if (world.viewableChunks[j, l] == null)
                    {
                        Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));
                        Chunk toAdd = new Chunk(newIndex);
                        world.viewableChunks[newIndex.X, newIndex.Z] = toAdd;
                        world.builder.build(toAdd);
                        initRendererAction(newIndex);
                    }
                }
            }
        }
        #endregion

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
        { //this is the old add + remove at the same time from a previous checkin
            uint x = (uint)camera.Position.X;
            uint y = (uint)camera.Position.Y;
            uint z = (uint)camera.Position.Z;

            uint cx = x / Chunk.CHUNK_XMAX;
            uint cz = z / Chunk.CHUNK_ZMAX;

            uint lx = x % Chunk.CHUNK_XMAX;
            uint ly = y % Chunk.CHUNK_YMAX;
            uint lz = z % Chunk.CHUNK_ZMAX;

            Vector3i currentChunkIndex = world.viewableChunks[cx, cz].Index;

            for (uint j = cx - (World.VIEW_CHUNKS_X * 2); j < cx + (World.VIEW_CHUNKS_X * 2); j++)
            {
                for (uint l = cz - (World.VIEW_CHUNKS_Z * 2); l < cz + (World.VIEW_CHUNKS_Z * 2); l++)
                {
                    int distancecx = (int)(cx - j);
                    int distancecz = (int)(cz - l);

                    if (distancecx < 0) distancecx = 0 - distancecx;
                    if (distancecz < 0) distancecz = 0 - distancecz;

                    if ((distancecx > World.VIEW_CHUNKS_X) || (distancecz > World.VIEW_CHUNKS_Z))
                    {
                        if ((world.viewableChunks[j, l] != null))
                        {
                            
                            Chunk chunk = world.viewableChunks[j, l];
                            chunk.visible = false;
                            world.viewableChunks[j, l] = null;
                            Vector3i removeIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));

                            ChunkRenderers.Remove(removeIndex);
                            }
                    }
                    else
                    {
                        if (world.viewableChunks[j, l] == null)
                        {
                            Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));
                            Chunk toAdd = new Chunk(newIndex);
                            world.viewableChunks[newIndex.X, newIndex.Z] = toAdd;
                            world.builder.build(toAdd);
                            initRendererAction(newIndex);
                        }
                    }
                }
            }

            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);

            foreach (ChunkRenderer chunkRenderer in ChunkRenderers.Values)
            {
                if (chunkRenderer.isInView(viewFrustum))
                {
                    chunkRenderer.update(gameTime);
                }
            }

        }
        #endregion

        #region Draw
        public virtual void Draw(GameTime gameTime)
        {
            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);

            GraphicsDevice.Clear(Color.SkyBlue);
            GraphicsDevice.RasterizerState = !this._wireframed ? this._normalRaster : this._wireframedRaster;

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _solidBlockEffect.Parameters["View"].SetValue(camera.View);
            _solidBlockEffect.Parameters["Projection"].SetValue(camera.Projection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            _solidBlockEffect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
            _solidBlockEffect.Parameters["AmbientIntensity"].SetValue(0.6f);
            _solidBlockEffect.Parameters["FogColor"].SetValue(Color.SkyBlue.ToVector4());
            _solidBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _solidBlockEffect.Parameters["BlockTexture"].SetValue(_textureAtlas);

            foreach (EffectPass pass in _solidBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (ChunkRenderer chunkRenderer in ChunkRenderers.Values)
                {
                    if (chunkRenderer.chunk.BoundingBox.Intersects(viewFrustum))
                    {
                        if (!chunkRenderer.chunk.generated) continue;
                        //if (chunkRenderer.chunk.dirty)
                        //{
                            chunkRenderer.draw(gameTime);
                        //}
                    }
                }
            }
        }
        #endregion

    }
}
