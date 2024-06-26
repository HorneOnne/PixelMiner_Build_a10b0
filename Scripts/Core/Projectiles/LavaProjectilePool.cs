using UnityEngine;

namespace PixelMiner.Core
{
    public static class LavaProjectilePool
    {
        public static int MaxPoolSize = 25;

        private static UnityEngine.Pool.ObjectPool<LavaProjectile> _pool;
        public static UnityEngine.Pool.ObjectPool<LavaProjectile> Pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new UnityEngine.Pool.ObjectPool<LavaProjectile>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, maxSize: MaxPoolSize);
                }
                return _pool;
            }
        }

        private static LavaProjectile CreatePooledItem()
        {
            var projectilePrefab = Resources.Load<LavaProjectile>("Particles/LavaProjectile");
            return Object.Instantiate(projectilePrefab);
        }


        // Called when an item is returned to the pool using Release
        private static void OnReturnedToPool(LavaProjectile lavaProjectile)
        {
            lavaProjectile.ResetProjectile();
            lavaProjectile.gameObject.SetActive(false);
        }

        // Called when an item is taken from the pool using Get
        private static void OnTakeFromPool(LavaProjectile lavaProjectile)
        {
           
        }


        private static void OnDestroyPoolObject(LavaProjectile lavaProjectile)
        {
            //Object.Destroy(lavaProjectile.gameObject);
        }


        public static void Reset()
        {
            Debug.Log("Reset lava projectile pool");
            if(_pool != null)
            {
                _pool.Dispose();
                _pool.Clear();
            }
          
        }
    }
}
