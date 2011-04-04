using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NewTake.model;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace NewTake.view
{
    class WorldRenderer
    {
        private World world;
        public readonly GraphicsDevice GraphicsDevice;
        private IList<ChunkRenderer> ChunkRenderers;
        private Effect _solidBlockEffect;
        private Texture2D _textureAtlas;
        public const float FARPLANE = 140 * 4;
        public const int FOGNEAR = 90 * 4;
        public const int FOGFAR = 140 * 4;        
        private readonly FirstPersonCamera camera;


        public WorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
        {
            this.world = world;
            this.GraphicsDevice = graphicsDevice;
            ChunkRenderers = new List<ChunkRenderer>();
            world.visitChunks(initRendererAction);
            this.camera = camera;
        }

        public void initRendererAction(Vector3i vector)
        {
            Chunk chunk = world.viewableChunks[vector.X,vector.Z];
            ChunkRenderer cRenderer = new ChunkRenderer(GraphicsDevice, world, chunk);
            ChunkRenderers.Add(cRenderer);
        }

        public void loadContent(ContentManager content){
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks");
            _solidBlockEffect = content.Load<Effect>("Effects\\SolidBlockEffect");
        }

        public void Update(GameTime gameTime)
        {
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
                            int jj = 0;
                            int kk = 0;

                            Chunk chunk = world.viewableChunks[j, l];
                            chunk.visible = false;
                            world.viewableChunks[j, l] = null;
                            Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));

                            foreach (ChunkRenderer chunkRenderer in ChunkRenderers)
                            {
                                if (chunkRenderer.chunk.Index == newIndex)
                                {
                                    kk = jj;
                                }
                                jj++;
                            }
                            ChunkRenderers.RemoveAt(kk);
                            //Debug.WriteLine("Chunks loaded = {0}",jj-1);
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
        }

        public void Draw(GameTime gameTime)
        {

            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);

            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
           /* GraphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame
            };*/

            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _solidBlockEffect.Parameters["View"].SetValue(camera.View);
            _solidBlockEffect.Parameters["Projection"].SetValue(camera.Projection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            _solidBlockEffect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
            _solidBlockEffect.Parameters["AmbientIntensity"].SetValue(0.8f);
            _solidBlockEffect.Parameters["FogColor"].SetValue(Color.SkyBlue.ToVector4());
            _solidBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _solidBlockEffect.Parameters["BlockTexture"].SetValue(_textureAtlas);


            foreach (EffectPass pass in _solidBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (ChunkRenderer chunkRenderer in ChunkRenderers)
                {
                    Chunk chunk = world.viewableChunks[chunkRenderer.chunk.Index.X, chunkRenderer.chunk.Index.Z];
                    if (chunk.BoundingBox.Intersects(viewFrustum))
                    {
                        chunkRenderer.draw(gameTime);
                    }
                }
            }
        
        }
    }
}
