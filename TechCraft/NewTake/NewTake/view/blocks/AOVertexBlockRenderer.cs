using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using NewTake.model;

namespace NewTake.view.blocks
{

    class AOVertexBlockRenderer
    {

        public List<VertexPositionDualTexture> vertexList;
        private readonly World world;

        public AOVertexBlockRenderer(World world)
        {
            this.world = world;
            Clear();
        }

        public void Clear()
        {
            vertexList = new List<VertexPositionDualTexture>();
        }

        #region BuildFaceVertices
        public void BuildFaceVertices(Vector3i blockPosition, Vector3i chunkRelativePosition, BlockFaceDirection faceDir, BlockType blockType, float aoTL, float aoTR, float aoBL, float aoBR)
        {
            BlockTexture texture = BlockInformation.GetTexture(blockType, faceDir);

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

            int crackStage = 0;


            Vector2[] UVList = TextureHelper.UVMappings[(int)texture * 6 + faceIndex];
            Vector2[] CrackUVList = TextureHelper.CrackMappings[crackStage * 6 + faceIndex];

            float light = 2;//TODO light hardcoded to 2

            Vector2 aoTilePosition = new Vector2(0, 0);

            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(1, 0, 0), light, UVList[0], CrackUVList[0], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(1, 0, 0), light, UVList[1], CrackUVList[1], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(1, 0, 0), light, UVList[2], CrackUVList[2], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(1, 0, 0), light, UVList[3], CrackUVList[3], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(1, 0, 0), light, UVList[4], CrackUVList[4], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(1, 0, 0), light, UVList[5], CrackUVList[5], aoBL);
                    }
                    break;

                case BlockFaceDirection.XDecreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(-1, 0, 0), light, UVList[0], CrackUVList[0], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(-1, 0, 0), light, UVList[1], CrackUVList[1], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(-1, 0, 0), light, UVList[2], CrackUVList[2], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(-1, 0, 0), light, UVList[3], CrackUVList[3], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(-1, 0, 0), light, UVList[4], CrackUVList[4], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(-1, 0, 0), light, UVList[5], CrackUVList[5], aoBR);
                    }
                    break;

                case BlockFaceDirection.YIncreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 1, 0), light, UVList[0], CrackUVList[0], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(0, 1, 0), light, UVList[1], CrackUVList[1], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(0, 1, 0), light, UVList[2], CrackUVList[2], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 1, 0), light, UVList[3], CrackUVList[3], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(0, 1, 0), light, UVList[4], CrackUVList[4], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(0, 1, 0), light, UVList[5], CrackUVList[5], aoTL);
                    }
                    break;

                case BlockFaceDirection.YDecreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, -1, 0), light, UVList[0], CrackUVList[0], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, -1, 0), light, UVList[1], CrackUVList[1], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, -1, 0), light, UVList[2], CrackUVList[2], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, -1, 0), light, UVList[3], CrackUVList[3], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, -1, 0), light, UVList[4], CrackUVList[4], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(0, -1, 0), light, UVList[5], CrackUVList[5], aoTR);
                    }
                    break;

                case BlockFaceDirection.ZIncreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(0, 0, 1), light, UVList[0], CrackUVList[0], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(0, 0, 1), light, UVList[1], CrackUVList[1], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, 0, 1), light, UVList[2], CrackUVList[2], aoBL);

                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(0, 0, 1), light, UVList[3], CrackUVList[3], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, 0, 1), light, UVList[4], CrackUVList[4], aoBL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, 0, 1), light, UVList[5], CrackUVList[5], aoBR);
                    }
                    break;

                case BlockFaceDirection.ZDecreasing:
                    {
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(0, 0, -1), light, UVList[0], CrackUVList[0], aoTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 0, -1), light, UVList[1], CrackUVList[1], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, 0, -1), light, UVList[2], CrackUVList[2], aoBR);

                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, 0, -1), light, UVList[3], CrackUVList[3], aoBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 0, -1), light, UVList[4], CrackUVList[4], aoTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(0, 0, -1), light, UVList[5], CrackUVList[5], aoBL);
                    }
                    break;
            }
        }
        #endregion

        private void AddVertex(Vector3i blockPosition, Vector3i chunkRelativePosition, Vector3 vertexAdd, Vector3 normal, float light, Vector2 uv1, Vector2 uv2, float aoWeight)
        {
            int x = (int)(chunkRelativePosition.X + vertexAdd.X);
            int y = (int)(chunkRelativePosition.Y + vertexAdd.Y);
            int z = (int)(chunkRelativePosition.Z + vertexAdd.Z);

            vertexList.Add(new VertexPositionDualTexture(blockPosition.asVector3() + vertexAdd, uv1, uv2, aoWeight));
            //indexList.Add(vertexIndex);
            //vertexInfo[x, y, z].Index = vertexIndex;
            //vertexInfo[x, y, z].Count++;
            //vertexIndex++;  
        }

    }
}
