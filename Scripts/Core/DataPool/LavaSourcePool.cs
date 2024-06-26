namespace PixelMiner.Core
{
    public static class LavaSourcePool
    {
        public static int MaxPoolSize = 30;

        private static UnityEngine.Pool.ObjectPool<LavaSource> _pool;
        public static UnityEngine.Pool.ObjectPool<LavaSource> Pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new UnityEngine.Pool.ObjectPool<LavaSource>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, maxSize: MaxPoolSize);
                }
                return _pool;
            }
        }

        private static LavaSource CreatePooledItem()
        {
            LavaSource lavaSource = new LavaSource();
            return lavaSource;
        }


        // Called when an item is returned to the pool using Release
        private static void OnReturnedToPool(LavaSource lavaSource)
        {
            lavaSource.Clear();
        }

        // Called when an item is taken from the pool using Get
        private static void OnTakeFromPool(LavaSource lavaSource)
        {

        }


        private static void OnDestroyPoolObject(LavaSource lavaSource)
        {

        }

        public static void Reset()
        {
            if(_pool != null )
            {
                _pool.Dispose();
                _pool.Clear();
            }
           
        }
    }
}
