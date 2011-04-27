using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;
using NewTake.profiling;
using NewTake.view.blocks;

namespace NewTake.view.renderers
{
    public class DiagnosticWorldRenderer : IRenderer
    {
        private BasicEffect _effect;
        private GraphicsDevice _graphicsDevice;
        private FirstPersonCamera _camera;
        private World _world;

        public DiagnosticWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;
            _world = world;
        }

        public void Initialize()
        {
            _effect = new BasicEffect(_graphicsDevice);
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {
            BoundingFrustum viewFrustum = new BoundingFrustum(_camera.View * _camera.Projection);

            foreach (Chunk chunk in _world.viewableChunks.Values)
            {
                if (chunk.BoundingBox.Intersects(viewFrustum))
                {
                    switch (chunk.State)
                    {
                        case ChunkState.AwaitingGenerate :
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Red);
                            break;
                        case ChunkState.Generating:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Pink);
                            break;
                        case ChunkState.AwaitingLighting:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Orange);
                            break;
                        case ChunkState.Lighting:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Yellow);
                            break;
                        case ChunkState.AwaitingBuild:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Green);
                            break;
                        case ChunkState.Building    :
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.LightGreen);
                            break;
                        case ChunkState.AwaitingRelighting:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Black);
                            break;
                    }
                }
            }
        }

        public void LoadContent(ContentManager content)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
