using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model.types
{
    public struct PositionedBlock
    {
        public readonly Vector3i position;
        public readonly Block block;

        public PositionedBlock(Vector3i position, Block block)
        {
            this.position=position;
            this.block = block;
        }
    }
}
