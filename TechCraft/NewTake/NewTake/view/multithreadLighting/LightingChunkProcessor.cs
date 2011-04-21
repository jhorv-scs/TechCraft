using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NewTake.model;
using NewTake.view.blocks;
using NewTake.profiling;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
