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

#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
//using System.Collections.Concurrent;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;
#endregion

namespace NewTake.view
{
    class MultiThreadLightingWorldRenderer : WorldRenderer
    {
        #region inits

        private const bool cloudsEnabled = true;

        private Task buildTask;
        private Task generateTask;
        private Task removeTask;

        //private Texture2D ambientOcclusionMap;

        //private readonly BlockingCollection<Vector3i> _generationQueue = new BlockingCollection<Vector3i>(); // uses concurrent queues by default.
        //private readonly BlockingCollection<Vector3i> _buildingQueue = new BlockingCollection<Vector3i>();

        #endregion

        #region SkyDome and Clouds
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
        #endregion

        public MultiThreadLightingWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world) :
            base(graphicsDevice, camera, world) { }

        public override void LoadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks");
            _solidBlockEffect = content.Load<Effect>("Effects\\LightingAOBlockEffect");

            #region SkyDome and Clouds
            // SkyDome
            skyDome = content.Load<Model>("Models\\dome");
            skyDome.Meshes[0].MeshParts[0].Effect = content.Load<Effect>("Effects\\SkyDome");
            cloudMap = content.Load<Texture2D>("Textures\\cloudMap");
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.3f, 1000.0f);

            // GPU Generated Clouds
            if (cloudsEnabled)
            {
                _perlinNoiseEffect = content.Load<Effect>("Effects\\PerlinNoise");
                PresentationParameters pp = GraphicsDevice.PresentationParameters;
                //the mipmap does not work on some pc ( i5 laptops at least), with mipmap false it s fine 
                cloudsRenderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
                cloudStaticMap = CreateStaticMap(32);
                fullScreenVertices = SetUpFullscreenVertices();
            }
            #endregion

        }

        #region DoBuild
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
        #endregion

        #region DoGenerate
        public override void DoGenerate(Vector3i index)
        {

            Chunk chunk = world.viewableChunks.load(index);            

            if (chunk == null)
            {
                // Create a new chunk
                chunk = new Chunk(world, index);
                // Generate the chunk with the current generator
                world.Generator.Generate(chunk);
            }
                // Assign a renderer
            ChunkRenderer cRenderer = new MultiThreadLightingChunkRenderer(GraphicsDevice, world, chunk);
            this.ChunkRenderers.TryAdd(chunk.Index,cRenderer);
            
            // Calculate lighting
            cRenderer.DoLighting();
            // Store the chunk in the view
            

            chunk.generated = true;
        }
        #endregion

        #region DrawSkyDome
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
                    currentEffect.Parameters["SunColor"].SetValue(Color.Blue.ToVector4());
                    currentEffect.Parameters["HorizonColor"].SetValue(Color.White.ToVector4());
                }
                mesh.Draw();
            }
        }
        #endregion

        #region Generate Clouds
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
            _perlinNoiseEffect.Parameters["xOvercast"].SetValue(0.8f);
            _perlinNoiseEffect.Parameters["xTime"].SetValue(time / 1000.0f);

            foreach (EffectPass pass in _perlinNoiseEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, fullScreenVertices, 0, 2);
            }

            GraphicsDevice.SetRenderTarget(null);
            cloudMap = cloudsRenderTarget;
        }
        #endregion

        //private void Worker()
        //{
        //    Vector3i newIndex;
        //    BlockingCollection<Vector3i>.TakeFromAny(new[] { _generationQueue, _buildingQueue }, out newIndex);

        //    if (world.viewableChunks[newIndex.X, newIndex.Z] == null)
        //    {
        //        //Debug.WriteLine("Worker Generate {0},{1}", newIndex.X, newIndex.Z);
        //        DoGenerate(newIndex);
        //    }
        //    else
        //    {
        //        Chunk chunk = world.viewableChunks[newIndex.X, newIndex.Z];
        //        if (chunk.dirty)
        //            //Debug.WriteLine("Worker Build {0},{1}", newIndex.X, newIndex.Z);
        //            DoBuild(newIndex);
        //    }
        //}

        #region Update
        public override void Update(GameTime gameTime)
        {
            uint x = (uint)camera.Position.X;
            uint z = (uint)camera.Position.Z;

            uint cx = x / Chunk.CHUNK_XMAX;
            uint cz = z / Chunk.CHUNK_ZMAX;

            uint lx = x % Chunk.CHUNK_XMAX;
            uint lz = z % Chunk.CHUNK_ZMAX;

            Vector3i currentChunkIndex = world.viewableChunks[cx, cz].Index;    // This is the chunk in which the camera currently resides

            if (currentChunkIndex != previousChunkIndex)
            {
                previousChunkIndex = currentChunkIndex;

                // Loop through all possible chunks around the camera in both X and Z directions
                for (uint j = cx - (World.VIEW_DISTANCE_FAR_X + 1); j < cx + (World.VIEW_DISTANCE_FAR_X + 1); j++)
                {
                    for (uint l = cz - (World.VIEW_DISTANCE_FAR_Z + 1); l < cz + (World.VIEW_DISTANCE_FAR_Z + 1); l++)
                    {
                        int distancecx = (int)(cx - j);        // The distance from the camera to the chunk in the X direction
                        int distancecz = (int)(cz - l);        // The distance from the camera to the chunk in the Z direction

                        if (distancecx < 0) distancecx = 0 - distancecx;        // If the distance is negative (behind the camera) make it positive
                        if (distancecz < 0) distancecz = 0 - distancecz;        // If the distance is negative (behind the camera) make it positive

                        // Remove Chunks
                        if ((distancecx > World.VIEW_DISTANCE_NEAR_X) || (distancecz > World.VIEW_DISTANCE_NEAR_Z))
                        {
                            if ((world.viewableChunks[j, l] != null)) // Chunk is created, therefore remove
                            {
                                Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));    // This is the chunk in the loop, offset from the camera
                                Chunk chunk = world.viewableChunks[j, l];
                                chunk.visible = false;
                                world.viewableChunks.Remove(j, l);
                                ChunkRenderer cr;
                                removeTask = Task.Factory.StartNew(() => ChunkRenderers.TryRemove(newIndex,out cr));
                                //removeTask.Wait();

                                //ChunkRenderers.Remove(newIndex);
                                //Debug.WriteLine("Removed chunk at {0},{1},{2}", chunk.Position.X, chunk.Position.Y, chunk.Position.Z);
                            }
                            else
                            {
                                //Debug.WriteLine("[Remove] chunk not found at at {0},0,{1}", j, l);
                            }
                        }
                        // Generate Chunks
                        else if ((distancecx > World.VIEW_CHUNKS_X) || (distancecz > World.VIEW_CHUNKS_Z))
                        {
                            // A new chunk is coming into view - we need to generate or load it
                            if (world.viewableChunks[j, l] == null) // Chunk is not created or loaded, therefore create - 
                            {
                                Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));    // This is the chunk in the loop, offset from the camera

                                try
                                {
                                    //this._generationQueue.Add(newIndex);
                                    //Task buildTask = Task.Factory.StartNew(Worker);

                                    generateTask = Task.Factory.StartNew(() => DoGenerate(newIndex));
                                    //generateTask.Wait();

                                    //DoGenerate(newIndex);
                                    //Debug.WriteLine("Built chunk at {0},{1},{2}", newIndex.X, newIndex.Y, newIndex.Z);
                                }
                                catch (AggregateException ae)
                                {
                                    Debug.WriteLine("Exception {0}", ae);
                                }
                            }
                        }
                        // Build Chunks
                        else
                        {
                            Chunk chunk = world.viewableChunks[j, l];
                            if ((!chunk.built) && (chunk.generated))//TODO why can it be null now 
                            {
                                // We have a chunk in view - it has been generated but we haven't built a vertex buffer for it
                                Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));    // This is the chunk in the loop, offset from the camera
                                
                                try
                                {
                                    //this._buildingQueue.Add(newIndex);
                                    //Task.Factory.StartNew(Worker);

                                    buildTask = Task.Factory.StartNew(() => DoBuild(newIndex));
                                    //buildTask.Wait();

                                    //DoBuild(newIndex);
                                    //Debug.WriteLine("Vertices built at {0},{1},{2}", newIndex.X, newIndex.Y, newIndex.Z);
                                }
                                catch (AggregateException ae)
                                {
                                    Debug.WriteLine("Exception {0}", ae);
                                }
                            }
                        }
                    }
                }
            }// end if

            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);

            foreach (ChunkRenderer chunkRenderer in ChunkRenderers.Values)
            {
                if (chunkRenderer == null) return;

                if (chunkRenderer.isInView(viewFrustum))
                {
                    // Only update chunks which have a valid vertex buffer
                    if (chunkRenderer.chunk.built)
                    {
                        chunkRenderer.Update(gameTime);
                    }
                }
            }
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            //currently a copy paste of base class but currently only :)

            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);
            if (cloudsEnabled)
            {
                // Generate the clouds
                float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
                GeneratePerlinNoise(time);
            }

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
            _solidBlockEffect.Parameters["FogColor"].SetValue(Color.Black.ToVector4());
            _solidBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _solidBlockEffect.Parameters["Texture1"].SetValue(_textureAtlas);

            _solidBlockEffect.Parameters["SunColor"].SetValue(Color.White.ToVector3());

            foreach (EffectPass pass in _solidBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (ChunkRenderer chunkRenderer in ChunkRenderers.Values)
                {
                    if (chunkRenderer.isInView(viewFrustum) && chunkRenderer.chunk.generated && !chunkRenderer.chunk.dirty)
                    {
                        chunkRenderer.Draw(gameTime);
                    }
                }
            }
        }
        #endregion

    }
}
