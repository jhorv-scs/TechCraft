using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using NewTake.view.blocks;
using NewTake.model;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace NewTake.view.renderers
{
    class SimpleRenderer : IRenderer
    {

        protected Effect _solidBlockEffect;
        protected Effect _waterBlockEffect;

        protected Texture2D _textureAtlas;
        private VertexBuildChunkProcessor _vertexBuildChunkProcessor;
        private LightingChunkProcessor _lightingChunkProcessor;

        private GraphicsDevice _graphicsDevice;
        private FirstPersonCamera _camera;
        private World _world;


        private const byte BUILD_RANGE = 15;
        private const byte LIGHT_RANGE = BUILD_RANGE + 1;
        private const byte GENERATE_RANGE_LOW = LIGHT_RANGE + 1;
        private const byte GENERATE_RANGE_HIGH = GENERATE_RANGE_LOW;

        #region Atmospheric settings
        protected Vector4 NIGHTCOLOR = Color.Black.ToVector4();
        public Vector4 SUNCOLOR = Color.White.ToVector4();
        protected Vector4 HORIZONCOLOR = Color.White.ToVector4();

        protected Vector4 EVENINGTINT = Color.Red.ToVector4();
        protected Vector4 MORNINGTINT = Color.Gold.ToVector4();

        private float _tod;
        public bool dayMode = false;
        public bool nightMode = false;
        public const int FOGNEAR = 14 * 16;//(BUILD_RANGE - 1) * 16;
        public const float FOGFAR = 16 * 16;//(BUILD_RANGE + 1) * 16;
        #endregion

        public SimpleRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;
            _world = world;
        }


        public void Initialize()
        {
            _vertexBuildChunkProcessor = new VertexBuildChunkProcessor(_graphicsDevice);
            _lightingChunkProcessor = new LightingChunkProcessor();

            Debug.WriteLine("Generate initial chunks");
            _world.visitChunks(DoGenerate, GENERATE_RANGE_HIGH);
            Debug.WriteLine("Light initial chunks");
            _world.visitChunks(DoLighting, LIGHT_RANGE);
            Debug.WriteLine("Build initial chunks");
            _world.visitChunks(DoBuild, BUILD_RANGE);
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks_APR28_3");
            _solidBlockEffect = content.Load<Effect>("Effects\\SolidBlockEffect");
            _waterBlockEffect = content.Load<Effect>("Effects\\WaterBlockEffect");

        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {

        }

        public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            DrawSolid(gameTime);
            DrawWater(gameTime);
        }

        #region DrawSolid
        private void DrawSolid(GameTime gameTime)
        {

            _tod = _world.tod;

            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _solidBlockEffect.Parameters["View"].SetValue(_camera.View);
            _solidBlockEffect.Parameters["Projection"].SetValue(_camera.Projection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(_camera.Position);
            _solidBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _solidBlockEffect.Parameters["Texture1"].SetValue(_textureAtlas);

            _solidBlockEffect.Parameters["HorizonColor"].SetValue(HORIZONCOLOR);
            _solidBlockEffect.Parameters["NightColor"].SetValue(NIGHTCOLOR);

            _solidBlockEffect.Parameters["MorningTint"].SetValue(MORNINGTINT);
            _solidBlockEffect.Parameters["EveningTint"].SetValue(EVENINGTINT);

            _solidBlockEffect.Parameters["SunColor"].SetValue(SUNCOLOR);
            _solidBlockEffect.Parameters["timeOfDay"].SetValue(_tod);

            BoundingFrustum viewFrustum = new BoundingFrustum(_camera.View * _camera.Projection);

            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (EffectPass pass in _solidBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (Chunk chunk in _world.Chunks.Values)
                {
                    if (chunk == null) continue;

                    if (chunk.BoundingBox.Intersects(viewFrustum) && chunk.IndexBuffer != null)
                    {
                        if (chunk.IndexBuffer.IndexCount > 0)
                        {
                            _graphicsDevice.SetVertexBuffer(chunk.VertexBuffer);
                            _graphicsDevice.Indices = chunk.IndexBuffer;
                            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, chunk.VertexBuffer.VertexCount, 0, chunk.IndexBuffer.IndexCount / 3);
                        }

                    }
                }
            }
        }
        #endregion
        #region DrawWater
        float rippleTime = 0;
        private void DrawWater(GameTime gameTime)
        {
            rippleTime += 0.1f;

            _tod = _world.tod;

            _waterBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _waterBlockEffect.Parameters["View"].SetValue(_camera.View);
            _waterBlockEffect.Parameters["Projection"].SetValue(_camera.Projection);
            _waterBlockEffect.Parameters["CameraPosition"].SetValue(_camera.Position);
            _waterBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _waterBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _waterBlockEffect.Parameters["Texture1"].SetValue(_textureAtlas);
            _waterBlockEffect.Parameters["SunColor"].SetValue(SUNCOLOR);

            _waterBlockEffect.Parameters["HorizonColor"].SetValue(HORIZONCOLOR);
            _waterBlockEffect.Parameters["NightColor"].SetValue(NIGHTCOLOR);

            _waterBlockEffect.Parameters["MorningTint"].SetValue(MORNINGTINT);
            _waterBlockEffect.Parameters["EveningTint"].SetValue(EVENINGTINT);

            _waterBlockEffect.Parameters["timeOfDay"].SetValue(_tod);
            _waterBlockEffect.Parameters["RippleTime"].SetValue(rippleTime);

            BoundingFrustum viewFrustum = new BoundingFrustum(_camera.View * _camera.Projection);

            _graphicsDevice.BlendState = BlendState.NonPremultiplied;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (EffectPass pass in _waterBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (Chunk chunk in _world.Chunks.Values)
                {
                    if (chunk == null) continue;

                    if (chunk.BoundingBox.Intersects(viewFrustum) && chunk.waterVertexBuffer != null)
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
        #endregion

      

        public void Stop()
        {

        }

        private Chunk DoLighting(Vector3i chunkIndex)
        {
            Chunk chunk = _world.Chunks[chunkIndex.X,chunkIndex.Z];
            return DoLighting(chunk);
        }

        private Chunk DoLighting(Chunk chunk)
        {
            _lightingChunkProcessor.ProcessChunk(chunk);
            return chunk;
        }

        private Chunk DoBuild(Vector3i chunkIndex)
        {
            Chunk chunk = _world.Chunks[chunkIndex.X, chunkIndex.Z]; 
            return DoBuild(chunk);
        }

        private Chunk DoBuild(Chunk chunk)
        {
            _vertexBuildChunkProcessor.ProcessChunk(chunk);
            return chunk;
        }

        private Chunk DoGenerate(Vector3i chunkIndex)
        {
            Chunk chunk = new Chunk(_world, chunkIndex);
            return DoGenerate(chunk);
        }

        private Chunk DoGenerate(Chunk chunk)
        {
            _world.Chunks[chunk.Index.X, chunk.Index.Z] = chunk;
            _world.Generator.Generate(chunk);
            return chunk;
        }

    }
}
