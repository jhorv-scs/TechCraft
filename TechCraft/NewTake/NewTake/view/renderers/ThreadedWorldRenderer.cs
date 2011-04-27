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

namespace NewTake.view.renderers
{
    public class ThreadedWorldRenderer : IRenderer
    {

        #region Fields
        protected Effect _solidBlockEffect;
        protected Effect _waterBlockEffect;

        protected Texture2D _textureAtlas;

        private VertexBuildChunkProcessor _vertexBuildChunkProcessor;
        private LightingChunkProcessor _lightingChunkProcessor;

        private Queue<Vector3i> _generateQueue = new Queue<Vector3i>();
        private Queue<Vector3i> _buildQueue = new Queue<Vector3i>();
        private Queue<Vector3i> _lightingQueue = new Queue<Vector3i>();

        private Thread _workerThread;
        private GraphicsDevice _graphicsDevice;
        private FirstPersonCamera _camera;
        private World _world;

        private Vector3i _previousChunkIndex;

        public const int FOGNEAR = 300;
        public const float FOGFAR = FOGNEAR*2;

        protected Vector4 NIGHTCOLOR = Color.Black.ToVector4();
        public Vector4 SUNCOLOR = Color.White.ToVector4();
        protected Vector4 HORIZONCOLOR = Color.White.ToVector4();

        protected Vector4 EVENINGTINT = Color.Red.ToVector4();
        protected Vector4 MORNINGTINT = Color.Gold.ToVector4();

        private float _tod;
        private bool _running = true;

        public bool dayMode = false;
        public bool nightMode = false;
        #endregion

        public ThreadedWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;
            _world = world;
        }

        #region Initialize
        public void Initialize()
        {
            _vertexBuildChunkProcessor = new VertexBuildChunkProcessor(_graphicsDevice);
            _lightingChunkProcessor = new LightingChunkProcessor();

            Debug.WriteLine("Generate initial chunks");
            _world.visitChunks(DoInitialGenerate,GENERATE_RANGE);
            Debug.WriteLine("Light initial chunks");
            _world.visitChunks(DoLighting,LIGHT_RANGE);
            Debug.WriteLine("Build initial chunks");
            _world.visitChunks(DoBuild,BUILD_RANGE);

            _workerThread = new Thread(new ThreadStart(WorkerThread));
            _workerThread.Priority = ThreadPriority.AboveNormal;
            _workerThread.IsBackground = true;
            _workerThread.Start();
        }
        #endregion

        public void LoadContent(ContentManager content)
        {
            _textureAtlas = content.Load<Texture2D>("Textures\\blocks");
            _solidBlockEffect = content.Load<Effect>("Effects\\SolidBlockEffect");
            _waterBlockEffect = content.Load<Effect>("Effects\\WaterBlockEffect");
        }

        public void QueueLighting(Vector3i chunkIndex)
        {
            lock (_lightingQueue)
            {
                _lightingQueue.Enqueue(chunkIndex);
            }
        }

        #region DoLighting
        private Chunk DoLighting(Vector3i chunkIndex) 
        {
            //Debug.WriteLine("DoLighting " + chunkIndex);
            Chunk chunk = _world.viewableChunks[chunkIndex.X, chunkIndex.Z];
            if (chunk.State == ChunkState.AwaitingLighting)
            {
                chunk.State = ChunkState.Lighting;
                _lightingChunkProcessor.ProcessChunk(chunk);
                chunk.State = ChunkState.AwaitingBuild;
            }
            if (chunk.State == ChunkState.AwaitingRelighting)
            {
                chunk.State = ChunkState.Lighting;
                _lightingChunkProcessor.ProcessChunk(chunk);
                chunk.State = ChunkState.AwaitingBuild;
                QueueBuild(chunkIndex);
            }
            return chunk;
        }
        #endregion

        public void QueueGenerate(Vector3i chunkIndex)
        {
            lock (_generateQueue)
            {
                _generateQueue.Enqueue(chunkIndex);
            }
        }

        #region DoInitialGenerate
        private Chunk DoInitialGenerate(Vector3i chunkIndex)
        {
            //Debug.WriteLine("DoGenerate " + chunkIndex);
            Chunk chunk = new Chunk(_world, chunkIndex);
            _world.viewableChunks[chunkIndex.X, chunkIndex.Z] = chunk;
            if (chunk.State == ChunkState.AwaitingGenerate)
            {
                chunk.State = ChunkState.Generating;
                _world.Generator.Generate(chunk);
                chunk.State = ChunkState.AwaitingLighting;
            }
            return chunk;
        }
        #endregion

        #region DoGenerate
        private Chunk DoGenerate(Vector3i chunkIndex)
        {
            //Debug.WriteLine("DoGenerate " + chunkIndex);
            Chunk chunk = _world.viewableChunks[chunkIndex.X, chunkIndex.Z];
            if (chunk == null)
            {
                // Thread sync issue - requeue
                QueueGenerate(chunkIndex);
                return null;
            }
            if (chunk.State == ChunkState.AwaitingGenerate)
            {
                chunk.State = ChunkState.Generating;
                _world.Generator.Generate(chunk);
                chunk.State = ChunkState.AwaitingLighting;
            }
            return chunk;
        }
        #endregion

        public void QueueBuild(Vector3i chunkIndex)
        {
            lock (_buildQueue)
            {
                _buildQueue.Enqueue(chunkIndex);
            }
        }

        #region DoBuild
        private Chunk DoBuild(Vector3i chunkIndex)
        {
            //Debug.WriteLine("DoBuild " + chunkIndex);
            Chunk chunk = _world.viewableChunks[chunkIndex.X, chunkIndex.Z];
            if (chunk == null) return null;
            if (chunk.State == ChunkState.AwaitingBuild || chunk.State == ChunkState.AwaitingRebuild)
            {
                chunk.State = ChunkState.Building;
                _vertexBuildChunkProcessor.ProcessChunk(chunk);
                chunk.State = ChunkState.Ready;
            }
            return chunk;
        }
        #endregion

        public void Draw(GameTime gameTime)
        {
            DrawSolid(gameTime);
            DrawWater(gameTime);
        }

        #region DrawSolid
        private void DrawSolid(GameTime gameTime)
        {

            _tod = _world.tod;

            if (_world.dayMode)
            {
                _tod = 12;
                _world.nightMode = false;
            }
            else if (_world.nightMode)
            {
                _tod = 0;
                _world.dayMode = false;
            }

            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _solidBlockEffect.Parameters["View"].SetValue(_camera.View);
            _solidBlockEffect.Parameters["Projection"].SetValue(_camera.Projection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(_camera.Position);
            //_solidBlockEffect.Parameters["FogColor"].SetValue(Color.White.ToVector4());
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

                foreach (Chunk chunk in _world.viewableChunks.Values)
                {
                    if (chunk.BoundingBox.Intersects(viewFrustum) && chunk.IndexBuffer != null)
                    {
                        lock (chunk)
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
            //_waterBlockEffect.Parameters["FogColor"].SetValue(Color.White.ToVector4());
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

                foreach (Chunk chunk in _world.viewableChunks.Values)
                {
                    if (chunk.BoundingBox.Intersects(viewFrustum) && chunk.waterVertexBuffer != null)
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
        #endregion

        private const byte REMOVE_RANGE = 16;
        private const byte GENERATE_RANGE = 15;
        private const byte LIGHT_RANGE = 14;
        private const byte BUILD_RANGE = 13;

        #region Update
        public void Update(GameTime gameTime)
        {
            uint cameraX = (uint) (_camera.Position.X / Chunk.SIZE.X);
            uint cameraZ = (uint) (_camera.Position.Z / Chunk.SIZE.Z);

            Vector3i currentChunkIndex = new Vector3i(cameraX, 0, cameraZ);
            //if (_previousChunkIndex != currentChunkIndex)
            //{
                _previousChunkIndex = currentChunkIndex;
                for (uint ix = cameraX - REMOVE_RANGE; ix < cameraX + REMOVE_RANGE; ix++)
                {
                    for (uint iz = cameraZ - REMOVE_RANGE; iz < cameraZ + REMOVE_RANGE; iz++)
                    {
                        int distX = (int)(ix - cameraX);
                        int distZ = (int)(iz - cameraZ);

                        if (distX < 0) distX = 0 - distX;
                        if (distZ < 0) distZ = 0 - distZ;

                        Vector3i chunkIndex = new Vector3i(ix, 0, iz);

                        if (distX >= REMOVE_RANGE || distZ >= REMOVE_RANGE)
                        {
                            if (_world.viewableChunks[ix, iz] != null)
                            {
                                _world.viewableChunks.Remove(ix, iz);
                            }
                            continue;
                        }
                        if (distX >= GENERATE_RANGE || distZ >= GENERATE_RANGE)
                        {
                            if (_world.viewableChunks[ix, iz] == null)
                            {
                                Chunk chunk = new Chunk(_world, chunkIndex);
                                chunk.State = ChunkState.AwaitingGenerate;
                                _world.viewableChunks[ix, iz] = chunk;
                                QueueGenerate(chunkIndex);
                            }
                            continue;
                        }
                        if (distX >= LIGHT_RANGE || distZ >= LIGHT_RANGE)
                        {
                            Chunk chunk = _world.viewableChunks[ix, iz];
                            if (chunk != null && chunk.State == ChunkState.AwaitingLighting)
                            {
                                QueueLighting(chunkIndex);
                            }
                            continue;
                        }
                        if (distX >= BUILD_RANGE || distZ >= BUILD_RANGE)
                        {
                            Chunk chunk = _world.viewableChunks[ix, iz];
                            if (chunk != null && chunk.State == ChunkState.AwaitingBuild)
                            {
                                QueueBuild(chunkIndex);
                            }
                            continue;
                        }
                        Chunk rebuildChunk = _world.viewableChunks[ix, iz];
                        if (rebuildChunk != null && rebuildChunk.State == ChunkState.AwaitingRelighting)
                        {
                            QueueLighting(chunkIndex);
                        }
                        if (rebuildChunk != null && rebuildChunk.State == ChunkState.AwaitingRebuild)
                        {
                            QueueBuild(chunkIndex);
                        }
                    }
                //}
            }
        }
        #endregion

        public void Stop()
        {
            _running = false;
        }

        #region WorkerThread
        private void WorkerThread()
        {
            Vector3i target = new Vector3i(0,0,0);
            bool foundGenerate, foundLighting, foundBuild;

            while (_running)
            {
                foundGenerate = false; foundLighting = false; foundBuild = false;

                // LOOK FOR CHUNKS REQUIRING GENERATION
                lock (_generateQueue)
                {
                    if (_generateQueue.Count > 0)
                    {
                        target = _generateQueue.Dequeue();
                        foundGenerate = true;
                    }
                }
                if (foundGenerate)
                {
                    DoGenerate(target);
                    continue;
                }

                // LOOK FOR CHUNKS REQUIRING LIGHTING
                lock (_lightingQueue)
                {
                    if (_lightingQueue.Count > 0)
                    {
                        target = _lightingQueue.Dequeue();
                        foundLighting = true;
                    }
                }
                if (foundLighting)
                {
                    DoLighting(target);
                    continue;
                }

                // LOOK FOR CHUNKS REQUIRING BUILD
                lock (_buildQueue)
                {
                    if (_buildQueue.Count > 0)
                    {
                        target = _buildQueue.Dequeue();
                        foundBuild = true;
                    }
                }
                if (foundBuild)
                {
                    DoBuild(target);
                    continue;
                }
                Thread.Sleep(10);
            }
        }
        #endregion

    }
}
