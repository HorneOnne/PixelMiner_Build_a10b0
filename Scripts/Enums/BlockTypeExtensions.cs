using UnityEngine;
using System.Collections.Generic;
namespace PixelMiner.Enums
{
    public static class BlockExtensions
    {
        /* 
         * Definition:
         * Solid: A block has collider in game.
         * NonSolid: A block that don't has collider in game. (Collider in this case mean like walls where player can't move through this, not impact detection logic.)
         * Opaque: Do not have any transparent in this texture. -> use 'opaque' surface type shader to render mesh.
         * Transparent : Has at least a part is transparent in this texture. -> use 'transparent' surface type shader to render mesh.
         * Voxel: A cube has 6 faces and size is 1 unit.
         * NonVoxel: !Voxel.
         */


        // bit 0: Solid opaque block
        // bit 1: Solid transparent block
        // bit 2: Solid opaque model (not block)
        // bit 3: Solid transparent model (not block)
        // bit 4: NonSolid opaque block
        // bit 5: NonSolid transparent block
        // bit 6: NonSolid opaque model (not block)
        // bit 7: NonSolid transparent model (not block)
        // bit 8: Is light source
        public static ushort[] BlockProperties;



        public static short[] Hardnesses;

        private static HashSet<BlockID> _solidOpaqueVoxels;
        private static HashSet<BlockID> _solidTransparentVoxels;
        private static HashSet<BlockID> _solidOpaqueNonVoxels;
        private static HashSet<BlockID> _solidTransparentNonVoxels;

        private static HashSet<BlockID> _nonSolidOpaqueVoxels;
        private static HashSet<BlockID> _nonSolidTransparentVoxels;
        private static HashSet<BlockID> _nonSolidOpaqueNonVoxels;
        private static HashSet<BlockID> _nonSolidTransparentNonVoxels;

        private static HashSet<BlockID> _lightSources;

        //private static HashSet<BlockID> _walkableBlockSet;

        private static Dictionary<BlockID, short> _hardnessMap;

        static BlockExtensions()
        {
            InitSolidOpaqueBlocks();
            InitSolidTransparentBlocks();
            InitSolidOpaqueNonBlocks();
            InitSolidTransparentNonBlocks();
            InitNonSolidOpaqueBlocks();
            InitNonSolidTransparentBlocks();
            InitNonSolidOpaqueNonBlocks();
            InitNonSolidTransparentNonBlocks();

            InitializLightsourceData();


            InitializeHardnessMap();
            
            // Hardness
            Hardnesses = new short[(int)BlockID.Count];
            foreach (var b in _hardnessMap)
            {
                Hardnesses[(ushort)b.Key] = b.Value;
            }



            // Bool bits
            BlockProperties = new ushort[(int)BlockID.Count];
            // bit 0: Solid opaque block.
            foreach (var solidBlock in _solidOpaqueVoxels)
            {
                BlockProperties[(ushort)solidBlock] = (ushort)(BlockProperties[(ushort)solidBlock] | (1 << 0));
            }
            // bit 1: Solid transparent block.
            foreach (var solidBlock in _solidTransparentVoxels)
            {
                BlockProperties[(ushort)solidBlock] = (ushort)(BlockProperties[(ushort)solidBlock] | (1 << 1));
            }
            // bit 2: Solid opaque model.
            foreach (var solidBlock in _solidOpaqueNonVoxels)
            {
                BlockProperties[(ushort)solidBlock] = (ushort)(BlockProperties[(ushort)solidBlock] | (1 << 2));
            }
            // bit 3: Solid transparent model.
            foreach (var solidBlock in _solidTransparentNonVoxels)
            {
                BlockProperties[(ushort)solidBlock] = (ushort)(BlockProperties[(ushort)solidBlock] | (1 << 3));
            }

            // bit 4: NonSolid opaque block.
            foreach (var solidBlock in _nonSolidOpaqueVoxels)
            {
                BlockProperties[(ushort)solidBlock] = (ushort)(BlockProperties[(ushort)solidBlock] | (1 << 4));
            }
            // bit 5: NonSolid transparent block.
            foreach (var solidBlock in _nonSolidTransparentVoxels)
            {
                BlockProperties[(ushort)solidBlock] = (ushort)(BlockProperties[(ushort)solidBlock] | (1 << 5));
            }
            // bit 6: NonSolid opaque model.
            foreach (var solidBlock in _nonSolidOpaqueNonVoxels)
            {
                BlockProperties[(ushort)solidBlock] = (ushort)(BlockProperties[(ushort)solidBlock] | (1 << 6));
            }
            // bit 7: NonSolid transparent model.
            foreach (var solidBlock in _nonSolidTransparentNonVoxels)
            {
                BlockProperties[(ushort)solidBlock] = (ushort)(BlockProperties[(ushort)solidBlock] | (1 << 7));
            }
            // bit 8: Is Light source
            foreach (var block in _lightSources)
            {
                BlockProperties[(ushort)block] = (ushort)(BlockProperties[(ushort)block] | (1 << 8));
            }


            _solidOpaqueVoxels = null;
            _solidTransparentVoxels = null;
            _solidOpaqueNonVoxels = null;
            _solidTransparentNonVoxels = null;
            _nonSolidOpaqueVoxels = null;
            _nonSolidTransparentVoxels = null;
            _nonSolidOpaqueNonVoxels = null;
            _nonSolidTransparentNonVoxels = null;
            _lightSources = null;
            _hardnessMap = null;
        }



        #region  INITIALIZE DATA
        private static void InitSolidOpaqueBlocks()
        {
            _solidOpaqueVoxels = new HashSet<BlockID>()
            {
                    {BlockID.DirtGrass},
                    {BlockID.Dirt},
                    {BlockID.Stone},
                    {BlockID.Sand},
                    {BlockID.SandMine},
                    {BlockID.Glass},
                    {BlockID.Ice},
                    {BlockID.Light},
                    {BlockID.Wood},
                    {BlockID.Bedrock},
                    {BlockID.Gravel},
                    {BlockID.Cactus},
                    {BlockID.PineWood},
                    {BlockID.SnowDirtGrass},
                    {BlockID.PineLeaves},
                    {BlockID.GoldBlock},

                    {BlockID.RedLight},
                    {BlockID.GreenLight},
                    {BlockID.BlueLight},
            };
        }
        private static void InitSolidTransparentBlocks()
        {
            _solidTransparentVoxels = new HashSet<BlockID>()
            {
                    {BlockID.Glass},
                    {BlockID.Leaves},
            };
        }
        private static void InitSolidOpaqueNonBlocks()
        {
            _solidOpaqueNonVoxels = new HashSet<BlockID>()
            {
                    {BlockID.Torch},
            };
        }
        private static void InitSolidTransparentNonBlocks()
        {
            _solidTransparentNonVoxels = new HashSet<BlockID>()
            {

            };
        }


        private static void InitNonSolidOpaqueBlocks()
        {
            _nonSolidOpaqueVoxels = new HashSet<BlockID>()
            {
                    {BlockID.Lava},
            };
        }
        private static void InitNonSolidTransparentBlocks()
        {
            _nonSolidTransparentVoxels = new HashSet<BlockID>()
            {
                    {BlockID.Water},
            };
        }
        private static void InitNonSolidOpaqueNonBlocks()
        {
            _nonSolidOpaqueNonVoxels = new HashSet<BlockID>()
            {

            };
        }
        private static void InitNonSolidTransparentNonBlocks()
        {
            _nonSolidTransparentNonVoxels = new HashSet<BlockID>()
            {
                  BlockID.Grass,
                  BlockID.TallGrass,
                  BlockID.Shrub,
            };
        }


        // Digging
        private static void InitializeHardnessMap()
        {
            _hardnessMap = new Dictionary<BlockID, short>()
            {
                 { BlockID.Air, 0 },
                 { BlockID.Stone, 5 },
                 { BlockID.Dirt, 3 },
                 { BlockID.DirtGrass, 3 },

                 { BlockID.Water, -1 },
                 { BlockID.Sand, -1 },
                 { BlockID.SandMine, 2 },
                 { BlockID.Glass, 2 },
                 { BlockID.Ice, 2 },
                 { BlockID.Torch, 2 },

                 { BlockID.Light, 3 },
                 { BlockID.Wood, 3 },
                 { BlockID.Leaves, 3 },

                 { BlockID.Grass, 1 },
                 { BlockID.TallGrass, 1 },

                 { BlockID.Bedrock, 100 },
                 { BlockID.Gravel, 1 },

                 { BlockID.Shrub, 1 },
                 { BlockID.Cactus, 2 },
                 { BlockID.PineWood, 3 },
                 { BlockID.PineLeaves, 2 },

                 { BlockID.SnowDirtGrass, 3 },
                 { BlockID.GoldBlock,5 },


                 {BlockID.RedLight, 1},
                {BlockID.GreenLight, 1},
                {BlockID.BlueLight, 1},
            };

        }

        #endregion



        private static void InitializLightsourceData()
        {
            _lightSources = new()
            {
                 { BlockID.Lava},
                 { BlockID.Light},
                 { BlockID.RedLight},
                 { BlockID.GreenLight},
                 { BlockID.BlueLight},
            };

        }


        #region Get Block Properties
        public static bool IsSolidOpaqueVoxel(this BlockID blockID)
        {
            // Get bit 0.
            return (BlockProperties[(ushort)blockID] & (1 << 0)) != 0;
        }

        public static bool IsSolidTransparentVoxel(this BlockID blockID)
        {
            // Get bit 1.
            return (BlockProperties[(ushort)blockID] & (1 << 1)) != 0;
        }

        public static bool IsSolidOpaqueNonvoxel(this BlockID blockID)
        {
            // Get bit 2.
            return (BlockProperties[(ushort)blockID] & (1 << 2)) != 0;
        }

        public static bool IsSolidTransparentNonvoxel(this BlockID blockID)
        {
            // Get bit 3.
            return (BlockProperties[(ushort)blockID] & (1 << 3)) != 0;
        }
        public static bool IsNonSolidOpaqueVoxel(this BlockID blockID)
        {
            // Get bit 4.
            return (BlockProperties[(ushort)blockID] & (1 << 4)) != 0;
        }

        public static bool IsNonSolidTransparentVoxel(this BlockID blockID)
        {
            // Get bit 5.
            return (BlockProperties[(ushort)blockID] & (1 << 5)) != 0;
        }

        public static bool IsNonSolidOpaqueNonvoxel(this BlockID blockID)
        {
            // Get bit 6.
            return (BlockProperties[(ushort)blockID] & (1 << 6)) != 0;
        }

        public static bool IsNonSolidTransparentNonvoxel(this BlockID blockID)
        {
            // Get bit 7.
            return (BlockProperties[(ushort)blockID] & (1 << 7)) != 0;
        }

        public static bool IsLightSource(this BlockID blockID)
        {
            // Get bit 8.
            return (BlockProperties[(ushort)blockID] & (1 << 8)) != 0;
        }
        #endregion






        public static bool IsDirt(this BlockID blockID)
        {
            return blockID == BlockID.Dirt ||
                   blockID == BlockID.DirtGrass ||
                   blockID == BlockID.SnowDirtGrass;
        }

        public static bool IsGrassType(this BlockID blockID)
        {
            return blockID == BlockID.Grass ||
                   blockID == BlockID.TallGrass ||
                   blockID == BlockID.Shrub ||
                   blockID == BlockID.OakTreeSampling ||
                   blockID == BlockID.PineTreeSeed
                   ;
        }

        public static bool AffectedByColorMap(this BlockID blockID)
        {
            return blockID == BlockID.Grass ||
                   blockID == BlockID.DirtGrass ||
                   blockID == BlockID.TallGrass ||
                   blockID == BlockID.OakTreeSampling ||
                   blockID == BlockID.PineTreeSeed ||
                   blockID == BlockID.Leaves;
        }

        public static bool IsTree(this BlockID blockID)
        {
            return blockID == BlockID.Wood ||
                   blockID == BlockID.PineWood ||
                   blockID == BlockID.Leaves ||
                   blockID == BlockID.PineLeaves;
        }





        public static short Hardness(this BlockID blockID)
        {
            return Hardnesses[(short)blockID];
        }

        public static bool Walkable(this BlockID blockID)
        {
            return blockID == BlockID.Air ||
                    blockID.IsNonSolidOpaqueVoxel() ||
                    blockID.IsNonSolidTransparentVoxel() ||
                    blockID.IsNonSolidOpaqueNonvoxel() ||
                    blockID.IsNonSolidTransparentNonvoxel();
        }
    }
}


