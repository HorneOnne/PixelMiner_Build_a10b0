using UnityEngine;
using PixelMiner.Enums;
using Sirenix.OdinInspector;

namespace PixelMiner.Core
{
    public abstract class EntityData : ScriptableObject
    {
        [Header("Entity")]
        public ItemID ID;
        public string ItemName;
        [PreviewField(60), HideLabel] public Sprite Icon;
    }


}
