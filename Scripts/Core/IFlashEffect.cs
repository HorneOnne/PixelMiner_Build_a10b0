using UnityEngine;

namespace PixelMiner.Core
{
    public interface IFlashEffect
    {
        public Renderer[] Rendereres { get; }
        public System.Collections.IEnumerator DoFlashing(float flashTime);
    }
}
