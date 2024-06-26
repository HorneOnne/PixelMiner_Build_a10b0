namespace PixelMiner.Core
{
    public struct LightColor
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Intensity;

        public LightColor(byte red, byte green, byte blue, byte intensity)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Intensity = intensity;
        }
    }
}

