using System.Collections.Generic;
using PixelMiner.Core;
namespace PixelMiner.Physics
{
    public class MyPhysicData
    {
        public HashSet<Chunk> PotentialChunkCollided = new();
        public List<CustomBoxCollider> PotientialBoxesCollided = new();
        public MyPhysicData()
        {
            PotentialChunkCollided = new();
            PotientialBoxesCollided = new();
        }
        public void Clear()
        {
            PotentialChunkCollided.Clear();
            PotientialBoxesCollided.Clear();
        }
    }

    public static class MyPhysicDataPool
    {
        public static bool CollectionChecks = true;
        public static int MaxPoolSize = 16;

        private static UnityEngine.Pool.ObjectPool<MyPhysicData> _pool;
        public static UnityEngine.Pool.ObjectPool<MyPhysicData> Pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new UnityEngine.Pool.ObjectPool<MyPhysicData>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, maxSize: MaxPoolSize);
                }
                return _pool;
            }
        }

        private static MyPhysicData CreatePooledItem()
        {
            MyPhysicData data = new MyPhysicData();
            return data;
        }


        // Called when an item is returned to the pool using Release
        private static void OnReturnedToPool(MyPhysicData data)
        {
            data.Clear();
        }

        // Called when an item is taken from the pool using Get
        private static void OnTakeFromPool(MyPhysicData data)
        {

        }


        private static void OnDestroyPoolObject(MyPhysicData data)
        {

        }
    }
}
