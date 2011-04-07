using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using NewTake.model;
using NewTake.view;
using System.Diagnostics;
using fbDeprofiler;
using NewTake.controllers;
using NewTake.view.blocks;
using NewTake.profiling;

namespace NewTake
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        World world;
        WorldRenderer renderer;
        private FirstPersonCamera _camera;
        private FirstPersonCameraController _cameraController;
        private MouseState _previousMouseState;
        private KeyboardState _oldKeyboardState;

        private Vector3 _lookVector;

        private bool releaseMouse = false;

        private int preferredBackBufferHeight, preferredBackBufferWidth;

        public Game1()
        {
            DeProfiler.Run();
            graphics = new GraphicsDeviceManager(this);
            preferredBackBufferHeight = graphics.PreferredBackBufferHeight;
            preferredBackBufferWidth = graphics.PreferredBackBufferWidth;
            FrameRateCounter frameRate = new FrameRateCounter(this);
            frameRate.DrawOrder = 1;
            Components.Add(frameRate);

            Content.RootDirectory = "Content";
            graphics.SynchronizeWithVerticalRetrace = true; // press f3 to set it to false at runtime 
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            world = new World();
            _camera = new FirstPersonCamera(GraphicsDevice);
            _camera.Initialize();
            _camera.Position = new Vector3(World.origin * Chunk.CHUNK_XMAX, Chunk.CHUNK_YMAX, World.origin * Chunk.CHUNK_ZMAX);
            _camera.LookAt(Vector3.Down);

            _cameraController = new FirstPersonCameraController(_camera);
            _cameraController.Initialize();


            renderer = new WorldRenderer(GraphicsDevice, _camera, world);

            base.Initialize();
        }

        protected override void OnExiting(Object sender, EventArgs args)
        {
            renderer._running = false;
            base.OnExiting(sender, args);
        }



        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            renderer.loadContent(Content);
            // TODO: use this.Content to load your game content here

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void ProcessDebugKeys()
        {
            KeyboardState keyState = Keyboard.GetState();

            //wireframe mode
            if (_oldKeyboardState.IsKeyUp(Keys.F7) && keyState.IsKeyDown(Keys.F7))
            {
                renderer.ToggleRasterMode();
            }

            // Allows the game to exit
            if (keyState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }


            // Release the mouse pointer
            if (_oldKeyboardState.IsKeyUp(Keys.Space) && keyState.IsKeyDown(Keys.Space))
            {
                this.releaseMouse = !this.releaseMouse;
                this.IsMouseVisible = !this.IsMouseVisible;
            }

            if (_oldKeyboardState.IsKeyUp(Keys.F3) && keyState.IsKeyDown(Keys.F3))
            {
                graphics.SynchronizeWithVerticalRetrace = ! graphics.SynchronizeWithVerticalRetrace;
                this.IsFixedTimeStep = ! this.IsFixedTimeStep;
                Debug.WriteIf(this.IsFixedTimeStep, "FixedTimeStep and v synch are active");
                graphics.ApplyChanges();
            }

            // stealth mode / keep screen space for profilers
            if (_oldKeyboardState.IsKeyUp(Keys.F4) && keyState.IsKeyDown(Keys.F4))
            {
                if (graphics.PreferredBackBufferHeight == preferredBackBufferHeight)
                {
                    graphics.PreferredBackBufferHeight = 100;
                    graphics.PreferredBackBufferWidth = 160;
                }
                else
                {
                    graphics.PreferredBackBufferHeight = preferredBackBufferHeight;
                    graphics.PreferredBackBufferWidth = preferredBackBufferWidth;
                }
                graphics.ApplyChanges();
            }

            this._oldKeyboardState = keyState;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            ProcessDebugKeys();

            // TODO: Add your update logic here

            if (this.IsActive)
            {
                if (!releaseMouse)
                {
                    _cameraController.Update(gameTime);
                    _camera.Update(gameTime);
                }

                Matrix rotationMatrix = Matrix.CreateRotationX(_camera.UpDownRotation) * Matrix.CreateRotationY(_camera.LeftRightRotation);
                _lookVector = Vector3.Transform(Vector3.Forward, rotationMatrix);
                _lookVector.Normalize();

                renderer.Update(gameTime);

                useTools(gameTime);

                _previousMouseState = Mouse.GetState();
                base.Update(gameTime);
            }
        }

        public void useTools(GameTime gameTime)
        {

            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton != ButtonState.Pressed)
            {
                int dir;

                if (_camera.Position.X < 0 || _camera.Position.Y < 0 || _camera.Position.Z < 0)
                {
                    dir = -1;
                }
                else
                {
                    dir = 1;
                }

                for (float x = 0.5f; x < 5f; x += 0.2f)
                {
                    Vector3 targetPoint;
                    if (dir == 1) targetPoint = _camera.Position + (_lookVector * x);
                    else targetPoint = _camera.Position - (_lookVector * x);

                    BlockType blockType = world.BlockAt(targetPoint).Type;

                    if (blockType != BlockType.None && blockType != BlockType.Water)
                    {
                        if (targetPoint.Y > 2)
                        {
                            // Can't dig water or lava

                            BlockType targetType = world.BlockAt(targetPoint).Type;

                            if (BlockInformation.IsDiggable(targetType))
                            {
                                Debug.WriteLine(targetPoint + "->" + blockType);
                                world.setBlock((uint)targetPoint.X, (uint)targetPoint.Y, (uint)targetPoint.Z, new Block(BlockType.None, 0));
                                //tempHookForWorldGen(targetPoint);
                            }
                        }
                        break;
                    }
                }
            }

            if (mouseState.RightButton == ButtonState.Pressed
                && _previousMouseState.RightButton != ButtonState.Pressed)
            {
                float hit = 0;
                for (float x = 0.8f; x < 5f; x += 0.1f)
                {
                    Vector3 targetPoint = _camera.Position + (_lookVector * x);
                    if (world.BlockAt(targetPoint).Type != BlockType.None)
                    {
                        hit = x;
                        break;
                    }
                }
                if (hit != 0)
                {
                    for (float x = hit; x > 0.7f; x -= 0.1f)
                    {
                        Vector3 targetPoint = _camera.Position + (_lookVector * x);
                        if (world.BlockAt(targetPoint).Type == BlockType.None)
                        {
                            world.setBlock((uint)targetPoint.X, (uint)targetPoint.Y, (uint)targetPoint.Z, new Block(BlockType.Tree, true));
                            //TODO block type added hardcoded to tree
                            break;
                        }
                    }
                }
            }

        }



        //private void tempHookForWorldGen(Vector3 targetPoint)
        //{
        //    uint x = (uint)targetPoint.X;
        //    uint y = (uint)targetPoint.Y;
        //    uint z = (uint)targetPoint.Z;

        //    uint cx = x / Chunk.CHUNK_XMAX;
        //    uint cz = z / Chunk.CHUNK_ZMAX;


        //    uint lx = x % Chunk.CHUNK_XMAX;
        //    uint ly = y % Chunk.CHUNK_YMAX;
        //    uint lz = z % Chunk.CHUNK_ZMAX;

        //    Vector3i currentChunkIndex = world.viewableChunks[cx, cz].Index;

        //    /* todo solve problems with adding removing blocks in negative coords space
        //    or move to unsigned ints and start at int maxvalue/2 !
        //     * if (lx == 0)
        //    {
        //        if (world.viewableChunks[cx - 1, cz]==null)
        //        {
        //            Vector3 newPos = currentChunkPos  -  new Vector3(Chunk.CHUNK_XMAX , 0 , 0);
        //            //grow the viewable sparsematrix world ( TODO : remove opposite chunks or we will fill the memory ! )   
        //            Chunk toAdd = new Chunk(newPos);
        //            world.viewableChunks[(cx - 1) , cz] = toAdd;
        //            world.builder.build(toAdd);
        //            renderer.initRendererAction((cx - 1) ,0, cz );
        //        }
        //    }*/

        //    if (lx == Chunk.CHUNK_XMAX - 1)
        //    {
        //        if (world.viewableChunks[cx + 1, cz] == null)
        //        {
        //            Vector3i newIndex = currentChunkIndex + new Vector3i(1, 0, 0);
        //            //grow the viewable sparsematrix world ( TODO : remove opposite chunks or we will fill the memory ! )   
        //            Chunk toAdd = new Chunk(newIndex);

        //            world.viewableChunks[newIndex.X, newIndex.Z] = toAdd;
        //            world.builder.build(toAdd);
        //            renderer.initRendererAction(newIndex);
        //        }
        //    }

        //}

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);

            // TODO: Add your drawing code here
            renderer.Draw(gameTime);


            base.Draw(gameTime);
        }
    }
}
