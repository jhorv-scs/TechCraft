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

#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NewTake.model;
using NewTake.view.blocks;
using NewTake.profiling;
#endregion

namespace NewTake.view
{
    public class LightingChunkProcessor : IChunkProcessor
    {
        private const int MAX_SUN_VALUE = 16;

        public void InitChunk(Chunk chunk)
        {
            ClearLighting(chunk);
        }

        public void ProcessChunk(Chunk chunk) {
            FillLighting(chunk);
        }

        #region ClearLighting
        private void ClearLighting(Chunk chunk)
        {
            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated each in in y-loop!
                    bool inShade = false;
                    for (byte y = Chunk.MAX.Y; y > 0; y--)
                    {
                        if (chunk.Blocks[offset + y].Type != BlockType.None) inShade = true;
                        if (!inShade)
                        {
                            chunk.Blocks[offset + y].Sun = MAX_SUN_VALUE;
                        }
                        else
                        {
                            chunk.Blocks[offset + y].Sun = 0;
                        }

                        if (chunk.Blocks[offset + y].Type == BlockType.RedFlower)
                        {
                            chunk.Blocks[offset + y].R = 16;
                            chunk.Blocks[offset + y].G = 10;
                            chunk.Blocks[offset + y].B = 5;
                        }
                        else
                        {
                            chunk.Blocks[offset + y].R = 0;
                            chunk.Blocks[offset + y].G = 0;
                            chunk.Blocks[offset + y].B = 0;
                        }
                    }
                }
            }
        }
        #endregion

        #region PropogateLight
        private void PropogateSunLight(Chunk chunk, byte x, byte y, byte z, byte light)
        {
            //if (!dirty) dirty = true;

            int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y + y;
            if (chunk.Blocks[offset].Type != BlockType.None) return;
            if (chunk.Blocks[offset].Sun >= light) return;
            chunk.Blocks[offset].Sun = light;

            if (light > 1)
            {
                light = (byte)(light - 1);

                // Propogate light within this chunk
                if (x > 0) PropogateSunLight(chunk, (byte)(x - 1), y, z, light);
                if (x < Chunk.MAX.X - 1) PropogateSunLight(chunk, (byte)(x + 1), y, z, light);
                if (y > 0) PropogateSunLight(chunk, x, (byte)(y - 1), z, light);
                if (y < Chunk.MAX.Y - 1) PropogateSunLight(chunk, x, (byte)(y + 1), z, light);
                if (z > 0) PropogateSunLight(chunk, x, y, (byte)(z - 1), light);
                if (z < Chunk.MAX.Z - 1) PropogateSunLight(chunk, x, y, (byte)(z + 1), light);

                if (x == 0) PropogateSunLight(chunk.E, (byte)(Chunk.MAX.X - 1), y, z, light);
                if (x == Chunk.MAX.X - 1) PropogateSunLight(chunk.W, 0, y, z, light);
                // No need to worry about y neighbours for the time being
                if (z == 0) PropogateSunLight(chunk.S, x, y, (byte)(Chunk.MAX.Z - 1), light);
                if (z == Chunk.MAX.Z - 1) PropogateSunLight(chunk.N, x, y, 0, light);
            }
        }

        private void PropogateLightR(Chunk chunk, byte x, byte y, byte z, byte lightR)
        {
            int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y + y;
            if (chunk.Blocks[offset].Type != BlockType.None) return;
            if (chunk.Blocks[offset].R >= lightR) return;
            chunk.Blocks[offset].R = lightR;

            if (lightR > 1)
            {
                lightR = (byte)(lightR - 1);

                if (x > 0) PropogateLightR(chunk, (byte)(x - 1), y, z, lightR);
                if (x < Chunk.MAX.X) PropogateLightR(chunk, (byte)(x + 1), y, z, lightR);
                if (y > 0) PropogateLightR(chunk, x, (byte)(y - 1), z, lightR);
                if (y < Chunk.MAX.Y) PropogateLightR(chunk, x, (byte)(y + 1), z, lightR);
                if (z > 0) PropogateLightR(chunk, x, y, (byte)(z - 1), lightR);
                if (z < Chunk.MAX.Z) PropogateLightR(chunk, x, y, (byte)(z + 1), lightR);
            }
        }

        private void PropogateLightG(Chunk chunk, byte x, byte y, byte z, byte lightG)
        {
            int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y + y;
            if (chunk.Blocks[offset].Type != BlockType.None) return;
            if (chunk.Blocks[offset].G >= lightG) return;
            chunk.Blocks[offset].G = lightG;

            if (lightG > 1)
            {
                lightG = (byte)(lightG - 1);
                if (x > 0) PropogateLightG(chunk, (byte)(x - 1), y, z, lightG);
                if (x < Chunk.MAX.X) PropogateLightG(chunk, (byte)(x + 1), y, z, lightG);
                if (y > 0) PropogateLightG(chunk, x, (byte)(y - 1), z, lightG);
                if (y < Chunk.MAX.Y) PropogateLightG(chunk, x, (byte)(y + 1), z, lightG);
                if (z > 0) PropogateLightG(chunk, x, y, (byte)(z - 1), lightG);
                if (z < Chunk.MAX.Z) PropogateLightG(chunk, x, y, (byte)(z + 1), lightG);
            }
        }

        private void PropogateLightB(Chunk chunk, byte x, byte y, byte z, byte lightB)
        {
            int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y + y;
            if (chunk.Blocks[offset].Type != BlockType.None) return;
            if (chunk.Blocks[offset].B >= lightB) return;
            chunk.Blocks[offset].B = lightB;

            if (lightB > 1)
            {
                lightB = (byte)(lightB - 1);

                if (x > 0) PropogateLightB(chunk, (byte)(x - 1), y, z, lightB);
                if (x < Chunk.MAX.X) PropogateLightB(chunk, (byte)(x + 1), y, z, lightB);
                if (y > 0) PropogateLightB(chunk, x, (byte)(y - 1), z, lightB);
                if (y < Chunk.MAX.Y) PropogateLightB(chunk, x, (byte)(y + 1), z, lightB);
                if (z > 0) PropogateLightB(chunk, x, y, (byte)(z - 1), lightB);
                if (z < Chunk.MAX.Z) PropogateLightB(chunk, x, y, (byte)(z + 1), lightB);
            }
        }
        #endregion

        #region FillLighting
        private void FillLighting(Chunk chunk)
        {
            FillSunLighting(chunk);
            FillLightingR(chunk);
            FillLightingG(chunk);
            FillLightingB(chunk);
        }

        private void FillSunLighting(Chunk chunk)
        {
            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated each in in y-loop!
                    for (byte y = 0; y < Chunk.SIZE.Y; y++)
                    {
                        if (chunk.Blocks[offset + y].Type == BlockType.None)
                        {
                            // Sunlight
                            if (chunk.Blocks[offset + y].Sun > 1)
                            {
                                byte light = (byte)((chunk.Blocks[offset + y].Sun / 10) * 9);

                                if (x > 0) PropogateSunLight(chunk,(byte)(x - 1), y, z, light);
                                if (x < Chunk.MAX.X) PropogateSunLight(chunk,(byte)(x + 1), y, z, light);
                                if (y > 0) PropogateSunLight(chunk,x, (byte)(y - 1), z, light);
                                if (y < Chunk.MAX.Y) PropogateSunLight(chunk,x, (byte)(y + 1), z, light);
                                if (z > 0) PropogateSunLight(chunk, x, y, (byte)(z - 1), light);
                                if (z < Chunk.MAX.Z) PropogateSunLight(chunk, x, y, (byte)(z + 1), light);
                            }
                        }
                    }
                }
            }
        }

        private void FillLightingR(Chunk chunk)
        {
            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated each in in y-loop!
                    for (byte y = 0; y < Chunk.SIZE.Y; y++)
                    {
                        if (chunk.Blocks[offset + y].Type == BlockType.None || chunk.Blocks[offset + y].Type == BlockType.RedFlower)
                        {
                            // Local light R
                            if (chunk.Blocks[offset + y].R > 1)
                            {
                                byte light = (byte)((chunk.Blocks[offset + y].R / 10) * 9);

                                if (x > 0) PropogateLightR(chunk, (byte)(x - 1), y, z, light);
                                if (x < Chunk.MAX.X) PropogateLightR(chunk, (byte)(x + 1), y, z, light);
                                if (y > 0) PropogateLightR(chunk, x, (byte)(y - 1), z, light);
                                if (y < Chunk.MAX.Y) PropogateLightR(chunk, x, (byte)(y + 1), z, light);
                                if (z > 0) PropogateLightR(chunk, x, y, (byte)(z - 1), light);
                                if (z < Chunk.MAX.Z) PropogateLightR(chunk, x, y, (byte)(z + 1), light);
                            }
                        }
                    }
                }
            }
        }

        private void FillLightingG(Chunk chunk)
        {
            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated each in in y-loop!
                    for (byte y = 0; y < Chunk.SIZE.Y; y++)
                    {
                        if (chunk.Blocks[offset + y].Type == BlockType.None || chunk.Blocks[offset + y].Type == BlockType.RedFlower)
                        {
                            // Local light G
                            if (chunk.Blocks[offset + y].G > 1)
                            {
                                byte light = (byte)((chunk.Blocks[offset + y].G / 10) * 9);
                                if (x > 0) PropogateLightG(chunk, (byte)(x - 1), y, z, light);
                                if (x < Chunk.MAX.X) PropogateLightG(chunk, (byte)(x + 1), y, z, light);
                                if (y > 0) PropogateLightG(chunk, x, (byte)(y - 1), z, light);
                                if (y < Chunk.MAX.Y) PropogateLightG(chunk, x, (byte)(y + 1), z, light);
                                if (z > 0) PropogateLightG(chunk, x, y, (byte)(z - 1), light);
                                if (z < Chunk.MAX.Z) PropogateLightG(chunk, x, y, (byte)(z + 1), light);
                            }
                        }
                    }
                }
            }
        }

        private void FillLightingB(Chunk chunk)
        {
            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = x * Chunk.FlattenOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated each in in y-loop!
                    for (byte y = 0; y < Chunk.SIZE.Y; y++)
                    {
                        if (chunk.Blocks[offset + y].Type == BlockType.None || chunk.Blocks[offset + y].Type == BlockType.RedFlower)
                        {
                            // Local light B
                            if (chunk.Blocks[offset + y].B > 1)
                            {
                                byte light = (byte)((chunk.Blocks[offset + y].B / 10) * 9);
                                if (x > 0) PropogateLightB(chunk, (byte)(x - 1), y, z, light);
                                if (x < Chunk.MAX.X) PropogateLightB(chunk, (byte)(x + 1), y, z, light);
                                if (y > 0) PropogateLightB(chunk, x, (byte)(y - 1), z, light);
                                if (y < Chunk.MAX.Y) PropogateLightB(chunk, x, (byte)(y + 1), z, light);
                                if (z > 0) PropogateLightB(chunk, x, y, (byte)(z - 1), light);
                                if (z < Chunk.MAX.Z) PropogateLightB(chunk, x, y, (byte)(z + 1), light);
                            }
                        }
                    }
                }
            }
        }
        #endregion

    }
}
