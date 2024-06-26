using UnityEngine;
using System.Collections.Generic;
using PixelMiner.DataStructure;

namespace PixelMiner.Core
{
    public class PathRequest 
    {
        public event System.Action<bool> OnRequestCompleted;

        public Vector3 StartPosition;
        public Vector3 TargetPosition;
       

        // Data
        public Dictionary<Vector3Int, AStarNode> AstarNodeDict;
        public PriorityQueue<AStarNode, AStarNode> OpenQueue;
        public HashSet<Vector3Int> OpenSet;
        public HashSet<Vector3Int> ClosedSet;
        public List<Vector3> Path;
        public List<Vector3> SimplifyPath;
        public List<Vector3Int> Neighbors;

        public bool Found = false;

        public PathRequest()
        {
            AstarNodeDict = new();
            Path = new();
            SimplifyPath = new();
            OpenQueue = new(NodeComparer.Instance);
            OpenSet = new();
            ClosedSet = new();
            Neighbors = new(8);
        }

        public void SetPath(Vector3 start, Vector3 end)
        {
            this.StartPosition = start;
            this.TargetPosition = end;
        }

        public void OnRequestComplete(bool success)
        {
            OnRequestCompleted?.Invoke(success);
        }

        public void Clear()
        {
            AstarNodeDict.Clear();
            Path.Clear();
            SimplifyPath.Clear();
            OpenQueue.Clear();
            OpenSet.Clear();
            ClosedSet.Clear();
            Neighbors.Clear();
            StartPosition = default;
            TargetPosition = default;
            Found = false;
        }
    }

}
