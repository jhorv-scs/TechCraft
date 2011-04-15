using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using NewTake.model;

namespace NewTake.view.blocks
{

    class LightingVertexBlockRenderer
    {
        public List<short> indexList;
        public List<VertexPositionTextureLight> vertexList;
        private readonly World world;
        private short index;

        public LightingVertexBlockRenderer(World world)
        {
            this.world = world;
            Clear();
        }

        public void Clear()
        {
            vertexList = new List<VertexPositionTextureLight>();
            indexList = new List<short>();
            index = 0;
        }

        #region BuildFaceVertices
        public void BuildFaceVertices(Vector3i blockPosition, Vector3i chunkRelativePosition, BlockFaceDirection faceDir, BlockType blockType, float aoTL, float aoTR, float aoBL, float aoBR)
        {
            BlockTexture texture = BlockInformation.GetTexture(blockType, faceDir);

            int faceIndex = (int)faceDir;

            Vector2[] UVList = TextureHelper.UVMappings[(int)texture * 6 + faceIndex];

            float sunLightTR, sunLightTL, sunLightBR, sunLightBL;
            //sunLightTR = 1f; sunLightTL = 1f; sunLightBR = 1f; sunLightBL = 1f;
            sunLightTR = aoTR; sunLightTL = aoTL; sunLightBR = aoBR; sunLightBL = aoBL;

            Vector3 localLightTR, localLightTL, localLightBR, localLightBL;

            localLightTR = Color.White.ToVector3();
            localLightTL = Color.White.ToVector3();
            localLightBR = Color.White.ToVector3();
            localLightBL = Color.White.ToVector3();

            switch (faceDir)
            {
                case BlockFaceDirection.XIncreasing:
                    {
                        //TR,TL,BR,BR,TL,BL
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(1, 0, 0), UVList[0], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(1, 0, 0), UVList[1], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(1, 0, 0), UVList[2], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(1, 0, 0), UVList[5], sunLightBL, localLightBL);
                        AddIndex(0, 1, 2, 2, 1, 3);
                    }
                    break;

                case BlockFaceDirection.XDecreasing:
                    {
                        //TR,TL,BL,TR,BL,BR
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(-1, 0, 0), UVList[0], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(-1, 0, 0), UVList[1], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(-1, 0, 0), UVList[5], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(-1, 0, 0), UVList[2], sunLightBL, localLightBL);
                        AddIndex(0, 1, 3, 0, 3, 2);
                    }
                    break;

                case BlockFaceDirection.YIncreasing:
                    {
                        //BL,BR,TR,BL,TR,TL
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(0, 1, 0), UVList[4], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(0, 1, 0), UVList[5], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(0, 1, 0), UVList[1], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 1, 0), UVList[3], sunLightBL, localLightBL);
                        AddIndex(3, 2, 0, 3, 0, 1);
                    }
                    break;

                case BlockFaceDirection.YDecreasing:
                    {
                        //TR,BR,TL,TL,BR,BL
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, -1, 0), UVList[0], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, -1, 0), UVList[2], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, -1, 0), UVList[4], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(0, -1, 0), UVList[5], sunLightBL, localLightBL);
                        AddIndex(0, 2, 1, 1, 2, 3);
                    }
                    break;

                case BlockFaceDirection.ZIncreasing:
                    {
                        //TR,TL,BL,TR,BL,BR
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 1), new Vector3(0, 0, 1), UVList[0], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 1), new Vector3(0, 0, 1), UVList[1], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 1), new Vector3(0, 0, 1), UVList[5], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 1), new Vector3(0, 0, 1), UVList[2], sunLightBL, localLightBL);
                        AddIndex(0, 1, 3, 0, 3, 2);
                    }
                    break;

                case BlockFaceDirection.ZDecreasing:
                    {
                        //TR,TL,BR,BR,TL,BL
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 1, 0), new Vector3(0, 0, -1), UVList[0], sunLightTR, localLightTR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 1, 0), new Vector3(0, 0, -1), UVList[1], sunLightTL, localLightTL);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(1, 0, 0), new Vector3(0, 0, -1), UVList[2], sunLightBR, localLightBR);
                        AddVertex(blockPosition, chunkRelativePosition, new Vector3(0, 0, 0), new Vector3(0, 0, -1), UVList[5], sunLightBL, localLightBL);
                        AddIndex(0, 1, 2, 2, 1, 3);
                    }
                    break;
            }
        }
        #endregion

        private void AddVertex(Vector3i blockPosition, Vector3i chunkRelativePosition, Vector3 vertexAdd, Vector3 normal, Vector2 uv1, float sunLight, Vector3 localLight)
        {
            vertexList.Add(new VertexPositionTextureLight(blockPosition.asVector3() + vertexAdd, uv1, sunLight, localLight));
        }

        private void AddIndex(short i1, short i2, short i3, short i4, short i5, short i6)
        {
            indexList.Add((short)(index + i1));
            indexList.Add((short)(index + i2));
            indexList.Add((short)(index + i3));
            indexList.Add((short)(index + i4));
            indexList.Add((short)(index + i5));
            indexList.Add((short)(index + i6));
            index += 4;
        }
    }
}
