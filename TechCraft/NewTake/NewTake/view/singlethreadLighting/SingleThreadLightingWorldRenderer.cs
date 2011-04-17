#region license

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
    class SingleThreadLightingWorldRenderer : WorldRenderer
    {
        private Texture2D ambientOcclusionMap;
        
        // SkyDome
        Model skyDome;
        Matrix projectionMatrix;
        Texture2D cloudMap;
        float rotation;

        // GPU generated clouds
        Texture2D cloudStaticMap;
        RenderTarget2D cloudsRenderTarget;
        Effect _perlinNoiseEffect;
        VertexPositionTexture[] fullScreenVertices;

        public SingleThreadLightingWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world) :
            base(graphicsDevice, camera, world) { }

        public override void loadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks");
            _solidBlockEffect = content.Load<Effect>("Effects\\LightingAOBlockEffect");

            // SkyDome
            skyDome = content.Load<Model>("Models\\dome");
            skyDome.Meshes[0].MeshParts[0].Effect = content.Load<Effect>("Effects\\SkyDome");
            cloudMap = content.Load<Texture2D>("Textures\\cloudMap");
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.3f, 1000.0f);

            // GPU Generated Clouds
            _perlinNoiseEffect = content.Load<Effect>("Effects\\PerlinNoise");
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            cloudsRenderTarget = new RenderTarget2D(GraphicsDevice,pp.BackBufferWidth,pp.BackBufferHeight,true,SurfaceFormat.Color,DepthFormat.None);
            cloudStaticMap = CreateStaticMap(32);
            fullScreenVertices = SetUpFullscreenVertices();
        }

        public override void DoBuild(Vector3i vector)
        {
            Chunk chunk = world.viewableChunks[vector.X, vector.Z];

            this.ChunkRenderers[chunk.Index].DoLighting();
            //chunk.Renderer.DoLighting();
            
            // Build a vertex buffer for this chunks
            //chunk.Renderer.BuildVertexList();
            this.ChunkRenderers[chunk.Index].BuildVertexList();
            
            // Add the renderer to the list so that it is drawn
            //TODO ?????????? ChunkRenderers.Add(chunk.Index, chunk.Renderer);

            chunk.built = true;
        }

        public override void DoGenerate(Vector3i vector)
        {
            // Create a new chunk
            Chunk chunk = new Chunk(world,vector);
            // Assign a renderer
            ChunkRenderer cRenderer = new SingleThreadLightingChunkRenderer(GraphicsDevice, world, chunk);
            this.ChunkRenderers.Add(chunk.Index,cRenderer);
            // Generate the chunk with the current generator
            world.Generator.Generate(chunk);
            // Calculate lighting
            cRenderer.DoLighting();
            // Store the chunk in the view
            world.viewableChunks[vector.X, vector.Z] = chunk;

            chunk.generated = true;
        }

        private void DrawSkyDome(Matrix currentViewMatrix)
        {

            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);
            //rotation += 0.0005f;
            rotation = 0;

            Matrix wMatrix = Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(0, 0, 0) * Matrix.CreateScale(100) * Matrix.CreateTranslation(camera.Position);
            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;

                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["xTexture"].SetValue(cloudMap);
                }
                mesh.Draw();
            }
        }

        private Texture2D CreateStaticMap(int resolution)
        {
            Random rand = new Random();
            Color[] noisyColors = new Color[resolution * resolution];
            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                    noisyColors[x + y * resolution] = new Color(new Vector3((float)rand.Next(1000) / 1000.0f, 0, 0));

            Texture2D noiseImage = new Texture2D(GraphicsDevice, resolution, resolution, true, SurfaceFormat.Color);
            noiseImage.SetData(noisyColors);
            return noiseImage;
        }

        private VertexPositionTexture[] SetUpFullscreenVertices()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 1));
            vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 0));

            return vertices;
        }

        private void GeneratePerlinNoise(float time)
        {
            GraphicsDevice.SetRenderTarget(cloudsRenderTarget);
            GraphicsDevice.Clear(Color.White);

            _perlinNoiseEffect.CurrentTechnique = _perlinNoiseEffect.Techniques["PerlinNoise"];
            _perlinNoiseEffect.Parameters["xTexture"].SetValue(cloudStaticMap);
            _perlinNoiseEffect.Parameters["xOvercast"].SetValue(1.1f);
            _perlinNoiseEffect.Parameters["xTime"].SetValue(time / 1000.0f);

            foreach (EffectPass pass in _perlinNoiseEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, fullScreenVertices, 0, 2);
            }

            GraphicsDevice.SetRenderTarget(null);
            cloudMap = cloudsRenderTarget;
        }

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            //currently a copy paste of base class but currently only :)

            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);

            // Generate the clouds
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
            GeneratePerlinNoise(time);

            GraphicsDevice.Clear(Color.White);
            GraphicsDevice.RasterizerState = !this._wireframed ? this._normalRaster : this._wireframedRaster;

            // Draw the skyDome
            DrawSkyDome(camera.View);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _solidBlockEffect.Parameters["View"].SetValue(camera.View);
            _solidBlockEffect.Parameters["Projection"].SetValue(camera.Projection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            _solidBlockEffect.Parameters["FogColor"].SetValue(Color.White.ToVector4());
            _solidBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _solidBlockEffect.Parameters["Texture1"].SetValue(_textureAtlas);

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
        }
        #endregion

    }
}
