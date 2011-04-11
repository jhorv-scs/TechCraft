using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;

namespace NewTake.view
{
    class SingleThreadWorldRenderer : WorldRenderer
    {

        public SingleThreadWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world) : 
            base (graphicsDevice,  camera,  world) { }

        protected override void postConstruct(){
            //I just extracted the thread stuff here in base class
        }

        public override void initRendererAction(Vector3i vector)
        {
            Chunk chunk = world.viewableChunks[vector.X, vector.Z];
            ChunkRenderer cRenderer = new SolidBoundsChunkRenderer(GraphicsDevice, world, chunk);
            ChunkRenderers.Add(chunk.Index,cRenderer);
        }

        #region Draw
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
        #endregion

    }
}
