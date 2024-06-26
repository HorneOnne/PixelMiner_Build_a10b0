using UnityEngine;
using PixelMiner.Core;
using PixelMiner.Enums;
using PixelMiner.Extensions;

namespace PixelMiner.Core
{
    public class Pickaxe : Item, IUseable
    {
        public bool Use(Player player)
        {
            if (Main.Instance.TryRemoveBlock(player.PlayerBehaviour.SampleBlockTrans.position, out Enums.BlockID removedBlock))
            {           
               

                return true;
            }
   
            return false;
        }
    }
}
