namespace PixelMiner.Enums
{
    public enum ItemID : ushort
    {
        // Blocks
        Air = 180,
        DirtGrass = 3,
        Dirt = 2,
        Stone = 1,
        Water = 207,
        Sand = 18,
        SandMine = 176,
        Glass = 49,
        Snow = 66,
        Ice = 67,
        Torch = 80,
        Light = 105,
        Wood = 20,
        Leaves = 52,
        Grass = 39,
        TallGrass = 169,
        Bedrock = 17,
        Gravel = 0,
        Shrub = 55,
        Cactus = 70,
        PineWood = 231,
        PineLeaves = 53,
        SnowDritGrass = 66,

        OakTreeSampling = 15,
        PineTreeSeed = 30,

        GoldBlock = 23,

        // Items 
        StoneAxe = 1001,
        Pickaxe = 1002,
        Sword = 1003,


        MAX = 2000,
    }


    public static class ItemIDExtension
    {
        private static string[] ItemNames;
        static ItemIDExtension()
        {
            ItemNames = new string[(int)ItemID.MAX];
            for (int i = 0; i < ItemNames.Length; i++)
            {
                ItemNames[i] = System.Enum.GetName(typeof(ItemID), i);
            }

        }

        public static string ToItemString(this ItemID itemID)
        {
            return ItemNames[(ushort)itemID];
        }
    }
}
