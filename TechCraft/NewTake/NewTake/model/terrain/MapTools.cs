using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model.terrain
{
    public class MapTools
    {

        public static void ClearChunkBlocks(Chunk chunk)
        {
            for (byte x = 0; x < Chunk.CHUNK_XMAX; x++)
            {
                for (byte y = 0; y < Chunk.CHUNK_YMAX; y++)
                {
                    for (byte z = 0; z < Chunk.CHUNK_ZMAX; z++)
                    {
                        chunk.setBlock(x, y, z, new Block(BlockType.None));
                    }
                }
            }
        }

        /// <summary>
        /// Get the interpolated points for the noise function
        /// </summary>
        /// <param name="noiseFn"></param>
        /// <returns></returns>
        public static double[,] SumNoiseFunctions(int width, int height, List<PerlinNoise2D> noiseFunctions)
        {
            double[,] summedValues = new double[width, height];

            // Sum each of the noise functions
            for (int i = 0; i < noiseFunctions.Count; i++)
            {
                double x_step = (float)width / (float)noiseFunctions[i].Frequency;
                double y_step = (float)height / (float)noiseFunctions[i].Frequency;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int a = (int)(x / x_step);
                        int b = a + 1;
                        int c = (int)(y / y_step);
                        int d = c + 1;

                        double intpl_val = noiseFunctions[i].getInterpolatedPoint(a, b, c, d, (x / x_step) - a, (y / y_step) - c);
                        summedValues[x,y] += intpl_val * noiseFunctions[i].Amplitude;
                    }
                }
            }
            return summedValues;
        }

    }
}
