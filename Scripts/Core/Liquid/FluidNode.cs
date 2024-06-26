using UnityEngine;

namespace PixelMiner.Core
{
    public struct FluidNode
    {
        public Vector3Int GlobalPosition;
        public byte Level;

        public FluidNode(Vector3Int globalPosition, byte level)
        {
            this.GlobalPosition = globalPosition;
            this.Level = level;
        }
    }
}
