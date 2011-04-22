using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model
{
    public enum ChunkState
    {
        AwaitingGenerate,
        Generating,
        AwaitingLighting,
        Lighting,
        AwaitingBuild,
        Building,
        Ready
    }
}
