using PixelMiner.Enums;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using TMPro;
using System.Buffers;
namespace PixelMiner.Core
{
    /// <summary>
    /// To calculate lighting i use global position for LightNode.Position for easily calcualting light propagate cross chunk.
    /// </summary>
    public class LightCalculator : MonoBehaviour
    {
        public static LightCalculator Instance { get; private set; }
        public TextMeshPro TextPrefab;
        private Dictionary<Vector3Int, TextMeshPro> Dict = new Dictionary<Vector3Int, TextMeshPro>();
        private WaitForSeconds _wait = new WaitForSeconds(0.2f);
        public bool ShowDiagonal = false;
        public float RemoveTime;
        public float PropagateTime;

        // main thread work only
        private static Dictionary<Vector3Int, LightNode> _spreadingLightDict = new Dictionary<Vector3Int, LightNode>();


        //if (Dict.ContainsKey(neighborNode.GlobalPosition) == false)
        //{
        //    var textInstance = Instantiate(TextPrefab, neighborNode.GlobalPosition + new Vector3(0.5f, 0.5f, 0.5f), Quaternion.Euler(90, 0, 0));
        //    textInstance.text = neighborNode.Red().ToString();
        //    Dict.Add(neighborNode.GlobalPosition, textInstance);
        //}
        //else
        //{
        //    Dict[neighborNode.GlobalPosition].text = neighborNode.Red().ToString();
        //}

        private void Awake()
        {
            Instance = this;
        }





        #region Propagate block light channel
        public static async Task PropagateRedLightAsync(Queue<LightNode> redLightBfsQueue, HashSet<Chunk> updatedChunks)
        {
            if (redLightBfsQueue.Count == 0) return;
            int attempts = 0;
            Main main = Main.Instance;
            int neighborsSize = 6;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(neighborsSize);
            Chunk targetChunk;



            foreach (var redLight in redLightBfsQueue)
            {
                if (Main.Instance.TryGetChunk(redLight.GlobalPosition, out targetChunk))
                {
                    Vector3Int relPosition = targetChunk.GetRelativePosition(redLight.GlobalPosition);
                    if (redLight.Invensity > targetChunk.GetRedLight(relPosition.x, relPosition.y, relPosition.z))
                    {
                        targetChunk.SetRedLight(relPosition.x, relPosition.y, relPosition.z, redLight.Invensity);
                    }
                }
            }



            await Task.Run(() =>
            {
                // red
                while (redLightBfsQueue.Count > 0)
                {
                    LightNode currentNode = redLightBfsQueue.Dequeue();
                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                    for (int i = 0; i < neighborsSize; i++)
                    {
                        if (main.TryGetChunk(neighbors[i], out targetChunk))
                        {
                            if (!targetChunk.HasDrawnFirstTime) continue;
                            if (!updatedChunks.Contains(targetChunk))
                            {
                                updatedChunks.Add(targetChunk);
                            }

                            Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
                            BlockID currentBlock = targetChunk.GetBlock(relativePos);
                            byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];

                            if (targetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity < currentNode.Invensity && currentNode.Invensity > 0)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                redLightBfsQueue.Enqueue(neighborNode);
                                targetChunk.SetRedLight(relativePos.x, relativePos.y, relativePos.z, neighborNode.Invensity);

                            }
                        }
                    }

                    attempts++;
                    if (attempts > 10000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });

            ArrayPool<Vector3Int>.Shared.Return(neighbors);
        }
        public static async Task PropagateGreenLightAsync(Queue<LightNode> greenLightBfsQueue, HashSet<Chunk> updatedChunks)
        {
            if (greenLightBfsQueue.Count == 0) return;
            int attempts = 0;
            Main main = Main.Instance;
            int neighborsSize = 6;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(neighborsSize);
            Chunk targetChunk;

            foreach (var greenLight in greenLightBfsQueue)
            {
                if (Main.Instance.TryGetChunk(greenLight.GlobalPosition, out targetChunk))
                {
                    Vector3Int relPosition = targetChunk.GetRelativePosition(greenLight.GlobalPosition);
                    if (greenLight.Invensity > targetChunk.GetGreenLight(relPosition.x, relPosition.y, relPosition.z))
                    {
                        targetChunk.SetGreenLight(relPosition.x, relPosition.y, relPosition.z, greenLight.Invensity);
                    }
                }
            }


            await Task.Run(() =>
            {
                while (greenLightBfsQueue.Count > 0)
                {
                    LightNode currentNode = greenLightBfsQueue.Dequeue();
                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                    for (int i = 0; i < neighborsSize; i++)
                    {
                        if (main.TryGetChunk(neighbors[i], out targetChunk))
                        {
                            if (!targetChunk.HasDrawnFirstTime) continue;
                            if (!updatedChunks.Contains(targetChunk))
                            {
                                updatedChunks.Add(targetChunk);
                            }

                            Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
                            BlockID currentBlock = targetChunk.GetBlock(relativePos);
                            byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];

                            if (targetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity < currentNode.Invensity && currentNode.Invensity > 0)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                greenLightBfsQueue.Enqueue(neighborNode);
                                targetChunk.SetGreenLight(relativePos.x, relativePos.y, relativePos.z, neighborNode.Invensity);
                            }
                        }
                    }

                    attempts++;
                    if (attempts > 10000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });
            ArrayPool<Vector3Int>.Shared.Return(neighbors);
        }
        public static async Task PropagateBlueLightAsync(Queue<LightNode> blueLightBfsQueue, HashSet<Chunk> updatedChunks)
        {
            if (blueLightBfsQueue.Count == 0) return;
            int attempts = 0;
            Main main = Main.Instance;
            int neighborsSize = 6;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(neighborsSize);
            Chunk targetChunk;
            foreach (var blueLight in blueLightBfsQueue)
            {
                if (Main.Instance.TryGetChunk(blueLight.GlobalPosition, out targetChunk))
                {
                    Vector3Int relPosition = targetChunk.GetRelativePosition(blueLight.GlobalPosition);
                    if (blueLight.Invensity > targetChunk.GetBlueLight(relPosition.x, relPosition.y, relPosition.z))
                    {
                        targetChunk.SetBlueLight(relPosition.x, relPosition.y, relPosition.z, blueLight.Invensity);
                    }
                }
            }

            await Task.Run(() =>
            {
                while (blueLightBfsQueue.Count > 0)
                {
                    LightNode currentNode = blueLightBfsQueue.Dequeue();
                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                    for (int i = 0; i < neighborsSize; i++)
                    {
                        if (main.TryGetChunk(neighbors[i], out targetChunk))
                        {
                            if (!targetChunk.HasDrawnFirstTime) continue;
                            if (!updatedChunks.Contains(targetChunk))
                            {
                                updatedChunks.Add(targetChunk);
                            }

                            Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
                            BlockID currentBlock = targetChunk.GetBlock(relativePos);
                            byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];

                            if (targetChunk.GetBlueLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity < currentNode.Invensity && currentNode.Invensity > 0)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                blueLightBfsQueue.Enqueue(neighborNode);
                                targetChunk.SetBlueLight(relativePos.x, relativePos.y, relativePos.z, neighborNode.Invensity);
                            }
                        }
                    }

                    attempts++;
                    if (attempts > 10000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });
            ArrayPool<Vector3Int>.Shared.Return(neighbors);
        }




        public static void PropagateRedLight(Queue<LightNode> redLightBfsQueue, HashSet<Chunk> updatedChunks)
        {
            if (redLightBfsQueue.Count == 0) return;

            int attempts = 0;
            int neighborsSize = 6;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(neighborsSize);
            Chunk targetChunk;

            foreach (var redLight in redLightBfsQueue)
            {
                if (Main.Instance.TryGetChunk(redLight.GlobalPosition, out targetChunk))
                {
                    Vector3Int relPosition = targetChunk.GetRelativePosition(redLight.GlobalPosition);
                    if (redLight.Invensity > targetChunk.GetRedLight(relPosition.x, relPosition.y, relPosition.z))
                    {
                        targetChunk.SetRedLight(relPosition.x, relPosition.y, relPosition.z, redLight.Invensity);
                    }
                }
            }

            // red
            while (redLightBfsQueue.Count > 0)
            {
                LightNode currentNode = redLightBfsQueue.Dequeue();
                GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                for (int i = 0; i < neighborsSize; i++)
                {
                    if (Main.Instance.TryGetChunk(neighbors[i], out targetChunk))
                    {
                        if (!targetChunk.HasDrawnFirstTime) continue;
                        if (!updatedChunks.Contains(targetChunk))
                        {
                            updatedChunks.Add(targetChunk);
                        }

                        Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
                        BlockID currentBlock = targetChunk.GetBlock(relativePos);
                        byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];

                        if (targetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity < currentNode.Invensity && currentNode.Invensity > 0)
                        {
                            LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                            redLightBfsQueue.Enqueue(neighborNode);
                            targetChunk.SetRedLight(relativePos.x, relativePos.y, relativePos.z, neighborNode.Invensity);
                        }
                    }

                }

                attempts++;
                if (attempts > 10000)
                {
                    Debug.LogWarning("Infinite loop");
                    break;
                }
            }

            ArrayPool<Vector3Int>.Shared.Return(neighbors);

        }
        public static void PropagateGreenLight(Queue<LightNode> greenLightBfsQueue, HashSet<Chunk> updatedChunks)
        {
            if (greenLightBfsQueue.Count == 0) return;
            int attempts = 0;
            Main main = Main.Instance;
            int neighborsSize = 6;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(neighborsSize);
            Chunk targetChunk;


            foreach (var greenLight in greenLightBfsQueue)
            {
                if (Main.Instance.TryGetChunk(greenLight.GlobalPosition, out targetChunk))
                {
                    Vector3Int relPosition = targetChunk.GetRelativePosition(greenLight.GlobalPosition);
                    if (greenLight.Invensity > targetChunk.GetGreenLight(relPosition.x, relPosition.y, relPosition.z))
                    {
                        targetChunk.SetGreenLight(relPosition.x, relPosition.y, relPosition.z, greenLight.Invensity);
                    }
                }
            }

            while (greenLightBfsQueue.Count > 0)
            {
                LightNode currentNode = greenLightBfsQueue.Dequeue();
                GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                for (int i = 0; i < neighborsSize; i++)
                {
                    if (main.TryGetChunk(neighbors[i], out targetChunk))
                    {
                        if (!updatedChunks.Contains(targetChunk))
                        {
                            updatedChunks.Add(targetChunk);
                        }

                        Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
                        BlockID currentBlock = targetChunk.GetBlock(relativePos);
                        byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];

                        if (targetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity < currentNode.Invensity && currentNode.Invensity > 0)
                        {
                            LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                            greenLightBfsQueue.Enqueue(neighborNode);
                            targetChunk.SetGreenLight(relativePos.x, relativePos.y, relativePos.z, neighborNode.Invensity);
                        }
                    }
                }

                attempts++;
                if (attempts > 10000)
                {
                    Debug.LogWarning("Infinite loop");
                    break;
                }
            }
            ArrayPool<Vector3Int>.Shared.Return(neighbors);
        }
        public static void PropagateBlueLight(Queue<LightNode> blueLightBfsQueue, HashSet<Chunk> updatedChunks)
        {
            if (blueLightBfsQueue.Count == 0) return;
            int attempts = 0;
            Main main = Main.Instance;
            int neighborsSize = 6;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(neighborsSize);
            Chunk targetChunk;
            foreach (var blueLight in blueLightBfsQueue)
            {
                if (Main.Instance.TryGetChunk(blueLight.GlobalPosition, out targetChunk))
                {
                    Vector3Int relPosition = targetChunk.GetRelativePosition(blueLight.GlobalPosition);
                    if (blueLight.Invensity > targetChunk.GetBlueLight(relPosition.x, relPosition.y, relPosition.z))
                    {
                        targetChunk.SetBlueLight(relPosition.x, relPosition.y, relPosition.z, blueLight.Invensity);
                    }
                }
            }

            while (blueLightBfsQueue.Count > 0)
            {
                LightNode currentNode = blueLightBfsQueue.Dequeue();
                GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                for (int i = 0; i < neighborsSize; i++)
                {
                    if (main.TryGetChunk(neighbors[i], out targetChunk))
                    {
                        if (!updatedChunks.Contains(targetChunk))
                        {
                            updatedChunks.Add(targetChunk);
                        }

                        Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
                        BlockID currentBlock = targetChunk.GetBlock(relativePos);
                        byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];

                        if (targetChunk.GetBlueLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity < currentNode.Invensity && currentNode.Invensity > 0)
                        {
                            LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                            blueLightBfsQueue.Enqueue(neighborNode);
                            targetChunk.SetBlueLight(relativePos.x, relativePos.y, relativePos.z, neighborNode.Invensity);
                        }
                    }
                }

                attempts++;
                if (attempts > 10000)
                {
                    Debug.LogWarning("Infinite loop");
                    break;
                }
            }
            ArrayPool<Vector3Int>.Shared.Return(neighbors);
        }
        #endregion




        #region Remove block light channel

        public static async Task RemoveRedLightAsync(
          Queue<LightNode> redLightRemoveLightBfsQueue,
          Queue<LightNode> redLightSpreadingBfsQueue,
           HashSet<Chunk> updatedChunks)
        {

            Main main = Main.Instance;


            Dictionary<Vector3Int, LightNode> spreadLightDict = new Dictionary<Vector3Int, LightNode>();
            Vector3Int[] neighbors = new Vector3Int[6];
            int attempts = 0;
            Chunk targetChunk;


            foreach (var redLight in redLightRemoveLightBfsQueue)
            {
                if (Main.Instance.TryGetChunk(redLight.GlobalPosition, out targetChunk))
                {
                    Vector3Int relPosition = targetChunk.GetRelativePosition(redLight.GlobalPosition);
                    if (targetChunk.GetRedLight(relPosition.x, relPosition.y, relPosition.z) <= redLight.Invensity)
                    {
                        targetChunk.SetRedLight(relPosition.x, relPosition.y, relPosition.z, 0);
                    }
                }
            }

            await Task.Run(() =>
            {
                while (redLightRemoveLightBfsQueue.Count > 0)
                {
                    //Debug.Log("red");
                    LightNode currentNode = redLightRemoveLightBfsQueue.Dequeue();
                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        // Check chunk need render or not prevent over use hashing -> improve speed.
                        if (main.TryGetChunk(neighbors[i], out targetChunk))
                        {
                            if (!targetChunk.HasDrawnFirstTime) continue;
                            if (!updatedChunks.Contains(targetChunk))
                            {
                                updatedChunks.Add(targetChunk);
                            }

                            Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
                            if (targetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z) == 0)
                            {
                                continue;
                            }


                            BlockID currentBlock = targetChunk.GetBlock(relativePos);
                            byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
                            if (targetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity <= currentNode.Invensity)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                redLightRemoveLightBfsQueue.Enqueue(neighborNode);
                                targetChunk.SetRedLight(relativePos.x, relativePos.y, relativePos.z, 0);
                            }
                            else
                            {
                                if (spreadLightDict.ContainsKey(neighbors[i]) == false)
                                {
                                    LightNode neighborNode = new LightNode(neighbors[i], targetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z));
                                    spreadLightDict.Add(neighborNode.GlobalPosition, neighborNode);
                                }     
                            }
                        }
                    }

                    attempts++;
                    if (attempts > 10000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });
            foreach (var node in spreadLightDict)
            {
                redLightSpreadingBfsQueue.Enqueue(node.Value);
            }

            if (redLightSpreadingBfsQueue.Count > 0)
            {
                await PropagateRedLightAsync(redLightSpreadingBfsQueue, updatedChunks);
            }
        }


        public static async Task RemoveGreenLightAsync(
            Queue<LightNode> greenLightRemoveLightBfsQueue,
            Queue<LightNode> greenLightSpreadingBfsQueue,
              HashSet<Chunk> updatedChunks)
        {
            Main main = Main.Instance;
            Dictionary<Vector3Int, LightNode> spreadLightDict = new Dictionary<Vector3Int, LightNode>();
            Vector3Int[] neighbors = new Vector3Int[6];
            int attempts = 0;
            Chunk targetChunk;


            foreach (var greenLight in greenLightRemoveLightBfsQueue)
            {
                if (Main.Instance.TryGetChunk(greenLight.GlobalPosition, out targetChunk))
                {
                    Vector3Int relPosition = targetChunk.GetRelativePosition(greenLight.GlobalPosition);
                    if (targetChunk.GetGreenLight(relPosition.x, relPosition.y, relPosition.z) <= greenLight.Invensity)
                    {
                        targetChunk.SetGreenLight(relPosition.x, relPosition.y, relPosition.z, 0);
                    }

                }
            }

            await Task.Run(() =>
            {
                while (greenLightRemoveLightBfsQueue.Count > 0)
                {
                    LightNode currentNode = greenLightRemoveLightBfsQueue.Dequeue();
                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        // Check chunk need render or not prevent over use hashing -> improve speed.
                        if (main.TryGetChunk(neighbors[i], out targetChunk))
                        {
                            if (!targetChunk.HasDrawnFirstTime) continue;
                            if (!updatedChunks.Contains(targetChunk))
                            {
                                updatedChunks.Add(targetChunk);
                            }

                            Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
                            if (targetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z) == 0)
                            {
                                continue;
                            }


                            BlockID currentBlock = targetChunk.GetBlock(relativePos);
                            byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
                            if (targetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity <= currentNode.Invensity)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                greenLightRemoveLightBfsQueue.Enqueue(neighborNode);
                                targetChunk.SetGreenLight(relativePos.x, relativePos.y, relativePos.z, 0);

                                if (spreadLightDict.ContainsKey(neighbors[i]))
                                {
                                    spreadLightDict.Remove(neighbors[i]);
                                }
                            }
                            else
                            {
                                if (spreadLightDict.ContainsKey(neighbors[i]) == false)
                                {
                                    LightNode neighborNode = new LightNode(neighbors[i], targetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z));
                                    spreadLightDict.Add(neighborNode.GlobalPosition, neighborNode);
                                }

                            }
                        }
                    }

                    attempts++;
                    if (attempts > 10000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });
            foreach (var node in spreadLightDict)
            {
                greenLightSpreadingBfsQueue.Enqueue(node.Value);
            }

            if (greenLightSpreadingBfsQueue.Count > 0)
            {
                await PropagateGreenLightAsync(greenLightSpreadingBfsQueue, updatedChunks);
            }
        }

        public static async Task RemoveBlueLightAsync(
          Queue<LightNode> blueLightRemoveLightBfsQueue,
          Queue<LightNode> blueLightSpreadingBfsQueue,
          HashSet<Chunk> updatedChunks)
        {
            Main main = Main.Instance;
            //HashSet<Chunk> chunksNeedUpdate = new();
            Dictionary<Vector3Int, LightNode> spreadLightDict = new Dictionary<Vector3Int, LightNode>();
            Vector3Int[] neighbors = new Vector3Int[6];
            int attempts = 0;
            Chunk targetChunk;

            foreach (var blueLight in blueLightRemoveLightBfsQueue)
            {
                if (Main.Instance.TryGetChunk(blueLight.GlobalPosition, out targetChunk))
                {
                    Vector3Int relPosition = targetChunk.GetRelativePosition(blueLight.GlobalPosition);
                    if (targetChunk.GetBlueLight(relPosition.x, relPosition.y, relPosition.z) <= blueLight.Invensity)
                    {
                        targetChunk.SetBlueLight(relPosition.x, relPosition.y, relPosition.z, 0);
                    }
                }
            }


            await Task.Run(() =>
            {
                while (blueLightRemoveLightBfsQueue.Count > 0)
                {
                    LightNode currentNode = blueLightRemoveLightBfsQueue.Dequeue();
                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        // Check chunk need render or not prevent over use hashing -> improve speed.
                        if (main.TryGetChunk(neighbors[i], out targetChunk))
                        {
                            if (!targetChunk.HasDrawnFirstTime) continue;
                            if (!updatedChunks.Contains(targetChunk))
                            {
                                updatedChunks.Add(targetChunk);
                            }

                            Vector3Int relativePos = targetChunk.GetRelativePosition(neighbors[i]);
                            if (targetChunk.GetBlueLight(relativePos.x, relativePos.y, relativePos.z) == 0)
                            {
                                continue;
                            }


                            BlockID currentBlock = targetChunk.GetBlock(relativePos);
                            byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
                            if (targetChunk.GetBlueLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity <= currentNode.Invensity)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                blueLightRemoveLightBfsQueue.Enqueue(neighborNode);
                                targetChunk.SetBlueLight(relativePos.x, relativePos.y, relativePos.z, 0);

                                if (spreadLightDict.ContainsKey(neighbors[i]))
                                {
                                    spreadLightDict.Remove(neighbors[i]);
                                }
                            }
                            else
                            {
                                if (spreadLightDict.ContainsKey(neighbors[i]) == false)
                                {
                                    LightNode neighborNode = new LightNode(neighbors[i], targetChunk.GetBlueLight(relativePos.x, relativePos.y, relativePos.z));
                                    spreadLightDict.Add(neighborNode.GlobalPosition, neighborNode);
                                }

                            }
                        }
                    }

                    attempts++;
                    if (attempts > 10000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });
            foreach (var node in spreadLightDict)
            {
                blueLightSpreadingBfsQueue.Enqueue(node.Value);
            }


            if (blueLightSpreadingBfsQueue.Count > 0)
            {
                await PropagateBlueLightAsync(blueLightSpreadingBfsQueue, updatedChunks);
            }
        }




        public static void RemoveRedLight(
        Queue<LightNode> redLightRemoveLightBfsQueue,
        Queue<LightNode> redLightSpreadingBfsQueue,
          HashSet<Chunk> updatedChunks)
        {
            Debug.Log("Remove red light");
            _spreadingLightDict.Clear();
            int neighborSize = 6;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(neighborSize);
            int attempts = 0;
            Chunk currentTargetChunk;
            LightNode startRedLightRemovalNode = redLightRemoveLightBfsQueue.Peek();


            if (Main.Instance.TryGetChunk(startRedLightRemovalNode.GlobalPosition, out currentTargetChunk))
            {
                updatedChunks.Add(currentTargetChunk);

                Vector3Int relPosition = currentTargetChunk.GetRelativePosition(startRedLightRemovalNode.GlobalPosition);

                if (currentTargetChunk.GetRedLight(relPosition.x, relPosition.y, relPosition.z) <= startRedLightRemovalNode.Invensity)
                    currentTargetChunk.SetRedLight(relPosition.x, relPosition.y, relPosition.z, 0);
            }

            // red
            while (redLightRemoveLightBfsQueue.Count > 0)
            {
                //Debug.Log("red");
                LightNode currentNode = redLightRemoveLightBfsQueue.Dequeue();
                GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                for (int i = 0; i < neighborSize; i++)
                {
                    // Check chunk need render or not prevent over use hashing -> improve speed.
                    if (Main.Instance.TryGetChunk(neighbors[i], out currentTargetChunk))
                    {
                        if (!updatedChunks.Contains(currentTargetChunk))
                        {
                            updatedChunks.Add(currentTargetChunk);
                        }

                        Vector3Int relativePos = currentTargetChunk.GetRelativePosition(neighbors[i]);
                        if (currentTargetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z) == 0)
                        {
                            continue;
                        }


                        BlockID currentBlock = currentTargetChunk.GetBlock(relativePos);
                        byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
                        if (currentTargetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity <= currentNode.Invensity)
                        {
                            LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                            redLightRemoveLightBfsQueue.Enqueue(neighborNode);
                            currentTargetChunk.SetRedLight(relativePos.x, relativePos.y, relativePos.z, 0);

                            if (_spreadingLightDict.ContainsKey(neighbors[i]))
                            {
                                _spreadingLightDict.Remove(neighbors[i]);
                            }
                        }
                        else
                        {
                            if (_spreadingLightDict.ContainsKey(neighbors[i]) == false)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], currentTargetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z));
                                _spreadingLightDict.Add(neighborNode.GlobalPosition, neighborNode);
                            }
                        }
                    }
                }

                attempts++;
                if (attempts > 10000)
                {
                    Debug.LogWarning("Infinite loop");
                    break;
                }
            }
            ArrayPool<Vector3Int>.Shared.Return(neighbors);
            foreach (var node in _spreadingLightDict)
            {
                redLightSpreadingBfsQueue.Enqueue(node.Value);
            }
            if (redLightSpreadingBfsQueue.Count > 0)
            {
                PropagateRedLight(redLightSpreadingBfsQueue, updatedChunks);
            }
        }

        public static void RemoveGreenLight(
            Queue<LightNode> greenLightRemoveLightBfsQueue,
            Queue<LightNode> greenLightSpreadingBfsQueue,
          HashSet<Chunk> updatedChunks)
        {
            _spreadingLightDict.Clear();
            int neighborSize = 6;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(neighborSize);
            int attempts = 0;
            Chunk currentTargetChunk;
            LightNode startGreenLightRemovalNode = greenLightRemoveLightBfsQueue.Peek();

            if (Main.Instance.TryGetChunk(startGreenLightRemovalNode.GlobalPosition, out currentTargetChunk))
            {
                updatedChunks.Add(currentTargetChunk);

                Vector3Int relPosition = currentTargetChunk.GetRelativePosition(startGreenLightRemovalNode.GlobalPosition);

                if (currentTargetChunk.GetGreenLight(relPosition.x, relPosition.y, relPosition.z) <= startGreenLightRemovalNode.Invensity)
                    currentTargetChunk.SetGreenLight(relPosition.x, relPosition.y, relPosition.z, 0);
            }


            while (greenLightRemoveLightBfsQueue.Count > 0)
            {
                //Debug.Log("Green A");
                LightNode currentNode = greenLightRemoveLightBfsQueue.Dequeue();
                GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                for (int i = 0; i < neighborSize; i++)
                {
                    //Debug.Log("Green B");
                    // Check chunk need render or not prevent over use hashing -> improve speed.
                    if (Main.Instance.TryGetChunk(neighbors[i], out currentTargetChunk))
                    {
                        if (!updatedChunks.Contains(currentTargetChunk))
                        {
                            updatedChunks.Add(currentTargetChunk);
                        }

                        Vector3Int relativePos = currentTargetChunk.GetRelativePosition(neighbors[i]);
                        if (currentTargetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z) == 0)
                        {
                            continue;
                        }

                        //Debug.Log("Green C");
                        BlockID currentBlock = currentTargetChunk.GetBlock(relativePos);
                        byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
                        if (currentTargetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity <= currentNode.Invensity)
                        {
                            //Debug.Log("Green D");
                            LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                            greenLightRemoveLightBfsQueue.Enqueue(neighborNode);
                            currentTargetChunk.SetGreenLight(relativePos.x, relativePos.y, relativePos.z, 0);

                            if (_spreadingLightDict.ContainsKey(neighbors[i]))
                            {
                                _spreadingLightDict.Remove(neighbors[i]);
                            }
                        }
                        else
                        {
                            if (_spreadingLightDict.ContainsKey(neighbors[i]) == false)
                            {
                                //Debug.Log("Green E");
                                LightNode neighborNode = new LightNode(neighbors[i], currentTargetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z));
                                _spreadingLightDict.Add(neighborNode.GlobalPosition, neighborNode);
                            }

                        }
                    }
                }

                attempts++;
                if (attempts > 10000)
                {
                    Debug.LogWarning("Infinite loop");
                    break;
                }
            }
            ArrayPool<Vector3Int>.Shared.Return(neighbors);
            foreach (var node in _spreadingLightDict)
            {
                greenLightSpreadingBfsQueue.Enqueue(node.Value);
            }
            if (greenLightSpreadingBfsQueue.Count > 0)
            {
                PropagateGreenLight(greenLightSpreadingBfsQueue, updatedChunks);
            }
        }

        public static void RemoveBlueLight(
          Queue<LightNode> blueLightRemoveLightBfsQueue,
          Queue<LightNode> blueLightSpreadingBfsQueue,
          HashSet<Chunk> updatedChunks)
        {
            _spreadingLightDict.Clear();
            int neighborSize = 6;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(neighborSize);
            int attempts = 0;
            Chunk currentTargetChunk;
            LightNode startBlueLightRemovalNode = blueLightRemoveLightBfsQueue.Peek();


            if (Main.Instance.TryGetChunk(startBlueLightRemovalNode.GlobalPosition, out currentTargetChunk))
            {
                updatedChunks.Add(currentTargetChunk);

                Vector3Int relPosition = currentTargetChunk.GetRelativePosition(startBlueLightRemovalNode.GlobalPosition);

                if (currentTargetChunk.GetBlueLight(relPosition.x, relPosition.y, relPosition.z) <= startBlueLightRemovalNode.Invensity)
                    currentTargetChunk.SetBlueLight(relPosition.x, relPosition.y, relPosition.z, 0);
            }


            while (blueLightRemoveLightBfsQueue.Count > 0)
            {
                LightNode currentNode = blueLightRemoveLightBfsQueue.Dequeue();
                GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                for (int i = 0; i < neighborSize; i++)
                {
                    // Check chunk need render or not prevent over use hashing -> improve speed.
                    if (Main.Instance.TryGetChunk(neighbors[i], out currentTargetChunk))
                    {
                        if (!updatedChunks.Contains(currentTargetChunk))
                        {
                            updatedChunks.Add(currentTargetChunk);
                        }

                        Vector3Int relativePos = currentTargetChunk.GetRelativePosition(neighbors[i]);
                        if (currentTargetChunk.GetBlueLight(relativePos.x, relativePos.y, relativePos.z) == 0)
                        {
                            continue;
                        }


                        BlockID currentBlock = currentTargetChunk.GetBlock(relativePos);
                        byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
                        if (currentTargetChunk.GetBlueLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity <= currentNode.Invensity)
                        {
                            LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                            blueLightRemoveLightBfsQueue.Enqueue(neighborNode);
                            currentTargetChunk.SetBlueLight(relativePos.x, relativePos.y, relativePos.z, 0);

                            if (_spreadingLightDict.ContainsKey(neighbors[i]))
                            {
                                _spreadingLightDict.Remove(neighbors[i]);
                            }
                        }
                        else
                        {
                            if (_spreadingLightDict.ContainsKey(neighbors[i]) == false)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], currentTargetChunk.GetBlueLight(relativePos.x, relativePos.y, relativePos.z));
                                _spreadingLightDict.Add(neighborNode.GlobalPosition, neighborNode);
                            }

                        }
                    }
                }

                attempts++;
                if (attempts > 10000)
                {
                    Debug.LogWarning("Infinite loop");
                    break;
                }
            }
            ArrayPool<Vector3Int>.Shared.Return(neighbors);
            foreach (var node in _spreadingLightDict)
            {
                blueLightSpreadingBfsQueue.Enqueue(node.Value);
            }
            if (blueLightSpreadingBfsQueue.Count > 0)
            {
                PropagateBlueLight(blueLightSpreadingBfsQueue, updatedChunks);
            }
        }
        #endregion
        //public static async Task RemoveRedLightAsync(Queue<LightNode> removeLightBfsQueue, HashSet<Chunk> chunksNeedUpdate)
        //{      
        //    Debug.Log("Remove Light");
        //    Main main = Main.Instance;
        //    Queue<LightNode> spreadLightBfsQueue = new Queue<LightNode>();
        //    Dictionary<Vector3Int, LightNode> spreadLightDict = new Dictionary<Vector3Int, LightNode>();
        //    Vector3Int[] neighbors = new Vector3Int[6];

        //    int attempts = 0;
        //    Chunk currentTargetChunk;

        //    LightNode startRemovalNode = removeLightBfsQueue.Peek();
        //    if (main.TryGetChunk(startRemovalNode.GlobalPosition, out currentTargetChunk))
        //    {
        //        chunksNeedUpdate.Add(currentTargetChunk);

        //        main.SetRedLight(removeLightBfsQueue.Peek().GlobalPosition, 0);
        //        Vector3Int relPosition = currentTargetChunk.GetRelativePosition(startRemovalNode.GlobalPosition);

        //        currentTargetChunk.SetRedLight(relPosition.x, relPosition.y, relPosition.z, 0);
        //        currentTargetChunk.SetGreenLight(relPosition.x, relPosition.y, relPosition.z, 0);
        //        currentTargetChunk.SetBlueLight(relPosition.x, relPosition.y, relPosition.z, 0);
        //    }



        //    await Task.Run(() =>
        //    {
        //         red
        //        while (removeLightBfsQueue.Count > 0)
        //        {
        //            LightNode currentNode = removeLightBfsQueue.Dequeue();
        //            GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
        //            for (int i = 0; i < neighbors.Length; i++)
        //            {
        //                 Check chunk need render or not prevent over use hashing -> improve speed.
        //                if (main.TryGetChunk(neighbors[i], out currentTargetChunk))
        //                {
        //                    if (!chunksNeedUpdate.Contains(currentTargetChunk))
        //                    {
        //                        chunksNeedUpdate.Add(currentTargetChunk);
        //                    }

        //                    Vector3Int relativePos = currentTargetChunk.GetRelativePosition(neighbors[i]);
        //                    if (currentTargetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z) == 0)
        //                    {
        //                        continue;
        //                    }

        //                    BlockID currentBlock = currentTargetChunk.GetBlock(relativePos);
        //                    byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
        //                    if (currentTargetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity <= currentNode.Red())
        //                    {
        //                        ushort lightData = currentNode.LightData;
        //                        LightUtils.SetRedLight(ref lightData, (byte)(currentNode.Red() - blockOpacity));
        //                        LightNode neighborNode = new LightNode(neighbors[i], lightData);
        //                        removeLightBfsQueue.Enqueue(neighborNode);
        //                        currentTargetChunk.SetRedLight(relativePos.x, relativePos.y, relativePos.z, 0);

        //                        if (spreadLightDict.ContainsKey(neighbors[i]))
        //                        {
        //                            spreadLightDict.Remove(neighbors[i]);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        LightNode neighborNode = new LightNode(neighbors[i], 0);
        //                        neighborNode.SetRedLight(currentTargetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z));
        //                        if (spreadLightDict.ContainsKey(neighborNode.GlobalPosition) == false)
        //                        {
        //                            spreadLightDict.Add(neighborNode.GlobalPosition, neighborNode);
        //                        }
        //                        else
        //                        {
        //                            spreadLightDict[neighborNode.GlobalPosition].SetRedLight(currentTargetChunk.GetRedLight(relativePos.x, relativePos.y, relativePos.z));
        //                        }
        //                    }
        //                }
        //            }

        //            attempts++;
        //            if (attempts > 10000)
        //            {
        //                Debug.LogWarning("Infinite loop");
        //                break;
        //            }
        //        }


        //         green
        //        removeLightBfsQueue.Enqueue(startRemovalNode);
        //        while (removeLightBfsQueue.Count > 0)
        //        {
        //            LightNode currentNode = removeLightBfsQueue.Dequeue();
        //            GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
        //            for (int i = 0; i < neighbors.Length; i++)
        //            {
        //                 Check chunk need render or not prevent over use hashing -> improve speed.
        //                if (main.TryGetChunk(neighbors[i], out currentTargetChunk))
        //                {
        //                    if (!chunksNeedUpdate.Contains(currentTargetChunk))
        //                    {
        //                        chunksNeedUpdate.Add(currentTargetChunk);
        //                    }

        //                    Vector3Int relativePos = currentTargetChunk.GetRelativePosition(neighbors[i]);
        //                    if (currentTargetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z) == 0)
        //                    {
        //                        continue;
        //                    }

        //                    BlockID currentBlock = currentTargetChunk.GetBlock(relativePos);
        //                    byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
        //                    Debug.Log($"{currentTargetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z)}    {blockOpacity}   {currentNode.Green()}");
        //                    if (currentTargetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z) + blockOpacity <= currentNode.Green())
        //                    {
        //                        ushort lightData = currentNode.LightData;
        //                        LightUtils.SetGreenLight(ref lightData, (byte)(currentNode.Green() - blockOpacity));
        //                        LightNode neighborNode = new LightNode(neighbors[i], lightData);
        //                        removeLightBfsQueue.Enqueue(neighborNode);
        //                        currentTargetChunk.SetGreenLight(relativePos.x, relativePos.y, relativePos.z, 0);

        //                        if (spreadLightDict.ContainsKey(neighbors[i]))
        //                        {
        //                            spreadLightDict.Remove(neighbors[i]);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        LightNode neighborNode = new LightNode(neighbors[i], 0);
        //                        neighborNode.SetGreenLight(currentTargetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z));
        //                        if (spreadLightDict.ContainsKey(neighborNode.GlobalPosition) == false)
        //                        {
        //                            spreadLightDict.Add(neighborNode.GlobalPosition, neighborNode);
        //                        }
        //                        else
        //                        {
        //                            spreadLightDict[neighborNode.GlobalPosition].SetGreenLight(currentTargetChunk.GetGreenLight(relativePos.x, relativePos.y, relativePos.z));
        //                        }
        //                    }
        //                }
        //            }

        //            attempts++;
        //            if (attempts > 10000)
        //            {
        //                Debug.LogWarning("Infinite loop");
        //                break;
        //            }
        //        }
        //    });


        //    Debug.Log($"Spread light dictionary: {spreadLightDict.Count}");
        //    foreach (var node in spreadLightDict)
        //    {
        //        spreadLightBfsQueue.Enqueue(node.Value);
        //    }
        //    if (spreadLightBfsQueue.Count > 0)
        //    {
        //        await PropagateBlockLightAsync(spreadLightBfsQueue, chunksNeedUpdate);
        //        PropagateBlockLightAsync(spreadLightBfsQueue, chunksNeedUpdate);
        //    }
        //}







        #region Ambient Light   
        public async Task SpreadAmbientLightTask(Chunk chunk)
        {
            //Debug.Log("SpreadAmbientLightTask");
            int attempts = 0;
            Main main = Main.Instance;
            Vector3Int[] neighbors = new Vector3Int[6];

            await Task.Run(() =>
            {
                while (chunk.AmbientLightBfsQueue.Count > 0)
                {
                    if (chunk.AmbientLightBfsQueue.TryDequeue(out LightNode currentNode))
                    {
                        GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                        for (int i = 0; i < neighbors.Length; i++)
                        {
                            if (main.InSideChunkBound(chunk, neighbors[i]))
                            {
                                Vector3Int relativePos = chunk.GetRelativePosition(neighbors[i]);
                                BlockID currentBlock = chunk.GetBlock(relativePos);
                                byte blockOpacity;
                                if (currentBlock == BlockID.Air && i == 5)
                                {
                                    blockOpacity = 0;
                                }
                                else
                                {
                                    blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
                                }


                                if (chunk.GetAmbientLight(relativePos) + blockOpacity < currentNode.Invensity && currentNode.Invensity > 0)
                                {
                                    LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                    chunk.AmbientLightBfsQueue.Enqueue(neighborNode);
                                    chunk.SetAmbientLight(relativePos, neighborNode.Invensity);
                                }
                            }
                            else
                            {

                                BlockID currentBlock = Main.Instance.GetBlock(neighbors[i]);
                                byte blockOpacity;
                                if (currentBlock == BlockID.Air && i == 5)
                                {
                                    blockOpacity = 0;
                                }
                                else
                                {
                                    blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
                                }

                                if (main.GetAmbientLight(neighbors[i]) + blockOpacity < currentNode.Invensity && currentNode.Invensity > 0)
                                {
                                    if (main.TryGetChunk(neighbors[i], out Chunk neighborChunk))
                                    {
                                        LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                        neighborChunk.SetAmbientLight(neighborChunk.GetRelativePosition(neighbors[i]), neighborNode.Invensity);
                                    }
                                }
                            }
                        }
                    }


                    attempts++;
                    if (attempts > 50000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });
        }
        public static async Task PropagateAmbientLightAsync(Queue<LightNode> lightBfsQueue)
        {
            //Debug.Log("Propagate ambient light");
            int attempts = 0;
            Main main = Main.Instance;
            HashSet<Chunk> chunksNeedUpdate = new();
            Vector3Int[] neighbors = new Vector3Int[6];
            LightNode startNode = lightBfsQueue.Peek();
            main.SetAmbientLight(startNode.GlobalPosition, startNode.Invensity);

            Chunk currentTargetChunk;
            main.TryGetChunk(startNode.GlobalPosition, out currentTargetChunk);
            chunksNeedUpdate.Add(currentTargetChunk);

            await Task.Run(() =>
            {
                while (lightBfsQueue.Count > 0)
                {
                    LightNode currentNode = lightBfsQueue.Dequeue();
                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        if (main.TryGetChunk(neighbors[i], out currentTargetChunk))
                        {
                            if (!chunksNeedUpdate.Contains(currentTargetChunk))
                            {
                                chunksNeedUpdate.Add(currentTargetChunk);
                            }

                            Vector3Int relativePos = currentTargetChunk.GetRelativePosition(neighbors[i]);
                            BlockID currentBlock = currentTargetChunk.GetBlock(relativePos);
                            byte blockOpacity;
                            if (currentBlock == BlockID.Air && i == 5)
                            {
                                blockOpacity = 0;
                            }
                            else
                            {
                                blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
                            }


                            if (currentTargetChunk.GetAmbientLight(relativePos) + blockOpacity < currentNode.Invensity && currentNode.Invensity > 0)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                lightBfsQueue.Enqueue(neighborNode);
                                currentTargetChunk.SetAmbientLight(relativePos, neighborNode.Invensity);
                            }
                        }
                    }

                    attempts++;
                    if (attempts > 10000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });

            foreach (var c in chunksNeedUpdate)
            {
                c.UpdateMask |= UpdateChunkMask.RenderAll;
            }
        }
        public static async Task RemoveAmbientLightAsync(Queue<LightNode> removeLightBfsQueue)
        {
            Main main = Main.Instance;
            main.SetAmbientLight(removeLightBfsQueue.Peek().GlobalPosition, 0);
            Queue<LightNode> spreadLightBfsQueue = new Queue<LightNode>();
            Dictionary<Vector3Int, LightNode> spreadLightDict = new Dictionary<Vector3Int, LightNode>();
            int attempts = 0;
            Vector3Int[] neighbors = new Vector3Int[6];
            HashSet<Chunk> chunksNeedUpdate = new();

            Chunk currentTargetChunk;
            main.TryGetChunk(removeLightBfsQueue.Peek().GlobalPosition, out currentTargetChunk);
            chunksNeedUpdate.Add(currentTargetChunk);




            await Task.Run(() =>
            {
                while (removeLightBfsQueue.Count > 0)
                {
                    LightNode currentNode = removeLightBfsQueue.Dequeue();

                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        if (main.TryGetChunk(neighbors[i], out currentTargetChunk))
                        {

                            Vector3Int relativePos = currentTargetChunk.GetRelativePosition(neighbors[i]);
                            if (!chunksNeedUpdate.Contains(currentTargetChunk))
                            {
                                chunksNeedUpdate.Add(currentTargetChunk);
                            }

                            if (currentTargetChunk.GetAmbientLight(relativePos) == 0)
                            {
                                continue;
                            }



                            BlockID currentBlock = currentTargetChunk.GetBlock(relativePos);
                            byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];

                            if (currentTargetChunk.GetAmbientLight(relativePos) + blockOpacity <= currentNode.Invensity)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                removeLightBfsQueue.Enqueue(neighborNode);
                                currentTargetChunk.SetAmbientLight(relativePos, 0);

                                if (spreadLightDict.ContainsKey(neighbors[i]))
                                {
                                    spreadLightDict.Remove(neighbors[i]);
                                }
                            }
                            else
                            {

                                LightNode neighborNode = new LightNode(neighbors[i], currentTargetChunk.GetAmbientLight(relativePos));

                                if (spreadLightDict.ContainsKey(neighborNode.GlobalPosition) == false)
                                {
                                    spreadLightDict.Add(neighborNode.GlobalPosition, neighborNode);
                                }
                                //else
                                //{
                                //    spreadLightDict[neighborNode.GlobalPosition] = neighborNode.SunLight();
                                //}
                            }
                        }
                    }


                    attempts++;
                    if (attempts > 10000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });


            foreach (var node in spreadLightDict)
            {
                spreadLightBfsQueue.Enqueue(node.Value);
            }
            if (spreadLightBfsQueue.Count > 0)
            {
                await PropagateAmbientLightAsync(spreadLightBfsQueue);
            }

            foreach (var c in chunksNeedUpdate)
            {
                c.UpdateMask |= UpdateChunkMask.RenderAll;
            }
        }




        public static async Task PropagateAmbientLightTask(Queue<LightNode> lightBfsQueue, HashSet<Chunk> updatedChunks)
        {
            //Debug.Log("Propagate ambient light");
            int attempts = 0;
            updatedChunks.Clear();
            Main main = Main.Instance;
            int neighborSize = 6;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(neighborSize);
            LightNode startNode = lightBfsQueue.Peek();
            main.SetAmbientLight(startNode.GlobalPosition, startNode.Invensity);

            Chunk currentTargetChunk;
            main.TryGetChunk(startNode.GlobalPosition, out currentTargetChunk);
            updatedChunks.Add(currentTargetChunk);

            await Task.Run(() =>
            {
                while (lightBfsQueue.Count > 0)
                {
                    LightNode currentNode = lightBfsQueue.Dequeue();
                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                    for (int i = 0; i < neighborSize; i++)
                    {
                        if (main.TryGetChunk(neighbors[i], out currentTargetChunk))
                        {
                            if (!updatedChunks.Contains(currentTargetChunk))
                            {
                                updatedChunks.Add(currentTargetChunk);
                            }

                            Vector3Int relativePos = currentTargetChunk.GetRelativePosition(neighbors[i]);
                            BlockID currentBlock = currentTargetChunk.GetBlock(relativePos);
                            byte blockOpacity;
                            if (currentBlock == BlockID.Air && i == 5)
                            {
                                blockOpacity = 0;
                            }
                            else
                            {
                                blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];
                            }


                            if (currentTargetChunk.GetAmbientLight(relativePos) + blockOpacity < currentNode.Invensity && currentNode.Invensity > 0)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                lightBfsQueue.Enqueue(neighborNode);
                                currentTargetChunk.SetAmbientLight(relativePos, neighborNode.Invensity);
                            }
                        }
                    }

                    attempts++;
                    if (attempts > 10000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }
                }
            });
            
            ArrayPool<Vector3Int>.Shared.Return(neighbors);
        }
        public static async Task RemoveAmbientLightTask(Queue<LightNode> removeLightBfsQueue, Queue<LightNode> spreadLightBfsQueue, HashSet<Chunk> updatedChunks)
        {
            updatedChunks.Clear();
            _spreadingLightDict.Clear();
            Main.Instance.SetAmbientLight(removeLightBfsQueue.Peek().GlobalPosition, 0);
            int attempts = 0;
            int neighborSize = 6;
            Vector3Int[] neighbors = ArrayPool<Vector3Int>.Shared.Rent(neighborSize);

            Chunk currentTargetChunk;
            Main.Instance.TryGetChunk(removeLightBfsQueue.Peek().GlobalPosition, out currentTargetChunk);
            updatedChunks.Add(currentTargetChunk);

            await Task.Run(() =>
            {
                while (removeLightBfsQueue.Count > 0)
                {
                    LightNode currentNode = removeLightBfsQueue.Dequeue();
                    GetVoxelNeighborPosition(currentNode.GlobalPosition, ref neighbors);
                    for (int i = 0; i < neighborSize; i++)
                    {
                        if (Main.Instance.TryGetChunk(neighbors[i], out currentTargetChunk))
                        {

                            Vector3Int relativePos = currentTargetChunk.GetRelativePosition(neighbors[i]);
                            if (!updatedChunks.Contains(currentTargetChunk))
                            {
                                updatedChunks.Add(currentTargetChunk);
                            }

                            if (currentTargetChunk.GetAmbientLight(relativePos) == 0)
                            {
                                continue;
                            }



                            BlockID currentBlock = currentTargetChunk.GetBlock(relativePos);
                            byte blockOpacity = LightUtils.BlocksLightResistance[(byte)currentBlock];

                            if (currentTargetChunk.GetAmbientLight(relativePos) + blockOpacity <= currentNode.Invensity)
                            {
                                LightNode neighborNode = new LightNode(neighbors[i], (byte)(currentNode.Invensity - blockOpacity));
                                removeLightBfsQueue.Enqueue(neighborNode);
                                currentTargetChunk.SetAmbientLight(relativePos, 0);

                                if (_spreadingLightDict.ContainsKey(neighbors[i]))
                                {
                                    _spreadingLightDict.Remove(neighbors[i]);
                                }
                            }
                            else
                            {

                                LightNode neighborNode = new LightNode(neighbors[i], currentTargetChunk.GetAmbientLight(relativePos));

                                if (_spreadingLightDict.ContainsKey(neighborNode.GlobalPosition) == false)
                                {
                                    _spreadingLightDict.Add(neighborNode.GlobalPosition, neighborNode);
                                }
                            }
                        }
                    }


                    attempts++;
                    if (attempts > 10000)
                    {
                        Debug.LogWarning("Infinite loop");
                        break;
                    }

                }
            });
           

            ArrayPool<Vector3Int>.Shared.Return(neighbors);
            foreach (var node in _spreadingLightDict)
            {
                spreadLightBfsQueue.Enqueue(node.Value);
            }
            if (spreadLightBfsQueue.Count > 0)
            {
                await PropagateAmbientLightTask(spreadLightBfsQueue, updatedChunks);
            }
        }
        #endregion






        #region Neighbors
        public static void GetVoxelNeighborPosition(Vector3Int position, ref Vector3Int[] neighborPosition)
        {
            neighborPosition[0] = new Vector3Int(position.x + 1, position.y, position.z);
            neighborPosition[1] = new Vector3Int(position.x - 1, position.y, position.z);
            neighborPosition[2] = new Vector3Int(position.x, position.y, position.z + 1);
            neighborPosition[3] = new Vector3Int(position.x, position.y, position.z - 1);
            neighborPosition[4] = new Vector3Int(position.x, position.y + 1, position.z);
            neighborPosition[5] = new Vector3Int(position.x, position.y - 1, position.z);


            //_neighborsPosition[6] = position + new Vector3Int(-1, 0, 1);
            //_neighborsPosition[7] = position + new Vector3Int(1, 0, 1);
            //_neighborsPosition[8] = position + new Vector3Int(-1, 0, -1);
            //_neighborsPosition[9] = position + new Vector3Int(1, 0, -1);


            //return _neighborsPosition;
        }
        #endregion
    }
}
