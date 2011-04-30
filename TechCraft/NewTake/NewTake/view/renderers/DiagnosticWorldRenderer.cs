#region License

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;
using NewTake.profiling;
using NewTake.view.blocks;
#endregion

namespace NewTake.view.renderers
{
    public class DiagnosticWorldRenderer : IRenderer
    {

        #region Fields
        private BasicEffect _effect;
        private GraphicsDevice _graphicsDevice;
        private FirstPersonCamera _camera;
        private World _world;

        #region debugFont
        SpriteBatch debugSpriteBatch;
        SpriteFont debugFont;
        Texture2D debugRectTexture;
        bool debugRectangle = true;
        #endregion

        #endregion

        public DiagnosticWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;
            _world = world;
        }

        public void Initialize()
        {
            _effect = new BasicEffect(_graphicsDevice);

            #region debugFont Rectangle
            debugRectTexture = new Texture2D(_graphicsDevice, 1, 1);
            Color[] texcol = new Color[1];
            debugRectTexture.GetData(texcol);
            texcol[0] = Color.Black;
            debugRectTexture.SetData(texcol);
            #endregion
        }

        public void LoadContent(ContentManager content)
        {
            debugSpriteBatch = new SpriteBatch(_graphicsDevice);
            debugFont = content.Load<SpriteFont>("Fonts\\debug");
        }

        public void Update(GameTime gameTime)
        {

        }

        #region Draw
        public void Draw(GameTime gameTime)
        {
            BoundingFrustum viewFrustum = new BoundingFrustum(_camera.View * _camera.Projection);

            int totalChunksCounter = 0;
            int awaitingGenerateCounter = 0;
            int generatingCounter = 0;
            int awaitingLightingCounter = 0;
            int lightingCounter = 0;
            int awaitingBuildCounter = 0;
            int awaitingRebuildCounter = 0;
            int buildingCounter = 0;
            int awaitingRelightingCounter = 0;
            int readyCounter = 0;

            foreach (Chunk chunk in _world.Chunks.Values)
            {
                //if (chunk.BoundingBox.Intersects(viewFrustum))
                //{
                    switch (chunk.State)
                    {
                        case ChunkState.AwaitingGenerate:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Red);
                            awaitingGenerateCounter++;
                            break;
                        case ChunkState.Generating:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Pink);
                            generatingCounter++;
                            break;
                        case ChunkState.AwaitingLighting:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Orange);
                            awaitingLightingCounter++;
                            break;
                        case ChunkState.Lighting:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Yellow);
                            lightingCounter++;
                            break;
                        case ChunkState.AwaitingBuild:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Green);
                            awaitingBuildCounter++;
                            break;
                        case ChunkState.AwaitingRebuild:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Green);
                            awaitingRebuildCounter++;
                            break;
                        case ChunkState.Building:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.LightGreen);
                            buildingCounter++;
                            break;
                        case ChunkState.AwaitingRelighting:
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Black);
                            awaitingRelightingCounter++;
                            break;
                        case ChunkState.Ready:
                            readyCounter++;
                            break;
                        default:
                            Debug.WriteLine("Unchecked State: {0}", chunk.State);
                            Utility.DrawBoundingBox(chunk.BoundingBox, _graphicsDevice, _effect, Matrix.Identity, _camera.View, _camera.Projection, Color.Blue);
                            break;
                    }
                //}
                totalChunksCounter++;
            }

            #region OSD debug texts
            debugSpriteBatch.Begin();

            if (debugRectangle)
            {
                Rectangle r = new Rectangle(680, 0, 120, 144);
                debugSpriteBatch.Draw(debugRectTexture, r, Color.Black);
            }
            debugSpriteBatch.DrawString(debugFont, "Chunks: " + totalChunksCounter.ToString(), new Vector2(680, 0), Color.White);
            debugSpriteBatch.DrawString(debugFont, "A.Generate: " + awaitingGenerateCounter.ToString(), new Vector2(680, 16), Color.White);
            debugSpriteBatch.DrawString(debugFont, "Generating: " + generatingCounter.ToString(), new Vector2(680, 32), Color.White);
            debugSpriteBatch.DrawString(debugFont, "A.Lighting: " + awaitingLightingCounter.ToString(), new Vector2(680, 48), Color.White);
            debugSpriteBatch.DrawString(debugFont, "Lighting: " + lightingCounter.ToString(), new Vector2(680, 64), Color.White);
            debugSpriteBatch.DrawString(debugFont, "A.Build: " + awaitingBuildCounter.ToString(), new Vector2(680, 80), Color.White);
            debugSpriteBatch.DrawString(debugFont, "A.Rebuild: " + awaitingRebuildCounter.ToString(), new Vector2(680, 96), Color.White);
            debugSpriteBatch.DrawString(debugFont, "A.Relighting: " + awaitingRelightingCounter.ToString(), new Vector2(680, 112), Color.White);
            debugSpriteBatch.DrawString(debugFont, "Ready: " + readyCounter.ToString(), new Vector2(680, 128), Color.White); 
            debugSpriteBatch.End();
            #endregion
        }
        #endregion

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
