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
    class SingleThreadAOWorldRenderer : WorldRenderer
    {
        private Texture2D ambientOcclusionMap;

        public SingleThreadAOWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world) : 
            base (graphicsDevice,  camera,  world) { }

        public override void loadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks");
            ambientOcclusionMap = content.Load<Texture2D>("Textures\\cracks");
            _solidBlockEffect = content.Load<Effect>("Effects\\DualTextureAOBlockEffect");
        }

        public override void DoBuild(Vector3i vector)
        {
            Chunk chunk = world.viewableChunks[vector.X, vector.Z];
            // Build a vertex buffer for this chunks
            chunk.Renderer.BuildVertexList();
            // Add the renderer to the list so that it is drawn
            ChunkRenderers.Add(chunk.Index, chunk.Renderer);

            chunk.built = true;
        }

        public override void DoGenerate(Vector3i vector)
        {
            // Create a new chunk
            Chunk chunk = new Chunk(vector);
            // Assign a renderer
            ChunkRenderer cRenderer = new SingleThreadAOChunkRenderer(GraphicsDevice, world, chunk);
            chunk.Renderer = cRenderer;
            // Generate the chunk with the current generator
            world.Generator.Generate(chunk);
            // Store the chunk in the view
            world.viewableChunks[vector.X, vector.Z] = chunk;

            chunk.generated = true;
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
            //_solidBlockEffect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
            //_solidBlockEffect.Parameters["AmbientIntensity"].SetValue(0.6f);
            _solidBlockEffect.Parameters["FogColor"].SetValue(Color.SkyBlue.ToVector4());
            _solidBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _solidBlockEffect.Parameters["Texture1"].SetValue(_textureAtlas);
            _solidBlockEffect.Parameters["Texture2"].SetValue(ambientOcclusionMap);

            //StatRenderer.Start("Chunk Rendering");
            foreach (EffectPass pass in _solidBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (ChunkRenderer chunkRenderer in ChunkRenderers.Values)
                {
                    if (chunkRenderer.isInView(viewFrustum) && chunkRenderer.chunk.generated && !chunkRenderer.chunk.dirty)
                    {
                        chunkRenderer.draw(gameTime);
                    }
                }
            }
            //StatRenderer.Stop("Chunk Rendering");

        }
        #endregion

    }
}
