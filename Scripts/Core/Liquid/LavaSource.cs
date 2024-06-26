using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PixelMiner.Core
{
    [System.Serializable]
    public class LavaSource
    {
        // lava spreading/removal
        public Queue<FluidNode> LavaSpreadingBfsQueue;
        public Queue<FluidNode> LavaRemovalBfsQueue;

        public List<Vector3Int> NewLavaSpreadingPositions;
        public HashSet<Chunk> SpreadingChunkEffected;  
        public HashSet<Chunk> RemovalChunkEffected;  

        public LavaSource()
        {
            LavaSpreadingBfsQueue = new();
            LavaRemovalBfsQueue = new();
            NewLavaSpreadingPositions = new(50);
            SpreadingChunkEffected = new();
            RemovalChunkEffected = new();
        }

        public bool IsEmpty()
        {
            return !(LavaSpreadingBfsQueue.Count > 0 ||
                LavaRemovalBfsQueue.Count > 0 ||
                NewLavaSpreadingPositions.Count > 0);
        }

        public void Clear()
        {
            // lava spreading/removal
            LavaSpreadingBfsQueue.Clear();
            LavaRemovalBfsQueue.Clear();

            NewLavaSpreadingPositions.Clear();
            SpreadingChunkEffected.Clear();
            RemovalChunkEffected.Clear();
        }

    }
}
