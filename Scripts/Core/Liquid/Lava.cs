using PixelMiner.Enums;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using System.Buffers;
namespace PixelMiner.Core
{
    public class Lava : MonoBehaviour
    {
        public static Lava Instance { get; private set; }
        public const byte MAX_LAVA_LEVEL = 7;
        public TextMeshPro TextPrefab;


        // Main thread
        //private List<Vector3Int> _newLavaSpreadingPositions = new();

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








        public static void PropagateLava(LavaSource lavaSource)
        {
            int attempts = 0;
            Main main = Main.Instance;
            Vector3Int[] rentNeighbors = ArrayPool<Vector3Int>.Shared.Rent(5);
            Chunk targetChunk;
           
            FluidNode startNode = lavaSource.LavaSpreadingBfsQueue.Peek();
            while (lavaSource.LavaSpreadingBfsQueue.Count > 0)
            {
                if (lavaSource.LavaSpreadingBfsQueue.Peek().Level != startNode.Level || lavaSource.LavaSpreadingBfsQueue.Peek().GlobalPosition.y != startNode.GlobalPosition.y)
                {
                    break;
                }
           
                FluidNode currentNode = lavaSource.LavaSpreadingBfsQueue.Dequeue();
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

                        Vector3Int relativePos = targetChunk.GetRelativePosition(rentNeighbors[i]);
                        BlockID currentBlock = targetChunk.GetBlock(relativePos);
                        if (currentBlock.IsGrassType())
                        {
                            Main.Instance.RemoveGrassBlocks(rentNeighbors[i]);
                        }
                        if (currentBlock.IsSolidOpaqueVoxel()) continue;

                        if (i == 0)
                        {
                            FluidNode neighborNode = new FluidNode(rentNeighbors[i], (byte)(MAX_LAVA_LEVEL - 1));
                            lavaSource.LavaSpreadingBfsQueue.Enqueue(neighborNode);

                            targetChunk.SetBlock(relativePos, BlockID.Lava);
                            targetChunk.SetLiquidLevel(relativePos.x, relativePos.y, relativePos.z, neighborNode.Level);

                            lavaSource.NewLavaSpreadingPositions.Add(rentNeighbors[i]);
                            break;
                        }
                        else
                        {
                            if (targetChunk.GetLiquidLevel(relativePos.x, relativePos.y, relativePos.z) < currentNode.Level - 1 && currentNode.Level > 1)
                            {
                                FluidNode neighborNode = new FluidNode(rentNeighbors[i], (byte)(currentNode.Level - 1));
                                lavaSource.LavaSpreadingBfsQueue.Enqueue(neighborNode);

                                targetChunk.SetBlock(relativePos, BlockID.Lava);
                                targetChunk.SetLiquidLevel(relativePos.x, relativePos.y, relativePos.z, neighborNode.Level);
                                lavaSource.NewLavaSpreadingPositions.Add(rentNeighbors[i]);
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

            ArrayPool<Vector3Int>.Shared.Return(rentNeighbors);         
        }


        public static void RemoveLava(LavaSource lavaSource)
        {
            //Debug.Log("Remove water");
            Main main = Main.Instance;
            int attempts = 0;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(5);
            FluidNode startNode = lavaSource.LavaRemovalBfsQueue.Peek();
            main.SetLiquidLevel(startNode.GlobalPosition, 0);
            Chunk targetChunk;
            while (lavaSource.LavaRemovalBfsQueue.Count > 0)
            {
                if (lavaSource.LavaRemovalBfsQueue.Peek().Level + 1 <= startNode.Level || lavaSource.LavaRemovalBfsQueue.Peek().GlobalPosition.y != startNode.GlobalPosition.y)
                {
                    break;
                }

                FluidNode currentNode = lavaSource.LavaRemovalBfsQueue.Dequeue();
                GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                for (int i = 0; i < 5; i++)
                {
                    if (main.TryGetChunk(neighbors[i], out targetChunk))
                    {
                        if (!lavaSource.SpreadingChunkEffected.Contains(targetChunk))
                        {
                            lavaSource.SpreadingChunkEffected.Add(targetChunk);
                        }

                        Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
                        BlockID currentBlock = targetChunk.GetBlock(relativePos);
                        if (currentBlock != BlockID.Lava) continue;
                        if (targetChunk.GetLiquidLevel(relativePos) == MAX_LAVA_LEVEL) continue;


                        if (i == 0)
                        {
                            FluidNode neighborNode = new FluidNode(neighbors[i], (byte)(MAX_LAVA_LEVEL - 1));
                            lavaSource.LavaRemovalBfsQueue.Enqueue(neighborNode);
                            targetChunk.SetBlock(relativePos, BlockID.Air);
                            targetChunk.SetLiquidLevel(relativePos.x, relativePos.y, relativePos.z, 0);
                            break;
                        }
                        else
                        {
                            if (targetChunk.GetLiquidLevel(relativePos.x, relativePos.y, relativePos.z) <= currentNode.Level - 1 && currentNode.Level >= 1)
                            {
                                FluidNode neighborNode = new FluidNode(neighbors[i], (byte)(currentNode.Level - 1));
                                lavaSource.LavaRemovalBfsQueue.Enqueue(neighborNode);
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

            ArrayPool<Vector3Int>.Shared.Return(neighbors);
        }

        //public void RemoveLava(Chunk chunk)
        //{
        //    Debug.Log("Remove water");
        //    Main main = Main.Instance;
        //    main.SetLiquidLevel(chunk.LavaRemovalBfsQueue.Peek().GlobalPosition, 0);
        //    Dictionary<Vector3Int, byte> spreadWaterDict = new Dictionary<Vector3Int, byte>();
        //    int attempts = 0;
        //    Vector3Int[] neighbors = new Vector3Int[5];
        //    FluidNode startNode = chunk.LavaRemovalBfsQueue.Peek();
        //    Chunk targetChunk = chunk;
        //    HashSet<Chunk> needRenderChunk = new();

        //    while (chunk.LavaRemovalBfsQueue.Count > 0)
        //    {
        //        if (chunk.LavaRemovalBfsQueue.Peek().Level != startNode.Level || chunk.LavaRemovalBfsQueue.Peek().GlobalPosition.y != startNode.GlobalPosition.y)
        //        {
        //            break;
        //        }


        //        FluidNode currentNode = chunk.LavaRemovalBfsQueue.Dequeue();
        //        GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
        //        for (int i = 0; i < neighbors.Length; i++)
        //        {
        //            if (main.InSideChunkBound(chunk, neighbors[i]))
        //            {
        //                targetChunk = chunk;
        //            }
        //            else
        //            {
        //                bool foundChunk = Main.Instance.TryGetChunk(neighbors[i], out targetChunk);
        //                if (foundChunk)
        //                {
        //                    if (needRenderChunk.Contains(targetChunk) == false)
        //                        needRenderChunk.Add(targetChunk);
        //                }
        //                else
        //                {
        //                    Debug.LogError("not found this chunk.");
        //                }

        //            }

        //            Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
        //            BlockID currentBlock = targetChunk.GetBlock(relativePos);
        //            if (currentBlock != BlockID.Water) continue;


        //            if (i == 0)
        //            {
        //                FluidNode neighborNode = new FluidNode(neighbors[i], (byte)(Water.MAX_WATER_LEVEL - 1));
        //                targetChunk.LavaRemovalBfsQueue.Enqueue(neighborNode);
        //                targetChunk.SetBlock(relativePos, BlockID.Air);
        //                targetChunk.SetLiquidLevel(relativePos.x, relativePos.y, relativePos.z, 0);
        //                break;
        //            }
        //            else
        //            {
        //                if (targetChunk.GetLiquidLevel(relativePos.x, relativePos.y, relativePos.z) <= currentNode.Level - 1 && currentNode.Level >= 1)
        //                {
        //                    FluidNode neighborNode = new FluidNode(neighbors[i], (byte)(currentNode.Level - 1));
        //                    targetChunk.LavaRemovalBfsQueue.Enqueue(neighborNode);
        //                    targetChunk.SetBlock(relativePos, BlockID.Air);
        //                    targetChunk.SetLiquidLevel(relativePos.x, relativePos.y, relativePos.z, 0);
        //                }
        //                else
        //                {
        //                    //WaterNode neighborNode = new WaterNode(neighbors[i], nbNodeChunk.GetWaterLevel(relativePos));
        //                    //if (spreadWaterDict.ContainsKey(neighborNode.GlobalPosition) == false)
        //                    //{
        //                    //    spreadWaterDict.Add(neighborNode.GlobalPosition, neighborNode.Level);
        //                    //}
        //                    //else
        //                    //{
        //                    //    spreadWaterDict[neighborNode.GlobalPosition] = neighborNode.Level;
        //                    //}
        //                }
        //            }
        //        }

        //        attempts++;
        //        if (attempts > 1000)
        //        {
        //            Debug.LogWarning("Infinite loop");
        //            break;
        //        }
        //    }

        //    foreach (var c in needRenderChunk)
        //    {
        //        c.UpdateMask |= UpdateChunkMask.RenderAll;
        //    }
        //}

    }
}
