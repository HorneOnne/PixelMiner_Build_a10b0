using UnityEngine;
namespace PixelMiner.Core
{
    public interface ILootable
    {
        public bool LootedBy(PlayerInventory inventory);
    }
}
