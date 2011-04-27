using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;
using NewTake.profiling;
using NewTake.view.blocks;

using System.Diagnostics;

namespace NewTake.view.renderers
{
    public class WaterRenderer : IRenderer
    {
        protected Effect _waterBlockEffect;
        protected Texture2D _textureAtlas;

        private VertexBuildChunkProcessor _vertexBuildChunkProcessor;

        private GraphicsDevice _graphicsDevice;
        private FirstPersonCamera _camera;
        private World _world;

        public const float FOGFAR = 220 * 4;
        public const int FOGNEAR = 200 * 4;

        public Vector3 SUNCOLOR = Color.White.ToVector3();

        public WaterRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;
            _world = world;
        }

        public void Initialize()
        {
            _vertexBuildChunkProcessor = new VertexBuildChunkProcessor(_graphicsDevice);
        }

        public void LoadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks");
            _waterBlockEffect = content.Load<Effect>("Effects\\WaterBlockEffect");
        }

        float rippleTime = 0;
        public void Draw(GameTime gameTime)
        {
            rippleTime += 0.1f;

            _waterBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _waterBlockEffect.Parameters["View"].SetValue(_camera.View);
            _waterBlockEffect.Parameters["Projection"].SetValue(_camera.Projection);
            _waterBlockEffect.Parameters["CameraPosition"].SetValue(_camera.Position);
            _waterBlockEffect.Parameters["FogColor"].SetValue(Color.White.ToVector4());
            _waterBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _waterBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _waterBlockEffect.Parameters["BlockTexture"].SetValue(_textureAtlas);
            //_waterBlockEffect.Parameters["SunColor"].SetValue(Color.White.ToVector3());
            //_waterBlockEffect.Parameters["timeOfDay"].SetValue(12);

            BoundingFrustum viewFrustum = new BoundingFrustum(_camera.View * _camera.Projection);

            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (EffectPass pass in _waterBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (Chunk chunk in _world.viewableChunks.Values)
                {
                    if (chunk.BoundingBox.Intersects(viewFrustum) && chunk.waterVertexBuffer!=null)
                    {
                        lock (chunk)
                        {
                            if (chunk.waterIndexBuffer.IndexCount > 0)
                            {
                                _graphicsDevice.SetVertexBuffer(chunk.waterVertexBuffer);
                                _graphicsDevice.Indices = chunk.waterIndexBuffer;
                                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.waterVertexBuffer.VertexCount, 0, chunk.waterIndexBuffer.IndexCount / 3);
                            }
                        }                        
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Stop()
        {
            //_running = false;
        }

    }
}
