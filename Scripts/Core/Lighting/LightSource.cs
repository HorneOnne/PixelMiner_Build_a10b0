using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelMiner.Core
{
    [System.Serializable]
    public class LightSource
    {
        public Queue<LightNode> RedLightSpreadingBfsQueue;
        public Queue<LightNode> GreenLightSpreadingBfsQueue;
        public Queue<LightNode> BlueLightSpreadingBfsQueue;
        public Queue<LightNode> RedLightRemovalBfsQueue;
        public Queue<LightNode> GreenLightRemovalBfsQueue;
        public Queue<LightNode> BlueLightRemovalBfsQueue;
        public Queue<LightNode> AmbientLightBfsQueue;
        public Queue<LightNode> AmbientLightRemovalBfsQueue;

        public HashSet<Chunk> SpreadingChunkEffected;
        public HashSet<Chunk> RemovalChunkEffected;

        public LightSource()
        {
            RedLightSpreadingBfsQueue = new(3000);
            GreenLightSpreadingBfsQueue = new(3000);
            BlueLightSpreadingBfsQueue = new(3000);
            RedLightRemovalBfsQueue = new(3000);
            GreenLightRemovalBfsQueue = new(3000);
            BlueLightRemovalBfsQueue = new(3000);
            AmbientLightBfsQueue = new(100);
            AmbientLightRemovalBfsQueue = new(100);

            SpreadingChunkEffected = new(4);
            RemovalChunkEffected = new(4);
        }

        public bool IsEmpty()
        {
            return !(RedLightSpreadingBfsQueue.Count > 0 ||
                    GreenLightSpreadingBfsQueue.Count > 0 ||
                    BlueLightSpreadingBfsQueue.Count > 0 ||
                    RedLightRemovalBfsQueue.Count > 0 ||
                    GreenLightRemovalBfsQueue.Count > 0 ||
                    BlueLightRemovalBfsQueue.Count > 0 ||
                    AmbientLightBfsQueue.Count > 0 ||
                    AmbientLightRemovalBfsQueue.Count > 0 ||
                    SpreadingChunkEffected.Count > 0 ||
                    RemovalChunkEffected.Count > 0);
        }

        public void Clear()
        {
            RedLightSpreadingBfsQueue.Clear();
            GreenLightSpreadingBfsQueue.Clear();
            BlueLightSpreadingBfsQueue.Clear();
            RedLightRemovalBfsQueue.Clear();
            GreenLightRemovalBfsQueue.Clear();
            BlueLightRemovalBfsQueue.Clear();
            AmbientLightBfsQueue = new(100);
            AmbientLightRemovalBfsQueue = new(100);
        }
    }
}
