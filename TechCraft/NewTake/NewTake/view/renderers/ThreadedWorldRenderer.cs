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
    public class ThreadedWorldRenderer : IRenderer
    {
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

        public const float FOGFAR = 290;
        public const int FOGNEAR = 260;

        public Vector3 SUNCOLOR = Color.White.ToVector3();
        private float _tod;
        private bool _running = true;

        public ThreadedWorldRenderer(GraphicsDevice graphicsDevice, FirstPersonCamera camera, World world)
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

        public void QueueGenerate(Vector3i chunkIndex)
        {
            lock (_generateQueue)
            {
                _generateQueue.Enqueue(chunkIndex);
            }
        }

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

        public void QueueBuild(Vector3i chunkIndex)
        {
            lock (_buildQueue)
            {
                _buildQueue.Enqueue(chunkIndex);
            }
        }

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

        public void Draw(GameTime gameTime)
        {
            DrawSolid(gameTime);
            DrawWater(gameTime);
        }

        private void DrawSolid(GameTime gameTime)
        {

            _tod = _world.tod;

            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _solidBlockEffect.Parameters["View"].SetValue(_camera.View);
            _solidBlockEffect.Parameters["Projection"].SetValue(_camera.Projection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(_camera.Position);
            _solidBlockEffect.Parameters["FogColor"].SetValue(Color.White.ToVector4());
            _solidBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _solidBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _solidBlockEffect.Parameters["Texture1"].SetValue(_textureAtlas);
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

        float rippleTime = 0;
        private void DrawWater(GameTime gameTime)
        {
            rippleTime += 0.1f;

            _tod = _world.tod;

            _waterBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _waterBlockEffect.Parameters["View"].SetValue(_camera.View);
            _waterBlockEffect.Parameters["Projection"].SetValue(_camera.Projection);
            _waterBlockEffect.Parameters["CameraPosition"].SetValue(_camera.Position);
            _waterBlockEffect.Parameters["FogColor"].SetValue(Color.White.ToVector4());
            _waterBlockEffect.Parameters["FogNear"].SetValue(FOGNEAR);
            _waterBlockEffect.Parameters["FogFar"].SetValue(FOGFAR);
            _waterBlockEffect.Parameters["Texture1"].SetValue(_textureAtlas);
            _waterBlockEffect.Parameters["SunColor"].SetValue(SUNCOLOR);
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

        private const byte REMOVE_RANGE = 13;
        private const byte GENERATE_RANGE = 12;
        private const byte LIGHT_RANGE = 11;
        private const byte BUILD_RANGE = 10;

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

        public void Stop()
        {
            _running = false;
        }

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
    }
}
