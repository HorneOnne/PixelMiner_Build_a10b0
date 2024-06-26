using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using PixelMiner.Enums;
using System;
using PixelMiner.Utilities;
using System.Collections.Generic;
using System.Linq;
using PixelMiner.Extensions;
using System.Buffers;

namespace PixelMiner.Core
{
    /* Voxel Face Index
        * 0: Right
        * 1: Up
        * 2: Front
        * 3: Left
        * 4: Down 
        * 5: Back
        */

    public class MeshUtils : MonoBehaviour
    {
        public static MeshUtils Instance { get; private set; }
        public ModelData TorchModel;
        private Main _main;
        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            _main = Main.Instance;
        }

        public static Vector2[,] ColorMapUVs =
        {
            /*
             * Order: BOTTOM_LEFT -> BOTTOM_RIGHT -> TOP_LEFT -> TOP_RIGHT.
             */            
            /*PLAINS*/
            {new Vector2(0.234375f, 0.640625f), new Vector2(0.25f, 0.640625f),
            new Vector2(0.234375f, 0.65625f), new Vector2(0.25f, 0.65625f)},


            /*FOREST*/
            {new Vector2(0.15625f, 0.796875f), new Vector2(0.171875f, 0.796875f),
            new Vector2(0.15625f, 0.8125f), new Vector2(0.171875f, 0.8125f)}, 

            /*JUNGLE*/
            {new Vector2(0.15625f, 0.796875f), new Vector2(0.171875f, 0.796875f),
            new Vector2(0.15625f, 0.8125f), new Vector2(0.171875f, 0.8125f)}, 

            /*DESERT*/
            {new Vector2(0f, 0f), new Vector2(0.015625f, 0f),
            new Vector2(0f, 0.015625f), new Vector2(0.015625f, 0.015625f)}, 

            /*NONE*/
            {new Vector2(0.8125f, 0.8125f), new Vector2(0.828125f, 0.8125f),
            new Vector2(0.8125f, 0.828125f), new Vector2(0.828125f, 0.828125f)},
        };

        public static Vector2[,] LightMapUVs =
        {
             {new Vector2(0.875f, 1f), new Vector2(0.9375f, 1f),
             new Vector2(0.875f, 0.9375f), new Vector2(0.9375f, 0.9375f)},

            {new Vector2(0.875f, 0.9375f), new Vector2(0.9375f, 0.9375f),
             new Vector2(0.875f, 0.875f), new Vector2(0.9375f, 0.875f)},

            {new Vector2(0.875f, 0.875f), new Vector2(0.9375f, 0.875f),
             new Vector2(0.875f, 0.8125f), new Vector2(0.9375f, 0.8125f)},

            {new Vector2(0.875f, 0.8125f), new Vector2(0.9375f, 0.8125f),
             new Vector2(0.875f, 0.75f), new Vector2(0.9375f, 0.75f)},

            {new Vector2(0.875f, 0.75f), new Vector2(0.9375f, 0.75f),
             new Vector2(0.875f, 0.6875f), new Vector2(0.9375f, 0.6875f)},

            {new Vector2(0.875f, 0.6875f), new Vector2(0.9375f, 0.6875f),
             new Vector2(0.875f, 0.625f), new Vector2(0.9375f, 0.625f)},

            {new Vector2(0.875f, 0.625f), new Vector2(0.9375f, 0.625f),
             new Vector2(0.875f, 0.5625f), new Vector2(0.9375f, 0.5625f)},

            {new Vector2(0.875f, 0.5625f), new Vector2(0.9375f, 0.5625f),
             new Vector2(0.875f, 0.5f), new Vector2(0.9375f, 0.5f)},

            {new Vector2(0.875f, 0.5f), new Vector2(0.9375f, 0.5f),
             new Vector2(0.875f, 0.4375f), new Vector2(0.9375f, 0.4375f)},

            {new Vector2(0.875f, 0.4375f), new Vector2(0.9375f, 0.4375f),
             new Vector2(0.875f, 0.375f), new Vector2(0.9375f, 0.375f)},

            {new Vector2(0.875f, 0.375f), new Vector2(0.9375f, 0.375f),
             new Vector2(0.875f, 0.3125f), new Vector2(0.9375f, 0.3125f)},

            {new Vector2(0.875f, 0.3125f), new Vector2(0.9375f, 0.3125f),
             new Vector2(0.875f, 0.25f), new Vector2(0.9375f, 0.25f)},

            {new Vector2(0.875f, 0.25f), new Vector2(0.9375f, 0.25f),
             new Vector2(0.875f, 0.1875f), new Vector2(0.9375f, 0.1875f)},

            {new Vector2(0.875f, 0.1875f), new Vector2(0.9375f, 0.1875f),
             new Vector2(0.875f, 0.125f), new Vector2(0.9375f, 0.125f)},

            {new Vector2(0.875f, 0.125f), new Vector2(0.9375f, 0.125f),
             new Vector2(0.875f, 0.0625f), new Vector2(0.9375f, 0.0625f)},

            {new Vector2(0.875f, 0.0625f), new Vector2(0.9375f, 0.0625f),
             new Vector2(0.875f, 0f), new Vector2(0.9375f, 0f)},
        };


        public static void GetColorMap(ref Vector2[] colorUVs, float heatValue, bool clear = false)
        {
            //float tileSize = 1 / 256f;
            //float u = 0f;
            ////float v = (heatValue) * 256;
            //float v = (0.76f) * 256;


            if (clear)
            {
                colorUVs[0] = new Vector2(0.8125f, 0.8125f);
                colorUVs[1] = new Vector2(0.828125f, 0.8125f);
                colorUVs[2] = new Vector2(0.8125f, 0.828125f);
                colorUVs[3] = new Vector2(0.828125f, 0.828125f);
            }
            else
            {
                //colorUVs[0] = new Vector2(tileSize * u, tileSize * v);
                //colorUVs[1] = new Vector2(tileSize * u + tileSize, tileSize * v);
                //colorUVs[2] = new Vector2(tileSize * u, tileSize * v + tileSize);
                //colorUVs[3] = new Vector2(tileSize * u + tileSize, tileSize * v + tileSize);


                colorUVs[0] = new Vector2(0.15625f, 0.796875f);
                colorUVs[1] = new Vector2(0.171875f, 0.796875f);
                colorUVs[2] = new Vector2(0.15625f, 0.8125f);
                colorUVs[3] = new Vector2(0.171875f, 0.8125f);
            }
        }

        public static void GetAOUVs(byte[] vertexAO, ref Vector4[] uv3s)
        {
            if (vertexAO != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (vertexAO[i] == 0)
                    {
                        vertexAO[i] = 208;
                    }
                    else if (vertexAO[i] == 1 || vertexAO[i] == 2)
                    {
                        vertexAO[i] = 224;
                    }
                    else if (vertexAO[i] == 3)
                    {
                        vertexAO[i] = 240;
                    }
                }
                uv3s[0] = new Vector4(vertexAO[0], vertexAO[1], vertexAO[2], vertexAO[3]);
                uv3s[1] = new Vector4(vertexAO[0], vertexAO[1], vertexAO[2], vertexAO[3]);
                uv3s[2] = new Vector4(vertexAO[0], vertexAO[1], vertexAO[2], vertexAO[3]);
                uv3s[3] = new Vector4(vertexAO[0], vertexAO[1], vertexAO[2], vertexAO[3]);
            }
        }




        public static void GetAmbientLightUVs(byte[] ambientLight, ref Vector4[] uv4s)
        {
            float i0 = ambientLight[0] / (float)LightUtils.MAX_LIGHT_INTENSITY;
            float i1 = ambientLight[1] / (float)LightUtils.MAX_LIGHT_INTENSITY;
            float i2 = ambientLight[2] / (float)LightUtils.MAX_LIGHT_INTENSITY;
            float i3 = ambientLight[3] / (float)LightUtils.MAX_LIGHT_INTENSITY;

            uv4s[0] = new Vector4(i0, i0, i0, i0);
            uv4s[1] = new Vector4(i1, i1, i1, i1);
            uv4s[2] = new Vector4(i2, i2, i2, i2);
            uv4s[3] = new Vector4(i3, i3, i3, i3);

            //uv4s[0] = new Vector4(i0, i0, i0, i0);
            //uv4s[1] = new Vector4(i0, i0, i0, i0);
            //uv4s[2] = new Vector4(i0, i0, i0, i0);
            //uv4s[3] = new Vector4(i0, i0, i0, i0);
        }

        public static void GetCrackUVs(Chunk chunk, Vector3Int relativePosition, ref Vector3[] uvs)
        {
            int hardnessTextureIndex;
            int index = chunk.IndexOf(relativePosition.x, relativePosition.y, relativePosition.z);
            float breakingPercent = 1.0f - chunk.Hardnesses[index] / (float)chunk.ChunkData[index].Hardness();


            if (breakingPercent == 0)
            {
                hardnessTextureIndex = (ushort)TextureType.NoCrack;
            }
            else
            {
                hardnessTextureIndex = (int)Mathf.Lerp((ushort)TextureType.Crack_0, (ushort)TextureType.Crack_9, breakingPercent);
                //Debug.Log(hardnessTextureIndex);
            }

            //hardnessTextureIndex = 246;
            uvs[0] = new Vector3(0, 0, hardnessTextureIndex);
            uvs[1] = new Vector3(1, 0, hardnessTextureIndex);
            uvs[2] = new Vector3(1, 1, hardnessTextureIndex);
            uvs[3] = new Vector3(0, 1, hardnessTextureIndex);
        }

        private static void GetGrassUVs(BlockID blockID, float heatValue, ref Vector3[] uvs, ref Vector2[] uv2s, int heightFromOrigin = 0)
        {
            //Debug.Log(heatValue);
            int blockIndex;
            GetColorMap(ref uv2s, heatValue, clear: true);

            if (blockID == BlockID.Grass)
            {
                GetColorMap(ref uv2s, heatValue);
                blockIndex = (ushort)BlockID.Grass;
                uvs[0] = new Vector3(0, 0, blockIndex);
                uvs[1] = new Vector3(1, 0, blockIndex);
                uvs[2] = new Vector3(1, 1, blockIndex);
                uvs[3] = new Vector3(0, 1, blockIndex);
            }
            else if (blockID == BlockID.TallGrass)
            {
                GetColorMap(ref uv2s, heatValue);
                switch (heightFromOrigin)
                {
                    default:
                    case 0:
                        blockIndex = (ushort)TextureType.BelowTallGrass;
                        break;
                    case 1:
                        blockIndex = (ushort)TextureType.AboveTallGrass;
                        break;
                }

                uvs[0] = new Vector3(0, 0, blockIndex);
                uvs[1] = new Vector3(1, 0, blockIndex);
                uvs[2] = new Vector3(1, 1, blockIndex);
                uvs[3] = new Vector3(0, 1, blockIndex);
            }
            else if (blockID == BlockID.Shrub)
            {
                blockIndex = (ushort)TextureType.Shrub;
                uvs[0] = new Vector3(0, 0, blockIndex);
                uvs[1] = new Vector3(1, 0, blockIndex);
                uvs[2] = new Vector3(1, 1, blockIndex);
                uvs[3] = new Vector3(0, 1, blockIndex);
            }
        }
        private static void GetBlockUVs(BlockID blockID, int face, int width, int height, ref Vector3[] uvs)
        {
            int blockIndex;
            switch (blockID)
            {
                default:
                    blockIndex = (ushort)blockID;
                    break;
                case BlockID.DirtGrass:
                    if (face == 1)
                    {
                        blockIndex = (ushort)TextureType.GrassTop;
                    }
                    else if (face == 4)
                    {
                        blockIndex = (ushort)TextureType.Dirt;
                    }
                    else
                    {
                        blockIndex = (ushort)blockID;
                    }
                    break;
                case BlockID.SnowDirtGrass:
                    if (face == 1)
                    {
                        blockIndex = (ushort)TextureType.SnowGrassTop;
                    }
                    else if (face == 4)
                    {
                        blockIndex = (ushort)TextureType.Dirt;
                    }
                    else
                    {
                        blockIndex = (ushort)TextureType.SnowGrassSide;
                    }
                    break;
                case BlockID.Leaves:
                    blockIndex = (ushort)blockID;
                    break;
                case BlockID.PineLeaves:
                    blockIndex = (ushort)blockID;
                    break;
                case BlockID.Wood:
                    if (face == 1 || face == 4)
                    {
                        blockIndex = (ushort)TextureType.HeartWood;
                    }
                    else
                    {
                        blockIndex = (ushort)TextureType.BarkWood;
                    }
                    break;
                case BlockID.PineWood:
                    if (face == 1 || face == 4)
                    {
                        blockIndex = (ushort)TextureType.HeartPineWood;
                    }
                    else
                    {
                        blockIndex = (ushort)TextureType.BarkPineWood;
                    }
                    break;
                case BlockID.Cactus:
                    if (face == 1)
                    {
                        blockIndex = (ushort)TextureType.Cactus_Upper;
                    }
                    else
                    {
                        blockIndex = (ushort)TextureType.Cactus_Middle;
                    }
                    break;
                case BlockID.GoldBlock:
                    blockIndex = (ushort)TextureType.GoldBlock;
                    break;
                case BlockID.RedLight:
                case BlockID.GreenLight:
                case BlockID.BlueLight:
                    blockIndex = (ushort)TextureType.LightSourceTest;
                    break;
            }

            uvs[0] = new Vector3(0, 0, blockIndex);
            uvs[1] = new Vector3(width, 0, blockIndex);
            uvs[2] = new Vector3(width, height, blockIndex);
            uvs[3] = new Vector3(0, height, blockIndex);
        }

        private static void GetNonblockUVs(BlockID blockID, ref Vector3[] uvs)
        {
            int blockIndex = (ushort)blockID;
            switch (blockID)
            {
                case BlockID.Torch:
                    uvs[0] = new Vector3(0, 0, blockIndex);
                    uvs[1] = new Vector3(1, 0, blockIndex);
                    uvs[2] = new Vector3(1, 1, blockIndex);
                    uvs[3] = new Vector3(0, 1, blockIndex);
                    break;
                default:
                    break;
            }
        }

        public static void GetColorMapUVs(BlockID blockID, int face, float heatValue, ref Vector2[] uv2s)
        {
            GetColorMap(ref uv2s, heatValue, clear: true);
            switch (blockID)
            {
                default:
                    break;
                case BlockID.DirtGrass:
                    if (face == 1)
                    {
                        GetColorMap(ref uv2s, heatValue);
                    }
                    break;
                case BlockID.Leaves:
                    GetColorMap(ref uv2s, heatValue);
                    break;
                case BlockID.PineLeaves:
                    GetColorMap(ref uv2s, heatValue);
                    break;
            }
        }

        public static Vector2[] GetDepthUVs(float depth)
        {
            float depthValue = MathHelper.Map(depth, 0.2f, 0.45f, 0.0f, 0.75f);
            float offset = 0.05f;

            return new Vector2[]
            {
                new Vector2(0, depthValue),
                new Vector2(1, depthValue),
                 new Vector2(0f, Mathf.Clamp01(depthValue + offset)),
                new Vector2(1, Mathf.Clamp01(depthValue + offset))
            };
        }

        private static float MapValue(float value, float originalMin, float originalMax, float targetMin, float targetMax)
        {
            // Ensure the value is within the original range
            float clampedValue = Mathf.Clamp(value, originalMin, originalMax);

            // Perform the mapping
            float mappedValue = targetMin + (clampedValue - originalMin) / (originalMax - originalMin) * (targetMax - targetMin);

            return mappedValue;
        }
        public static float GetLightIntensityNormalize(byte lightIntensity, AnimationCurve lightAnimCurve)
        {
            return lightAnimCurve.Evaluate(lightIntensity / (float)LightUtils.MAX_LIGHT_INTENSITY);
        }

        private static Color32 GetBlockLightPropagationForAdjacentFace(Chunk chunk, Vector3Int blockPosition, int voxelFace, BlockID blockID)
        {
            Vector3Int offset = Vector3Int.up;
            switch (voxelFace)
            {
                case 0:
                    offset = Vector3Int.right;
                    break;
                case 1:
                    offset = Vector3Int.up;
                    break;
                case 2:
                    offset = Vector3Int.forward;
                    break;
                case 3:
                    offset = Vector3Int.left;
                    break;
                case 4:
                    offset = Vector3Int.down;
                    break;
                case 5:
                    offset = Vector3Int.back;
                    break;
                default:
                    offset = Vector3Int.zero;
                    break;
            }

            Vector3Int blockOffsetPosition = blockPosition + offset;
            chunk.GetBlockLight(blockOffsetPosition, out byte red, out byte green, out byte blue);
            int unitEachBlock = 255 / LightUtils.MAX_LIGHT_INTENSITY;
            byte redChannel = (byte)(red * unitEachBlock);
            byte greenChannel = (byte)(green * unitEachBlock);
            byte blueChannel = (byte)(blue * unitEachBlock);
            return new Color32(redChannel, greenChannel, blueChannel, 255);
        }
        private static byte GetAmbientLightPropagationForAdjacentFace(Chunk chunk, Vector3Int blockPosition, int face)
        {
            Vector3Int offset;

            switch (face)
            {
                case 0:
                    offset = Vector3Int.right;
                    break;
                case 1:
                    offset = Vector3Int.up;
                    break;
                case 2:
                    offset = Vector3Int.forward;
                    break;
                case 3:
                    offset = Vector3Int.left;
                    break;
                case 4:
                    offset = Vector3Int.down;
                    break;
                case 5:
                    offset = Vector3Int.back;
                    break;
                default:
                    offset = Vector3Int.zero;
                    break;

            }

            //if (face == 1)
            //{
            //    offset = Vector3Int.up;
            //}
            //else if (face == 5)
            //{
            //    offset = new Vector3Int(0,-1,-2);
            //}
            //else
            //{
            //    offset = Vector3Int.zero;
            //}

            //Vector3Int blockOffsetPosition = blockPosition + offset;
            //blockOffsetPosition = new Vector3Int(Mathf.Clamp(blockOffsetPosition.x, 0, chunk._width),
            //                               Mathf.Clamp(blockOffsetPosition.y, 0, chunk._height),
            //                               Mathf.Clamp(blockOffsetPosition.z, 0, chunk._depth));
            //return chunk.GetAmbientLight(blockOffsetPosition);

            Vector3Int blockOffsetPosition = blockPosition + offset;
            return chunk.GetAmbientLight(blockOffsetPosition);
        }
        private static BlockID GetblockIDLightAdjacentFace(Chunk chunk, Vector3Int blockPosition, int face)
        {
            Vector3Int offset;

            switch (face)
            {
                case 0:
                    offset = Vector3Int.right;
                    break;
                case 1:
                    offset = Vector3Int.up;
                    break;
                case 2:
                    offset = Vector3Int.forward;
                    break;
                case 3:
                    offset = Vector3Int.left;
                    break;
                case 4:
                    offset = Vector3Int.down;
                    break;
                case 5:
                    offset = Vector3Int.back;
                    break;
                default:
                    offset = Vector3Int.zero;
                    break;

            }


            Vector3Int blockOffsetPosition = blockPosition + offset;
            return chunk.GetBlock(blockOffsetPosition);
        }










        #region Create Unity Mesh
        public static void CreateMesh(MeshData meshData, ref Mesh mesh)
        {
            mesh.Clear();
            //Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(meshData.Vertices);
            mesh.SetColors(meshData.Colors);
            mesh.SetTriangles(meshData.Triangles, 0);
            mesh.SetUVs(0, meshData.UVs);
            mesh.SetUVs(1, meshData.UV2s);
            mesh.SetUVs(2, meshData.UV3s);
            mesh.SetUVs(3, meshData.UV4s);
            mesh.SetUVs(4, meshData.UV5s);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            //return mesh;
        }
        public static void CreateWaterMesh(MeshData meshData, ref Mesh mesh)
        {
            mesh.Clear();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(meshData.Vertices);
            mesh.SetColors(meshData.Colors);
            mesh.SetTriangles(meshData.Triangles, 0);
            mesh.SetUVs(0, meshData.UVs);
            mesh.SetUVs(1, meshData.UV3s);
            mesh.SetUVs(2, meshData.UV4s);
            mesh.SetUVs(3, meshData.WaterFlowDirMapUVs);
            mesh.SetUVs(4, meshData.WaterFlowingOffsetUVs);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
        #endregion












        #region Render blocks
        private async Task<ChunkMeshBuilder> RenderChunkFace(Chunk chunk, int voxelFace, bool isTransparentMesh, CancellationToken token)
        {
            ChunkMeshBuilder builder = ChunkMeshBuilderPool.Get();
            builder.InitOrLoad(chunk.Dimensions);

            Vector3Int dimensions = chunk.Dimensions;
            bool isBackFace = voxelFace > 2;
            int d = voxelFace % 3;
            int u, v;
            BlockID currBlock;
            bool smoothLight = true;


            Vector3Int startPos, currPos, quadSize = Vector3Int.one, m, n, offsetPos;
            Vector3[] vertices = ArrayPool<Vector3>.Shared.Rent(4);
            Vector3[] uvs = ArrayPool<Vector3>.Shared.Rent(4);     // Block textures
            Vector2[] uv2s = ArrayPool<Vector2>.Shared.Rent(4);    // Colormap
            Vector4[] uv3s = ArrayPool<Vector4>.Shared.Rent(4);    // AO
            Vector4[] uv4s = ArrayPool<Vector4>.Shared.Rent(4);    // Ambient light
            Vector3[] uv5s = ArrayPool<Vector3>.Shared.Rent(4);    // Breaking textures
            Color32[] colors = ArrayPool<Color32>.Shared.Rent(4);  // Block light
            byte[] verticesAO = ArrayPool<byte>.Shared.Rent(4);   // AO (0 -> 3)
            byte[] ambientColorIntensity = ArrayPool<byte>.Shared.Rent(4);
            Vector3Int[] faceNeighbors = ArrayPool<Vector3Int>.Shared.Rent(6);
            BlockID[] slaf = ArrayPool<BlockID>.Shared.Rent(4);   // Smooth light adjacent face neighbors
            bool[] slafSolid = ArrayPool<bool>.Shared.Rent(4);   // Smooth light adjacent face neighbors

            await Task.Run(() =>
            {
                switch (d)
                {
                    case 0:
                        u = 2;
                        v = 1;
                        break;
                    case 1:
                        u = 0;
                        v = 2;
                        break;
                    default:
                        u = 0;
                        v = 1;
                        break;
                }


                startPos = new Vector3Int();
                currPos = new Vector3Int();


                for (startPos[d] = 0; startPos[d] < dimensions[d]; startPos[d]++)
                {
                    Array.Clear(builder.Merged[d], 0, builder.Merged[d].Length);

                    // Build the slices of mesh.
                    for (startPos[u] = 0; startPos[u] < dimensions[u]; startPos[u]++)
                    {
                        for (startPos[v] = 0; startPos[v] < dimensions[v]; startPos[v]++)
                        {
                            if (startPos.y == 0 && voxelFace != 1) continue;
                            currBlock = chunk.GetBlock(startPos);
                            if (currBlock == BlockID.Air) continue;


                            if (isTransparentMesh)
                            {
                                // TRANSPARENT SOLID
                                if (chunk.GetBlock(startPos).IsSolidTransparentVoxel() == false)
                                {
                                    continue;
                                }

                                if (chunk.IsBlockFaceVisible(startPos, d, isBackFace, voxelFace) == false)
                                {
                                    continue;
                                }

                                if (chunk.IsTransparentBlockFaceVisible(startPos, d, isBackFace) == false)
                                {
                                    continue;
                                }

                            }
                            else
                            {
                                // OPAQUE SOLID
                                if (chunk.GetBlock(startPos).IsSolidOpaqueVoxel() == false ||
                                    chunk.IsBlockFaceVisible(startPos, d, isBackFace, voxelFace) == false)
                                {
                                    continue;
                                }

                                if (chunk.GetBlock(startPos).IsSolidTransparentVoxel() == true)
                                {
                                    continue;
                                }
                            }


                            #region ambient occlusion
                            // Ambient occlusion
                            // =================                
                            verticesAO[0] = (byte)VoxelAO.ProcessAO(chunk, startPos, 0, voxelFace);
                            verticesAO[1] = (byte)VoxelAO.ProcessAO(chunk, startPos, 1, voxelFace);
                            verticesAO[2] = (byte)VoxelAO.ProcessAO(chunk, startPos, 2, voxelFace);
                            verticesAO[3] = (byte)VoxelAO.ProcessAO(chunk, startPos, 3, voxelFace);
                            #endregion





                            // Add new quad to mesh data.
                            m = new Vector3Int();
                            n = new Vector3Int();

                            m[u] = quadSize[u];
                            n[v] = quadSize[v];

                            offsetPos = startPos;
                            offsetPos[d] += isBackFace ? 0 : 1;

                            vertices[0] = offsetPos;
                            vertices[1] = offsetPos + m;
                            vertices[2] = offsetPos + m + n;
                            vertices[3] = offsetPos + n;


                            if (voxelFace == 3 || voxelFace == 2)
                            {
                                m = -m;
                            }



                            slaf[0] = GetblockIDLightAdjacentFace(chunk, startPos, voxelFace);
                            slaf[1] = GetblockIDLightAdjacentFace(chunk, startPos + m, voxelFace);
                            slaf[2] = GetblockIDLightAdjacentFace(chunk, startPos + m + n, voxelFace);
                            slaf[3] = GetblockIDLightAdjacentFace(chunk, startPos + n, voxelFace);

                            slafSolid[0] = slaf[0].IsSolidOpaqueVoxel();
                            slafSolid[1] = slaf[1].IsSolidOpaqueVoxel();
                            slafSolid[2] = slaf[2].IsSolidOpaqueVoxel();
                            slafSolid[3] = slaf[3].IsSolidOpaqueVoxel();


                            #region block light
                            // BLock light
                            // ===========
                            // Because at solid block light not exist. We can only get light by the adjacent block and use it as the light as solid voxel face.
                            if (smoothLight)
                            {
                                if (!slafSolid[0] && !slafSolid[1] && !slafSolid[2] && !slafSolid[3])
                                {
                                    colors[0] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[1] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace, currBlock);
                                    colors[2] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + m + n, voxelFace, currBlock);
                                    colors[3] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + n, voxelFace, currBlock);


                                }
                                else if (!slafSolid[0] && !slafSolid[1] && slafSolid[2] && slafSolid[3])
                                {
                                    colors[0] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[1] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace, currBlock);
                                    colors[2] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace, currBlock);
                                    colors[3] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                }
                                else if (!slafSolid[0] && slafSolid[1] && slafSolid[2] && !slafSolid[3])
                                {
                                    colors[0] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[1] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[2] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + n, voxelFace, currBlock);
                                    colors[3] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + n, voxelFace, currBlock);
                                }
                                else if (!slafSolid[0] && slafSolid[1] && !slafSolid[2] && !slafSolid[3])
                                {
                                    colors[0] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[1] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[2] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + m + n, voxelFace, currBlock);
                                    colors[3] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + n, voxelFace, currBlock);
                                }
                                else if (!slafSolid[0] && slafSolid[1] && slafSolid[2] && slafSolid[3])
                                {
                                    colors[0] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[1] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[2] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[3] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                }
                                else if (!slafSolid[0] && !slafSolid[1] && !slafSolid[2] && slafSolid[3])
                                {
                                    colors[0] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[1] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace, currBlock);
                                    colors[2] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + m + n, voxelFace, currBlock);
                                    colors[3] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                }
                                else if (!slafSolid[0] && !slafSolid[1] && slafSolid[2] && !slafSolid[3])
                                {
                                    colors[0] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[1] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace, currBlock);
                                    colors[2] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace, currBlock);
                                    colors[3] = GetBlockLightPropagationForAdjacentFace(chunk, startPos + n, voxelFace, currBlock);
                                }
                                else
                                {
                                    colors[0] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[1] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[2] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                    colors[3] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                }
                            }
                            else
                            {
                                colors[0] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                colors[1] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                colors[2] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                colors[3] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                            }
                            #endregion


                            #region sun light
                            // Ambient Lights
                            byte ambientLight = chunk.GetAmbientLight(startPos);
                            ambientColorIntensity[0] = 0;
                            ambientColorIntensity[1] = 0;
                            ambientColorIntensity[2] = 0;
                            ambientColorIntensity[3] = 0;

                            if (!slafSolid[0] && !slafSolid[1] && !slafSolid[2] && !slafSolid[3])
                            {
                                ambientColorIntensity[0] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[1] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace);
                                ambientColorIntensity[2] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + m + n, voxelFace);
                                ambientColorIntensity[3] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + n, voxelFace);
                            }
                            else if (!slafSolid[0] && !slafSolid[1] && slafSolid[2] && slafSolid[3])
                            {
                                ambientColorIntensity[0] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[1] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace);
                                ambientColorIntensity[2] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace);
                                ambientColorIntensity[3] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                            }
                            else if (!slafSolid[0] && slafSolid[1] && slafSolid[2] && !slafSolid[3])
                            {
                                ambientColorIntensity[0] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[1] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[2] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + n, voxelFace);
                                ambientColorIntensity[3] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + n, voxelFace);
                            }
                            else if (!slafSolid[0] && slafSolid[1] && !slafSolid[2] && !slafSolid[3])
                            {
                                ambientColorIntensity[0] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[1] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[2] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + m + n, voxelFace);
                                ambientColorIntensity[3] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + n, voxelFace);
                            }
                            else if (!slafSolid[0] && slafSolid[1] && slafSolid[2] && slafSolid[3])
                            {
                                ambientColorIntensity[0] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[1] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[2] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[3] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                            }
                            else if (!slafSolid[0] && !slafSolid[1] && !slafSolid[2] && slafSolid[3])
                            {
                                ambientColorIntensity[0] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[1] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace);
                                ambientColorIntensity[2] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + m + n, voxelFace);
                                ambientColorIntensity[3] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                            }
                            else if (!slafSolid[0] && !slafSolid[1] && slafSolid[2] && !slafSolid[3])
                            {
                                ambientColorIntensity[0] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[1] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace);
                                ambientColorIntensity[2] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + m, voxelFace);
                                ambientColorIntensity[3] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos + n, voxelFace);
                            }
                            else
                            {
                                ambientColorIntensity[0] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[1] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[2] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[3] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                            }
                            #endregion

                            GetBlockUVs(currBlock, voxelFace, quadSize[u], quadSize[v], ref uvs);
                            GetColorMapUVs(currBlock, voxelFace, chunk.GetHeat(currPos), ref uv2s);
                            GetAOUVs(verticesAO, ref uv3s);
                            GetAmbientLightUVs(ambientColorIntensity, ref uv4s);
                            GetCrackUVs(chunk, startPos, ref uv5s);
                            builder.AddQuadFace(voxelFace, vertices, colors, uvs, uv2s, uv3s, uv4s, uv5s);


                            // Mark at this position has been merged
                            for (int g = 0; g < quadSize[u]; g++)
                            {
                                for (int h = 0; h < quadSize[v]; h++)
                                {
                                    builder.Merged[d][startPos[u] + g, startPos[v] + h] = true;
                                }
                            }
                        }
                    }

                    if(token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);

            ArrayPool<Vector3>.Shared.Return(vertices);
            ArrayPool<Vector3>.Shared.Return(uvs);
            ArrayPool<Vector2>.Shared.Return(uv2s);
            ArrayPool<Vector4>.Shared.Return(uv3s);
            ArrayPool<Vector4>.Shared.Return(uv4s);
            ArrayPool<Vector3>.Shared.Return(uv5s);
            ArrayPool<Color32>.Shared.Return(colors);
            ArrayPool<byte>.Shared.Return(verticesAO);
            ArrayPool<byte>.Shared.Return(ambientColorIntensity);
            ArrayPool<Vector3Int>.Shared.Return(faceNeighbors);
            ArrayPool<BlockID>.Shared.Return(slaf);
            ArrayPool<bool>.Shared.Return(slafSolid);

            return builder;
        }
        public async Task<MeshData> RenderSolidMesh(Chunk chunk, bool isTransparentMesh, CancellationToken token)
        {
            List<Task<ChunkMeshBuilder>> buildMeshTaskList = new List<Task<ChunkMeshBuilder>>();
            ChunkMeshBuilder finalBuilder = ChunkMeshBuilderPool.Get();
            finalBuilder.InitOrLoad(chunk.Dimensions);

            await Task.Run(() =>
            {
                // Iterate over each aface of the blocks.
                for (int voxelFace = 0; voxelFace < 6; voxelFace++)
                {
                    /* Voxel Face Index
                    * 0: Right
                    * 1: Up
                    * 2: Front
                    * 3: Left
                    * 4: Down 
                    * 5: Back
                    * 
                    * BackFace -> Face that drawn in clockwise direction. (Need detect which face is clockwise in order to draw it on 
                    * Unity scene).
                    */

                    buildMeshTaskList.Add(RenderChunkFace(chunk, voxelFace, isTransparentMesh, token));

                    if(token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);

            await Task.WhenAll(buildMeshTaskList);
            for (int i = 0; i < buildMeshTaskList.Count; i++)
            {
                finalBuilder.Add(buildMeshTaskList[i].Result);
            }


            MeshData meshData = finalBuilder.ToMeshData();
            ChunkMeshBuilderPool.Release(finalBuilder);
            for (int i = 0; i < buildMeshTaskList.Count; i++)
            {
                ChunkMeshBuilderPool.Release(buildMeshTaskList[i].Result);
            }


            return meshData;
        }
        #endregion




        #region Render non blocks
        public async Task<MeshData> RenderSolidNonvoxelMesh(Chunk chunk, CancellationToken token)
        {
            ChunkMeshBuilder builder = ChunkMeshBuilderPool.Get();
            builder.InitOrLoad(chunk.Dimensions);

            Vector3[] vertices = new Vector3[4];
            Vector3[] uvs = new Vector3[4];
            Vector2[] uv2s = new Vector2[4];
            Vector4[] uv3s = new Vector4[4];
            Color32[] colors = new Color32[4];
            int[] tris = new int[6];
            byte[] verticesAO = new byte[4];
            byte[] vertexColorIntensity = new byte[4];
            Vector3 _centerOffset = new Vector3(0.5f, 0.5f, 0.5f);

            await Task.Run(() =>
            {
                for (int i = 0; i < chunk.ChunkData.Length; i++)
                {
                    int x = i % chunk.Width;
                    int y = (i / chunk.Width) % chunk.Height;
                    int z = i / (chunk.Width * chunk.Height);

                    BlockID currBlock = chunk.ChunkData[i];
                    if (currBlock == BlockID.Torch)
                    {
                        Vector3Int curBlockPos = new Vector3Int(x, y, z);
                        Vector3 offsetTorchPos = curBlockPos + new Vector3(0.5f, 0.5f, 0.5f);
                        for (int j = 0; j < TorchModel.Vertices.Count; j++)
                        {
                            if (j % 4 == 0)
                            {
                                vertices[0] = offsetTorchPos + TorchModel.Vertices[j];
                                vertices[1] = offsetTorchPos + TorchModel.Vertices[j + 1];
                                vertices[2] = offsetTorchPos + TorchModel.Vertices[j + 2];
                                vertices[3] = offsetTorchPos + TorchModel.Vertices[j + 3];

                                tris[0] = TorchModel.Triangles[j / 4 * 6 + 0];
                                tris[1] = TorchModel.Triangles[j / 4 * 6 + 1];
                                tris[2] = TorchModel.Triangles[j / 4 * 6 + 2];
                                tris[3] = TorchModel.Triangles[j / 4 * 6 + 3];
                                tris[4] = TorchModel.Triangles[j / 4 * 6 + 4];
                                tris[5] = TorchModel.Triangles[j / 4 * 6 + 5];


                                GetNonblockUVs(BlockID.Torch, ref uvs);
                                builder.AddQuadFace(vertices, tris, uvs);
                            }
                        }

                        if(token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
                        }
                    }
                }
            }, token);



            MeshData meshData = builder.ToMeshData();
            ChunkMeshBuilderPool.Release(builder);
            return meshData;
        }
        #endregion





        #region Render grass
        public async Task<MeshData> GetChunkGrassMeshData(Chunk chunk, FastNoiseLite randomNoise, CancellationToken token)
        {
            ChunkMeshBuilder builder = ChunkMeshBuilderPool.Get();
            builder.InitOrLoad(chunk.Dimensions);

            Vector3[] vertices = ArrayPool<Vector3>.Shared.Rent(4);
            Vector3[] uvs = ArrayPool<Vector3>.Shared.Rent(4);
            Vector2[] uv2s = ArrayPool<Vector2>.Shared.Rent(4);
            Vector4[] uv3s = ArrayPool<Vector4>.Shared.Rent(4);
            Color32[] colors = ArrayPool<Color32>.Shared.Rent(4);
            byte[] verticesAO = ArrayPool<byte>.Shared.Rent(4);
            byte[] vertexColorIntensity = ArrayPool<byte>.Shared.Rent(4);

            Vector3 _centerOffset = new Vector3(0.5f, 0.5f, 0.5f);
            bool applyRotation = true;
            float minRotationAngle = 25f;
            float maxRotationAngle = 75f;
            Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
            {
                return rotation * (point - pivot) + pivot;
            }

            await Task.Run(() =>
            {
                for (int i = 0; i < chunk.ChunkData.Length; i++)
                {
                    int x = i % chunk.Width;
                    int y = (i / chunk.Width) % chunk.Height;
                    int z = i / (chunk.Width * chunk.Height);

                    BlockID currBlock = chunk.ChunkData[i];
                    if (currBlock == BlockID.Grass || currBlock == BlockID.Shrub)
                    {
                        Vector3Int curBlockPos = new Vector3Int(x, y, z);
                        // Generate a random float in the range -0.3 to 0.3
                        float randomFloatX = MapValue(randomNoise.GetNoise(x, z), -1f, 1f, -0.3f, 0.3f);
                        float randomFloatZ = MapValue(randomNoise.GetNoise(x + 1, z), -1f, 1f, -0.3f, 0.3f);
                        Vector3 randomOffset = new Vector3(randomFloatX, 0, randomFloatZ);
                        Vector3 offsetPos = curBlockPos + randomOffset;


                        chunk.GetBlockLight(curBlockPos, out byte red, out byte green, out byte blue);
                        float redChannel = red / (float)LightUtils.MAX_LIGHT_INTENSITY;
                        float greenChannel = green / (float)LightUtils.MAX_LIGHT_INTENSITY;
                        float blueChannel = blue / (float)LightUtils.MAX_LIGHT_INTENSITY;
                        Color blockLightColor = new Color(redChannel, greenChannel, blueChannel, 1.0f);
                        colors[0] = blockLightColor;
                        colors[1] = blockLightColor;
                        colors[2] = blockLightColor;
                        colors[3] = blockLightColor;


                        if (applyRotation)
                        {
                            float rotationAngle = ((float)(randomNoise.GetNoise(x, y + 1) + 1.0f) / 2.0f * (maxRotationAngle - minRotationAngle) + minRotationAngle);
                            float rotationAngleRad = rotationAngle * Mathf.Deg2Rad;
                            Quaternion rotation = Quaternion.Euler(0f, rotationAngle, 0f);
                            vertices[0] = RotatePointAroundPivot(offsetPos, _centerOffset + offsetPos, rotation);
                            vertices[1] = RotatePointAroundPivot(offsetPos + new Vector3Int(1, 0, 1), _centerOffset + offsetPos, rotation);
                            vertices[2] = RotatePointAroundPivot(offsetPos + new Vector3Int(1, 1, 1), _centerOffset + offsetPos, rotation);
                            vertices[3] = RotatePointAroundPivot(offsetPos + new Vector3Int(0, 1, 0), _centerOffset + offsetPos, rotation);
                            GetGrassUVs(currBlock, chunk.GetHeat(curBlockPos), ref uvs, ref uv2s);
                            builder.AddQuadFace(vertices, uvs, uv2s, colors);


                            vertices[0] = RotatePointAroundPivot(offsetPos + new Vector3Int(0, 0, 1), _centerOffset + offsetPos, rotation);
                            vertices[1] = RotatePointAroundPivot(offsetPos + new Vector3Int(1, 0, 0), _centerOffset + offsetPos, rotation);
                            vertices[2] = RotatePointAroundPivot(offsetPos + new Vector3Int(1, 1, 0), _centerOffset + offsetPos, rotation);
                            vertices[3] = RotatePointAroundPivot(offsetPos + new Vector3Int(0, 1, 1), _centerOffset + offsetPos, rotation);
                            GetGrassUVs(currBlock, chunk.GetHeat(curBlockPos), ref uvs, ref uv2s);
                            builder.AddQuadFace(vertices, uvs, uv2s, colors);
                        }
                        else
                        {
                            vertices[0] = offsetPos;
                            vertices[1] = offsetPos + new Vector3Int(1, 0, 1);
                            vertices[2] = offsetPos + new Vector3Int(1, 1, 1);
                            vertices[3] = offsetPos + new Vector3Int(0, 1, 0);
                            GetGrassUVs(currBlock, chunk.GetHeat(curBlockPos), ref uvs, ref uv2s);
                            builder.AddQuadFace(vertices, uvs, uv2s, colors);


                            vertices[0] = offsetPos + new Vector3Int(0, 0, 1);
                            vertices[1] = offsetPos + new Vector3Int(1, 0, 0);
                            vertices[2] = offsetPos + new Vector3Int(1, 1, 0);
                            vertices[3] = offsetPos + new Vector3Int(0, 1, 1);
                            GetGrassUVs(currBlock, chunk.GetHeat(curBlockPos), ref uvs, ref uv2s);
                            builder.AddQuadFace(vertices, uvs, uv2s, colors);
                        }

                    }
                    else if (chunk.ChunkData[i] == BlockID.TallGrass)
                    {
                        Vector3Int curBlockPos = new Vector3Int(x, y, z);
                        // Generate a random float in the range -0.3 to 0.3
                        float randomFloatX = MapValue(randomNoise.GetNoise(x, z), -1f, 1f, -0.3f, 0.3f);
                        float randomFloatZ = MapValue(randomNoise.GetNoise(x + 1, z), -1f, 1f, -0.3f, 0.3f);
                        Vector3 randomOffset = new Vector3(randomFloatX, 0, randomFloatZ);
                        Vector3 offsetPos = curBlockPos + randomOffset;

                        chunk.GetBlockLight(curBlockPos, out byte red, out byte green, out byte blue);
                        float redChannel = red / (float)LightUtils.MAX_LIGHT_INTENSITY;
                        float greenChannel = green / (float)LightUtils.MAX_LIGHT_INTENSITY;
                        float blueChannel = blue / (float)LightUtils.MAX_LIGHT_INTENSITY;
                        Color blockLightColor = new Color(redChannel, greenChannel, blueChannel, 1.0f);
                        colors[0] = blockLightColor;
                        colors[1] = blockLightColor;
                        colors[2] = blockLightColor;
                        colors[3] = blockLightColor;

                        if (applyRotation)
                        {
                            float rotationAngle = ((float)(randomNoise.GetNoise(x + 1, y + 1) + 1.0f) / 2.0f * (maxRotationAngle - minRotationAngle) + minRotationAngle);
                            float rotationAngleRad = rotationAngle * Mathf.Deg2Rad;
                            Quaternion rotation = Quaternion.Euler(0f, rotationAngle, 0f);
                            int heightFromOrigin = _main.GetBlockHeightFromOrigin(chunk, curBlockPos);
                            vertices[0] = RotatePointAroundPivot(offsetPos, _centerOffset + offsetPos, rotation);
                            vertices[1] = RotatePointAroundPivot(offsetPos + new Vector3Int(1, 0, 1), _centerOffset + offsetPos, rotation);
                            vertices[2] = RotatePointAroundPivot(offsetPos + new Vector3Int(1, 1, 1), _centerOffset + offsetPos, rotation);
                            vertices[3] = RotatePointAroundPivot(offsetPos + new Vector3Int(0, 1, 0), _centerOffset + offsetPos, rotation);
                            GetGrassUVs(BlockID.TallGrass, chunk.GetHeat(curBlockPos), ref uvs, ref uv2s, heightFromOrigin);
                            builder.AddQuadFace(vertices, uvs, uv2s, colors);


                            vertices[0] = RotatePointAroundPivot(offsetPos + new Vector3Int(0, 0, 1), _centerOffset + offsetPos, rotation);
                            vertices[1] = RotatePointAroundPivot(offsetPos + new Vector3Int(1, 0, 0), _centerOffset + offsetPos, rotation);
                            vertices[2] = RotatePointAroundPivot(offsetPos + new Vector3Int(1, 1, 0), _centerOffset + offsetPos, rotation);
                            vertices[3] = RotatePointAroundPivot(offsetPos + new Vector3Int(0, 1, 1), _centerOffset + offsetPos, rotation);
                            GetGrassUVs(BlockID.TallGrass, chunk.GetHeat(curBlockPos), ref uvs, ref uv2s, heightFromOrigin);
                            builder.AddQuadFace(vertices, uvs, uv2s, colors);
                        }
                        else
                        {
                            int heightFromOrigin = _main.GetBlockHeightFromOrigin(chunk, curBlockPos);
                            vertices[0] = offsetPos;
                            vertices[1] = offsetPos + new Vector3Int(1, 0, 1);
                            vertices[2] = offsetPos + new Vector3Int(1, 1, 1);
                            vertices[3] = offsetPos + new Vector3Int(0, 1, 0);
                            GetGrassUVs(BlockID.TallGrass, chunk.GetHeat(curBlockPos), ref uvs, ref uv2s, heightFromOrigin);
                            builder.AddQuadFace(vertices, uvs, uv2s, colors);


                            vertices[0] = offsetPos + new Vector3Int(0, 0, 1);
                            vertices[1] = offsetPos + new Vector3Int(1, 0, 0);
                            vertices[2] = offsetPos + new Vector3Int(1, 1, 0);
                            vertices[3] = offsetPos + new Vector3Int(0, 1, 1);
                            GetGrassUVs(BlockID.TallGrass, chunk.GetHeat(curBlockPos), ref uvs, ref uv2s, heightFromOrigin);
                            builder.AddQuadFace(vertices, uvs, uv2s, colors);
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);



            MeshData meshData = builder.ToMeshData();
            ChunkMeshBuilderPool.Release(builder);

            ArrayPool<Vector3>.Shared.Return(vertices);
            ArrayPool<Vector3>.Shared.Return(uvs);
            ArrayPool<Vector2>.Shared.Return(uv2s);
            ArrayPool<Vector4>.Shared.Return(uv3s);
            ArrayPool<Color32>.Shared.Return(colors);
            ArrayPool<byte>.Shared.Return(verticesAO);
            ArrayPool<byte>.Shared.Return(vertexColorIntensity);
            return meshData;
        }
        #endregion






        #region Render Liquids 
        public async Task<MeshData> RenderLavaMeshingAsync(Chunk chunk, CancellationToken token)
        {
            ChunkMeshBuilder builder = ChunkMeshBuilderPool.Get();
            builder.InitOrLoad(chunk.Dimensions);

            Vector3 quadSize = Vector3Int.one, m, n, offsetPos;
            Vector3Int startPos;
            Vector3Int dimensions = chunk.Dimensions;
            int d, u, v;
            bool isBackFace;
      
            Vector3[] vertices = ArrayPool<Vector3>.Shared.Rent(4);
            Vector3[] uvs = ArrayPool<Vector3>.Shared.Rent(4);
            Vector4[] uv3s = ArrayPool<Vector4>.Shared.Rent(4); // Water AO
            Vector4[] uv4s = ArrayPool<Vector4>.Shared.Rent(4);
            Color32[] colors = ArrayPool<Color32>.Shared.Rent(4);
            Vector2[] lavaFlowDirMapUVs = ArrayPool<Vector2>.Shared.Rent(4);
            Vector2[] lavaFlowingOffsetUVs = ArrayPool<Vector2>.Shared.Rent(4);
            byte[] verticesAO = ArrayPool<byte>.Shared.Rent(4);   // AO (0 -> 3)
            byte[] blockColorIntensity = ArrayPool<byte>.Shared.Rent(4);
            byte[] ambientColorIntensity = ArrayPool<byte>.Shared.Rent(4);
            Vector3Int[] faceNeighbors = ArrayPool<Vector3Int>.Shared.Rent(6);
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(8);
            Vector3Int[] upperNeighbors = ArrayPool<Vector3Int>.Shared.Rent(8);
            Vector3[] verticesUp = ArrayPool<Vector3>.Shared.Rent(4);

            await Task.Run(() =>
            {
                for (int voxelFace = 0; voxelFace < 6; voxelFace++)
                {
                    isBackFace = voxelFace > 2;
                    d = voxelFace % 3;
                    switch (d)
                    {
                        case 0:
                            u = 2;
                            v = 1;
                            break;
                        case 1:
                            u = 0;
                            v = 2;
                            break;
                        default:
                            u = 0;
                            v = 1;
                            break;
                    }


                    startPos = new Vector3Int();
                    for (startPos[d] = 0; startPos[d] < dimensions[d]; startPos[d]++)
                    {
                        // Build the slices of mesh.
                        for (startPos[u] = 0; startPos[u] < dimensions[u]; startPos[u]++)
                        {
                            for (startPos[v] = 0; startPos[v] < dimensions[v]; startPos[v]++)
                            {
                                BlockID currBlock = chunk.GetBlock(startPos);
                                if (currBlock != BlockID.Lava ||
                                    chunk.IsLavaFaceVisible(startPos, d, isBackFace) == false)
                                {
                                    continue;
                                }


                                // Add new quad to mesh data.
                                m = Vector3.zero;
                                n = Vector3.zero;
                                m[u] = quadSize[u];
                                n[v] = quadSize[v];

                                offsetPos = startPos;
                                offsetPos[d] += isBackFace ? 0 : 1;



                                /*
                                 * 0: center highest level.
                                 * 1: smaller north, larger south, larger east, equal north east
                                 * 2: larger north, smaller south, larger east, equal south east
                                 * 3: smaller north, larger south, smaller east, smaller north east
                                 * 4: smaller north, larger south, smaller east, smaller north east
                                 */

                                #region ambient occlusion
                                // Ambient occlusion
                                // =================
                                verticesAO[0] = (byte)VoxelAO.ProcessAO(chunk, startPos, 0, voxelFace);
                                verticesAO[1] = (byte)VoxelAO.ProcessAO(chunk, startPos, 1, voxelFace);
                                verticesAO[2] = (byte)VoxelAO.ProcessAO(chunk, startPos, 2, voxelFace);
                                verticesAO[3] = (byte)VoxelAO.ProcessAO(chunk, startPos, 3, voxelFace);
                                GetAOUVs(verticesAO, ref uv3s);
                                #endregion


                                #region fluid meshing
                                GetVerticesUp(chunk, startPos, ref neighbors, ref upperNeighbors, ref verticesUp, LiquidType.Lava);
                                if (chunk.GetLiquidLevel(startPos) == Lava.MAX_LAVA_LEVEL)
                                {
                                    vertices[0] = offsetPos;
                                    vertices[1] = offsetPos + m;
                                    vertices[2] = offsetPos + m + n;
                                    vertices[3] = offsetPos + n;

                                    Vector2 staticFlow = new Vector2(0, 0.35f);
                                    lavaFlowingOffsetUVs[0] = staticFlow;
                                    lavaFlowingOffsetUVs[1] = staticFlow;
                                    lavaFlowingOffsetUVs[2] = staticFlow;
                                    lavaFlowingOffsetUVs[3] = staticFlow;

                                    lavaFlowDirMapUVs[0] = new Vector2(0, 0);
                                    lavaFlowDirMapUVs[1] = new Vector2(quadSize[u], 0);
                                    lavaFlowDirMapUVs[2] = new Vector2(quadSize[u], quadSize[v]);
                                    lavaFlowDirMapUVs[3] = new Vector2(0, quadSize[v]);
                                }
                                else
                                {
                                    Vector3Int west = (startPos - m).ToVector3Int();
                                    Vector3Int east = (startPos + m).ToVector3Int();
                                    Vector3Int north = (startPos + n).ToVector3Int();
                                    Vector3Int south = (startPos - n).ToVector3Int();
                                    Vector3Int nw = (startPos - m + n).ToVector3Int();
                                    Vector3Int ne = (startPos + m + n).ToVector3Int();
                                    Vector3Int sw = (startPos - m - n).ToVector3Int();
                                    Vector3Int se = (startPos + m - n).ToVector3Int();


                                    neighbors[0] = west;
                                    neighbors[1] = east;
                                    neighbors[2] = north;
                                    neighbors[3] = south;
                                    neighbors[4] = nw;
                                    neighbors[5] = ne;
                                    neighbors[6] = sw;
                                    neighbors[7] = se;

                                    if (voxelFace == 0)
                                    {
                                        if (chunk.GetBlock(north) == BlockID.Lava)
                                        {
                                            vertices[0] = offsetPos;
                                            vertices[1] = offsetPos + m;
                                            vertices[2] = offsetPos + m + n;
                                            vertices[3] = offsetPos + n;
                                        }
                                        else
                                        {
                                            vertices[0] = offsetPos;
                                            vertices[1] = offsetPos + m;
                                            vertices[3] = verticesUp[1];
                                            vertices[2] = verticesUp[2];
                                        }
                                    }
                                    else if (voxelFace == 1)
                                    {
                                        vertices[0] = verticesUp[0];
                                        vertices[1] = verticesUp[1];
                                        vertices[2] = verticesUp[2];
                                        vertices[3] = verticesUp[3];
                                    }
                                    else if (voxelFace == 2)
                                    {
                                        if (chunk.GetBlock(north) == BlockID.Lava)
                                        {
                                            vertices[0] = offsetPos;
                                            vertices[1] = offsetPos + m;
                                            vertices[2] = offsetPos + m + n;
                                            vertices[3] = offsetPos + n;
                                        }
                                        else
                                        {
                                            vertices[0] = offsetPos;
                                            vertices[1] = offsetPos + m;
                                            vertices[2] = verticesUp[2];
                                            vertices[3] = verticesUp[3];
                                        }
                                    }
                                    else if (voxelFace == 3)
                                    {
                                        if (chunk.GetBlock(north) == BlockID.Lava)
                                        {
                                            vertices[0] = offsetPos;
                                            vertices[1] = offsetPos + m;
                                            vertices[2] = offsetPos + m + n;
                                            vertices[3] = offsetPos + n;
                                        }
                                        else
                                        {
                                            vertices[0] = offsetPos;
                                            vertices[1] = offsetPos + m;
                                            vertices[2] = verticesUp[3];
                                            vertices[3] = verticesUp[0];
                                        }
                                    }
                                    else if (voxelFace == 4)
                                    {
                                        vertices[0] = offsetPos;
                                        vertices[1] = offsetPos + m;
                                        vertices[2] = offsetPos + m + n;
                                        vertices[3] = offsetPos + n;
                                    }                                   
                                    else if (voxelFace == 5)
                                    {
                                        if (chunk.GetBlock(north) == BlockID.Lava)
                                        {
                                            vertices[0] = offsetPos;
                                            vertices[1] = offsetPos + m;
                                            vertices[2] = offsetPos + m + n;
                                            vertices[3] = offsetPos + n;
                                        }
                                        else
                                        {
                                            vertices[0] = offsetPos;
                                            vertices[1] = offsetPos + m;
                                            vertices[2] = verticesUp[1];
                                            vertices[3] = verticesUp[0];
                                        }
                                    }                                   
                                    else
                                    {
                                        vertices[0] = offsetPos;
                                        vertices[1] = offsetPos;
                                        vertices[2] = offsetPos;
                                        vertices[3] = offsetPos;
                                    }

                                    GetLiquidFlowingOffsetUVs(voxelFace, verticesUp, ref lavaFlowingOffsetUVs);
                                    GetLiquidFlowMappingDirectionUVs(voxelFace, verticesUp, ref lavaFlowDirMapUVs);
                                }
                                #endregion


                             
                             



                                #region block light color
                                colors[0] = GetBlockLightPropagationForAdjacentFace(chunk, (startPos + m + n).ToVector3Int(), voxelFace, currBlock);
                                colors[1] = GetBlockLightPropagationForAdjacentFace(chunk, (startPos + m + n).ToVector3Int(), voxelFace, currBlock);
                                colors[2] = GetBlockLightPropagationForAdjacentFace(chunk, (startPos + m + n).ToVector3Int(), voxelFace, currBlock);
                                colors[3] = GetBlockLightPropagationForAdjacentFace(chunk, (startPos + m + n).ToVector3Int(), voxelFace, currBlock);
                                #endregion


                                #region sun light
                                // Ambient Lights
                                byte ambientLight = chunk.GetAmbientLight(startPos);
                                ambientColorIntensity[0] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[1] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[2] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[3] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                GetAmbientLightUVs(ambientColorIntensity, ref uv4s);
                                #endregion


                                uvs[0] = new Vector3(0, 0, 1);
                                uvs[1] = new Vector3(1, 0, 1);
                                uvs[2] = new Vector3(1, 1, 1);
                                uvs[3] = new Vector3(0, 1, 1);
                                builder.AddWaterQuadFace(voxelFace, vertices, colors, uvs, uv3s, uv4s, lavaFlowDirMapUVs, lavaFlowingOffsetUVs);
                            }
                        }
                    }

                    if(token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);


            MeshData meshData = builder.ToWaterMeshData();
            ChunkMeshBuilderPool.Release(builder);

            // Release Array Pool
            ArrayPool<Vector3>.Shared.Return(vertices);
            ArrayPool<Vector3>.Shared.Return(uvs);
            ArrayPool<Vector4>.Shared.Return(uv3s);
            ArrayPool<Vector4>.Shared.Return(uv4s);
            ArrayPool<Color32>.Shared.Return(colors);
            ArrayPool<Vector2>.Shared.Return(lavaFlowDirMapUVs);
            ArrayPool<Vector2>.Shared.Return(lavaFlowingOffsetUVs);
            ArrayPool<byte>.Shared.Return(verticesAO);
            ArrayPool<byte>.Shared.Return(blockColorIntensity);
            ArrayPool<byte>.Shared.Return(ambientColorIntensity);
            ArrayPool<Vector3Int>.Shared.Return(faceNeighbors);
            ArrayPool<Vector3Int>.Shared.Return(neighbors);
            ArrayPool<Vector3Int>.Shared.Return(upperNeighbors);
            ArrayPool<Vector3>.Shared.Return(verticesUp);

            return meshData;
        }

        public async Task<MeshData> WaterGreedyMeshingAsync(Chunk chunk, CancellationToken token)
        {
            bool GreedyCompareLogic(Vector3Int a, Vector3Int b, int dimension, bool isBackFace)
            {
                BlockID blockA = chunk.GetBlock(a);
                BlockID blockB = chunk.GetBlock(b);

                return blockA == blockB && chunk.IsWater(b) &&
                       chunk.GetLiquidLevel(a) == chunk.GetLiquidLevel(b) &&
                       chunk.GetLiquidLevel(a) == Water.MAX_WATER_LEVEL &&
                       chunk.GetLightData(a) == chunk.GetLightData(b) &&
                       chunk.IsWaterFaceVisible(b, dimension, isBackFace);
            }

            ChunkMeshBuilder builder = ChunkMeshBuilderPool.Get();
            builder.InitOrLoad(chunk.Dimensions);

            int d, u, v;
            bool isBackFace;
            Vector3Int dimensions = chunk.Dimensions;
            Vector3 quadSize = Vector3Int.one, m, n, offsetPos;
            Vector3Int startPos, currPos;
            Vector3[] vertices = ArrayPool<Vector3>.Shared.Rent(4);
            Vector3[] uvs = ArrayPool<Vector3>.Shared.Rent(4);
            Vector4[] uv3s = ArrayPool<Vector4>.Shared.Rent(4); // Water AO
            Vector4[] uv4s = ArrayPool<Vector4>.Shared.Rent(4); ;
            Color32[] colors = ArrayPool<Color32>.Shared.Rent(4); ;
            Vector2[] waterFlowDirMapUVs = ArrayPool<Vector2>.Shared.Rent(4);
            Vector2[] waterFlowingOffsetUVs = ArrayPool<Vector2>.Shared.Rent(4);
            byte[] verticesAO = ArrayPool<byte>.Shared.Rent(4);  // AO (0 -> 3)
            byte[] blockColorIntensity = ArrayPool<byte>.Shared.Rent(4); ;
            byte[] ambientColorIntensity = ArrayPool<byte>.Shared.Rent(4); ;
            Vector3Int[] faceNeighbors = ArrayPool<Vector3Int>.Shared.Rent(6); ;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(8);
            Vector3Int[] upperNeighbors = ArrayPool<Vector3Int>.Shared.Rent(4);
            Vector3[] verticesUp = ArrayPool<Vector3>.Shared.Rent(4);
            await Task.Run(() =>
            {
                for (int voxelFace = 0; voxelFace < 6; voxelFace++)
                {
                    if (voxelFace == 4) continue;
                    //if (voxelFace != 1 && voxelFace != 3 && voxelFace != 0 && voxelFace != 2 && voxelFace != 5) continue;

                    isBackFace = voxelFace > 2;
                    d = voxelFace % 3;
                    switch (d)
                    {
                        case 0:
                            u = 2;
                            v = 1;
                            break;
                        case 1:
                            u = 0;
                            v = 2;
                            break;
                        default:
                            u = 0;
                            v = 1;
                            break;
                    }


                    startPos = new Vector3Int();
                    currPos = new Vector3Int();


                    for (startPos[d] = 0; startPos[d] < dimensions[d]; startPos[d]++)
                    {
                        Array.Clear(builder.Merged[d], 0, builder.Merged[d].Length);

                        // Build the slices of mesh.
                        for (startPos[u] = 0; startPos[u] < dimensions[u]; startPos[u]++)
                        {
                            for (startPos[v] = 0; startPos[v] < dimensions[v]; startPos[v]++)
                            {
                                if (startPos.y == 0 && voxelFace != 1) continue;
                                if (builder.Merged[d][startPos[u], startPos[v]])
                                {
                                    continue;
                                }

                                BlockID currBlock = chunk.GetBlock(startPos);

                                // If this block has already been merged, is air, or not visible -> skip it.
                                if (currBlock != BlockID.Water ||
                                    chunk.IsWaterFaceVisible(startPos, d, isBackFace) == false ||
                                    builder.Merged[d][startPos[u], startPos[v]])
                                {
                                    continue;
                                }



                                #region ambient occlusion
                                // Ambient occlusion
                                // =================
                                verticesAO[0] = (byte)VoxelAO.ProcessAO(chunk, startPos, 0, voxelFace);
                                verticesAO[1] = (byte)VoxelAO.ProcessAO(chunk, startPos, 1, voxelFace);
                                verticesAO[2] = (byte)VoxelAO.ProcessAO(chunk, startPos, 2, voxelFace);
                                verticesAO[3] = (byte)VoxelAO.ProcessAO(chunk, startPos, 3, voxelFace);
                                #endregion


                                #region greedy meshing
                                bool greedyMeshing = true;
                                if (greedyMeshing)
                                {
                                    quadSize = new Vector3();
                                    // Next step is loop to figure out width and height of the new merged quad.
                                    for (currPos = startPos, currPos[u]++;
                                        currPos[u] < dimensions[u] &&
                                         GreedyCompareLogic(startPos, currPos, d, isBackFace) &&
                                        !builder.Merged[d][currPos[u], currPos[v]];
                                        currPos[u]++)
                                    { }
                                    quadSize[u] = currPos[u] - startPos[u];

                                    for (currPos = startPos, currPos[v]++;
                                        currPos[v] < dimensions[v] &&
                                        GreedyCompareLogic(startPos, currPos, d, isBackFace) &&
                                        !builder.Merged[d][currPos[u], currPos[v]];
                                        currPos[v]++)
                                    {


                                        for (currPos[u] = startPos[u];
                                            currPos[u] < dimensions[u] &&
                                             GreedyCompareLogic(startPos, currPos, d, isBackFace) &&
                                            !builder.Merged[d][currPos[u], currPos[v]];
                                            currPos[u]++)
                                        { }


                                        if (currPos[u] - startPos[u] < quadSize[u])
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            currPos[u] = startPos[u];
                                        }
                                    }

                                    quadSize[v] = currPos[v] - startPos[v];
                                }
                                else
                                {
                                    quadSize = Vector3.one;
                                }
                                #endregion

                                // Add new quad to mesh data.
                                m = new Vector3();
                                n = new Vector3();

                                m[u] = quadSize[u];
                                n[v] = quadSize[v];

                                offsetPos = startPos;
                                offsetPos[d] += isBackFace ? 0 : 1;

                                /*
                                 * 0: center highest level.
                                 * 1: smaller north, larger south, larger east, equal north east
                                 * 2: larger north, smaller south, larger east, equal south east
                                 * 3: smaller north, larger south, smaller east, smaller north east
                                 * 4: smaller north, larger south, smaller east, smaller north east
                                 */


                                GetVerticesUp(chunk, startPos, ref neighbors, ref upperNeighbors, ref verticesUp, LiquidType.Water);
                                if (chunk.GetLiquidLevel(startPos) == Water.MAX_WATER_LEVEL)
                                {
                                    vertices[0] = offsetPos;
                                    vertices[1] = offsetPos + m;
                                    vertices[2] = offsetPos + m + n;
                                    vertices[3] = offsetPos + n;

                                    uv3s[0] = new Vector4(255, 255, 255, 255);
                                    uv3s[1] = new Vector4(255, 255, 255, 255);
                                    uv3s[2] = new Vector4(255, 255, 255, 255);
                                    uv3s[3] = new Vector4(255, 255, 255, 255);

                                    Vector2 flow = new Vector2(0.0f, 0.2f);
                                    waterFlowingOffsetUVs[0] = flow;
                                    waterFlowingOffsetUVs[1] = flow;
                                    waterFlowingOffsetUVs[2] = flow;
                                    waterFlowingOffsetUVs[3] = flow;

                                    waterFlowDirMapUVs[0] = new Vector2(0, 0);
                                    waterFlowDirMapUVs[1] = new Vector2(quadSize[u], 0);
                                    waterFlowDirMapUVs[2] = new Vector2(quadSize[u], quadSize[v]);
                                    waterFlowDirMapUVs[3] = new Vector2(0, quadSize[v]);                             
                                }
                                else
                                {
                                    if (chunk.GetLiquidLevel(offsetPos.ToVector3Int()) == Water.MAX_WATER_LEVEL)
                                    {
                                        vertices[0] = offsetPos;
                                        vertices[1] = offsetPos + m;
                                        vertices[2] = offsetPos + m + n;
                                        vertices[3] = offsetPos + n;                                 
                                    }
                                    else
                                    {
                                        Vector3Int west = (startPos - m).ToVector3Int();
                                        Vector3Int east = (startPos + m).ToVector3Int();
                                        Vector3Int north = (startPos + n).ToVector3Int();
                                        Vector3Int south = (startPos - n).ToVector3Int();
                                        Vector3Int nw = (startPos - m + n).ToVector3Int();
                                        Vector3Int ne = (startPos + m + n).ToVector3Int();
                                        Vector3Int sw = (startPos - m - n).ToVector3Int();
                                        Vector3Int se = (startPos + m - n).ToVector3Int();

                                        neighbors[0] = west;
                                        neighbors[1] = east;
                                        neighbors[2] = north;
                                        neighbors[3] = south;
                                        neighbors[4] = nw;
                                        neighbors[5] = ne;
                                        neighbors[6] = sw;
                                        neighbors[7] = se;

                                        if (voxelFace == 0)
                                        {
                                            if (chunk.GetBlock(north) == BlockID.Water)
                                            {
                                                vertices[0] = offsetPos;
                                                vertices[1] = offsetPos + m;
                                                vertices[2] = offsetPos + m + n;
                                                vertices[3] = offsetPos + n;
                                            }
                                            else
                                            {
                                                vertices[0] = offsetPos;
                                                vertices[1] = offsetPos + m;
                                                vertices[3] = verticesUp[1];
                                                vertices[2] = verticesUp[2];
                                            }

                                        }
                                        else if (voxelFace == 1)
                                        {
                                            vertices[0] = verticesUp[0];
                                            vertices[1] = verticesUp[1];
                                            vertices[2] = verticesUp[2];
                                            vertices[3] = verticesUp[3];
                                        }
                                        else if (voxelFace == 2)
                                        {
                                            if (chunk.GetBlock(north) == BlockID.Water)
                                            {
                                                vertices[0] = offsetPos;
                                                vertices[1] = offsetPos + m;
                                                vertices[2] = offsetPos + m + n;
                                                vertices[3] = offsetPos + n;
                                            }
                                            else
                                            {
                                                vertices[0] = offsetPos;
                                                vertices[1] = offsetPos + m;
                                                vertices[2] = verticesUp[2];
                                                vertices[3] = verticesUp[3];
                                            }
                                        }
                                        else if (voxelFace == 3)
                                        {
                                            if (chunk.GetBlock(north) == BlockID.Water)
                                            {
                                                vertices[0] = offsetPos;
                                                vertices[1] = offsetPos + m;
                                                vertices[2] = offsetPos + m + n;
                                                vertices[3] = offsetPos + n;
                                            }
                                            else
                                            {
                                                vertices[0] = offsetPos;
                                                vertices[1] = offsetPos + m;
                                                vertices[2] = verticesUp[3];
                                                vertices[3] = verticesUp[0];
                                            }
                                        }
                                        else if (voxelFace == 4)
                                        {
                                            vertices[0] = offsetPos;
                                            vertices[1] = offsetPos + m;
                                            vertices[2] = offsetPos + m + n;
                                            vertices[3] = offsetPos + n;
                                        }
                                        else if (voxelFace == 5)
                                        {
                                            if (chunk.GetBlock(north) == BlockID.Water)
                                            {
                                                vertices[0] = offsetPos;
                                                vertices[1] = offsetPos + m;
                                                vertices[2] = offsetPos + m + n;
                                                vertices[3] = offsetPos + n;
                                            }
                                            else
                                            {
                                                vertices[0] = offsetPos;
                                                vertices[1] = offsetPos + m;
                                                vertices[2] = verticesUp[1];
                                                vertices[3] = verticesUp[0];
                                            }
                                        }
                                        else
                                        {
                                            vertices[0] = offsetPos;
                                            vertices[1] = offsetPos;
                                            vertices[2] = offsetPos;
                                            vertices[3] = offsetPos;
                                        }
                                    }


                                    GetLiquidFlowingOffsetUVs(voxelFace, verticesUp, ref waterFlowingOffsetUVs);
                                    GetLiquidFlowMappingDirectionUVs(voxelFace, verticesUp, ref waterFlowDirMapUVs);
                                    GetAOUVs(verticesAO, ref uv3s);
                                }



                                if (chunk.GetLiquidLevel(startPos) == Water.MAX_WATER_LEVEL)
                                {
                                    uvs[0] = new Vector3(0, 0, Water.MAX_WATER_LEVEL);
                                    uvs[1] = new Vector3(quadSize[u], 0, Water.MAX_WATER_LEVEL);
                                    uvs[2] = new Vector3(quadSize[u], quadSize[v], Water.MAX_WATER_LEVEL);
                                    uvs[3] = new Vector3(0, quadSize[v], Water.MAX_WATER_LEVEL);
                                }
                                else
                                {
                                    byte waterLevel = chunk.GetLiquidLevel(startPos);
                                    uvs[0] = new Vector3(0, 0, waterLevel);
                                    uvs[1] = new Vector3(quadSize[u], 0, waterLevel);
                                    uvs[2] = new Vector3(quadSize[u], quadSize[v], waterLevel);
                                    uvs[3] = new Vector3(0, quadSize[v], waterLevel);
                                }


                                // block lights
                                colors[0] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                colors[1] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                colors[2] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);
                                colors[3] = GetBlockLightPropagationForAdjacentFace(chunk, startPos, voxelFace, currBlock);

                                // Ambient Lights
                                byte ambientLight = chunk.GetAmbientLight(startPos);
                                ambientColorIntensity[0] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[1] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[2] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                ambientColorIntensity[3] = GetAmbientLightPropagationForAdjacentFace(chunk, startPos, voxelFace);
                                GetAmbientLightUVs(ambientColorIntensity, ref uv4s);


                                builder.AddWaterQuadFace(voxelFace, vertices, colors, uvs, uv3s, uv4s, waterFlowDirMapUVs, waterFlowingOffsetUVs);



                                // Mark at this position has been merged
                                for (int g = 0; g < quadSize[u]; g++)
                                {
                                    for (int h = 0; h < quadSize[v]; h++)
                                    {
                                        builder.Merged[d][startPos[u] + g, startPos[v] + h] = true;
                                    }
                                }
                            }
                        }
                    }

                    if(token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);


            MeshData meshData = builder.ToWaterMeshData();
            ChunkMeshBuilderPool.Release(builder);

            // release array pool
            ArrayPool<Vector3>.Shared.Return(vertices);
            ArrayPool<Vector3>.Shared.Return(uvs);
            ArrayPool<Vector4>.Shared.Return(uv3s);
            ArrayPool<Vector4>.Shared.Return(uv4s);
            ArrayPool<Color32>.Shared.Return(colors);
            ArrayPool<Vector2>.Shared.Return(waterFlowDirMapUVs);
            ArrayPool<Vector2>.Shared.Return(waterFlowingOffsetUVs);
            ArrayPool<byte>.Shared.Return(verticesAO);
            ArrayPool<byte>.Shared.Return(blockColorIntensity);
            ArrayPool<byte>.Shared.Return(ambientColorIntensity);
            ArrayPool<Vector3Int>.Shared.Return(faceNeighbors);
            ArrayPool<Vector3Int>.Shared.Return(neighbors);
            ArrayPool<Vector3Int>.Shared.Return(upperNeighbors);
            ArrayPool<Vector3>.Shared.Return(verticesUp);

            return meshData;
        }


        private void GetVerticesUp(Chunk chunk, Vector3Int startPos, ref Vector3Int[] liquidNeighbors, ref Vector3Int[] upperNeighbors, ref Vector3[] results, LiquidType liquidType)
        {
            Vector3Int west = new Vector3Int(startPos.x - 1, startPos.y, startPos.z);
            Vector3Int east = new Vector3Int(startPos.x + 1, startPos.y, startPos.z);
            Vector3Int north = new Vector3Int(startPos.x, startPos.y, startPos.z + 1);
            Vector3Int south = new Vector3Int(startPos.x, startPos.y, startPos.z - 1);
            Vector3Int nw = new Vector3Int(startPos.x - 1, startPos.y, startPos.z + 1);
            Vector3Int ne = new Vector3Int(startPos.x + 1, startPos.y, startPos.z + 1);
            Vector3Int sw = new Vector3Int(startPos.x - 1, startPos.y, startPos.z - 1);
            Vector3Int se = new Vector3Int(startPos.x + 1, startPos.y, startPos.z - 1);


            Vector3Int up = new Vector3Int(startPos.x, startPos.y + 1, startPos.z);
            Vector3Int uw = new Vector3Int(up.x - 1, up.y, up.z);
            Vector3Int ue = new Vector3Int(up.x + 1, up.y, up.z);
            Vector3Int un = new Vector3Int(up.x, up.y, up.z + 1);
            Vector3Int us = new Vector3Int(up.x, up.y, up.z - 1);
            Vector3Int unw = new Vector3Int(up.x - 1, up.y, up.z + 1);
            Vector3Int une = new Vector3Int(up.x + 1, up.y, up.z + 1);
            Vector3Int usw = new Vector3Int(up.x - 1, up.y, up.z - 1);
            Vector3Int use = new Vector3Int(up.x + 1, up.y, up.z - 1);


            liquidNeighbors[0] = west;
            liquidNeighbors[1] = east;
            liquidNeighbors[2] = north;
            liquidNeighbors[3] = south;
            liquidNeighbors[4] = nw;
            liquidNeighbors[5] = ne;
            liquidNeighbors[6] = sw;
            liquidNeighbors[7] = se;


            //upperNeighbors[0] = up;
            upperNeighbors[0] = uw;
            upperNeighbors[1] = ue;
            upperNeighbors[2] = un;
            upperNeighbors[3] = us;
            upperNeighbors[4] = unw;
            upperNeighbors[5] = une;
            upperNeighbors[6] = usw;
            upperNeighbors[7] = use;


            //Vector3 up = startPos + Vector3.up;
            //Vector3[] verticesUp = new Vector3[4];
            // default upper mesh.
            results[0] = new Vector3(up.x, chunk.GetLiquidBlockHeight(startPos, 1), up.z);
            results[1] = new Vector3((up + new Vector3(1, 0, 0)).x, chunk.GetLiquidBlockHeight(startPos, 1), (up + new Vector3(1, 0, 0)).z);
            results[2] = new Vector3((up + new Vector3(1, 0, 1)).x, chunk.GetLiquidBlockHeight(startPos, 1), (up + new Vector3(1, 0, 1)).z);
            results[3] = new Vector3((up + new Vector3(0, 0, 1)).x, chunk.GetLiquidBlockHeight(startPos, 1), (up + new Vector3(0, 0, 1)).z);



            var largerLiquidLevelNeighborsMask = chunk.FindLargerLiquidLevelNeighbors(startPos, ref liquidNeighbors);
            int upperLiquidNeighborsMask;

            switch (liquidType)
            {
                default:
                case LiquidType.Water:
                    upperLiquidNeighborsMask = chunk.FindUpperWaterNeighbors(ref upperNeighbors);

                    break;
                case LiquidType.Lava:
                    upperLiquidNeighborsMask = chunk.FindUpperLavaNeighbors(ref upperNeighbors);
                    break;
            }

            for (int i = 0; i < sizeof(int) * 8; i++)
            {
                // Check if the i-th bit is set in the mask
                if ((largerLiquidLevelNeighborsMask & (1 << i)) != 0)
                {
                    switch (i)
                    {
                        case 0: // west
                            results[0] = new Vector3(up.x, chunk.GetLiquidBlockHeight(west, 1), up.z);
                            results[3] = new Vector3((up + new Vector3(0, 0, 1)).x, chunk.GetLiquidBlockHeight(west, 1), (up + new Vector3(0, 0, 1)).z);
                            break;
                        case 1: //east
                            results[1] = new Vector3((up + new Vector3(1, 0, 0)).x, chunk.GetLiquidBlockHeight(east, 1), (up + new Vector3(1, 0, 0)).z);
                            results[2] = new Vector3((up + new Vector3(1, 0, 1)).x, chunk.GetLiquidBlockHeight(east, 1), (up + new Vector3(1, 0, 1)).z);
                            break;
                        case 2: //north
                            results[2] = new Vector3((up + new Vector3(1, 0, 1)).x, chunk.GetLiquidBlockHeight(north, 1), (up + new Vector3(1, 0, 1)).z);
                            results[3] = new Vector3((up + new Vector3(0, 0, 1)).x, chunk.GetLiquidBlockHeight(north, 1), (up + new Vector3(0, 0, 1)).z);
                            break;
                        case 3: //south
                            results[0] = new Vector3(up.x, chunk.GetLiquidBlockHeight(south, 1), up.z);
                            results[1] = new Vector3((up + new Vector3(1, 0, 0)).x, chunk.GetLiquidBlockHeight(south, 1), (up + new Vector3(1, 0, 0)).z);
                            break;
                        case 4: //northwest
                            results[3] = new Vector3((up + new Vector3(0, 0, 1)).x, chunk.GetLiquidBlockHeight(nw, 1), (up + new Vector3(0, 0, 1)).z);
                            break;
                        case 5: //northeast      
                            results[2] = new Vector3((up + new Vector3(1, 0, 1)).x, chunk.GetLiquidBlockHeight(ne, 1), (up + new Vector3(1, 0, 1)).z);
                            break;
                        case 6: //southwest
                            results[0] = new Vector3(up.x, chunk.GetLiquidBlockHeight(sw, 1), up.z);
                            break;
                        case 7: //southeast
                            results[1] = new Vector3((up + new Vector3(1, 0, 0)).x, chunk.GetLiquidBlockHeight(se, 1), (up + new Vector3(1, 0, 0)).z);
                            break;
                        default:
                            Debug.Log("why default");
                            break;
                    }
                }
            }

            for (int i = 0; i < sizeof(int) * 8; i++)
            {
                if ((upperLiquidNeighborsMask & (1 << i)) != 0)
                {
                    switch (i)
                    {
                        case 0: // uw
                            results[0] = up;
                            results[3] = up + new Vector3Int(0, 0, 1);
                            break;
                        case 1: // ue
                            results[1] = ue;
                            results[2] = ue + new Vector3Int(0, 0, 1);
                            break;
                        case 2: // un
                            results[2] = un + Vector3.right;
                            results[3] = un;
                            break;
                        case 3: // us
                            results[0] = us + Vector3.forward;
                            results[1] = us + new Vector3Int(1, 0, 1);
                            break;
                        case 4: // unw
                            results[3] = unw + Vector3.right;
                            break;
                        case 5: // une
                            results[2] = une;
                            break;
                        case 6: // usw
                            results[0] = usw + new Vector3Int(1, 0, 1);
                            break;
                        case 7: // use
                            results[1] = use + new Vector3Int(0, 0, 1);
                            break;
                        default:
                            Debug.LogWarning("Why default");
                            break;
                    }
                }

            }
        }

        public static void GetLiquidFlowMappingDirectionUVs(int voxelFace, Vector3[] upvertices, ref Vector2[] uv2s)
        {
            if (voxelFace == 1)
            {
                int minMask = GetLowestLiquidVertices(upvertices);
                if (minMask == 1)   // 0 lowest
                {
                    float angle = 45.0f * Mathf.Deg2Rad; // Convert angle to radians
                    float cosAngle = Mathf.Cos(angle);
                    float sinAngle = Mathf.Sin(angle);

                    // Apply rotation matrix
                    uv2s[0] = new Vector2(cosAngle * 0 - sinAngle * 0, sinAngle * 0 + cosAngle * 0);
                    uv2s[1] = new Vector2(cosAngle * 1 - sinAngle * 0, sinAngle * 1 + cosAngle * 0);
                    uv2s[2] = new Vector2(cosAngle * 1 - sinAngle * 1, sinAngle * 1 + cosAngle * 1);
                    uv2s[3] = new Vector2(cosAngle * 0 - sinAngle * 1, sinAngle * 0 + cosAngle * 1);
                }
                else if (minMask == 2) // 1 lowest
                {
                    float angle = -45.0f * Mathf.Deg2Rad; // Convert angle to radians
                    float cosAngle = Mathf.Cos(angle);
                    float sinAngle = Mathf.Sin(angle);

                    // Apply rotation matrix
                    uv2s[0] = new Vector2(cosAngle * 0 - sinAngle * 0, sinAngle * 0 + cosAngle * 0);
                    uv2s[1] = new Vector2(cosAngle * 1 - sinAngle * 0, sinAngle * 1 + cosAngle * 0);
                    uv2s[2] = new Vector2(cosAngle * 1 - sinAngle * 1, sinAngle * 1 + cosAngle * 1);
                    uv2s[3] = new Vector2(cosAngle * 0 - sinAngle * 1, sinAngle * 0 + cosAngle * 1);
                }
                else if (minMask == 4)  // 2 lowest
                {
                    float angle = 45.0f * Mathf.Deg2Rad; // Convert angle to radians
                    float cosAngle = Mathf.Cos(angle);
                    float sinAngle = Mathf.Sin(angle);

                    // Apply rotation matrix
                    uv2s[0] = new Vector2(cosAngle * 0 - sinAngle * 0, sinAngle * 0 + cosAngle * 0);
                    uv2s[1] = new Vector2(cosAngle * 1 - sinAngle * 0, sinAngle * 1 + cosAngle * 0);
                    uv2s[2] = new Vector2(cosAngle * 1 - sinAngle * 1, sinAngle * 1 + cosAngle * 1);
                    uv2s[3] = new Vector2(cosAngle * 0 - sinAngle * 1, sinAngle * 0 + cosAngle * 1);
                }
                else if (minMask == 8) // 3 lowest
                {
                    float angle = -45.0f * Mathf.Deg2Rad; // Convert angle to radians
                    float cosAngle = Mathf.Cos(angle);
                    float sinAngle = Mathf.Sin(angle);

                    // Apply rotation matrix
                    uv2s[0] = new Vector2(cosAngle * 0 - sinAngle * 0, sinAngle * 0 + cosAngle * 0);
                    uv2s[1] = new Vector2(cosAngle * 1 - sinAngle * 0, sinAngle * 1 + cosAngle * 0);
                    uv2s[2] = new Vector2(cosAngle * 1 - sinAngle * 1, sinAngle * 1 + cosAngle * 1);
                    uv2s[3] = new Vector2(cosAngle * 0 - sinAngle * 1, sinAngle * 0 + cosAngle * 1);
                }
                else if (minMask == 3) // 0 == 1 lowest
                {
                    uv2s[0] = new Vector2(0, 0);
                    uv2s[1] = new Vector2(1, 0);
                    uv2s[2] = new Vector2(1, 1);
                    uv2s[3] = new Vector2(0, 1);
                }
                else if (minMask == 6) // 1 == 2 lowest
                {
                    uv2s[0] = new Vector2(0, 1);
                    uv2s[1] = new Vector2(0, 0);
                    uv2s[2] = new Vector2(1, 0);
                    uv2s[3] = new Vector2(1, 1);
                }
                else if (minMask == 9) // 0 == 3 lowest
                {
                    uv2s[0] = new Vector2(0, 1);
                    uv2s[1] = new Vector2(0, 0);
                    uv2s[2] = new Vector2(1, 0);
                    uv2s[3] = new Vector2(1, 1);
                }
                else if (minMask == 12) // 2 == 3 lowest
                {
                    uv2s[0] = new Vector2(0, 0);
                    uv2s[1] = new Vector2(1, 0);
                    uv2s[2] = new Vector2(1, 1);
                    uv2s[3] = new Vector2(0, 1);
                }
                else if (minMask == 7) // 0 == 1 == 2 lowest => 3 highest flow diagonal vertex => 1 lowest
                {
                    float angle = -45.0f * Mathf.Deg2Rad; // Convert angle to radians
                    float cosAngle = Mathf.Cos(angle);
                    float sinAngle = Mathf.Sin(angle);

                    // Apply rotation matrix
                    uv2s[0] = new Vector2(cosAngle * 0 - sinAngle * 0, sinAngle * 0 + cosAngle * 0);
                    uv2s[1] = new Vector2(cosAngle * 1 - sinAngle * 0, sinAngle * 1 + cosAngle * 0);
                    uv2s[2] = new Vector2(cosAngle * 1 - sinAngle * 1, sinAngle * 1 + cosAngle * 1);
                    uv2s[3] = new Vector2(cosAngle * 0 - sinAngle * 1, sinAngle * 0 + cosAngle * 1);
                }
                else if (minMask == 14) // 1 == 2 == 3 lowest => 0 highest flow diagonal vertex => 2 lowest
                {
                    float angle = 45.0f * Mathf.Deg2Rad; // Convert angle to radians
                    float cosAngle = Mathf.Cos(angle);
                    float sinAngle = Mathf.Sin(angle);

                    // Apply rotation matrix
                    uv2s[0] = new Vector2(cosAngle * 0 - sinAngle * 0, sinAngle * 0 + cosAngle * 0);
                    uv2s[1] = new Vector2(cosAngle * 1 - sinAngle * 0, sinAngle * 1 + cosAngle * 0);
                    uv2s[2] = new Vector2(cosAngle * 1 - sinAngle * 1, sinAngle * 1 + cosAngle * 1);
                    uv2s[3] = new Vector2(cosAngle * 0 - sinAngle * 1, sinAngle * 0 + cosAngle * 1);
                }
                else if (minMask == 13) // 2 == 3 == 0 lowest => 1 highest flow diagonal vertex => 3 lowest
                {
                    float angle = -45.0f * Mathf.Deg2Rad; // Convert angle to radians
                    float cosAngle = Mathf.Cos(angle);
                    float sinAngle = Mathf.Sin(angle);

                    // Apply rotation matrix
                    uv2s[0] = new Vector2(cosAngle * 0 - sinAngle * 0, sinAngle * 0 + cosAngle * 0);
                    uv2s[1] = new Vector2(cosAngle * 1 - sinAngle * 0, sinAngle * 1 + cosAngle * 0);
                    uv2s[2] = new Vector2(cosAngle * 1 - sinAngle * 1, sinAngle * 1 + cosAngle * 1);
                    uv2s[3] = new Vector2(cosAngle * 0 - sinAngle * 1, sinAngle * 0 + cosAngle * 1);
                }
                else if (minMask == 11) // 3 == 0 == 1 lowest => 2 highest flow diagonal vertex => 0 lowest
                {
                    float angle = 45.0f * Mathf.Deg2Rad; // Convert angle to radians
                    float cosAngle = Mathf.Cos(angle);
                    float sinAngle = Mathf.Sin(angle);

                    // Apply rotation matrix
                    uv2s[0] = new Vector2(cosAngle * 0 - sinAngle * 0, sinAngle * 0 + cosAngle * 0);
                    uv2s[1] = new Vector2(cosAngle * 1 - sinAngle * 0, sinAngle * 1 + cosAngle * 0);
                    uv2s[2] = new Vector2(cosAngle * 1 - sinAngle * 1, sinAngle * 1 + cosAngle * 1);
                    uv2s[3] = new Vector2(cosAngle * 0 - sinAngle * 1, sinAngle * 0 + cosAngle * 1);
                }
                else if (minMask == 15)
                {
                    uv2s[0] = new Vector2(0, 1);
                    uv2s[1] = new Vector2(0, 0);
                    uv2s[2] = new Vector2(1, 0);
                    uv2s[3] = new Vector2(1, 1);
                }
            }
            else
            {
                float angle = 180 * Mathf.Deg2Rad; // Convert angle to radians
                float cosAngle = Mathf.Cos(angle);
                float sinAngle = Mathf.Sin(angle);

                // Apply rotation matrix
                uv2s[0] = new Vector2(cosAngle * 0 - sinAngle * 0, sinAngle * 0 + cosAngle * 0);
                uv2s[1] = new Vector2(cosAngle * 1 - sinAngle * 0, sinAngle * 1 + cosAngle * 0);
                uv2s[2] = new Vector2(cosAngle * 1 - sinAngle * 1, sinAngle * 1 + cosAngle * 1);
                uv2s[3] = new Vector2(cosAngle * 0 - sinAngle * 1, sinAngle * 0 + cosAngle * 1);
            }

        }
        public static void GetLiquidFlowingOffsetUVs(int voxelFace, Vector3[] upvertices, ref Vector2[] uvs)
        {
            if (voxelFace == 1)
            {
                int minMask = GetLowestLiquidVertices(upvertices);
                if (minMask == 1)   // 0 lowest
                {

                    uvs[0] = new Vector2(0, 1);
                    uvs[1] = new Vector2(0, 1);
                    uvs[2] = new Vector2(0, 1);
                    uvs[3] = new Vector2(0, 1);
                }
                else if (minMask == 2) // 1 lowest
                {
                    uvs[0] = new Vector2(0, 1);
                    uvs[1] = new Vector2(0, 1);
                    uvs[2] = new Vector2(0, 1);
                    uvs[3] = new Vector2(0, 1);
                }
                else if (minMask == 4)  // 2 lowest
                {
                    uvs[0] = new Vector2(0, -1);
                    uvs[1] = new Vector2(0, -1);
                    uvs[2] = new Vector2(0, -1);
                    uvs[3] = new Vector2(0, -1);
                }
                else if (minMask == 8) // 3 lowest
                {
                    uvs[0] = new Vector2(0, -1);
                    uvs[1] = new Vector2(0, -1);
                    uvs[2] = new Vector2(0, -1);
                    uvs[3] = new Vector2(0, -1);
                }
                else if (minMask == 3) // 0 == 1 lowest
                {
                    uvs[0] = new Vector2(0, 1);
                    uvs[1] = new Vector2(0, 1);
                    uvs[2] = new Vector2(0, 1);
                    uvs[3] = new Vector2(0, 1);
                }
                else if (minMask == 6) // 1 == 2 lowest
                {
                    uvs[0] = new Vector2(0, 1);
                    uvs[1] = new Vector2(0, 1);
                    uvs[2] = new Vector2(0, 1);
                    uvs[3] = new Vector2(0, 1);
                }
                else if (minMask == 9) // 0 == 3 lowest
                {
                    uvs[0] = new Vector2(0, -1);
                    uvs[1] = new Vector2(0, -1);
                    uvs[2] = new Vector2(0, -1);
                    uvs[3] = new Vector2(0, -1);
                }
                else if (minMask == 12) // 2 == 3 lowest
                {
                    uvs[0] = new Vector2(0, -1);
                    uvs[1] = new Vector2(0, -1);
                    uvs[2] = new Vector2(0, -1);
                    uvs[3] = new Vector2(0, -1);
                }
                else if (minMask == 7)// 0 == 1 == 2 lowest => 3 highest flow diagonal vertex => 1 lowest
                {
                    uvs[0] = new Vector2(0, 1);
                    uvs[1] = new Vector2(0, 1);
                    uvs[2] = new Vector2(0, 1);
                    uvs[3] = new Vector2(0, 1);
                }
                else if (minMask == 14)  // 1 == 2 == 3 lowest => 0 highest flow diagonal vertex => 2 lowest
                {
                    uvs[0] = new Vector2(0, -1);
                    uvs[1] = new Vector2(0, -1);
                    uvs[2] = new Vector2(0, -1);
                    uvs[3] = new Vector2(0, -1);
                }
                else if (minMask == 13) // 2 == 3 == 0 lowest => 1 highest flow diagonal vertex => 3 lowest
                {
                    uvs[0] = new Vector2(0, -1);
                    uvs[1] = new Vector2(0, -1);
                    uvs[2] = new Vector2(0, -1);
                    uvs[3] = new Vector2(0, -1);
                }
                else if (minMask == 11) // 3 == 0 == 1 lowest => 2 highest flow diagonal vertex => 0 lowest
                {
                    uvs[0] = new Vector2(0, 1);
                    uvs[1] = new Vector2(0, 1);
                    uvs[2] = new Vector2(0, 1);
                    uvs[3] = new Vector2(0, 1);
                }
                else if (minMask == 15)
                {
                    uvs[0] = new Vector2(0, 0);
                    uvs[1] = new Vector2(0, 0);
                    uvs[2] = new Vector2(0, 0);
                    uvs[3] = new Vector2(0, 0);
                }
            }
            else
            {
                // downfall
                uvs[0] = new Vector2(0, -1);
                uvs[1] = new Vector2(0, -1);
                uvs[2] = new Vector2(0, -1);
                uvs[3] = new Vector2(0, -1);
            }
        }

        private static int GetLowestLiquidVertices(Vector3[] verticesUp)
        {
            int indexMask = 1;
            float minY = verticesUp[0].y;
            for (int i = 1; i < 4; i++)
            {
                if (verticesUp[i].y < minY)
                {
                    minY = verticesUp[i].y;
                    indexMask = (1 << i);
                }
                else if (verticesUp[i].y == minY)
                {
                    indexMask |= (1 << i);
                }
            }
            return indexMask;
        }

        #endregion
    }
}
