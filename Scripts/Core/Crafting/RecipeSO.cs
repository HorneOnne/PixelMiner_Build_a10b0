using System.Collections.Generic;
using UnityEngine;
using PixelMiner.Enums;
namespace PixelMiner.Core
{
    [CreateAssetMenu(menuName = "PixelMiner/Crafting Recipe")]
    public class RecipeSO : ScriptableObject
    {
        public ItemID ResultItem;
        public byte ResultQuantity;
        [Space(5)]
        public List<RequireSlot> RequiresMaterials;



        [System.Serializable]
        public struct RequireSlot
        {
            public ItemID ItemID;
            public byte RequireQuantity;
        }
    }
}
