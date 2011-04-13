using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NewTake.model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewTake.controllers;
using Microsoft.Xna.Framework.Input;
using NewTake.view.blocks;

namespace NewTake.view
{
    /*render player and his hands / tools / attached parts */
    public class PlayerRenderer
    {

        private readonly Player player;
        private readonly Viewport  viewport;
        public readonly FirstPersonCamera camera;
        private readonly FirstPersonCameraController cameraController;

        public Vector3 lookVector;//TODO lookvector should be private

        private MouseState previousMouseState;

        public PlayerRenderer(Player player, Viewport viewport)
        {
            this.player = player;
            this.viewport = viewport;
            this.camera = new FirstPersonCamera(viewport);
            this.cameraController = new FirstPersonCameraController(camera);
        }

        public void Initialize()
        {
           
            camera.Initialize();
            camera.Position = new Vector3(World.origin * Chunk.CHUNK_XMAX, Chunk.CHUNK_YMAX, World.origin * Chunk.CHUNK_ZMAX);
            camera.LookAt(Vector3.Down);

           
            cameraController.Initialize();
        }

        public void update(GameTime gameTime)
        {

            cameraController.Update(gameTime);
            camera.Update(gameTime);

            Matrix rotationMatrix = Matrix.CreateRotationX(camera.UpDownRotation) * Matrix.CreateRotationY(camera.LeftRightRotation);
            lookVector = Vector3.Transform(Vector3.Forward, rotationMatrix);
            lookVector.Normalize();
            useTools(gameTime);

            previousMouseState = Mouse.GetState();  
        }

        public void draw(GameTime gameTime)
        {
            //TODO draw the player / 3rd person /  tools
        }


     
        #region useTools
        public void useTools(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton != ButtonState.Pressed)
            {
                for (float x = 0.5f; x < 8f; x += 0.1f)
                {
                    Vector3 targetPoint;
                    targetPoint = camera.Position + (lookVector * x);

                    BlockType blockType = player.world.BlockAt(targetPoint).Type;

                    if (blockType != BlockType.None && blockType != BlockType.Water)
                    {
                        if (targetPoint.Y > 2)
                        {
                            // Can't dig water or lava

                            BlockType targetType = player.world.BlockAt(targetPoint).Type;

                            if (BlockInformation.IsDiggable(targetType))
                            {
                                //Debug.WriteLine(targetPoint + "->" + blockType);
                                player.world.setBlock((uint)targetPoint.X, (uint)targetPoint.Y, (uint)targetPoint.Z, new Block(BlockType.None, 0));

                            }
                        }
                        break;
                    }
                }
            }

            if (mouseState.RightButton == ButtonState.Pressed
                && previousMouseState.RightButton != ButtonState.Pressed)
            {
                float hit = 0;
                for (float x = 0.8f; x < 8f; x += 0.1f)
                {
                    Vector3 targetPoint = camera.Position + (lookVector * x);
                    if (player.world.BlockAt(targetPoint).Type != BlockType.None)
                    {
                        hit = x;
                        break;
                    }
                }



                if (hit != 0)
                {
                    for (float x = hit; x > 0.7f; x -= 0.1f)
                    {
                        Vector3 targetPoint = camera.Position + (lookVector * x);
                        if (player.world.BlockAt(targetPoint).Type == BlockType.None)
                        {
                            player.world.setBlock((uint)targetPoint.X, (uint)targetPoint.Y, (uint)targetPoint.Z, new Block(BlockType.Tree, true));
                            //TODO block type added hardcoded to tree
                            break;
                        }
                    }
                }
            }

        }
        #endregion

    }
}
