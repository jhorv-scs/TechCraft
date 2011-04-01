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
        public const float FARPLANE = 140 * 8;
        public const int FOGNEAR = 90 * 8;
        public const int FOGFAR = 140 * 8;        
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

        public void draw(GameTime gameTime)
        {


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
                    chunkRenderer.draw(gameTime);
                }
            }
        
        }
    }
}
