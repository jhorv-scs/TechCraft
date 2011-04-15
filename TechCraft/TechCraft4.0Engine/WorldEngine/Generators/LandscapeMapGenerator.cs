using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TechCraftEngine.WorldEngine.Generators
{
    public class LandscapeMapGenerator : IMapGenerator
    {
        private BlockType[, ,] _map;
        Random r = new Random();

        public BlockType[, ,] GenerateMap()
        {
            _map = new BlockType[WorldSettings.MAPWIDTH, WorldSettings.MAPHEIGHT, WorldSettings.MAPLENGTH];
            MapTools.Clear(_map, BlockType.None);

            GenerateRockLayer();
            GenerateDirtLayer(15,false);
            CarveLandscape();            
            GenerateDirtLayer(10,true);
            GenerateSandLayer();
            CarveTunnels(200);
            GenerateWaterLayer();
            return _map;
        }

        private void CarveTunnel(int count, int radius) {

            int sx = r.Next(WorldSettings.MAPWIDTH);
            int sy = r.Next(WorldSettings.MAPHEIGHT) / 2;
            int sz = r.Next(WorldSettings.MAPLENGTH);

            int xdir = r.Next(2); if (xdir == 0) xdir = -1;
            int ydir = ydir = -1; // r.Next(2); if (ydir == 0) ydir = -1;
            int zdir = r.Next(2); if (zdir == 0) zdir = -1;

            while (count > 0)
            {
                if (sx>radius + 5 && sy >radius + 5 && sz>radius + 5 &&
                    sx < WorldSettings.MAPWIDTH - radius - 5 &&
                    sy < WorldSettings.MAPHEIGHT - radius - 5 &&
                    sz < WorldSettings.MAPLENGTH - radius - 5) {
                        PaintAtPoint(sx, sy, sz, radius, BlockType.None);
                }
                sx += (r.Next(3) * xdir);
                sy += (r.Next(2) * ydir);
                sz += (r.Next(3) * zdir);

                radius += r.Next(2) - r.Next(2);

                if (radius < 1) radius = 2;
                if (radius > 4) radius = 3;

                count--;
            }
        }

        private void CarveTunnels(int count)
        {
            
            for (int x = 0; x < count; x++)
            {
                CarveTunnel(1000, r.Next(5) + 2);
            }
        }

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

            for (int i = 0; i < 40 + r.Next(4);i++ )
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

        public void PaintAtPoint(int x, int y, int z,int paintRadius, BlockType paintValue)
        {
            for (int dx = -paintRadius; dx <= paintRadius; dx++)
                for (int dy = -paintRadius; dy <= paintRadius; dy++)
                    for (int dz = -paintRadius; dz <= paintRadius; dz++)
                            if (dx * dx + dy * dy + dz * dz < paintRadius * paintRadius)
                            {
                                //Debug.WriteLine(string.Format("{0},{1},{2}", x + dx, y + dy, z + dz));
                                _map[x + dx, y + dy, z + dz] = paintValue;
                            }
        }

        private void CarveLandscape()
        {
            List<PerlinNoise2D> heightFunctions1 = new List<PerlinNoise2D>();
            heightFunctions1.Add(new PerlinNoise2D(1, 2f));
            heightFunctions1.Add(new PerlinNoise2D(8, 1.5f));
            heightFunctions1.Add(new PerlinNoise2D(12, .25f));
            heightFunctions1.Add(new PerlinNoise2D(26, 2.625f));
            heightFunctions1.Add(new PerlinNoise2D(34, .0625f));
            heightFunctions1.Add(new PerlinNoise2D(64, 0.1425f));

            List<PerlinNoise2D> heightFunctions2 = new List<PerlinNoise2D>();
            heightFunctions2.Add(new PerlinNoise2D(2, 2f));
            heightFunctions2.Add(new PerlinNoise2D(8, .5f));
            heightFunctions2.Add(new PerlinNoise2D(12, .25f));
            heightFunctions2.Add(new PerlinNoise2D(26, 3.125f));
            heightFunctions2.Add(new PerlinNoise2D(34, .0625f));
            heightFunctions2.Add(new PerlinNoise2D(64, 0.1425f));

            double[,] heightData1 = MapTools.SumNoiseFunctions(WorldSettings.MAPWIDTH, WorldSettings.MAPLENGTH, heightFunctions1);
            double[,] heightData2 = MapTools.SumNoiseFunctions(WorldSettings.MAPWIDTH, WorldSettings.MAPLENGTH, heightFunctions2);


            for (int x = 0; x < WorldSettings.MAPWIDTH; x++)
            {
                for (int z = 0; z < WorldSettings.MAPLENGTH; z++)
                {
                    int height1 = (int)(heightData1[x, z] * 10) + 3 + WorldSettings.SEALEVEL;
                    int height2 = (int)(heightData2[x, z] * 15) + 5 + WorldSettings.SEALEVEL;
                    if (height2 > WorldSettings.MAPHEIGHT) height2 = WorldSettings.MAPHEIGHT;

                    if (height2 > height1)
                    {
                        for (int y = height1; y < height2; y++)
                        {
                            _map[x, y, z] = BlockType.None;
                        }
                    }
                }
            }
        }

        private void GenerateWaterLayer()
        {
            for (int x = 0; x < WorldSettings.MAPWIDTH; x++)
            {
                for (int z = 0; z < WorldSettings.MAPLENGTH; z++)
                {
                    for (int y = WorldSettings.SEALEVEL + 35; y > 0; y--)
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

        private void GenerateSandLayer()
        {
            List<PerlinNoise2D> noiseFunctions = new List<PerlinNoise2D>();
            noiseFunctions.Add(new PerlinNoise2D(4, 2f));
            noiseFunctions.Add(new PerlinNoise2D(8, .5f));
            noiseFunctions.Add(new PerlinNoise2D(12, .25f));
            noiseFunctions.Add(new PerlinNoise2D(26, .125f));
            noiseFunctions.Add(new PerlinNoise2D(34, .0625f));
            noiseFunctions.Add(new PerlinNoise2D(64, .0425f));

            double[,] data = MapTools.SumNoiseFunctions(WorldSettings.MAPWIDTH, WorldSettings.MAPLENGTH, noiseFunctions);

            for (int x = 0; x < WorldSettings.MAPWIDTH; x++)
            {
                for (int z = 0; z < WorldSettings.MAPLENGTH; z++)
                {
                    int height = (int)(data[x, z] * 5) + 20 + WorldSettings.SEALEVEL;
                    for (int y = 0; y < height; y++)
                    {
                        if (_map[x, y, z] == BlockType.None)
                        {
                            _map[x, y, z] = BlockType.Sand;
                        }
                    }
                }
            }
        }

        private void GenerateDirtLayer(int offset, bool trees)
        {

            List<PerlinNoise2D> noiseFunctions = new List<PerlinNoise2D>();
            noiseFunctions.Add(new PerlinNoise2D(4, 2f));
            noiseFunctions.Add(new PerlinNoise2D(8, .5f));
            noiseFunctions.Add(new PerlinNoise2D(12, .25f));
            noiseFunctions.Add(new PerlinNoise2D(26, .125f));
            noiseFunctions.Add(new PerlinNoise2D(34, .0625f));
            noiseFunctions.Add(new PerlinNoise2D(64, .0125f));

            double[,] data = MapTools.SumNoiseFunctions(WorldSettings.MAPWIDTH, WorldSettings.MAPLENGTH, noiseFunctions);

            for (int x = 0; x < WorldSettings.MAPWIDTH; x++)
            {
                for (int z = 0; z < WorldSettings.MAPLENGTH; z++)
                {
                    int height = (int)(data[x, z] * 20) + offset + WorldSettings.SEALEVEL;
                    for (int y = 0; y < height; y++)
                    {
                        if (_map[x, y, z] == BlockType.None)
                        {
                            if (y == height - 1)
                            {
                                if (r.Next(250) == 1 && trees)
                                {
                                    BuildTree(x, y, z);
                                }
                                else
                                {
                                    _map[x, y, z] = BlockType.Grass;
                                    
                                }
                            }
                            else
                            {
                                if (r.Next(20) == 1)
                                {
                                    _map[x, y, z] = BlockType.Gravel;
                                }
                                else
                                {
                                    _map[x, y, z] = BlockType.Dirt;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GenerateRockLayer()
        {

            List<PerlinNoise2D> noiseFunctions = new List<PerlinNoise2D>();
            noiseFunctions.Add(new PerlinNoise2D(4, 5f));
            noiseFunctions.Add(new PerlinNoise2D(8, .5f));
            noiseFunctions.Add(new PerlinNoise2D(12, .25f));
            noiseFunctions.Add(new PerlinNoise2D(26, .125f));
            noiseFunctions.Add(new PerlinNoise2D(34, .0625f));
            noiseFunctions.Add(new PerlinNoise2D(64, .3825f));

            double[,] data = MapTools.SumNoiseFunctions(WorldSettings.MAPWIDTH, WorldSettings.MAPLENGTH, noiseFunctions);

            for (int x = 0; x < WorldSettings.MAPWIDTH; x++)
            {
                for (int z = 0; z < WorldSettings.MAPLENGTH; z++)
                {
                    int height = (int)(data[x, z] * 10) + 15 + WorldSettings.SEALEVEL;

                    for (int y = 0; y < height; y++)
                    {
                        if (y > WorldSettings.SNOWLINE + r.Next(3))
                        {
                            _map[x, y, z] = BlockType.Snow;
                        } 
                        else if (r.Next(8) == 1)
                        {
                            _map[x, y, z] = BlockType.Gravel;
                        }
                        else
                        {
                            _map[x, y, z] = BlockType.Rock;
                        }
                    }
                }
            }
        }
    }
}
