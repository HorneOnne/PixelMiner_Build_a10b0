using PixelMiner.DataStructure;

namespace PixelMiner.Core
{
    public static class ChunkDataPool
    {
        //public static ObjectPool<ChunkGenData> Pool = new ObjectPool<ChunkGenData>(10);

        //public static ChunkGenData Get()
        //{
        //    return Pool.Get();
        //}

        //public static void Release(ChunkGenData chunkData)
        //{
        //    chunkData.Reset();
        //    Pool.Release(chunkData);
        //}



        public static bool CollectionChecks = true;
        public static int MaxPoolSize = 10;

        private static UnityEngine.Pool.ObjectPool<ChunkGenData> _pool;
        public static UnityEngine.Pool.ObjectPool<ChunkGenData> Pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new UnityEngine.Pool.ObjectPool<ChunkGenData>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, maxSize: MaxPoolSize);
                }
                return _pool;
            }
        }

        private static ChunkGenData CreatePooledItem()
        {
            ChunkGenData data = new ChunkGenData();
            data.Init(Main.Instance.ChunkDimension.x, Main.Instance.ChunkDimension.y, Main.Instance.ChunkDimension.z);
            return data;
        }


        // Called when an item is returned to the pool using Release
        private static void OnReturnedToPool(ChunkGenData data)
        {
            data.Reset();
        }

        // Called when an item is taken from the pool using Get
        private static void OnTakeFromPool(ChunkGenData data)
        {

        }


        private static void OnDestroyPoolObject(ChunkGenData data)
        {

        }
    }
}
