using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NewTake.model;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace NewTake.view
{
    class SingleThreadWorldRenderer : WorldRenderer
    {

        public SingleThreadWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world) : 
            base (graphicsDevice,  camera,  world) 
        {
         }

        protected override void postConstruct(){
            //I just extracted the thread stuff here in base class
        }

        public override void initRendererAction(Vector3i vector)
        {
            Chunk chunk = world.viewableChunks[vector.X, vector.Z];
            ChunkRenderer cRenderer = new SolidBoundsChunkRenderer(GraphicsDevice, world, chunk);
            ChunkRenderers.Add(chunk.Index,cRenderer);
        }

        public override void Update(GameTime gameTime)
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

        public override void Draw(GameTime gameTime)
        {
            //currently a copy paste of base class but currently only :)

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
                    if (chunkRenderer.isInView(viewFrustum))
                    {
                       chunkRenderer.draw(gameTime);
                    }
                }
            }

        }
    }
}
