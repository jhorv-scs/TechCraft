using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NewTake.model;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace NewTake.view.blocks
{
    class VertexBlockRenderer
    {
        private readonly World world;

        public VertexBlockRenderer(World world)
        {        
            this.world = world;
        }
        /// <summary>
        /// BuildBlockVertices making a block
        /// TODO surrounding block faces for digging ?
        /// <param name="block">block to build</param>
        /// <param name="blockPosition"> in viewableWorld coordinates already offset with current chunk position  </param>         
        /// </summary>
        public void BuildBlockVertices(ref List<VertexPositionTextureShade> vertexList, Block block, Vector3i blockPosition)
        {
            //TODO maybe optimizable by using chunk.Blocks[][][] except for "out of current chunk" blocks
 
            Block blockXDecreasing = world.BlockAt(blockPosition.X - 1, blockPosition.Y, blockPosition.Z);
            Block blockXIncreasing = world.BlockAt(blockPosition.X + 1, blockPosition.Y, blockPosition.Z);

            Block blockYDecreasing = world.BlockAt(blockPosition.X, blockPosition.Y - 1, blockPosition.Z);
            Block blockYIncreasing = world.BlockAt(blockPosition.X, blockPosition.Y + 1, blockPosition.Z);

            Block blockZDecreasing = world.BlockAt(blockPosition.X, blockPosition.Y, blockPosition.Z - 1);
            Block blockZIncreasing = world.BlockAt(blockPosition.X, blockPosition.Y, blockPosition.Z + 1);


            if (!blockXDecreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.XDecreasing, block.Type);
            if (!blockXIncreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.XIncreasing, block.Type);

            if (!blockYDecreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.YDecreasing, block.Type);
            if (!blockYIncreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.YIncreasing, block.Type);

            if (!blockZDecreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.ZDecreasing, block.Type);
            if (!blockZIncreasing.Solid) BuildFaceVertices(ref vertexList, blockPosition, BlockFaceDirection.ZIncreasing, block.Type);
        }


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

        private VertexPositionTextureShade ToVertexPositionTextureShade(Vector3i blockPosition, Vector3 vertexAdd, Vector3 normal, float light, Vector2 uv)
        {
           
            return new VertexPositionTextureShade(blockPosition.asVector3() + vertexAdd, normal, light, uv);

        }

    }
}
