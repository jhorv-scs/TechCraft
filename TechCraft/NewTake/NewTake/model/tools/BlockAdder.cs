using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model.tools
{
    public class BlockAdder : Tool
    {
        public BlockAdder(Player player) : base(player) { }

        public override void Use()
        {
            if (player.currentSelectedAdjacent.HasValue)
            {
                player.world.setBlock(player.currentSelectedAdjacent.Value.position, new Block(BlockType.Tree, true));
            }
        }
    }
}
