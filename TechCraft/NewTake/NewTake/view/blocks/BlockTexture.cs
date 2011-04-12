using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using NewTake.model;

namespace NewTake.view.blocks
{

    public enum BlockTexture
    {
        Brick,
        Dirt,
        Gold,
        GrassSide,
        GrassTop,
        Iron,
        Lava,
        Leaves,
        Gravel,
        Rock,
        Sand,
        Snow,
        TreeTop,
        TreeSide,
        Water,
        MAXIMUM
    }

    public static class TextureHelper
    {
        public const int TEXTUREATLASSIZE = 8;
        public const int CRACKATLASSIZE = 3;

        public static Dictionary<int, Vector2[]> UVMappings;
        public static Dictionary<int, Vector2[]> CrackMappings;

        static TextureHelper()
        {
            BuildUVMappings();
            BuildCrackMappings();
        }

        private static Dictionary<int, Vector2[]> BuildCrackMappings()
        {
            CrackMappings = new Dictionary<int, Vector2[]>();
            for (int i = 0; i < 256; i++)
            {
                CrackMappings.Add((i * 6), TextureHelper.GetCrackMapping(i, BlockFaceDirection.XIncreasing));
                CrackMappings.Add((i * 6) + 1, TextureHelper.GetCrackMapping(i, BlockFaceDirection.XDecreasing));
                CrackMappings.Add((i * 6) + 2, TextureHelper.GetCrackMapping(i, BlockFaceDirection.YIncreasing));
                CrackMappings.Add((i * 6) + 3, TextureHelper.GetCrackMapping(i, BlockFaceDirection.YDecreasing));
                CrackMappings.Add((i * 6) + 4, TextureHelper.GetCrackMapping(i, BlockFaceDirection.ZIncreasing));
                CrackMappings.Add((i * 6) + 5, TextureHelper.GetCrackMapping(i, BlockFaceDirection.ZDecreasing));
            }
            return CrackMappings;
        }

        private static Dictionary<int, Vector2[]> BuildUVMappings()
        {
            UVMappings = new Dictionary<int, Vector2[]>();
            for (int i = 0; i < (int)BlockTexture.MAXIMUM; i++)
            {
                UVMappings.Add((i * 6), TextureHelper.GetUVMapping(i, BlockFaceDirection.XIncreasing));
                UVMappings.Add((i * 6) + 1, TextureHelper.GetUVMapping(i, BlockFaceDirection.XDecreasing));
                UVMappings.Add((i * 6) + 2, TextureHelper.GetUVMapping(i, BlockFaceDirection.YIncreasing));
                UVMappings.Add((i * 6) + 3, TextureHelper.GetUVMapping(i, BlockFaceDirection.YDecreasing));
                UVMappings.Add((i * 6) + 4, TextureHelper.GetUVMapping(i, BlockFaceDirection.ZIncreasing));
                UVMappings.Add((i * 6) + 5, TextureHelper.GetUVMapping(i, BlockFaceDirection.ZDecreasing));
            }
            return UVMappings;
        }

        public static Vector2[] GetHiresMapping()
        {
            Vector2[] UVList = new Vector2[6];
            UVList[0] = new Vector2(0, 0);
            UVList[1] = new Vector2(1, 0);
            UVList[2] = new Vector2(0, 1);
            UVList[3] = new Vector2(0, 1);
            UVList[4] = new Vector2(1, 0);
            UVList[5] = new Vector2(1, 1);
            return UVList;
        }


        public static Vector2[] GetCrackMapping(int texture, BlockFaceDirection faceDir)
        {
            int textureIndex = texture;
            // Assumes a Crack atlas of 3x3 textures

            int y = textureIndex / CRACKATLASSIZE;
            int x = textureIndex % CRACKATLASSIZE;

            float ofs = 1f / 3f;

            float yOfs = y * ofs;
            float xOfs = x * ofs;

            //ofs -= 0.01f;

            Vector2[] UVList = new Vector2[6];

            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.XDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[3] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[4] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[5] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    break;
                case BlockFaceDirection.YIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[1] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.YDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.ZIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[3] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[4] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[5] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    break;
                case BlockFaceDirection.ZDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
            }
            return UVList;
        }

        public static Vector2[] GetUVMapping(int texture, BlockFaceDirection faceDir)
        {
            int textureIndex = texture;
            // Assumes a texture atlas of 8x8 textures

            int y = textureIndex / TEXTUREATLASSIZE;
            int x = textureIndex % TEXTUREATLASSIZE;

            float ofs = 1f / 8f;

            float yOfs = y * ofs;
            float xOfs = x * ofs;

            //ofs -= 0.01f;

            Vector2[] UVList = new Vector2[6];

            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.XDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[3] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[4] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[5] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    break;
                case BlockFaceDirection.YIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[1] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.YDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
                case BlockFaceDirection.ZIncreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[3] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[4] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    UVList[5] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    break;
                case BlockFaceDirection.ZDecreasing:
                    UVList[0] = new Vector2(xOfs, yOfs);                // 0,0
                    UVList[1] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[2] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[3] = new Vector2(xOfs, yOfs + ofs);          // 0,1
                    UVList[4] = new Vector2(xOfs + ofs, yOfs);          // 1,0
                    UVList[5] = new Vector2(xOfs + ofs, yOfs + ofs);    // 1,1
                    break;
            }
            return UVList;
        }
    }

}
