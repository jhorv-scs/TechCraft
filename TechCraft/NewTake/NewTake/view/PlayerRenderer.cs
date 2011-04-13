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

        private readonly Player player;
        private readonly Viewport  viewport;
        public readonly FirstPersonCamera camera;
        private readonly FirstPersonCameraController cameraController;

        public Vector3 lookVector;//TODO lookvector should be private

        private MouseState previousMouseState;

        private readonly GraphicsDevice GraphicsDevice;


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
        }

        public void Initialize()
        {
           
            camera.Initialize();
            camera.Position = new Vector3(World.origin * Chunk.CHUNK_XMAX, Chunk.CHUNK_YMAX, World.origin * Chunk.CHUNK_ZMAX);
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

            cameraController.Update(gameTime);
            Matrix previousView = camera.View;
            //TODO alternative would be checking input states changed
            camera.Update(gameTime);

            //do not do this each tick
            if (previousView.Equals(camera.View))
            {

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
                if (player.currentSelectedAdjacent.HasValue){
                    player.world.setBlock(player.currentSelectedAdjacent.Value.position, new Block(BlockType.Tree,true));                    
                }
            }
            
            if (mouseState.LeftButton == ButtonState.Pressed
             && previousMouseState.LeftButton != ButtonState.Pressed)
            {
                if (player.currentSelection.HasValue)
                {
                    player.world.setBlock(player.currentSelection.Value.position, new Block(BlockType.None, false));
                }
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
        /*
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
        #endregion*/

    }
}
