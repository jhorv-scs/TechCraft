using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NewTake.view
{
    interface IChunkRenderer
    {
         void BuildVertexList();

         void draw(GameTime gameTime);
    }
}
