using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechCraftEngine.WorldEngine.Generators
{
    class DualLayerTerrainWithMediumValleys : LandscapeMapGenerator
    {
        private BlockType[, ,] _map;

        Random r = new Random();// using WorldSettings.SEED would be good

        public new BlockType[, ,] GenerateMap()
        {
            _map = new BlockType[WorldSettings.MAPWIDTH, WorldSettings.MAPHEIGHT, WorldSettings.MAPLENGTH];
            MapTools.Clear(_map, BlockType.None);


            for (int x = 0; x < WorldSettings.MAPWIDTH; x++)
            {
                for (int z = 0; z < WorldSettings.MAPLENGTH; z++)
                {
                    generateTerrain(x, z, WorldSettings.MAPHEIGHT);
                }
            }

            GenerateWaterLayer(WorldSettings.SEALEVEL);


            return _map;
        }


        protected void generateTerrain(int x, int z, int worldDepthInBlocks)
        {

            int groundHeight = (int)GetBlockNoise(x, z);
            if (groundHeight < 1)
            {
                groundHeight = 1;
            }
            else if (groundHeight > 128)
            {
                groundHeight = 96;
            }

            // Default to sunlit.. for caves
            bool sunlit = true;
            BlockType type = BlockType.None;
            _map[x,groundHeight,z] = BlockType.Grass;
            _map[x,0,z] = BlockType.Dirt;
            for (int y = worldDepthInBlocks - 1; y > 0; y--)
            {
                if (y > groundHeight)
                {
                    type = BlockType.None;
                }

                // Or we at or below ground height?
                else if (y < groundHeight)
                {
                    // Since we are at or below ground height, let's see if we need
                    // to make
                    // a cave
                    int noiseX = (x + WorldSettings.SEED);
                    float octave1 = PerlinSimplexNoise.noise(noiseX * 0.009f, z * 0.009f, y * 0.009f) * 0.25f;

                    float initialNoise = octave1 + PerlinSimplexNoise.noise(noiseX * 0.04f, z * 0.04f, y * 0.04f) * 0.15f;
                    initialNoise += PerlinSimplexNoise.noise(noiseX * 0.08f, z * 0.08f, y * 0.08f) * 0.05f;

                    if (initialNoise > 0.2f)
                    {
                        type = BlockType.None;
                    }
                    else
                    {
                        // We've placed a block of dirt instead...nothing below us
                        // will be sunlit
                        if (sunlit)
                        {
                            sunlit = false;
                            type = BlockType.Grass;

                            if (y>WorldSettings.SEALEVEL && r.Next(250) == 1)
                            {
                                //no trees under the see
                               BuildTree(x, y, z);
                            }

                        }
                        else
                        {
                            type = BlockType.Dirt;
                            if (octave1 < 0.2f)
                            {
                                type = BlockType.Rock;
                            }
                        }
                    }
                }

                _map[x,y,z] = type;
            }
        }

        private float GetBlockNoise(int blockX, int blockZ)
        {
            float mediumDetail = PerlinSimplexNoise.noise(blockX / 300.0f, blockZ / 300.0f, 20);
            float fineDetail = PerlinSimplexNoise.noise(blockX / 80.0f, blockZ / 80.0f, 30);
            float bigDetails = PerlinSimplexNoise.noise(blockX / 800.0f, blockZ / 800.0f);
            float noise = bigDetails * 64.0f + mediumDetail * 32.0f + fineDetail * 16.0f; // *(bigDetails
            // *
            // 64.0f);
            return noise + 16;
        }

        #region copypaste from LandscapeMapGenerator ;)

        private void BuildTree(int tx, int ty, int tz)
        {
            int height = 4 + r.Next(3);

            if ((ty + height) < WorldSettings.MAPHEIGHT)
            {
                for (int y = ty; y < ty + height; y++)
                {
                    _map[tx, y, tz] = BlockType.Tree;
                }
            }

            int radius = 3 + r.Next(2);
            int ny = ty + height;

            for (int i = 0; i < 40 + r.Next(4); i++)
            {
                int lx = tx + r.Next(radius) - r.Next(radius);
                int ly = ny + r.Next(radius) - r.Next(radius);
                int lz = tz + r.Next(radius) - r.Next(radius);

                if (MapTools.WithinMapBounds(lx, ly, lz))
                {
                    if (_map[lx, ly, lz] == BlockType.None) _map[lx, ly, lz] = BlockType.Leaves;
                }

            }

        }

        private void GenerateWaterLayer(int seaLevel)
        {
            for (int x = 0; x < WorldSettings.MAPWIDTH; x++)
            {
                for (int z = 0; z < WorldSettings.MAPLENGTH; z++)
                {
                    for (int y = seaLevel; y > 0; y--)
                    {
                        if (_map[x, y, z] == BlockType.None)
                        {
                            _map[x, y, z] = BlockType.Water;
                        }
                        else
                        {
                            if (_map[x, y, z] == BlockType.Grass)
                            {
                                // Grass doesn't grow under water
                                _map[x, y, z] = BlockType.Sand;
                            }
                            break;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
