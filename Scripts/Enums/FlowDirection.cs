namespace PixelMiner.Enums
{
    [System.Flags]
    public enum FlowDirection
    {
        None = 0,
        West = 1 << 1,
        East = 1 << 2,
        North = 1 << 3,
        South = 1 << 4,
        Northwest = 1 << 5,
        Northeast = 1 << 6,
        Southwest = 1 << 7,
        Southeast = 1 << 8,

        All = West | East | North | South | Northwest | Northeast | Southwest | Southeast
    }
}

