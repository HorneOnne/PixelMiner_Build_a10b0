using UnityEngine;


namespace PixelMiner.Core
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "PixelMiner/ItemData", order = 51)]
    public class ItemData : EntityData
    {
        [Header("Inventory")]
        // Inventory
        public int MaxUses;
        public int MaxStack;

        // Physics
        [Header("Physics")]
        public Vector3 Center;
        public Vector3 BoundSize;
        public float Mass;


        // Visualize
        [Header("Visualize")]
        public Texture2D Texture;
        public Vector3 OffsetPosition;
        public Vector3 RotateAngles;
    }
}
