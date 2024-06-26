using UnityEngine;
using System.Collections.Generic;

namespace PixelMiner.Extensions
{
    public static class UnityExtension
    {
        const long PRIME1 = 73856093;
        const long PRIME2 = 19349663;
        const long PRIME3 = 83492791;

        static UnityExtension()
        {

        }

        public static Vector3Int ToVector3Int(this Vector3 vector3)
        {
            return new Vector3Int(
                Mathf.FloorToInt(vector3.x),
                Mathf.FloorToInt(vector3.y),
                Mathf.FloorToInt(vector3.z)
            );
        }

        public static long ToSpatialHashing(this Vector3Int relativeChunkPosition)
        {
            long x = relativeChunkPosition.x;
            long y = relativeChunkPosition.y;
            long z = relativeChunkPosition.z;

            long hash = x * PRIME1 + y * PRIME2 + z * PRIME3;
            return hash;


        }
              
    }
}
