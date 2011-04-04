﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace NewTake.model
{
    public enum BlockType : byte
    {
        None=0,
        Dirt=1,
        Grass=2,
        Lava=3,
        Leaves=4,
        Rock=5,
        Sand=6,
        Tree=7,
        Water=8,
        Snow = 9,
        MAXIMUM = 15
    }

  
    public struct Block
    {
        //blocktype + light amount stored in one byte 
        private byte store ;
        
        public BlockType Type
        {
            get { return (BlockType)(store >> (byte)4); }
            set {store = (byte)(((byte)value << (byte)4) | (store & 0x0F)); }
        }

        public byte LightAmount
        {
            get { return (byte)(store & 0x0F); }
            set { store = (byte)(store | value); }
        }
      
        public Block(BlockType blockType, bool sunlit)
        {
            store = (byte)(((byte)blockType<<(byte)4) | (byte)15);
          
        }

        public Block(BlockType blockType, byte lightAmount)
        {
            store = (byte)(((byte)blockType << (byte)4) | lightAmount);
        }

        public void debug(String s)
        {
            Debug.WriteLine("["+s +"] -> "+ store +" : " +Type + ", " + LightAmount);         
        }

        public bool Solid
        {
            get { return Type != BlockType.None; } 
        }
    }
}
