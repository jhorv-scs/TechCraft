﻿#region license

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
#endregion

using System;
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

    #region Block structure
    public struct Block
    {
        public  BlockType Type;
        public byte Sun;
        public byte R, G, B;
        
        public Block(BlockType blockType)
        {
            Type=blockType;
            Sun = 0;
            R = 0; G = 0; B = 0;
        }

        public bool Solid
        {
            get { return Type != BlockType.None; } 
        }
    }

    public struct LowMemBlock
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
      
        public LowMemBlock(BlockType blockType, bool sunlit)
        {
           if (sunlit) store = (byte)(((byte)blockType<<(byte)4) | (byte)15);
           else store = (byte)(((byte)blockType<<(byte)4) | (byte)0);
        }

        public LowMemBlock(BlockType blockType, byte lightAmount)
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
    #endregion

}
