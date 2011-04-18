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
using System.Diagnostics;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using NewTake.model;
#endregion

namespace NewTake.view
{
    class WorldRenderer
    {
        #region inits

        public const float FARPLANE = 140 * 2;
        public const int FOGNEAR = 90 * 2;
        public const int FOGFAR = 140 * 2;

        protected World world;
        protected readonly GraphicsDevice GraphicsDevice;
        protected Dictionary<Vector3i, ChunkRenderer> ChunkRenderers;
        protected Effect _solidBlockEffect;
        protected Texture2D _textureAtlas;
        public readonly FirstPersonCamera camera;
        protected Vector3i previousChunkIndex;

        //public TimeSpan removeTime = TimeSpan.Zero;
        //public Queue<Chunk> _toBuild;
        //public bool _running = true;
        //public Thread _buildingThread;

        protected readonly RasterizerState _wireframedRaster = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        protected readonly RasterizerState _normalRaster = new RasterizerState() { CullMode = CullMode.CullCounterClockwiseFace, FillMode = FillMode.Solid };
        protected bool _wireframed = false;

        #endregion

        public void ToggleRasterMode()
        {
            this._wireframed = !this._wireframed;
        }

        public WorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
        {
            this.world = world;
            this.GraphicsDevice = graphicsDevice;
            ChunkRenderers = new Dictionary<Vector3i, ChunkRenderer>();

            // Generate the initial chunks
            var generatingWatch = new Stopwatch();
            generatingWatch.Start();
            Debug.Write("Generating initial chunks.. ");
            world.visitChunks(DoGenerate);
            generatingWatch.Stop();
            Debug.WriteLine(generatingWatch.Elapsed);

            // Build the initial chunks
            var buildWatch = new Stopwatch();
            buildWatch.Start();
            Debug.Write("Building initial chunks.. ");
            world.visitChunks(DoBuild);
            buildWatch.Stop();
            Debug.WriteLine(buildWatch.Elapsed);

            this.camera = camera;

            this.previousChunkIndex = new Vector3i();
        }

        public virtual void LoadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks)");
            _solidBlockEffect = content.Load<Effect>("Effects\\SolidBlockEffect");
        }

        public virtual void DoBuild(Vector3i vector)
        {
            throw new Exception("Must override Build");
        }

        public virtual void DoGenerate(Vector3i vector)
        {
            throw new Exception("Must override Generate");
        }

        #region Update
        public virtual void Update(GameTime gameTime)
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
                                world.viewableChunks.Remove( j, l);
                                ChunkRenderers.Remove(newIndex);
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
                            if ((!chunk.built) && (chunk.generated))//TODO why can it be null now 
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
        public virtual void Draw(GameTime gameTime)
        {
            // A renderer must implement it's own draw method
            throw new Exception("Must override draw");
        }
        #endregion

    }
}
