using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using NewTake.view;

namespace NewTake.controllers
{
    public class FirstPersonCameraController
    {
        #region inits

        private const float MOVEMENTSPEED = 0.25f;
        private const float ROTATIONSPEED = 0.1f;

        private MouseState _mouseMoveState;
        private MouseState _mouseState;

        private readonly FirstPersonCamera camera;

        #endregion

        public FirstPersonCameraController(FirstPersonCamera camera) 
        {
            this.camera=camera;
        }

        public void Initialize()
        {
            _mouseState = Mouse.GetState();
        }

        public void Update(GameTime gameTime)
        {
            ProcessInput(gameTime);
        }

        #region ProcessInput
        public void ProcessInput(GameTime gameTime)
        {
            //PlayerIndex activeIndex;

            Vector3 moveVector = new Vector3(0,0,0);
            KeyboardState keyState = Keyboard.GetState();
            
            if (keyState.IsKeyDown(Keys.W))
            {
                moveVector += Vector3.Forward;
            }
            if (keyState.IsKeyDown(Keys.S))
            {
                moveVector += Vector3.Backward;
            }
            if (keyState.IsKeyDown(Keys.A))
            {
                moveVector += Vector3.Left;
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                moveVector += Vector3.Right;
            }

            if (moveVector != Vector3.Zero)
            {
                Matrix rotationMatrix = Matrix.CreateRotationX(camera.UpDownRotation) * Matrix.CreateRotationY(camera.LeftRightRotation);
                Vector3 rotatedVector = Vector3.Transform(moveVector, rotationMatrix);
                camera.Position += rotatedVector * MOVEMENTSPEED;
            }

            MouseState currentMouseState = Mouse.GetState();

            float mouseDX = currentMouseState.X - _mouseMoveState.X;
            float mouseDY = currentMouseState.Y - _mouseMoveState.Y;

            if (mouseDX != 0)
            {
                camera.LeftRightRotation -= ROTATIONSPEED * (mouseDX / 50);
            }
            if (mouseDY != 0)
            {
                camera.UpDownRotation -= ROTATIONSPEED * (mouseDY / 50);
            }

            //camera.LeftRightRotation -= GamePad.GetState(Game.ActivePlayerIndex).ThumbSticks.Right.X / 20;
            //camera.UpDownRotation += GamePad.GetState(Game.ActivePlayerIndex).ThumbSticks.Right.Y / 20;

            _mouseMoveState = new MouseState(camera.viewport.Width / 2,
                    camera.viewport.Height / 2,
                    0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
                
            Mouse.SetPosition((int)_mouseMoveState.X, (int)_mouseMoveState.Y);
            _mouseState = Mouse.GetState();
        }
        #endregion

    }
}