using UnityEngine;
using PixelMiner.Enums;

namespace PixelMiner.Core
{
    public class VoxelAO
    {
        /* Voxel Face Index
        * 0: Right
        * 1: Up
        * 2: Front
        * 3: Left
        * 4: Down 
        * 5: Back
        */

        private static int CalculateVertexAO(bool side1, bool side2, bool corner)
        {
            if (side1 && side2)
                return 0;

            return 3 - (System.Convert.ToInt32(side1) + System.Convert.ToInt32(side2) + System.Convert.ToInt32(corner));
        }

        public static int ProcessAO(Chunk chunk, Vector3Int relativePosition, int vertex, int voxelFace)
        {
            int vertexAO = 3;
            BlockID side1 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z);
            BlockID side2 = chunk.GetBlock(relativePosition.x, relativePosition.y + 1, relativePosition.z - 1);
            BlockID corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z - 1);
            switch (voxelFace)
            {
                case 0:
                    if (vertex == 0)
                    {
                        side1 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y - 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y, relativePosition.z - 1);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y - 1, relativePosition.z - 1);
                    }
                    else if (vertex == 1)
                    {
                        side1 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y - 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y - 1, relativePosition.z + 1);
                    }
                    else if (vertex == 2)
                    {
                        side1 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y + 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y + 1, relativePosition.z + 1);
                    }
                    else if (vertex == 3)
                    {
                        side1 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y + 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y, relativePosition.z - 1);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y + 1, relativePosition.z - 1);
                    }
                    break;
                //default:
                case 1:
                    if (vertex == 0)
                    {
                        side1 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x, relativePosition.y + 1, relativePosition.z - 1);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z - 1);
                    }
                    else if (vertex == 1)
                    {
                        side1 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y + 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x, relativePosition.y + 1, relativePosition.z - 1);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y + 1, relativePosition.z - 1);
                    }
                    else if (vertex == 2)
                    {
                        side1 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y + 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x, relativePosition.y + 1, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y + 1, relativePosition.z + 1);
                    }
                    else if (vertex == 3)
                    {
                        side1 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x, relativePosition.y + 1, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z + 1);
                    }
                    break;
                case 2:
                    if (vertex == 0)
                    {
                        side1 = chunk.GetBlock(relativePosition.x, relativePosition.y - 1, relativePosition.z + 1);
                        side2 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y - 1, relativePosition.z + 1);
                    }
                    else if (vertex == 1)
                    {
                        side1 = chunk.GetBlock(relativePosition.x, relativePosition.y - 1, relativePosition.z + 1);
                        side2 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y - 1, relativePosition.z + 1);
                    }
                    else if (vertex == 2)
                    {
                        side1 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y, relativePosition.z + 1);
                        side2 = chunk.GetBlock(relativePosition.x, relativePosition.y + 1, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z + 1);

                    }
                    else if (vertex == 3)
                    {
                        side1 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y, relativePosition.z + 1);
                        side2 = chunk.GetBlock(relativePosition.x, relativePosition.y + 1, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y + 1, relativePosition.z + 1);
                    }
                    break;
                case 3:
                    if (vertex == 0)
                    {
                        side1 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y - 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y - 1, relativePosition.z + 1);
                    }
                    else if (vertex == 1)
                    {
                        side1 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y - 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y, relativePosition.z - 1);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y - 1, relativePosition.z - 1);
                    }
                    else if (vertex == 2)
                    {
                        side1 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y, relativePosition.z - 1);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z - 1);
                    }
                    else if (vertex == 3)
                    {
                        side1 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z + 1);
                    }
                    break;
                case 4:
                    if (vertex == 0)
                    {
                        side1 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y - 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x, relativePosition.y - 1, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y - 1, relativePosition.z + 1);
                    }
                    else if (vertex == 1)
                    {
                        side1 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y - 1, relativePosition.z);
                        side2 = chunk.GetBlock(relativePosition.x, relativePosition.y - 1, relativePosition.z + 1);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y - 1, relativePosition.z - 1);
                    }
                    else if (vertex == 2)
                    {
                        side1 = chunk.GetBlock(relativePosition.x, relativePosition.y - 1, relativePosition.z - 1);
                        side2 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y - 1, relativePosition.z);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y - 1, relativePosition.z - 1);
                    }
                    else if (vertex == 3)
                    {
                        side1 = chunk.GetBlock(relativePosition.x, relativePosition.y - 1, relativePosition.z - 1);
                        side2 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y - 1, relativePosition.z);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y - 1, relativePosition.z - 1);
                    }
                    break;
                case 5:
                    if (vertex == 0)
                    {
                        side1 = chunk.GetBlock(relativePosition.x, relativePosition.y - 1, relativePosition.z - 1);
                        side2 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y, relativePosition.z - 1);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y - 1, relativePosition.z - 1);
                    }
                    else if (vertex == 1)
                    {
                        side1 = chunk.GetBlock(relativePosition.x, relativePosition.y - 1, relativePosition.z - 1);
                        side2 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y, relativePosition.z - 1);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y - 1, relativePosition.z - 1);
                    }
                    else if (vertex == 2)
                    {
                        side1 = chunk.GetBlock(relativePosition.x, relativePosition.y + 1, relativePosition.z - 1);
                        side2 = chunk.GetBlock(relativePosition.x + 1, relativePosition.y, relativePosition.z - 1);
                        corner = chunk.GetBlock(relativePosition.x + 1, relativePosition.y + 1, relativePosition.z - 1);
                    }
                    else if (vertex == 3)
                    {
                        side1 = chunk.GetBlock(relativePosition.x, relativePosition.y + 1, relativePosition.z - 1);
                        side2 = chunk.GetBlock(relativePosition.x - 1, relativePosition.y, relativePosition.z - 1);
                        corner = chunk.GetBlock(relativePosition.x - 1, relativePosition.y + 1, relativePosition.z - 1);
                    }
                    break;
            }

            if (chunk.GetBlock(relativePosition) == BlockID.Water)
            {
                if (voxelFace == 1)
                {
                    vertexAO = CalculateVertexAO(false, false, false);
                }
                else
                {
                    vertexAO = CalculateVertexAO(true, true, corner == BlockID.Water);
                }
                //else if(voxelFace == 0)
                //{
                //    vertexAO = CalculateVertexAO(true, true, corner == BlockID.Water);
                //}
                //else
                //{
                //    vertexAO = CalculateVertexAO(side1 == BlockID.Water, side2 == BlockID.Water, corner == BlockID.Water);
                //}             
            }
            else
            {
                vertexAO = CalculateVertexAO(side1.IsSolidOpaqueVoxel() || side1.IsSolidTransparentVoxel() || side1 == BlockID.Water,
                side2.IsSolidOpaqueVoxel() || side2.IsSolidTransparentVoxel() || side2 == BlockID.Water,
                corner.IsSolidOpaqueVoxel() || corner.IsSolidTransparentVoxel() || corner == BlockID.Water);
            }

            return vertexAO;
        }

    }
}
