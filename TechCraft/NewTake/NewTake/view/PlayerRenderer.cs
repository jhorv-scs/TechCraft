﻿#region license

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
using NewTake.model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewTake.controllers;
using Microsoft.Xna.Framework.Input;
using NewTake.view.blocks;
using NewTake.model.types;
using Microsoft.Xna.Framework.Content;

namespace NewTake.view
{
    /*render player and his hands / tools / attached parts */
    public class PlayerRenderer
    {

        public readonly Player player;
        private readonly Viewport  viewport;
        public readonly FirstPersonCamera camera;
        private readonly FirstPersonCameraController cameraController;

        public Vector3 lookVector;//TODO lookvector should be private

        private MouseState previousMouseState;

        private readonly GraphicsDevice GraphicsDevice;

        private PlayerPhysics physics;

        // SelectionBlock
        public Model SelectionBlock;
        BasicEffect _selectionBlockEffect;
        Texture2D SelectionBlockTexture;

        public PlayerRenderer(GraphicsDevice graphicsDevice, Player player)
        {
            this.GraphicsDevice = graphicsDevice;
            this.player = player;
            this.viewport = graphicsDevice.Viewport;
            this.camera = new FirstPersonCamera(viewport);
            this.cameraController = new FirstPersonCameraController(camera);
            physics = new PlayerPhysics(this);
        }

        public void Initialize()
        {
           
            camera.Initialize();
            camera.Position = new Vector3(World.origin * Chunk.CHUNK_XMAX, Chunk.CHUNK_YMAX, World.origin * Chunk.CHUNK_ZMAX);
            player.position = camera.Position;
            camera.LookAt(Vector3.Down);

            cameraController.Initialize();

            // SelectionBlock
            _selectionBlockEffect = new BasicEffect(GraphicsDevice);
        }

        public void LoadContent(ContentManager content)
        {
            // SelectionBlock
            SelectionBlock = content.Load<Model>("Models\\SelectionBlock");
            SelectionBlockTexture = content.Load<Texture2D>("Textures\\SelectionBlock");
        }

        public void update(GameTime gameTime)
        {
            Matrix previousView = camera.View;
            cameraController.Update(gameTime);
            // alternative would be checking input states changed
            camera.Update(gameTime);


            //do not do this each tick
            if (! previousView.Equals(camera.View))
            {

                physics.move(gameTime);

                Matrix rotationMatrix = Matrix.CreateRotationX(camera.UpDownRotation) * Matrix.CreateRotationY(camera.LeftRightRotation);
                lookVector = Vector3.Transform(Vector3.Forward, rotationMatrix);
                lookVector.Normalize();

                bool waterSelectable = false;
                float x = setPlayerSelectedBlock(waterSelectable);
                if (x != 0) // x==0 is equivalent to payer.currentSelection == null
                {
                    setPlayerAdjacentSelectedBlock(x);
                }

            }

            MouseState mouseState = Mouse.GetState();
            

            if (mouseState.RightButton == ButtonState.Pressed
             && previousMouseState.RightButton != ButtonState.Pressed) {
                 player.RightTool.Use();   
            }
            
            if (mouseState.LeftButton == ButtonState.Pressed
             && previousMouseState.LeftButton != ButtonState.Pressed)
            {
                player.LeftTool.Use();   
            }

            previousMouseState = Mouse.GetState();  
        }

      


        public void Draw(GameTime gameTime)
        {
            //TODO draw the player / 3rd person /  tools
          
            RenderSelectionBlock(gameTime);
        }


        public void RenderSelectionBlock(GameTime gameTime)
        {

            GraphicsDevice.BlendState = BlendState.NonPremultiplied; // allows any transparent pixels in original PNG to draw transparent
            /*
            Vector3 intargetPoint = new Vector3(Math.Abs((uint)targetPoint.X),
                                                Math.Abs((uint)targetPoint.Y),
                                                Math.Abs((uint)targetPoint.Z)); // makes the targetpoint a non float

            Vector3 position = intargetPoint + new Vector3(0.5f, 0.5f, 0.5f);
            */
            if (!player.currentSelection.HasValue) {
                return;
            }
            
            //TODO why the +0.5f for rendering slection block ?
            Vector3 position = player.currentSelection.Value.position.asVector3() + new Vector3(0.5f, 0.5f, 0.5f);
              
            Matrix matrix_a, matrix_b;
            Matrix identity = Matrix.Identity;                       // setup the matrix prior to translation and scaling  
            Matrix.CreateTranslation(ref position, out matrix_a);    // translate the position a half block in each direction
            Matrix.CreateScale((float)0.51f, out matrix_b);          // scales the selection box slightly larger than the targetted block

            identity = Matrix.Multiply(matrix_b, matrix_a);          // the final position of the block

            // set up the World, View and Projection
            _selectionBlockEffect.World = identity;
            _selectionBlockEffect.View = camera.View;
            _selectionBlockEffect.Projection = camera.Projection;
            _selectionBlockEffect.Texture = SelectionBlockTexture;
            _selectionBlockEffect.TextureEnabled = true;

            // apply the effect
            foreach (EffectPass pass in _selectionBlockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                DrawSelectionBlockMesh(GraphicsDevice, SelectionBlock.Meshes[0], _selectionBlockEffect);
            }

        }

        private void DrawSelectionBlockMesh(GraphicsDevice graphicsdevice, ModelMesh mesh, Effect effect)
        {
            int count = mesh.MeshParts.Count;
            for (int i = 0; i < count; i++)
            {
                ModelMeshPart parts = mesh.MeshParts[i];
                if (parts.NumVertices > 0)
                {
                    GraphicsDevice.Indices = parts.IndexBuffer;
                    //TODO better use DrawUserIndexedPrimitives for fully dynamic content
                    GraphicsDevice.SetVertexBuffer(parts.VertexBuffer);
                    graphicsdevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, parts.NumVertices, parts.StartIndex, parts.PrimitiveCount);
                }
            }
        }


        //sets player currentSelection (does nothing if no selection available, like looking ath the sky)
        // returns x float where selection was found for further selection processing (eg finding adjacent block where to add a new block)
        private float setPlayerSelectedBlock(bool waterSelectable)
        {
            for (float x = 0.5f; x < 8f; x += 0.1f)
            {
                Vector3 targetPoint = camera.Position + (lookVector * x);

                Block block = player.world.BlockAt(targetPoint);

                if (block.Type != BlockType.None && ( waterSelectable || block.Type != BlockType.Water))
                {
                    //Debug.WriteLine(
                    //    (Math.Abs((uint)targetPoint.X % Chunk.CHUNK_XMAX)) + "-" +
                    //    (Math.Abs((uint)targetPoint.Y % Chunk.CHUNK_YMAX)) + "-" +
                    //    (Math.Abs((uint)targetPoint.Z % Chunk.CHUNK_ZMAX)) + "->" +
                    //    blockType + ", x=" + x);

                    player.currentSelection =  new PositionedBlock(new Vector3i(targetPoint), block);
                    return x;
                }
            }
            return 0;
        }

        private void setPlayerAdjacentSelectedBlock(float xStart)
        {
            for (float x = xStart; x > 0.7f; x -= 0.1f)
            {
                Vector3 targetPoint = camera.Position + (lookVector * x);
                Block block = player.world.BlockAt(targetPoint);
                
                //TODO smelly - check we really iterate here, and parametrize the type.none
                if (player.world.BlockAt(targetPoint).Type == BlockType.None)
                {
                    player.currentSelectedAdjacent = new PositionedBlock(new Vector3i(targetPoint), block);
                    break;
                }
            }
        }
    }
}
