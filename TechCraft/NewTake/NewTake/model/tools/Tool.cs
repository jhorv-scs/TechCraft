using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model
{
    public abstract class Tool
    {
        protected Player player;
        
        public Tool(Player player)
        {
            this.player = player;
        }

        public abstract void Use();

    }
}
