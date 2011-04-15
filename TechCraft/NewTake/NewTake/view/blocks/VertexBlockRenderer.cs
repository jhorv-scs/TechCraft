using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using NewTake.model;

namespace NewTake.view.blocks
{
    public class VertexBlockRenderer
    {
        private readonly World world;

        public VertexBlockRenderer(World world)
        {        
            this.world = world;
        }

        #region BuildBlockVertices
        /// <summary>
        /// BuildBlockVertices making a block
        /// TODO surrounding block faces for digging ?
        /// <param name="block">block to build</param>
        /// <param name="blockPosition"> in viewableWorld coordinates already offset with current chunk position  </param>         
        /// </summary>
        public void BuildBlockVertices(ref List<VertexPositionTextureShade> vertexList, Block block, Chunk chunk, Vector3i chunkRelativePosition)
        {
            //optimized by using chunk.Blocks[][][] except for "out of current chunk" blocks

            Vector3i blockPosition = chunk.Position + chunkRelativePosition;

            Block blockXDecreasing, blockXIncreasing, blockYDecreasing, blockYIncreasing, blockZDecreasing, blockZIncreasing;

            if (chunkRelativePosition.X == 0 || 
                chunkRelativePosition.Y == 0 || 
                chunkRelativePosition.Z == 0 || 
                chunkRelativePosition.X == Chunk.SIZE.X - 1 || 
                chunkRelativePosition.Y == Chunk.SIZE.Y - 1 || 
                chunkRelativePosition.Z == Chunk.SIZE.Z - 1 )
            {
                blockXDecreasing = world.BlockAt(blockPosition.X - 1, blockPosition.Y, blockPosition.Z);
                blockYDecreasing = world.BlockAt(blockPosition.X, blockPosition.Y - 1, blockPosition.Z);
                blockZDecreasing = world.BlockAt(blockPosition.X, blockPosition.Y, blockPosition.Z - 1);
                blockXIncreasing = world.BlockAt(blockPosition.X + 1, blockPosition.Y, blockPosition.Z);
                blockYIncreasing = world.BlockAt(blockPosition.X, blockPosition.Y + 1, blockPosition.Z);
                blockZIncreasing = world.BlockAt(blockPosition.X, blockPosition.Y, blockPosition.Z + 1);
            }
            else
            {
                //blockXDecreasing = chunk.Blocks[chunkRelativePosition.X - 1, chunkRelativePosition.Y, chunkRelativePosition.Z];
                blockXDecreasing = chunk.Blocks[(chunkRelativePosition.X - 1) * Chunk.FlattenOffset + chunkRelativePosition.Z * Chunk.CHUNK_YMAX + chunkRelativePosition.Y];

                //blockXIncreasing = chunk.Blocks[chunkRelativePosition.X + 1, chunkRelativePosition.Y, chunkRelativePosition.Z];
                blockXIncreasing = chunk.Blocks[(chunkRelativePosition.X + 1) * Chunk.FlattenOffset + chunkRelativePosition.Z * Chunk.CHUNK_YMAX + chunkRelativePosition.Y];

                //blockYDecreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y - 1, chunkRelativePosition.Z];
                blockYDecreasing = chunk.Blocks[chunkRelativePosition.X * Chunk.FlattenOffset + chunkRelativePosition.Z * Chunk.CHUNK_YMAX + (chunkRelativePosition.Y - 1)];

                //blockYIncreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y + 1, chunkRelativePosition.Z];
                blockYIncreasing = chunk.Blocks[chunkRelativePosition.X * Chunk.FlattenOffset + chunkRelativePosition.Z * Chunk.CHUNK_YMAX + (chunkRelativePosition.Y + 1)];

                //blockZDecreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z - 1];
                blockZDecreasing = chunk.Blocks[chunkRelativePosition.X * Chunk.FlattenOffset + (chunkRelativePosition.Z - 1) * Chunk.CHUNK_YMAX + chunkRelativePosition.Y];

                //blockZIncreasing = chunk.Blocks[chunkRelativePosition.X, chunkRelativePosition.Y, chunkRelativePosition.Z + 1];
                blockZIncreasing = chunk.Blocks[chunkRelativePosition.X * Chunk.FlattenOffset + (chunkRelativePosition.Z + 1) * Chunk.CHUNK_YMAX + chunkRelativePosition.Y];   
            }

            if (!blockXDecreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.XDecreasing, block.Type);
            if (!blockXIncreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.XIncreasing, block.Type);

            if (!blockYDecreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.YDecreasing, block.Type);
            if (!blockYIncreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.YIncreasing, block.Type);

            if (!blockZDecreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.ZDecreasing, block.Type);
            if (!blockZIncreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.ZIncreasing, block.Type);
        }
        #endregion

        #region BuildFaceVertices
        public void BuildFaceVertices(ref List<VertexPositionTextureShade> vertexList, Vector3i blockPosition, BlockFaceDirection faceDir, BlockType blockType)
        {
            BlockTexture texture = BlockInformation.GetTexture(blockType, faceDir);

            //Debug.WriteLine(string.Format("BuildBlockVertices ({0},{1},{2}) : {3} ->{4} :", x, y, z, faceDir, texture));

            int faceIndex = 0;
            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    faceIndex = 0;
                    break;
                case BlockFaceDirection.XDecreasing:
                    faceIndex = 1;
                    break;
                case BlockFaceDirection.YIncreasing:
                    faceIndex = 2;
                    break;
                case BlockFaceDirection.YDecreasing:
                    faceIndex = 3;
                    break;
                case BlockFaceDirection.ZIncreasing:
                    faceIndex = 4;
                    break;
                case BlockFaceDirection.ZDecreasing:
                    faceIndex = 5;
                    break;
            }

            Vector2[] UVList = TextureHelper.UVMappings[(int)texture * 6 + faceIndex];

            float light = 2;//TODO light hardcoded to 2
           
            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    {
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition, new Vector3(1, 1, 1), new Vector3(1, 0, 0), light, UVList[0]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 1, 0), new Vector3(1, 0, 0), light, UVList[1]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 0, 1), new Vector3(1, 0, 0), light, UVList[2]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 0, 1), new Vector3(1, 0, 0), light, UVList[3]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 1, 0), new Vector3(1, 0, 0), light, UVList[4]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 0, 0), new Vector3(1, 0, 0), light, UVList[5]));
                    }
                    break;

                case BlockFaceDirection.XDecreasing:
                    {
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 1, 0), new Vector3(-1, 0, 0), light, UVList[0]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 1, 1), new Vector3(-1, 0, 0), light, UVList[1]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 0, 1), new Vector3(-1, 0, 0), light, UVList[2]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 1, 0), new Vector3(-1, 0, 0), light, UVList[3]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 0, 1), new Vector3(-1, 0, 0), light, UVList[4]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 0, 0), new Vector3(-1, 0, 0), light, UVList[5]));
                    }
                    break;

                case BlockFaceDirection.YIncreasing:
                    {
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 1, 0), new Vector3(0, 1, 0), light, UVList[0]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 1, 0), new Vector3(0, 1, 0), light, UVList[1]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 1, 1), new Vector3(0, 1, 0), light, UVList[2]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 1, 0), new Vector3(0, 1, 0), light, UVList[3]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 1, 1), new Vector3(0, 1, 0), light, UVList[4]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 1, 1), new Vector3(0, 1, 0), light, UVList[5]));
                    }
                    break;

                case BlockFaceDirection.YDecreasing:
                    {
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 0, 1), new Vector3(0, -1, 0), light, UVList[0]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 0, 0), new Vector3(0, -1, 0), light, UVList[1]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 0, 1), new Vector3(0, -1, 0), light, UVList[2]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 0, 1), new Vector3(0, -1, 0), light, UVList[3]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 0, 0), new Vector3(0, -1, 0), light, UVList[4]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 0, 0), new Vector3(0, -1, 0), light, UVList[5]));
                    }
                    break;

                case BlockFaceDirection.ZIncreasing:
                    {
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 1, 1), new Vector3(0, 0, 1), light, UVList[0]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 1, 1), new Vector3(0, 0, 1), light, UVList[1]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 0, 1), new Vector3(0, 0, 1), light, UVList[2]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 1,  1), new Vector3(0, 0, 1), light, UVList[3]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 0,  1), new Vector3(0, 0, 1), light, UVList[4]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 0,  1), new Vector3(0, 0, 1), light, UVList[5]));
                    }
                    break;

                case BlockFaceDirection.ZDecreasing:
                    {
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 1, 0), new Vector3(0, 0, -1), light, UVList[0]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 1, 0), new Vector3(0, 0, -1), light, UVList[1]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 0, 0), new Vector3(0, 0, -1), light, UVList[2]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(1, 0, 0), new Vector3(0, 0, -1), light, UVList[3]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 1, 0), new Vector3(0, 0, -1), light, UVList[4]));
                        vertexList.Add(ToVertexPositionTextureShade(blockPosition,new Vector3(0, 0, 0), new Vector3(0, 0, -1), light, UVList[5]));
                    }
                    break;
            }
        }
        #endregion

        private VertexPositionTextureShade ToVertexPositionTextureShade(Vector3i blockPosition, Vector3 vertexAdd, Vector3 normal, float light, Vector2 uv)
        {
            return new VertexPositionTextureShade(blockPosition.asVector3() + vertexAdd, normal, light, uv);
        }

    }
}
