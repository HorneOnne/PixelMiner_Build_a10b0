using PixelMiner.Enums;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using TMPro;
using System.Collections;
using System.Buffers;
namespace PixelMiner.Core
{
    public class Water : MonoBehaviour
    {
        public static Water Instance { get; private set; }
        public const byte MAX_WATER_LEVEL = 7;
        public TextMeshPro TextPrefab;

        //static Dictionary<Vector3Int, TextMeshPro> TextDict = new();
        private void Awake()
        {
            Instance = this;
        }


        private void OnDestroy()
        {
            //TextDict.Clear();
        }


        public static void GetVoxelNeighborPosition(Vector3Int position, ref Vector3Int[] neighborPosition)
        {
            neighborPosition[0] = new Vector3Int(position.x, position.y - 1, position.z);   // down
            neighborPosition[1] = new Vector3Int(position.x - 1, position.y, position.z);   // west
            neighborPosition[2] = new Vector3Int(position.x + 1, position.y, position.z);   // east
            neighborPosition[3] = new Vector3Int(position.x, position.y, position.z + 1);   // north
            neighborPosition[4] = new Vector3Int(position.x, position.y, position.z - 1);   //south
        }






        public static async Task PropagateWaterTask(WaterSource waterSource)
        {
            int attempts = 0;
            Main main = Main.Instance;
            Vector3Int[] rentNeighbors = ArrayPool<Vector3Int>.Shared.Rent(5);
            FluidNode startNode = waterSource.WaterSpreadingBfsQueue.Peek();
            Chunk targetChunk;
            int spreadingBlockCount = 0;
            await Task.Run(() =>
            {
                while (waterSource.WaterSpreadingBfsQueue.Count > 0)
                {
                    // earyly break condition (animation purpose)
                    if ((waterSource.WaterSpreadingBfsQueue.Peek().Level + 1 <= startNode.Level || waterSource.WaterSpreadingBfsQueue.Peek().GlobalPosition.y != startNode.GlobalPosition.y) &&
                        spreadingBlockCount >= 4)
                    {
                        break;
                    }


                    FluidNode currentNode = waterSource.WaterSpreadingBfsQueue.Dequeue();
                    int spreadingMask = main.GetNearestFlowDirection(currentNode.GlobalPosition);
                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref rentNeighbors);
                    for (int i = 0; i < 5; i++)
                    {
                        if (main.TryGetChunk(rentNeighbors[i], out targetChunk))
                        {       
                            if (i > 0)
                            {
                                if ((spreadingMask & (1 << i - 1)) == 0)  // bit is not set
                                {
                                    continue;
                                }
                            }
                            if (!waterSource.ChunkEffected.Contains(targetChunk))
                            {
                                waterSource.ChunkEffected.Add(targetChunk);
                            }

                            Vector3Int relativePos = targetChunk.GetRelativePosition(rentNeighbors[i]);
                            BlockID currentBlock = targetChunk.GetBlock(relativePos);
                            if (currentBlock.IsGrassType())
                            {
                                Main.Instance.RemoveGrassBlocks(rentNeighbors[i]);
                            }
                            if (currentBlock.IsSolidOpaqueVoxel()) continue;
                            if (targetChunk.GetLiquidLevel(relativePos) == MAX_WATER_LEVEL) continue;

                            spreadingBlockCount++;
                            if (i == 0)
                            {
                                FluidNode neighborNode = new FluidNode(rentNeighbors[i], (byte)(Water.MAX_WATER_LEVEL - 1));
                                waterSource.WaterSpreadingBfsQueue.Enqueue(neighborNode);

                                targetChunk.SetBlock(relativePos, BlockID.Water);
                                targetChunk.SetLiquidLevel(relativePos.x, relativePos.y, relativePos.z, neighborNode.Level);
                                break;
                            }
                            else
                            {
                                if (targetChunk.GetLiquidLevel(relativePos.x, relativePos.y, relativePos.z) < currentNode.Level - 1 && currentNode.Level > 1)
                                {
                                    FluidNode neighborNode = new FluidNode(rentNeighbors[i], (byte)(currentNode.Level - 1));
                                    waterSource.WaterSpreadingBfsQueue.Enqueue(neighborNode);

                                    targetChunk.SetBlock(relativePos, BlockID.Water);
                                    targetChunk.SetLiquidLevel(relativePos.x, relativePos.y, relativePos.z, neighborNode.Level);
                                }
                            }
                        }
                    }

                    attempts++;
                    if (attempts > 1000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });

            //Debug.Log($"Attempt: {attempts}");
            ArrayPool<Vector3Int>.Shared.Return(rentNeighbors);
        }

        public static async Task RemoveWaterTask(WaterSource waterSource)
        {
            //Debug.Log("Remove water");
            Main main = Main.Instance;
            int attempts = 0;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(5);
            FluidNode startNode = waterSource.WaterRemovalBfsQueue.Peek();
            main.SetLiquidLevel(startNode.GlobalPosition, 0);
            Chunk targetChunk;
            await Task.Run(() =>
            {
                while (waterSource.WaterRemovalBfsQueue.Count > 0)
                {
                    if (waterSource.WaterRemovalBfsQueue.Peek().Level + 1 <= startNode.Level || waterSource.WaterRemovalBfsQueue.Peek().GlobalPosition.y != startNode.GlobalPosition.y)
                    {
                        break;
                    }

                    FluidNode currentNode = waterSource.WaterRemovalBfsQueue.Dequeue();
                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                    for (int i = 0; i < 5; i++)
                    {
                        if (main.TryGetChunk(neighbors[i], out targetChunk))
                        {
                            if (!waterSource.ChunkEffected.Contains(targetChunk))
                            {
                                waterSource.ChunkEffected.Add(targetChunk);
                            }

                            Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
                            BlockID currentBlock = targetChunk.GetBlock(relativePos);
                            if (currentBlock != BlockID.Water) continue;
                            if (targetChunk.GetLiquidLevel(relativePos) == Water.MAX_WATER_LEVEL) continue;


                            if (i == 0)
                            {
                                FluidNode neighborNode = new FluidNode(neighbors[i], (byte)(Water.MAX_WATER_LEVEL - 1));
                                waterSource.WaterRemovalBfsQueue.Enqueue(neighborNode);
                                targetChunk.SetBlock(relativePos, BlockID.Air);
                                targetChunk.SetLiquidLevel(relativePos.x, relativePos.y, relativePos.z, 0);
                                break;
                            }
                            else
                            {
                                if (targetChunk.GetLiquidLevel(relativePos.x, relativePos.y, relativePos.z) <= currentNode.Level - 1 && currentNode.Level >= 1)
                                {
                                    FluidNode neighborNode = new FluidNode(neighbors[i], (byte)(currentNode.Level - 1));
                                    waterSource.WaterRemovalBfsQueue.Enqueue(neighborNode);
                                    targetChunk.SetBlock(relativePos, BlockID.Air);
                                    targetChunk.SetLiquidLevel(relativePos.x, relativePos.y, relativePos.z, 0);
                                }
                            }
                        }
                    }


                    attempts++;
                    if (attempts > 1000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });

            ArrayPool<Vector3Int>.Shared.Return(neighbors);
        }
    }
}
