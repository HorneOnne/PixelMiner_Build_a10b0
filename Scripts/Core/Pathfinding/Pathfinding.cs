using System.Collections.Generic;
using UnityEngine;
using PixelMiner.DataStructure;
using TMPro;
using PixelMiner.Enums;
using PixelMiner.Extensions;
using System.Threading.Tasks;
using System.Threading;
namespace PixelMiner.Core
{


    /// <summary>
    /// Warning: Pathfinding cannot work in parallel.
    /// </summary>
    public class Pathfinding : MonoBehaviour
    {
        private static Main _main;


        private void Start()
        {
            _main = Main.Instance;

        }


        public static async Task<bool> FindPathTask(PathRequest request, int maxAttempt)
        {
            //Debug.Log($"PathFinding: {startPosition}      {targetPosition}");

            int loopCount = 0;
            AStarNode startNode;
            AStarNode targetNode;

            if (TryGetPathfindingNode(request, request.StartPosition.ToVector3Int(), out startNode) == false)
            {
                return false;
            }
            if (TryGetPathfindingNode(request, request.TargetPosition.ToVector3Int(), out targetNode) == false)
            {
                return false;
            }

            request.OpenQueue.Enqueue(startNode, startNode);
            int totalWorkCount = 0;
            bool foundPath = false;
            await Task.Run(() =>
            {
                while (request.OpenQueue.Count > 0)
                {
                    // Get lowest fcost from OPEN list. 
                    // Then remove from OPEN, add to CLOSED
                    AStarNode currNode = request.OpenQueue.Dequeue();
                    request.ClosedSet.Add(currNode.GlobalPosition);

                    //Debug.Log($"A: {currNode.WorldPos}  {targetNode.WorldPos}");
                    if (currNode.GlobalPosition == targetNode.GlobalPosition)
                    {
                        if (TryGetPathfindingNode(request, startNode.GlobalPosition, out startNode) == false)
                        {
                            //return false;
                            break;
                        }
                        if (TryGetPathfindingNode(request, targetNode.GlobalPosition, out targetNode) == false)
                        {
                            //return false;
                            break;
                        }
                        Retracepath(request, startNode, targetNode);

                        foundPath = true;
                        break;
                    }

                    GetNeighbors(currNode.GlobalPosition, ref request.Neighbors);
                    foreach (var nb in request.Neighbors)
                    {
                        BlockID blockID = Main.Instance.GetBlock(nb);
                        if (blockID.Walkable() == false || request.ClosedSet.Contains(nb)) continue;

                        float newGCostToNeighbors = (currNode.GCost + GetEuclideanDistance(currNode.GlobalPosition, nb));
                        if (!TryGetPathfindingNode(request, nb, out AStarNode nbNode))
                        {
                            continue;
                        }


                        if (newGCostToNeighbors < nbNode.GCost || request.OpenSet.Contains(nb) == false)
                        {
                            nbNode.GCost = newGCostToNeighbors;
                            nbNode.HCost = GetEuclideanDistance(nb, targetNode.GlobalPosition);

                            var newNode = new AStarNode()
                            {
                                GlobalPosition = nb,
                                Walkable = nbNode.Walkable,
                                GCost = newGCostToNeighbors,
                                HCost = GetEuclideanDistance(nb, targetNode.GlobalPosition),
                                ParentGlobalPos = currNode.GlobalPosition,
                            };

                            //GridNode[nb.x + nb.y * _width] = newNode;
                            SetPathfindingNode(request, newNode);

                            if (request.OpenSet.Contains(nb) == false)
                            {
                                request.OpenSet.Add(nb);
                                request.OpenQueue.Enqueue(nbNode, nbNode);
                            }

                        }
                        totalWorkCount++;
                    }

                    loopCount++;
                    if (loopCount > maxAttempt)
                    {
                        Debug.LogWarning("Oh nooo! I try to find the path but i can't");
                        break;
                    }
                }
            });



            return foundPath;
        }




        private static void Retracepath(PathRequest request, AStarNode startNode, AStarNode endNode, bool reverse = true)
        {
            //Debug.Log($"Retracepath: from {startNode}  to   {endNode}");
            request.Path.Clear();
            Vector3 offsetBlockPos = new Vector3(0.5f, 0f, 0.5f);
            AStarNode currNode = endNode;
            int loopCount = 0;

            while (currNode.GlobalPosition.Equals(startNode.GlobalPosition) == false)
            {
                request.Path.Add(currNode.GlobalPosition + offsetBlockPos);
                TryGetPathfindingNode(request, currNode.ParentGlobalPos, out currNode);

                loopCount++;
                if (loopCount > 10000)
                {
                    Debug.Log("Loop count > 10000");
                    break;
                }
            }
            request.Path.Add(request.StartPosition);
            SimplifyPath(request);

            // Reverse the sequence of moves to get the correct order
            //if (reverse)
            //{
            //    //request.Path.Reverse();
            //    request.SimplifyPath.Reverse();
            //}

        }
        private static void SimplifyPath(PathRequest request)
        {
            Vector3 currDir = default;
            for (int i = 0; i < request.Path.Count - 1; i++)
            {
                Vector3 nextDir = (request.Path[i + 1] - request.Path[i]).normalized;
                if (currDir != nextDir)
                {
                    currDir = nextDir;
                    request.SimplifyPath.Add(request.Path[i]);
                }
            }
            request.SimplifyPath.Add(request.Path[request.Path.Count - 1]);
        }


        #region Heuristic distance (Hcost)


        public static float GetManhattanDistance(Vector3Int a, Vector3Int b)
        {
            return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z));
        }

        public static float GetEuclideanDistance(Vector3Int a, Vector3Int b)
        {
            return Vector3.SqrMagnitude(b - a);
            //return Vector3.Distance(a, b);
        }
        public static float GetChebyshevDistance(Vector3Int a, Vector3Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y), Mathf.Abs(a.z - b.z));
        }
        #endregion



        public static void GetNeighbors(Vector3Int position, ref List<Vector3Int> neighbors)
        {
            //Vector3Int up = position + new Vector3Int(0, 1, 0);
            //Vector3Int down = position + new Vector3Int(0, -1, 0);
            //Vector3Int w = position + new Vector3Int(-1, 0, 0);
            //Vector3Int e = position + new Vector3Int(1, 0, 0);
            //Vector3Int n = position + new Vector3Int(0, 0, 1);
            //Vector3Int s = position + new Vector3Int(0, 0, -1);
            //Vector3Int nw = position + new Vector3Int(-1, 0, 1);
            //Vector3Int ne = position + new Vector3Int(1, 0, 1);
            //Vector3Int sw = position + new Vector3Int(-1, 0, -1);
            //Vector3Int se = position + new Vector3Int(1, 0, -1);

            Vector3Int up = new Vector3Int(position.x, position.y + 1, position.z);
            Vector3Int down = new Vector3Int(position.x, position.y - 1, position.z);
            Vector3Int w = new Vector3Int(position.x - 1, position.y, position.z);
            Vector3Int e = new Vector3Int(position.x + 1, position.y, position.z);
            Vector3Int n = new Vector3Int(position.x, position.y, position.z + 1);
            Vector3Int s = new Vector3Int(position.x, position.y, position.z - 1);
            Vector3Int nw = new Vector3Int(position.x - 1, position.y, position.z + 1);
            Vector3Int ne = new Vector3Int(position.x + 1, position.y, position.z + 1);
            Vector3Int sw = new Vector3Int(position.x - 1, position.y, position.z - 1);
            Vector3Int se = new Vector3Int(position.x + 1, position.y, position.z - 1);

            neighbors.Clear();


            neighbors.Add(down);
            neighbors.Add(up);
            neighbors.Add(w);
            neighbors.Add(e);
            neighbors.Add(n);
            neighbors.Add(s);

            BlockID nBlockID = _main.GetBlock(n);
            BlockID sBlockID = _main.GetBlock(s);
            BlockID wBlockID = _main.GetBlock(w);
            BlockID eBlockID = _main.GetBlock(e);

            if (nBlockID.Walkable() && wBlockID.Walkable())
            {
                neighbors.Add(nw);
            }
            if (nBlockID.Walkable() && eBlockID.Walkable())
            {
                neighbors.Add(ne);
            }
            if (sBlockID.Walkable() && wBlockID.Walkable())
            {
                neighbors.Add(sw);
            }
            if (sBlockID.Walkable() && eBlockID.Walkable())
            {
                neighbors.Add(se);
            }
        }



        public static bool TryGetPathfindingNode(PathRequest requset, Vector3Int globalPosition, out AStarNode node)
        {
            if (requset.AstarNodeDict.ContainsKey(globalPosition))
            {
                node = requset.AstarNodeDict[globalPosition];
                return true;
            }
            else
            {
                if (_main.TryGetChunk(globalPosition, out var chunk))
                {
                    BlockID blockID = chunk.GetBlock(_main.GlobalToRelativeBlockPosition(globalPosition));
                    bool walkable = blockID.Walkable();


                    node = new AStarNode()
                    {
                        GlobalPosition = globalPosition,
                        Walkable = walkable,
                    };
                    requset.AstarNodeDict.Add(globalPosition, node);
                    return true;
                }
            }

            node = default;
            return false;
        }


        public static void SetPathfindingNode(PathRequest requset, AStarNode node)
        {
            if (requset.AstarNodeDict.ContainsKey(node.GlobalPosition))
            {
                requset.AstarNodeDict[node.GlobalPosition] = node;
            }
            else
            {
                if (_main.TryGetChunk(node.GlobalPosition, out var chunk))
                {
                    requset.AstarNodeDict.Add(node.GlobalPosition, node);
                }
            }
        }


        private void OnDrawGizmos()
        {
            //if (Application.isPlaying)
            //{
            //    Vector3 offsetCenter = Vector3.one * 0.5f;
            //    if (SimpPath.Count > 0)
            //    {
            //        for (int i  = 0; i < SimpPath.Count - 1; i++)
            //        {
            //            Gizmos.DrawLine(SimpPath[i] + offsetCenter, SimpPath[i + 1] + offsetCenter);
            //        }


            //        Gizmos.color = Color.cyan;
            //        for (int i = 0; i < SimpPath.Count; i++)
            //        {
            //            Gizmos.DrawCube(SimpPath[i] + offsetCenter, new Vector3(0.5f, 0.5f, 0.5f));
            //        }
            //    }
            //}
        }
    }
}
