namespace PixelMiner.Enums
{
    [System.Flags]
    public enum BlockEntityType
    {
        None = 1 << 0,
        OpaqueColliderVoxel = 1 << 1,
        OpaqueColliderNonVoxel = 1 << 2,
        TransparentColliderVoxel = 1 << 3,
        TransparentColliderNonVoxel = 1 << 4,

        OpaqueNonColliderVoxel = 1 << 5,
        OpaqueNonColliderNonVoxel = 1 << 6,
        TransparentNonColliderVoxel = 1 << 7,
        TransparentNonColliderNonVoxel = 1 << 8,
    }


    // 1: Voxel / Nonvoxel
    // 2: Opaque / Transparent
    // 3: Collided / Non-collided
}

