using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace NewTake.view
{
    public class HudRenderer
    {

        GraphicsDevice GraphicsDevice;

        public HudRenderer(GraphicsDevice device)
        {
            this.GraphicsDevice = device;
        }

        // Crosshair
        private Texture2D _crosshairTexture;
        private SpriteBatch _spriteBatch;


        public void Initialize()
        {
            // Used for crosshair sprite/texture at the moment
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }


        public void loadContent(ContentManager Content)
        {
            // Crosshair
            _crosshairTexture = Content.Load<Texture2D>("Textures\\crosshair");
        }



        public void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // Draw the crosshair
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.Draw(_crosshairTexture, new Vector2(
                (GraphicsDevice.Viewport.Width / 2) - 10,
                (GraphicsDevice.Viewport.Height / 2) - 10), Color.White);
            _spriteBatch.End();
        }
    }
}
