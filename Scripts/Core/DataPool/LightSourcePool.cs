using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelMiner.Core
{
    public static class LightSourcePool
    {
        public static int MaxPoolSize = 20;

        private static UnityEngine.Pool.ObjectPool<LightSource> _pool;
        public static UnityEngine.Pool.ObjectPool<LightSource> Pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new UnityEngine.Pool.ObjectPool<LightSource>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, maxSize: MaxPoolSize);
                }
                return _pool;
            }
        }

        private static LightSource CreatePooledItem()
        {
            LightSource lightSource = new LightSource();
            return lightSource;
        }


        // Called when an item is returned to the pool using Release
        private static void OnReturnedToPool(LightSource lightSource)
        {
            lightSource.Clear();
        }

        // Called when an item is taken from the pool using Get
        private static void OnTakeFromPool(LightSource lightSource)
        {

        }


        private static void OnDestroyPoolObject(LightSource lightSource)
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
