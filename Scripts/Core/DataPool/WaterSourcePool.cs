namespace PixelMiner.Core
{
    public static class WaterSourcePool
    {
        public static int MaxPoolSize = 30;

        private static UnityEngine.Pool.ObjectPool<WaterSource> _pool;
        public static UnityEngine.Pool.ObjectPool<WaterSource> Pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new UnityEngine.Pool.ObjectPool<WaterSource>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, maxSize: MaxPoolSize);
                }
                return _pool;
            }
        }

        private static WaterSource CreatePooledItem()
        {
            WaterSource waterSource = new WaterSource();
            return waterSource;
        }


        // Called when an item is returned to the pool using Release
        private static void OnReturnedToPool(WaterSource waterSource)
        {
            waterSource.Clear();
        }

        // Called when an item is taken from the pool using Get
        private static void OnTakeFromPool(WaterSource waterSource)
        {

        }


        private static void OnDestroyPoolObject(WaterSource waterSource)
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
