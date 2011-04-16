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
