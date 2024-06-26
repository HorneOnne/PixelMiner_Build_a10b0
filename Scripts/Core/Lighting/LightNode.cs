using UnityEngine;

namespace PixelMiner.Core
{
    public struct LightNode
    {
        public Vector3Int GlobalPosition;
        public byte Invensity;

        public LightNode(Vector3Int globalPosition, byte intensity)
        {
            this.GlobalPosition = globalPosition;
            this.Invensity = intensity;
        }
    }

    public static class LightNodeExtensions
    {
        //public static byte SunLight(this LightNode lightNode)
        //{
        //    return (byte)((lightNode.LightData >> 12) & 0xF);
        //}

        //public static byte Red(this LightNode lightNode)
        //{
        //    return (byte)((lightNode.LightData >> 8) & 0xF);
        //}

        //public static byte Green(this LightNode lightNode)
        //{
        //    return (byte)((lightNode.LightData >> 4) & 0xF);
        //}

        //public static byte Blue(this LightNode lightNode)
        //{
        //    return (byte)(lightNode.LightData & 0xF);
        //}
    }

}
