using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model.tools
{

    //removes an entire column of blocks, just an example of what can be done with tools and how easy this is !
    // just assign it on one of the player 's tools in player class (   LeftTool = new PowerDrill(this); in player.cs) 
    public class PowerDrill : Tool
    {
        public PowerDrill(Player player) : base(player) { }

        public override void Use()
        {
            if (player.currentSelection.HasValue)
            {
                Vector3i position =  player.currentSelection.Value.position;

                for (uint y = position.Y; y > 0; y--)
                {
                    player.world.setBlock(position.X,y,position.Z, new Block(BlockType.None));
                }
                    
            }
        }
    }
}
