using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using fbDeprofiler;

using NewTake.model;
using NewTake.view;
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
        #region inits

        private GraphicsDeviceManager graphics;
        private World world;
        private WorldRenderer renderer;

        private KeyboardState _oldKeyboardState;

        private bool releaseMouse = false;

        private int preferredBackBufferHeight, preferredBackBufferWidth;

       

        private HudRenderer hud;

        private Player player1;//wont add a player2 for some time, but naming like this helps designing  
        private PlayerRenderer player1Renderer;

        #endregion

        public Game1()
        {
            DeProfiler.Run();
            graphics = new GraphicsDeviceManager(this);
            
            //graphics.IsFullScreen = true;

            preferredBackBufferHeight = graphics.PreferredBackBufferHeight;
            preferredBackBufferWidth = graphics.PreferredBackBufferWidth;
            
            //enter stealth mode at start
           // graphics.PreferredBackBufferHeight = 100;
           // graphics.PreferredBackBufferWidth = 160;

            FrameRateCounter frameRate = new FrameRateCounter(this);
            frameRate.DrawOrder = 1;
            Components.Add(frameRate);

            Content.RootDirectory = "Content";
            graphics.SynchronizeWithVerticalRetrace = true; // press f3 to set it to false at runtime 
        }

        #region Initialize
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            world = new World();

            player1 = new Player(world);

            player1Renderer = new PlayerRenderer(GraphicsDevice,player1);
            player1Renderer.Initialize();

            hud = new HudRenderer(GraphicsDevice);
            hud.Initialize();

            //renderer = new WorldRenderer(GraphicsDevice, player1Renderer.camera, world);
            //renderer = new SingleThreadWorldRenderer(GraphicsDevice, player1Renderer.camera, world);
            renderer = new SingleThreadAOWorldRenderer(GraphicsDevice, player1Renderer.camera, world);
            //TODO refactor WorldRenderer needs player position + view frustum 

         

            base.Initialize();
        }
        #endregion

        protected override void OnExiting(Object sender, EventArgs args)
        {
            //renderer._running = false;
            base.OnExiting(sender, args);
        }

        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            renderer.loadContent(Content);
            player1Renderer.LoadContent(Content);
            hud.loadContent(Content);
        }
        #endregion

        #region UnloadContent
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        #endregion

        #region ProcessDebugKeys
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
                graphics.SynchronizeWithVerticalRetrace = !graphics.SynchronizeWithVerticalRetrace;
                this.IsFixedTimeStep = !this.IsFixedTimeStep;
                Debug.WriteLine("FixedTimeStep and v synch are " + this.IsFixedTimeStep);
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
        #endregion

       
        #region Update
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            ProcessDebugKeys();

            if (this.IsActive)
            {
                if (!releaseMouse)
                {
                    player1Renderer.update(gameTime);
                }

                renderer.Update(gameTime);
                base.Update(gameTime);
            }
        }
        #endregion

        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);
            renderer.Draw(gameTime);

            player1Renderer.Draw(gameTime);
       
            hud.Draw(gameTime);

            base.Draw(gameTime);

        }
        #endregion

    }
}
