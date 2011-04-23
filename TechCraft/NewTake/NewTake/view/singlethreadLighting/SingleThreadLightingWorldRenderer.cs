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
using System.Threading;
using System.Diagnostics;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;
using NewTake.profiling;
using NewTake.view.blocks;
#endregion

namespace NewTake.view
{
    class SingleThreadLightingWorldRenderer : WorldRenderer
    {
        #region Fields

        private BasicEffect _debugEffect;

        private VertexBuildChunkProcessor _vertexBuildChunkProcessor;
        private LightingChunkProcessor _lightingChunkProcessor;

        #endregion

        public SingleThreadLightingWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world) :
            base(graphicsDevice, camera, world) { }

        #region Initialize
        public override void Initialize()
        {
            _vertexBuildChunkProcessor = new VertexBuildChunkProcessor(GraphicsDevice);
            _lightingChunkProcessor = new LightingChunkProcessor();

            base.Initialize();
        }
        #endregion

        public override void LoadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks");
            _solidBlockEffect = content.Load<Effect>("Effects\\LightingAOBlockEffect");
            _debugEffect = new BasicEffect(GraphicsDevice);

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
                cloudsRenderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
                cloudStaticMap = CreateStaticMap(32);
                fullScreenVertices = SetUpFullscreenVertices();
            }
            #endregion

        }

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
                // Clear down the chunk lighting 
                _lightingChunkProcessor.InitChunk(chunk);
            }
            // Assign a renderer
            //ChunkRenderer cRenderer = new MultiThreadLightingChunkRenderer(GraphicsDevice, world, chunk);
            //this.ChunkRenderers.TryAdd(chunk.Index,cRenderer);           

            //chunk.generated = true;
            chunk.State = ChunkState.AwaitingBuild;
        }
        #endregion

        #region DoBuild
        public override void DoBuild(Vector3i vector)
        {
            Chunk chunk = world.viewableChunks[vector.X, vector.Z];
            // Propogate the chunk lighting
            _lightingChunkProcessor.ProcessChunk(chunk);
            _vertexBuildChunkProcessor.ProcessChunk(chunk);
            //chunk.built = true;
            chunk.State = ChunkState.Ready;
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {

            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);
            if (cloudsEnabled)
            {
                // Generate the clouds
                float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
                base.GeneratePerlinNoise(time);
            }

            GraphicsDevice.Clear(Color.White);
            GraphicsDevice.RasterizerState = !this._wireframed ? this._normalRaster : this._wireframedRaster;

            // Draw the skyDome
            base.DrawSkyDome(camera.View);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _solidBlockEffect.Parameters["View"].SetValue(camera.View);
            _solidBlockEffect.Parameters["Projection"].SetValue(camera.Projection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            _solidBlockEffect.Parameters["FogColor"].SetValue(FOGCOLOR);
            _solidBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _solidBlockEffect.Parameters["Texture1"].SetValue(_textureAtlas);

            _solidBlockEffect.Parameters["SunColor"].SetValue(SUNCOLOR);

            foreach (EffectPass pass in _solidBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (Chunk chunk in world.viewableChunks.Values)
                {
                    //if (chunk.BoundingBox.Intersects(viewFrustum) && chunk.generated && !chunk.dirty)
                    if (chunk.BoundingBox.Intersects(viewFrustum) && (chunk.State == ChunkState.Ready) && !chunk.dirty)
                    {
                        base.DrawChunk(chunk);
                    }
                }
            }

            // Diagnostic rendering
            if (diagnosticsMode)
            {
                foreach (Chunk chunk in world.viewableChunks.Values)
                {
                    if (chunk.BoundingBox.Intersects(viewFrustum))
                    {
                        if (chunk.broken)
                        {
                            Utility.DrawBoundingBox(chunk.BoundingBox, GraphicsDevice, _debugEffect, Matrix.Identity, camera.View, camera.Projection, Color.Red);
                        }
                        else
                        {
                            //if (!chunk.built)
                            if (chunk.State != ChunkState.Ready)
                            {
                                // Draw the bounding box for the chunk so we can see them
                                Utility.DrawBoundingBox(chunk.BoundingBox, GraphicsDevice, _debugEffect, Matrix.Identity, camera.View, camera.Projection, Color.Green);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            uint x = (uint)camera.Position.X;
            uint z = (uint)camera.Position.Z;

            uint cx = x / Chunk.SIZE.X;
            uint cz = z / Chunk.SIZE.Z;

            Vector3i currentChunkIndex = new Vector3i(cx, 0, cz);

            if (currentChunkIndex != previousChunkIndex)
            {
                previousChunkIndex = currentChunkIndex;

                Chunk fromChunk = world.viewableChunks[currentChunkIndex.X, currentChunkIndex.Z];

                Cardinal facing = camera.FacingCardinal();

                Process(fromChunk, facing, 0);


            }
        }
        #endregion

        #region Process
        private void Process(Chunk fromChunk, Cardinal cardinal, int recursion)
        {
            if (fromChunk==null || recursion == 2 ) return;

            try
            {
                Vector3i chunkIndexAdd = new Vector3i();
                Vector3i chunkIndexRemove = new Vector3i();

                Chunk nextChunk = fromChunk.GetNeighbour(cardinal);
                SignedVector3i sv = Cardinals.VectorFrom(cardinal);
                chunkIndexAdd = new Vector3i((uint)(nextChunk.Index.X + sv.X), 0, (uint)(nextChunk.Index.Z + sv.Z));

                SignedVector3i removeDelta = Cardinals.OppositeVectorFrom(cardinal) * World.VIEW_DISTANCE_NEAR_X;
                chunkIndexRemove = new Vector3i((uint)(nextChunk.Index.X + removeDelta.X), 0, (uint)(nextChunk.Index.Z + removeDelta.Z));

                // Remove opposite chunk
                //Debug.WriteLine("Process Remove {0}, {1}, {2}", nextChunk.Index, cardinal, recursion);
                world.viewableChunks.Remove(chunkIndexRemove.X, chunkIndexRemove.Z);//null safe
            
                // Generate & Build cardinal chunk
                if (world.viewableChunks[chunkIndexAdd.X, chunkIndexAdd.Z] == null)
                {
                    //Debug.WriteLine("Process Add {0}, {1}, {2}", nextChunk.Index, cardinal, recursion);
                    DoGenerate(chunkIndexAdd);
                    DoBuild(chunkIndexAdd);
                    Process(nextChunk, cardinal, recursion + 1);

                   /* if (cardinal == Cardinal.N)
                    {
                        Process(fromChunk.E, Cardinal.N, recursion );
                        Process(fromChunk.W, Cardinal.N, recursion );
                    }*/
                  
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Process exception");
            }
        }
        #endregion

        #region Update_nonCardinal
        public void Update_nonCardinal(GameTime gameTime)
        {

            uint x = (uint)camera.Position.X;
            uint z = (uint)camera.Position.Z;

            uint cx = x / Chunk.SIZE.X;
            uint cz = z / Chunk.SIZE.Z;

            Vector3i currentChunkIndex = new Vector3i(cx, 0, cz);

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
                                //chunk.visible = false;
                                world.viewableChunks.Remove(j, l);
                                //ChunkRenderer outChunkRenderer;
                                //ChunkRenderers.TryRemove(newIndex,out outChunkRenderer);
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
                                DoGenerate(newIndex);
                                //Debug.WriteLine("Built chunk at {0},{1},{2}", newIndex.X, newIndex.Y, newIndex.Z);
                            }
                        }
                        // Build Chunks
                        else
                        {
                            Chunk chunk = world.viewableChunks[j, l];
                            //if ((!chunk.built) && (chunk.generated))
                            if (chunk.State == ChunkState.AwaitingBuild)
                            {
                                // We have a chunk in view - it has been generated but we haven't built a vertex buffer for it
                                Vector3i newIndex = currentChunkIndex + new Vector3i((j - cx), 0, (l - cz));    // This is the chunk in the loop, offset from the camera
                                DoBuild(newIndex);
                                //Debug.WriteLine("Vertices built at {0},{1},{2}", newIndex.X, newIndex.Y, newIndex.Z);
                            }
                        }

                    }
                }
            }// end if
            BoundingFrustum viewFrustum = new BoundingFrustum(camera.View * camera.Projection);
        }
        #endregion

    }
}
