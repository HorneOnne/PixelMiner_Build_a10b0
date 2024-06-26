namespace PixelMiner.Core
{
    public static class PathRequestPool
    {
        public static bool CollectionChecks = true;
        public static int MaxPoolSize = 10;

        private static UnityEngine.Pool.ObjectPool<PathRequest> _pool;
        public static UnityEngine.Pool.ObjectPool<PathRequest> Pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new UnityEngine.Pool.ObjectPool<PathRequest>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, maxSize: MaxPoolSize);
                }
                return _pool;
            }
        }

        private static PathRequest CreatePooledItem()
        {
            PathRequest request = new PathRequest();
            return request;
        }


        // Called when an item is returned to the pool using Release
        private static void OnReturnedToPool(PathRequest request)
        {
            request.Clear();
        }

        // Called when an item is taken from the pool using Get
        private static void OnTakeFromPool(PathRequest request)
        {

        }

   
        private static void OnDestroyPoolObject(PathRequest request)
        {
          
        }
    }

}
