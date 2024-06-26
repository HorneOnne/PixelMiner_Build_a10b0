using System.Collections.Generic;
using UnityEngine;

namespace PixelMiner.Core
{
    [System.Serializable]
    public class WaterSource
    {
        public Queue<FluidNode> WaterSpreadingBfsQueue;
        public Queue<FluidNode> WaterRemovalBfsQueue;
        public HashSet<Chunk> ChunkEffected;    // cached chunk for render after finish.

        public WaterSource()
        {
            WaterSpreadingBfsQueue = new();
            WaterRemovalBfsQueue = new();
            ChunkEffected = new();
        }

        public bool IsEmpty()
        {
            return !(WaterSpreadingBfsQueue.Count > 0 || WaterRemovalBfsQueue.Count > 0);
        }

        public void Clear()
        {
            WaterSpreadingBfsQueue.Clear();
            WaterRemovalBfsQueue.Clear();
            ChunkEffected.Clear();
        }
    }
}
