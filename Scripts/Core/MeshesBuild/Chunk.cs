using UnityEngine;
using PixelMiner.Enums;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using PixelMiner.DataStructure;
using Sirenix.OdinInspector;
using System.Buffers;
using PixelMiner.Miscellaneous;

namespace PixelMiner.Core
{
    [SelectionBase]
    public class Chunk : MonoBehaviour
    {
        [SerializeField] public MeshFilter SolidOpaqueVoxelmf;
        [SerializeField] public MeshFilter SolidTransparentVoxelmf;
        [SerializeField] public MeshFilter SolidOpaqueNonvoxelmf;
        public MeshFilter Grassmf;
        public MeshFilter Watermf { get; private set; }
        public MeshFilter Lavarmf { get; private set; }


        public Mesh SolidVoxelMesh;
        private Mesh _solidNonVoxelMesh;
        private Mesh _solidTransparentVoxelMesh;
        private Mesh _grassMesh;
        private Mesh _waterMesh;
        private Mesh _lavaMesh;


        public int FrameX;
        public int FrameY;
        public int FrameZ;
        public int Width;
        public int Height;
        public int Depth;
        public Vector3Int Dimensions;
        public bool HasDrawnFirstTime = false;


        #region neighbors
        // Neighbors
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk West { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk East { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk North { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk South { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk Northwest { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk Northeast { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk Southwest { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk Southeast { get; set; }

        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk Up { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk Down { get; set; }

        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk UpWest { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk UpEast { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk UpNorth { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk UpSouth { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk UpNorthwest { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk UpNortheast { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk UpSouthwest { get; set; }
        [field: SerializeField, FoldoutGroup("Neighbors")] public Chunk UpSoutheast { get; set; }
        #endregion

        // Global chunk bounds
        public int MinXGPos { get; private set; }
        public int MaxXGPos { get; private set; }
        public int MinYGPos { get; private set; }
        public int MaxYGPos { get; private set; }
        public int MinZGPos { get; private set; }
        public int MaxZGPos { get; private set; }



        // Data
        public BlockID[] ChunkData { get; private set; }
        [HideInInspector] public HeatType[] HeatData;
        [HideInInspector] public MoistureType[] MoistureData;
        [HideInInspector] public BiomeType[] BiomesData;
        [HideInInspector] public float[] HeatValues;
        [HideInInspector] public short[] Hardnesses;
        [HideInInspector] public Queue<RiverNode> RiverBfsQueue = new Queue<RiverNode>();
        [HideInInspector] public Queue<LightNode> AmbientLightBfsQueue;
        [HideInInspector] public BiomeType[] RiverBiomes;
        [HideInInspector] public byte[] LiquidLevels;    // use bfs spreading like lighting
        [HideInInspector] public bool HasOceanBiome;

        private Vector3Int[] _faceNeighbors = new Vector3Int[6];
        public UpdateChunkMask UpdateMask = 0;
        [field: SerializeField] public int MaxBlocksHeightInit { get; private set; } = 0; // Used  to optimize ambient light propagate.


        private FastNoiseLite _grassNoiseDistribute;


        // Lighting
        // Bits = SSSS RRRR GGGG BBBB
        [HideInInspector] public ushort[] LightData;

        // Place/ Destroy block
        private IEnumerator _healingBlockCoroutine;
        private WaitForSeconds _healingTime;
        private Vector3Int _currDestroyBlockRelativePos = Vector3Int.zero;


        // Physics
        [ReadOnly]
        public int FallingBlockCount = 0;


        #region Properties
        public Vector3 Position { get; private set; }
        public Bounds SolidVoxelBounds { get; private set; }
        #endregion
        private void Awake()
        {
            HasOceanBiome = false;
            Watermf = transform.Find("Water").GetComponent<MeshFilter>();
            Lavarmf = transform.Find("Lava").GetComponent<MeshFilter>();
            Grassmf = transform.Find("Grass").GetComponent<MeshFilter>();
            SolidVoxelMesh = new Mesh();
            _solidNonVoxelMesh = new Mesh();
            _solidTransparentVoxelMesh = new Mesh();
            _grassMesh = new Mesh();
            _waterMesh = new Mesh();
            _lavaMesh = new Mesh();
        }

        private void Start()
        {
            _healingTime = new WaitForSeconds(0.35f);
            Position = transform.position;
        }


        //private void Update()
        //{
        //    if (_waterMesh != null)
        //    {
        //        for (int i = 0; i < _waterMesh.vertices.Length - 1; i++)
        //        {
        //            if (i % 4 == 0)
        //            {
        //                DrawBounds.Instance.AddLine(GlobalPosition + _waterMesh.vertices[i + 2], GlobalPosition + _waterMesh.vertices[i + 1], Color.red);
        //                DrawBounds.Instance.AddLine(GlobalPosition + _waterMesh.vertices[i + 1], GlobalPosition + _waterMesh.vertices[i], Color.red);
        //                DrawBounds.Instance.AddLine(GlobalPosition + _waterMesh.vertices[i], GlobalPosition + _waterMesh.vertices[i + 2], Color.red);

        //                DrawBounds.Instance.AddLine(GlobalPosition + _waterMesh.vertices[i + 3], GlobalPosition + _waterMesh.vertices[i + 2], Color.red);
        //                DrawBounds.Instance.AddLine(GlobalPosition + _waterMesh.vertices[i + 2], GlobalPosition + _waterMesh.vertices[i + 0], Color.red);
        //                DrawBounds.Instance.AddLine(GlobalPosition + _waterMesh.vertices[i], GlobalPosition + _waterMesh.vertices[i + 3], Color.red);
        //            }

        //        }
        //    }
        //}

        private void OnDestroy()
        {
            //ChunkData = null;
            //HeatData = null;
            //MoistureData = null;
            //HeightValues = null;
            //HeatValues = null;
            //MoistureValues = null;
            //_solidNeighbors = null;

            Object.Destroy(SolidOpaqueVoxelmf.sharedMesh);
            Object.Destroy(Watermf.sharedMesh);
            //Destroy(SolidMeshFilter);
            //Destroy(WaterMeshFilter);
        }

        public override bool Equals(object other)
        {
            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked  // overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + FrameX.GetHashCode();
                hash = hash * 23 + FrameY.GetHashCode();
                hash = hash * 23 + FrameZ.GetHashCode();
                return hash;
            }
        }


        public void Init(int frameX, int frameY, int frameZ, int width, int height, int depth, FastNoiseLite _grassNosie)
        {

            // Set properties
            this.FrameX = frameX;
            this.FrameY = frameY;
            this.FrameZ = frameZ;
            this.Width = width;
            this.Height = height;
            this.Depth = depth;

            // Init data
            int size3D = Width * Height * Depth;
            ChunkData = new BlockID[size3D];
            HeatData = new HeatType[size3D];
            MoistureData = new MoistureType[size3D];
            BiomesData = new BiomeType[size3D];
            Dimensions = new Vector3Int(Width, Height, Depth);
            //VoxelLightData = new byte[size3D];
            //AmbientLightData = new byte[size3D];
            LightData = new ushort[size3D];
            Hardnesses = new short[size3D];
            RiverBiomes = new BiomeType[Width * Depth];
            AmbientLightBfsQueue = new Queue<LightNode>();
            LiquidLevels = new byte[size3D];


            MinXGPos = GlobalPosition.x;
            MaxXGPos = GlobalPosition.x + Width;
            MinYGPos = GlobalPosition.y;
            MaxYGPos = GlobalPosition.y + Height;
            MinZGPos = GlobalPosition.z;
            MaxZGPos = GlobalPosition.z + Depth;

            _grassNoiseDistribute = _grassNosie;

            //// Set all light dark by default
            //for (int i = 0; i < VoxelLightData.Length; i++)
            //{
            //    VoxelLightData[i] = 0;
            //}

            //for (int i = 0; i < AmbientLightData.Length; i++)
            //{
            //    AmbientLightData[i] = 0;
            //}

            // Set default water level (0 no water)
            for (int i = 0; i < LiquidLevels.Length; i++)
            {
                LiquidLevels[i] = 0;
            }
        }

        public void UpdateMaxBlocksHeight(int newMaxHeight)
        {
            if (MaxBlocksHeightInit < newMaxHeight)
            {
                MaxBlocksHeightInit = newMaxHeight;
            }
        }

        public bool IsSolid(Vector3Int relativePosition)
        {
            BlockID block = GetBlock(relativePosition);
            return block.IsSolidOpaqueVoxel();
            //return block != blockID.Air && block != blockID.Water;
        }
        public bool IsWater(Vector3Int relativePosition)
        {
            BlockID block = GetBlock(relativePosition);
            return block == BlockID.Water;
        }
        public bool CanSee(Vector3Int relativePosition, ref Vector3Int[] faceNeighbors)
        {
            GetFaceNeighbors(relativePosition, ref faceNeighbors);

            for (int i = 0; i < _faceNeighbors.Length; i++)
            {
                if (IsValidRelativePosition(_faceNeighbors[i]))
                {
                    if (ChunkData[IndexOf(_faceNeighbors[i].x, _faceNeighbors[i].y, _faceNeighbors[i].z)].IsSolidTransparentVoxel())
                    {
                        return false;
                    }
                }
                else
                {
                    if (FindNeighbor(_faceNeighbors[i], out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                    {
                        if (neighborChunk.ChunkData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)].IsSolidTransparentVoxel())
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public bool IsNeighborFaceHasSameBlock(Vector3Int relativePosition, BlockID blockID, ref Vector3Int[] faceNeighbors)
        {
            GetFaceNeighbors(relativePosition, ref faceNeighbors);

            for (int i = 0; i < _faceNeighbors.Length; i++)
            {
                if (IsValidRelativePosition(_faceNeighbors[i]))
                {
                    if (ChunkData[IndexOf(_faceNeighbors[i].x, _faceNeighbors[i].y, _faceNeighbors[i].z)] != blockID)
                    {
                        Debug.Log($"F {relativePosition}: {ChunkData[IndexOf(_faceNeighbors[i].x, _faceNeighbors[i].y, _faceNeighbors[i].z)]}");
                        return false;
                    }
                }
                else
                {
                    if (FindNeighbor(_faceNeighbors[i], out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                    {
                        Debug.Log("G");
                        if (neighborChunk.ChunkData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)] != blockID)
                        {
                            Debug.Log("H");
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public bool IsNeighborHasAirBlock(Vector3Int relativePosition, ref Vector3Int[] faceNeighbors)
        {
            GetFaceNeighbors(relativePosition, ref faceNeighbors);

            for (int i = 0; i < _faceNeighbors.Length; i++)
            {
                if (IsValidRelativePosition(_faceNeighbors[i]))
                {

                    if (ChunkData[IndexOf(_faceNeighbors[i].x, _faceNeighbors[i].y, _faceNeighbors[i].z)] == BlockID.Air)
                    {
                        return true;
                    }

                    //try
                    //{
                    //    if (ChunkData[IndexOf(_faceNeighbors[i].x, _faceNeighbors[i].y, _faceNeighbors[i].z)] == blockID.Air)
                    //    {
                    //        return true;
                    //    }
                    //}
                    //catch
                    //{
                    //    Debug.Log($"{_faceNeighbors[i]}  {IsValidRelativePosition(_faceNeighbors[i])}" );
                    //}
                }
                else
                {
                    if (FindNeighbor(_faceNeighbors[i], out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                    {
                        if (neighborChunk.ChunkData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)] == BlockID.Air)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        public float GetHeat(Vector3Int relativePosition)
        {
            if (IsValidRelativePosition(relativePosition))
            {
                return HeatValues[IndexOf(relativePosition.x, relativePosition.z)];
            }
            else
            {
                //Vector3Int globalPosition = GlobalPosition + relativePosition;
                //if(Main.Instance.TryGetChunk(GetGlobalPosition(globalPosition), out Chunk neighborChunk))
                //{
                //    return neighborChunk.GetHeat(neighborChunk.GetRelativePosition(globalPosition));
                //}

                if (FindNeighbor(relativePosition, out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    return neighborChunk.HeatValues[IndexOf(nbRelativePosition.x, nbRelativePosition.z)];
                }
                else
                {
                    return 0;
                }
            }
        }


        public BlockID GetBlock(Vector3Int relativePosition)
        {
            if (IsValidRelativePosition(relativePosition))
            {
                return ChunkData[IndexOf(relativePosition.x, relativePosition.y, relativePosition.z)];
            }
            else
            {
                if (FindNeighbor(relativePosition, out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    return neighborChunk.ChunkData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)];
                }
                else
                {
                    return BlockID.Air;
                }
            }
        }
        public BlockID GetBlock(int x, int y, int z)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                return ChunkData[IndexOf(x, y, z)];
            }
            else
            {
                Vector3Int relativePosition = new Vector3Int(x, y, z);
                if (FindNeighbor(relativePosition, out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    return neighborChunk.ChunkData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)];
                }
                else
                {
                    return BlockID.Air;
                }
            }
        }


        public void SetBlock(Vector3Int relativePosition, BlockID blockID)
        {
            if (IsValidRelativePosition(relativePosition))
            {
                int index = IndexOf(relativePosition.x, relativePosition.y, relativePosition.z);
                ChunkData[index] = blockID;
                Hardnesses[index] = blockID.Hardness();

                if (blockID == BlockID.SandMine)
                    FallingBlockCount++;

                if (blockID != BlockID.Water)
                    LiquidLevels[index] = 0;
            }
            else
            {
                if (FindNeighbor(relativePosition, out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    int nbIndex = neighborChunk.IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z);
                    neighborChunk.ChunkData[nbIndex] = blockID;
                    neighborChunk.Hardnesses[nbIndex] = blockID.Hardness();

                    if (blockID == BlockID.SandMine)
                        neighborChunk.FallingBlockCount++;

                    if (blockID != BlockID.Water)
                        neighborChunk.LiquidLevels[nbIndex] = 0;
                }
            }




            //if(blockID == BlockID.Water)
            //{
            //    byte blockLight = GetBlockLight(relativePosition);
            //    byte blockLightResistance = LightUtils.BlocksLightResistance[(byte)blockID];
            //    Debug.Log(blockLightResistance);
            //    if (blockLight > blockLightResistance)
            //        SetBlockLight(relativePosition, (byte)(blockLight - blockLightResistance));
            //    else
            //        SetBlockLight(relativePosition, 0);
            //}
        }
        public void SetBlock(int x, int y, int z, BlockID blockID)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                int index = IndexOf(x, y, z);
                ChunkData[index] = blockID;
                Hardnesses[index] = blockID.Hardness();

                if (blockID == BlockID.SandMine)
                    FallingBlockCount++;

                if (blockID != BlockID.Water)
                    LiquidLevels[index] = 0;
            }
            else
            {
                Vector3Int relativePosition = new Vector3Int(x, y, z);
                if (FindNeighbor(relativePosition, out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    int nbIndex = IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z);
                    neighborChunk.ChunkData[nbIndex] = blockID;
                    neighborChunk.Hardnesses[nbIndex] = blockID.Hardness();

                    if (blockID == BlockID.SandMine)
                        neighborChunk.FallingBlockCount++;

                    if (blockID != BlockID.Water)
                        neighborChunk.LiquidLevels[nbIndex] = 0;
                }
            }
        }

        public void RemoveBlock(int x, int y, int z)
        {
            int index = IndexOf(x, y, z);
            if (ChunkData[index] == BlockID.SandMine)
                FallingBlockCount--;

            ChunkData[index] = BlockID.Air;
            LiquidLevels[index] = 0;
        }

        public bool TryDestroyBlock(Vector3Int relativePosition, byte destructLevel)
        {
            //if (_currDestroyBlockRelativePos != relativePosition)
            //{
            //    HealBlock(_currDestroyBlockRelativePos);
            //    if (_healingBlockCoroutine != null)
            //        StopCoroutine(_healingBlockCoroutine);
            //    _currDestroyBlockRelativePos = relativePosition;
            //    _healingBlockCoroutine = StartCoroutine(HealBlockCoroutine());
            //}
            //else
            //{
            //    if (_healingBlockCoroutine != null)
            //        StopCoroutine(_healingBlockCoroutine);
            //    _healingBlockCoroutine = StartCoroutine(HealBlockCoroutine());
            //}


            //int index = IndexOf(relativePosition.x, relativePosition.y, relativePosition.z);
            //Hardnesses[index] = (byte)Mathf.Max(0, Hardnesses[index] - destructLevel);
            //if (Hardnesses[index] == 0)
            //{
            //    StopCoroutine(_healingBlockCoroutine);
            //    return true;
            //}
            //return false;

            int index = IndexOf(relativePosition.x, relativePosition.y, relativePosition.z);
            if (Hardnesses[index] == -1) return false;


            if (_currDestroyBlockRelativePos != relativePosition)
            {
                HealBlock(_currDestroyBlockRelativePos);
                if (_healingBlockCoroutine != null)
                    StopCoroutine(_healingBlockCoroutine);
                _currDestroyBlockRelativePos = relativePosition;
                _healingBlockCoroutine = HealBlockCoroutine();
                StartCoroutine(_healingBlockCoroutine);
            }
            else
            {
                if (_healingBlockCoroutine != null)
                    StopCoroutine(_healingBlockCoroutine);
                _healingBlockCoroutine = HealBlockCoroutine();
                StartCoroutine(_healingBlockCoroutine);
            }

            //int index = IndexOf(relativePosition.x, relativePosition.y, relativePosition.z);
            Hardnesses[index] = (short)Mathf.Max(0, Hardnesses[index] - destructLevel);
            if (Hardnesses[index] == 0)
            {
                if (ChunkData[index] == BlockID.SandMine)
                    FallingBlockCount--;

                StopCoroutine(_healingBlockCoroutine);
                return true;
            }
            return false;
        }
        public void HealBlock(Vector3Int relativePosition)
        {
            int index = IndexOf(relativePosition.x, relativePosition.y, relativePosition.z);
            Hardnesses[index] = ChunkData[index].Hardness();
        }
        private IEnumerator HealBlockCoroutine()
        {
            yield return _healingTime;
            HealBlock(_currDestroyBlockRelativePos);
            UpdateMask |= UpdateChunkMask.RenderAll;
        }



        public void SetBiome(Vector3Int relativePosition, BiomeType biomeType)
        {
            BiomesData[IndexOf(relativePosition.x, relativePosition.y, relativePosition.z)] = biomeType;
        }
        public BiomeType GetBiome(Vector3Int relativePosition)
        {
            if (IsValidRelativePosition(relativePosition))
            {
                return BiomesData[IndexOf(relativePosition.x, relativePosition.y, relativePosition.z)];
            }
            else
            {
                if (FindNeighbor(relativePosition, out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    return neighborChunk.BiomesData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)];
                }
                else
                {
                    return BiomeType.Ocean;
                }
            }
        }


        public bool IsBlockFaceVisible(Vector3Int position, int dimension, bool isBackFace, int voxelFace)
        {
            if (position.y == 7 && voxelFace == 1) return true;

            position[dimension] += isBackFace ? -1 : 1;
            BlockID block = GetBlock(position);
            return block.IsSolidOpaqueVoxel() == false ||
                   block.IsNonSolidOpaqueVoxel();
        }
        public bool IsTransparentBlockFaceVisible(Vector3Int position, int dimension, bool isBackFace)
        {
            position[dimension] += isBackFace ? -1 : 1;
            return !GetBlock(position).IsSolidTransparentVoxel();
        }

        public bool IsWaterFaceVisible(Vector3Int position, int dimension, bool isBackFace)
        {
            position[dimension] += isBackFace ? -1 : 1;
            return GetBlock(position) == BlockID.Air;

        }

        public bool IsLavaFaceVisible(Vector3Int position, int dimension, bool isBackFace)
        {
            position[dimension] += isBackFace ? -1 : 1;
            return GetBlock(position) == BlockID.Air;

        }











        #region Water spreading
        public void SetLiquidLevel(int x, int y, int z, byte level)
        {
            LiquidLevels[IndexOf(x, y, z)] = level;
        }
        public void SetLiquidLevel(Vector3Int relativePosition, byte level)
        {
            LiquidLevels[IndexOf(relativePosition.x, relativePosition.y, relativePosition.z)] = level;
        }

        public byte GetLiquidLevel(int x, int y, int z)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                return LiquidLevels[IndexOf(x, y, z)];
            }
            else
            {
                if (FindNeighbor(new Vector3Int(x, y, z), out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    return neighborChunk.LiquidLevels[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)];
                }
            }
            return 0;
        }
        public byte GetLiquidLevel(Vector3Int relativePosition)
        {
            return GetLiquidLevel(relativePosition.x, relativePosition.y, relativePosition.z);
        }

        public float GetLiquidBlockHeight(Vector3Int relativePosition, int voxelFace)
        {
            float GetHeightBaseOnFluidLevel(byte level)
            {
                //return level / (float)Chunk.MAX_WATER_LEVEL;

                switch (level)
                {
                    case 7:
                        return 1;
                    case 6:
                        return 0.65f;
                    case 5:
                        return 0.50f;
                    case 4:
                        return 0.4f;
                    case 3:
                        return 0.3f;
                    case 2:
                        return 0.2f;
                    case 1:
                        return 0.1f;
                    default:
                    case 0:
                        return 0.0f;
                }
            }


            switch (voxelFace)
            {
                case 1:
                    return relativePosition.y + GetHeightBaseOnFluidLevel(GetLiquidLevel(relativePosition));
                case 3:
                    return relativePosition.y + 1.0f;
                default:
                    return 1;
            }
        }




        public int FindLargerLiquidLevelNeighbors(Vector3Int relativePosition, ref Vector3Int[] neighbors)
        {
            int mask = 0;
            byte centerLevel = GetLiquidLevel(relativePosition);
            for (int i = 0; i < 8; i++)
            {
                if (centerLevel < GetLiquidLevel(neighbors[i]))
                {
                    mask |= 1 << i;
                }
            }
            return mask;
        }


        public int FindUpperWaterNeighbors(ref Vector3Int[] upperNeighbors)
        {
            int upperWaterNeighborsMask = 0;
            for (int i = 0; i < 8; i++)
            {
                if (GetBlock(upperNeighbors[i]) == BlockID.Water)
                {
                    upperWaterNeighborsMask |= 1 << i;
                }
            }
            return upperWaterNeighborsMask;
        }
        public int FindUpperLavaNeighbors(ref Vector3Int[] upperNeighbors)
        {
            int upperWaterNeighborsMask = 0;
            for (int i = 0; i < 8; i++)
            {
                if (GetBlock(upperNeighbors[i]) == BlockID.Lava)
                {
                    upperWaterNeighborsMask |= 1 << i;
                }
            }
            return upperWaterNeighborsMask;
        }


        public Vector3 GetLiquidFlowDirection(Vector3Int relPosition, LiquidType liquidType)
        {
            int GetLowestLiquidVertices(Vector3[] upperFaceVertices)
            {
                int indexMask = 1;
                float minY = upperFaceVertices[0].y;
                for (int i = 1; i < 4; i++)
                {
                    if (upperFaceVertices[i].y < minY)
                    {
                        minY = upperFaceVertices[i].y;
                        indexMask = (1 << i);
                    }
                    else if (upperFaceVertices[i].y == minY)
                    {
                        indexMask |= (1 << i);
                    }
                }
                return indexMask;
            }

            Vector3 flowDir = Vector3.zero;
            Vector3Int west = new Vector3Int(relPosition.x - 1, relPosition.y, relPosition.z);
            Vector3Int east = new Vector3Int(relPosition.x + 1, relPosition.y, relPosition.z);
            Vector3Int north = new Vector3Int(relPosition.x, relPosition.y, relPosition.z + 1);
            Vector3Int south = new Vector3Int(relPosition.x, relPosition.y, relPosition.z - 1);
            Vector3Int nw = new Vector3Int(relPosition.x - 1, relPosition.y, relPosition.z + 1);
            Vector3Int ne = new Vector3Int(relPosition.x + 1, relPosition.y, relPosition.z + 1);
            Vector3Int sw = new Vector3Int(relPosition.x - 1, relPosition.y, relPosition.z - 1);
            Vector3Int se = new Vector3Int(relPosition.x + 1, relPosition.y, relPosition.z - 1);


            Vector3Int up = new Vector3Int(relPosition.x, relPosition.y + 1, relPosition.z);
            Vector3Int uw = new Vector3Int(up.x - 1, up.y, up.z);
            Vector3Int ue = new Vector3Int(up.x + 1, up.y, up.z);
            Vector3Int un = new Vector3Int(up.x, up.y, up.z + 1);
            Vector3Int us = new Vector3Int(up.x, up.y, up.z - 1);
            Vector3Int unw = new Vector3Int(up.x - 1, up.y, up.z + 1);
            Vector3Int une = new Vector3Int(up.x + 1, up.y, up.z + 1);
            Vector3Int usw = new Vector3Int(up.x - 1, up.y, up.z - 1);
            Vector3Int use = new Vector3Int(up.x + 1, up.y, up.z - 1);


            var rentWaterNeighborsArray = ArrayPool<Vector3Int>.Shared.Rent(8);
            var rentUpperWaterNeighborsArray = ArrayPool<Vector3Int>.Shared.Rent(8);
            var rentUpperFaceVertices = ArrayPool<Vector3>.Shared.Rent(4);

            try
            {
                rentWaterNeighborsArray[0] = west;
                rentWaterNeighborsArray[1] = east;
                rentWaterNeighborsArray[2] = north;
                rentWaterNeighborsArray[3] = south;
                rentWaterNeighborsArray[4] = nw;
                rentWaterNeighborsArray[5] = ne;
                rentWaterNeighborsArray[6] = sw;
                rentWaterNeighborsArray[7] = se;


                rentUpperWaterNeighborsArray[0] = uw;
                rentUpperWaterNeighborsArray[1] = ue;
                rentUpperWaterNeighborsArray[2] = un;
                rentUpperWaterNeighborsArray[3] = us;
                rentUpperWaterNeighborsArray[4] = unw;
                rentUpperWaterNeighborsArray[5] = une;
                rentUpperWaterNeighborsArray[6] = usw;
                rentUpperWaterNeighborsArray[7] = use;

                rentUpperFaceVertices[0] = new Vector3(up.x, GetLiquidBlockHeight(relPosition, 1), up.z);
                rentUpperFaceVertices[1] = new Vector3((up + new Vector3(1, 0, 0)).x, GetLiquidBlockHeight(relPosition, 1), (up + new Vector3(1, 0, 0)).z);
                rentUpperFaceVertices[2] = new Vector3((up + new Vector3(1, 0, 1)).x, GetLiquidBlockHeight(relPosition, 1), (up + new Vector3(1, 0, 1)).z);
                rentUpperFaceVertices[3] = new Vector3((up + new Vector3(0, 0, 1)).x, GetLiquidBlockHeight(relPosition, 1), (up + new Vector3(0, 0, 1)).z);


                /* upper face vertices of water single quad mesh.
                 * 3---2
                 * |   |
                 * 0---1
                 */


                var largerWaterLevelNeighborsMask = FindLargerLiquidLevelNeighbors(relPosition, ref rentWaterNeighborsArray);
                int upperWaterNeighborsMask;

                switch (liquidType)
                {
                    default:
                    case LiquidType.Water:
                        upperWaterNeighborsMask = FindUpperWaterNeighbors(ref rentUpperWaterNeighborsArray);
                        break;
                    case LiquidType.Lava:
                        upperWaterNeighborsMask = FindUpperLavaNeighbors(ref rentUpperWaterNeighborsArray);
                        break;
                }


                for (int i = 0; i < sizeof(int) * 8; i++)
                {
                    // Check if the i-th bit is set in the mask
                    if ((largerWaterLevelNeighborsMask & (1 << i)) != 0)
                    {
                        switch (i)
                        {
                            case 0: // west
                                rentUpperFaceVertices[0] = new Vector3(up.x, GetLiquidBlockHeight(west, 1), up.z);
                                rentUpperFaceVertices[3] = new Vector3((up + new Vector3(0, 0, 1)).x, GetLiquidBlockHeight(west, 1), (up + new Vector3(0, 0, 1)).z);
                                break;
                            case 1: //east
                                rentUpperFaceVertices[1] = new Vector3((up + new Vector3(1, 0, 0)).x, GetLiquidBlockHeight(east, 1), (up + new Vector3(1, 0, 0)).z);
                                rentUpperFaceVertices[2] = new Vector3((up + new Vector3(1, 0, 1)).x, GetLiquidBlockHeight(east, 1), (up + new Vector3(1, 0, 1)).z);
                                break;
                            case 2: //north
                                rentUpperFaceVertices[2] = new Vector3((up + new Vector3(1, 0, 1)).x, GetLiquidBlockHeight(north, 1), (up + new Vector3(1, 0, 1)).z);
                                rentUpperFaceVertices[3] = new Vector3((up + new Vector3(0, 0, 1)).x, GetLiquidBlockHeight(north, 1), (up + new Vector3(0, 0, 1)).z);
                                break;
                            case 3: //south
                                rentUpperFaceVertices[0] = new Vector3(up.x, GetLiquidBlockHeight(south, 1), up.z);
                                rentUpperFaceVertices[1] = new Vector3((up + new Vector3(1, 0, 0)).x, GetLiquidBlockHeight(south, 1), (up + new Vector3(1, 0, 0)).z);
                                break;
                            case 4: //northwest
                                rentUpperFaceVertices[3] = new Vector3((up + new Vector3(0, 0, 1)).x, GetLiquidBlockHeight(nw, 1), (up + new Vector3(0, 0, 1)).z);
                                break;
                            case 5: //northeast      
                                rentUpperFaceVertices[2] = new Vector3((up + new Vector3(1, 0, 1)).x, GetLiquidBlockHeight(ne, 1), (up + new Vector3(1, 0, 1)).z);
                                break;
                            case 6: //southwest
                                rentUpperFaceVertices[0] = new Vector3(up.x, GetLiquidBlockHeight(sw, 1), up.z);
                                break;
                            case 7: //southeast
                                rentUpperFaceVertices[1] = new Vector3((up + new Vector3(1, 0, 0)).x, GetLiquidBlockHeight(se, 1), (up + new Vector3(1, 0, 0)).z);
                                break;
                            default:
                                Debug.Log("why default");
                                break;
                        }
                    }
                }

                for (int i = 0; i < sizeof(int) * 8; i++)
                {
                    if ((upperWaterNeighborsMask & (1 << i)) != 0)
                    {
                        switch (i)
                        {
                            case 0: // uw
                                rentUpperFaceVertices[0] = up;
                                rentUpperFaceVertices[3] = up + new Vector3Int(0, 0, 1);
                                break;
                            case 1: // ue
                                rentUpperFaceVertices[1] = ue;
                                rentUpperFaceVertices[2] = ue + new Vector3Int(0, 0, 1);
                                break;
                            case 2: // un
                                rentUpperFaceVertices[2] = un + Vector3.right;
                                rentUpperFaceVertices[3] = un;
                                break;
                            case 3: // us
                                rentUpperFaceVertices[0] = us + Vector3.forward;
                                rentUpperFaceVertices[1] = us + new Vector3Int(1, 0, 1);
                                break;
                            case 4: // unw
                                rentUpperFaceVertices[3] = unw + Vector3.right;
                                break;
                            case 5: // une
                                rentUpperFaceVertices[2] = une;
                                break;
                            case 6: // usw
                                rentUpperFaceVertices[0] = usw + new Vector3Int(1, 0, 1);
                                break;
                            case 7: // use
                                rentUpperFaceVertices[1] = use + new Vector3Int(0, 0, 1);
                                break;
                            default:
                                Debug.LogWarning("Why default");
                                break;
                        }
                    }

                }

                int mask = GetLowestLiquidVertices(rentUpperFaceVertices);
                //Debug.Log(mask);
                if (mask == 1 || mask == 11)   // 0 lowest
                {
                    flowDir = new Vector3(-1, 0, -1);
                }
                else if (mask == 2 || mask == 7) // 1 lowest
                {
                    flowDir = new Vector3(1, 0, -1);
                }
                else if (mask == 4 || mask == 14)  // 2 lowest
                {
                    flowDir = new Vector3(1, 0, 1);
                }
                else if (mask == 8 || mask == 13) // 3 lowest
                {
                    flowDir = new Vector3(-1, 0, 1);
                }
                else if (mask == 3) // 0 == 1 lowest
                {
                    flowDir = new Vector3(0, 0, -1);
                }
                else if (mask == 6) // 1 == 2 lowest
                {
                    flowDir = new Vector3(1, 0, 0);
                }
                else if (mask == 9) // 0 == 3 lowest
                {
                    flowDir = new Vector3(-1, 0, 0);
                }
                else if (mask == 12) // 2 == 3 lowest
                {
                    flowDir = new Vector3(0, 0, 1);
                }
                //else if (mask == 7)// 0 == 1 == 2 lowest => 3 highest flow diagonal vertex => 1 lowest
                //{

                //}
                //else if (mask == 14)  // 1 == 2 == 3 lowest => 0 highest flow diagonal vertex => 2 lowest
                //{

                //}
                //else if (mask == 13) // 2 == 3 == 0 lowest => 1 highest flow diagonal vertex => 3 lowest
                //{

                //}
                //else if (mask == 11) // 3 == 0 == 1 lowest => 2 highest flow diagonal vertex => 0 lowest
                //{

                //}
                else if (mask == 15)
                {

                }

                //Debug.Log($"{rentUpperFaceVertices[0]}   {rentUpperFaceVertices[1]}    {rentUpperFaceVertices[2]}    {rentUpperFaceVertices[3]}");
            }
            finally
            {
                ArrayPool<Vector3Int>.Shared.Return(rentWaterNeighborsArray);
                ArrayPool<Vector3Int>.Shared.Return(rentUpperWaterNeighborsArray);
                ArrayPool<Vector3>.Shared.Return(rentUpperFaceVertices);
            }

            return flowDir;
        }


        public int GetNearestFlowDirection(Vector3Int relPosition)
        {
            int currentOffset = 1;
            int maxCheck = 5;
            int mask = 15;
            while (true)
            {
                Vector3Int west = new Vector3Int(relPosition.x - currentOffset, relPosition.y, relPosition.z);
                Vector3Int east = new Vector3Int(relPosition.x + currentOffset, relPosition.y, relPosition.z);
                Vector3Int north = new Vector3Int(relPosition.x, relPosition.y, relPosition.z + currentOffset);
                Vector3Int south = new Vector3Int(relPosition.x, relPosition.y, relPosition.z - currentOffset);

                Vector3Int downwest = new Vector3Int(relPosition.x - currentOffset, relPosition.y - 1, relPosition.z);
                Vector3Int downeast = new Vector3Int(relPosition.x + currentOffset, relPosition.y - 1, relPosition.z);
                Vector3Int downnorth = new Vector3Int(relPosition.x, relPosition.y - 1, relPosition.z + currentOffset);
                Vector3Int downsouth = new Vector3Int(relPosition.x, relPosition.y - 1, relPosition.z - currentOffset);

                int westCount = 0;      // bit 0
                int eastCount = 0;      // bit 1
                int northCount = 0;     // bit 2
                int southCount = 0;     // bit 3

                if (GetBlock(west) == BlockID.Air && GetBlock(downwest) == BlockID.Air)
                {
                    westCount++;
                }
                if (GetBlock(east) == BlockID.Air && GetBlock(downeast) == BlockID.Air)
                {
                    eastCount++;
                }

                if (GetBlock(north) == BlockID.Air && GetBlock(downnorth) == BlockID.Air)
                {
                    northCount++;
                }

                if (GetBlock(south) == BlockID.Air && GetBlock(downsouth) == BlockID.Air)
                {
                    southCount++;
                }



                if (westCount != eastCount || eastCount != northCount || northCount != southCount)
                {
                    mask = 0;
                    if (westCount != 0)
                    {
                        mask |= 1 << 0;
                    }
                    if (eastCount != 0)
                    {
                        mask |= 1 << 1;
                    }
                    if (northCount != 0)
                    {
                        mask |= 1 << 2;
                    }
                    if (southCount != 0)
                    {
                        mask |= 1 << 3;
                    }
                    return mask;
                }
                else
                {
                    if (westCount == 1)
                    {
                        return mask;
                    }
                }

                currentOffset++;
                if (currentOffset >= maxCheck)
                {
                    //Debug.Log($"max check: {currentOffset}  {maxCheck}");
                    break;
                }

            }

            return mask;
        }

        #endregion













        #region Renders

        public async Task RenderChunkTask(CancellationToken token)
        {
            if (!HasDrawnFirstTime)
            {
                HasDrawnFirstTime = true;
            }

            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();

            //MeshData solidVoxelMeshData = MeshDataPool.Get();
            MeshData solidVoxelMeshData = await MeshUtils.Instance.RenderSolidMesh(this, isTransparentMesh: false, token);

            // MeshData transparentSolidMeshData = MeshDataPool.Get(); 
            MeshData transparentSolidMeshData = await MeshUtils.Instance.RenderSolidMesh(this, isTransparentMesh: true, token);

            //MeshData solidNonVoxelMeshData = MeshDataPool.Get();
            //MeshData solidNonVoxelMeshData = await MeshUtils.Instance.RenderSolidNonvoxelMesh(this);


            //MeshData grassMeshData = MeshDataPool.Get();
            MeshData grassMeshData = await MeshUtils.Instance.GetChunkGrassMeshData(this, _grassNoiseDistribute, token);


            //MeshData waterMeshData = MeshDataPool.Get();
            MeshData waterMeshData = await MeshUtils.Instance.WaterGreedyMeshingAsync(this, token);


            MeshData lavaMeshData = await MeshUtils.Instance.RenderLavaMeshingAsync(this, token);


            MeshUtils.CreateMesh(solidVoxelMeshData, ref SolidVoxelMesh);
            SolidOpaqueVoxelmf.sharedMesh = SolidVoxelMesh;


            MeshUtils.CreateMesh(transparentSolidMeshData, ref _solidTransparentVoxelMesh);
            SolidTransparentVoxelmf.sharedMesh = _solidTransparentVoxelMesh;


            MeshUtils.CreateWaterMesh(waterMeshData, ref _waterMesh);
            Watermf.sharedMesh = _waterMesh;


            MeshUtils.CreateWaterMesh(lavaMeshData, ref _lavaMesh);
            Lavarmf.sharedMesh = _lavaMesh;

            //MeshUtils.CreateMesh(solidNonVoxelMeshData, ref _solidNonVoxelMesh);
            //SolidOpaqueNonvoxelmf.sharedMesh = _solidNonVoxelMesh;

            // Grass
            // -----
            MeshUtils.CreateMesh(grassMeshData, ref _grassMesh);
            Grassmf.sharedMesh = _grassMesh;




            // Release mesh data
            MeshDataPool.Release(solidVoxelMeshData);
            MeshDataPool.Release(transparentSolidMeshData);
            MeshDataPool.Release(grassMeshData);
            MeshDataPool.Release(waterMeshData);
            MeshDataPool.Release(lavaMeshData);
            //MeshDataPool.Release(solidNonVoxelMeshData);


            // Update collider
            if (SolidVoxelMesh.vertexCount > 0)
            {
                SolidOpaqueVoxelmf.gameObject.GetComponent<MeshCollider>().sharedMesh = SolidVoxelMesh;
            }
            else
            {
                SolidOpaqueVoxelmf.gameObject.GetComponent<MeshCollider>().sharedMesh = null;
            }

            if (_solidNonVoxelMesh.vertexCount > 0)
            {
                SolidTransparentVoxelmf.gameObject.GetComponent<MeshCollider>().sharedMesh = _solidTransparentVoxelMesh;
            }
            else
            {
                SolidTransparentVoxelmf.gameObject.GetComponent<MeshCollider>().sharedMesh = null;
            }


            //stopwatch.Stop();
            //Debug.Log($"Render chunk elapsed: {stopwatch.ElapsedMilliseconds / 1000f} s");

            SolidVoxelBounds = new Bounds(transform.position + SolidVoxelMesh.bounds.center, SolidVoxelMesh.bounds.size);
        }

        #endregion












        #region Neighbors

        public bool HasNeighbors()
        {
            return West != null && East != null && North != null && South != null &&
                   Northeast != null && Northwest != null && Southeast != null && Southwest != null &&

                   Up != null &&
                   //Down != null &&

                   UpWest != null && UpEast != null && UpNorth != null && UpSouth != null &&
                   UpNortheast != null && UpNorthwest != null && UpSoutheast != null && UpSouthwest != null;
        }
        public bool FindNeighbor(Vector3Int relativePosition, out Chunk neighborChunk, out Vector3Int nbRelativePosition)
        {
            Vector3Int neighborOffset = default;
            nbRelativePosition = default;
            bool foundNeighborChunk = false;

            if (relativePosition.x >= Width)
            {
                neighborOffset.x = 1;
            }
            else if (relativePosition.x < 0)
            {
                neighborOffset.x = -1;
            }

            if (relativePosition.y >= Height)
            {
                neighborOffset.y = 1;
            }
            else if (relativePosition.y < 0)
            {
                neighborOffset.y = -1;
            }
            if (relativePosition.z >= Depth)
            {
                neighborOffset.z = 1;
            }
            else if (relativePosition.z < 0)
            {
                neighborOffset.z = -1;
            }


            neighborChunk = FindNeighbor(neighborOffset);
            if (neighborChunk != null)
            {
                foundNeighborChunk = true;

                if (neighborOffset.x == -1)
                {
                    nbRelativePosition.x = Width + relativePosition.x;
                }
                else if (neighborOffset.x == 0)
                {
                    nbRelativePosition.x = relativePosition.x;
                }
                else if (neighborOffset.x == 1)
                {
                    nbRelativePosition.x = relativePosition.x - Width;
                }
                else
                {
                    Debug.LogError("Out of range width!");
                }

                if (neighborOffset.y == -1)
                {
                    nbRelativePosition.y = Height + relativePosition.y;
                }
                else if (neighborOffset.y == 0)
                {
                    nbRelativePosition.y = relativePosition.y;
                }
                else if (neighborOffset.y == 1)
                {
                    nbRelativePosition.y = relativePosition.y - Height;
                }
                else
                {
                    Debug.LogError("Out of range height!");
                }

                if (neighborOffset.z == -1)
                {
                    nbRelativePosition.z = Depth + relativePosition.z;
                }
                else if (neighborOffset.z == 0)
                {
                    nbRelativePosition.z = relativePosition.z;
                }
                else if (neighborOffset.z == 1)
                {
                    nbRelativePosition.z = relativePosition.z - Depth;
                }
                else
                {
                    Debug.LogError("Out of range depth!");
                }
            }

            return foundNeighborChunk;
        }
        public Chunk FindNeighbor(Vector3Int neighborOffset)
        {
            if (neighborOffset == new Vector3Int(-1, 0, 0))
            {
                return West;
            }
            else if (neighborOffset == new Vector3Int(1, 0, 0))
            {
                return East;
            }
            else if (neighborOffset == new Vector3Int(0, 0, 1))
            {
                return North;
            }
            else if (neighborOffset == new Vector3Int(0, 0, -1))
            {
                return South;
            }
            else if (neighborOffset == new Vector3Int(-1, 0, -1))
            {
                return Southwest;
            }
            else if (neighborOffset == new Vector3Int(1, 0, -1))
            {
                return Southeast;
            }
            else if (neighborOffset == new Vector3Int(-1, 0, 1))
            {
                return Northwest;
            }
            else if (neighborOffset == new Vector3Int(1, 0, 1))
            {
                return Northeast;
            }


            else if (neighborOffset == new Vector3Int(0, 1, 0))
            {
                return Up;
            }
            else if (neighborOffset == new Vector3Int(0, -1, 0))
            {
                return Down;
            }


            else if (neighborOffset == new Vector3Int(-1, 1, 0))
            {
                return UpWest;
            }
            else if (neighborOffset == new Vector3Int(1, 1, 0))
            {
                return UpEast;
            }
            else if (neighborOffset == new Vector3Int(0, 1, 1))
            {
                return UpNorth;
            }
            else if (neighborOffset == new Vector3Int(0, 1, -1))
            {
                return UpSouth;
            }
            else if (neighborOffset == new Vector3Int(-1, 1, -1))
            {
                return UpSouthwest;
            }
            else if (neighborOffset == new Vector3Int(1, 1, -1))
            {
                return UpSoutheast;
            }
            else if (neighborOffset == new Vector3Int(-1, 1, 1))
            {
                return UpNorthwest;
            }
            else if (neighborOffset == new Vector3Int(1, 1, 1))
            {
                return UpNortheast;
            }


            return null;
        }

        private void GetFaceNeighbors(Vector3Int relativePosition, ref Vector3Int[] faceNeighbors)
        {
            faceNeighbors[0] = relativePosition + Vector3Int.left;
            faceNeighbors[1] = relativePosition + Vector3Int.right;
            faceNeighbors[2] = relativePosition + Vector3Int.forward;
            faceNeighbors[3] = relativePosition + Vector3Int.back;
            faceNeighbors[4] = relativePosition + Vector3Int.up;
            faceNeighbors[5] = relativePosition + Vector3Int.down;
        }

        #endregion














        #region Lighting      
        public ushort GetLightData(Vector3Int relativePosition)
        {
            if (IsValidRelativePosition(relativePosition))
            {
                int index = IndexOf(relativePosition.x, relativePosition.y, relativePosition.z);
                return LightData[index];
            }
            else
            {
                if (FindNeighbor(relativePosition, out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    int nbIndex = neighborChunk.IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z);
                    return neighborChunk.LightData[nbIndex];
                }
                else
                {
                    return 0;
                }
            }
        }
        public ushort GetLightData(int x, int y, int z)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                int index = IndexOf(x, y, z);
                return LightData[index];
            }
            else
            {
                if (FindNeighbor(new Vector3Int(x, y, z), out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    int nbIndex = neighborChunk.IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z);
                    return neighborChunk.LightData[nbIndex];
                }
                else
                {
                    return 0;
                }
            }
        }



        public void GetBlockLight(Vector3Int relativePosition, out byte red, out byte green, out byte blue)
        {
            if (IsValidRelativePosition(relativePosition))
            {
                int index = IndexOf(relativePosition.x, relativePosition.y, relativePosition.z);
                red = (byte)((LightData[index] >> 8) & 0xF);
                green = (byte)((LightData[index] >> 4) & 0xF);
                blue = (byte)(LightData[index] & 0xF);
            }
            else
            {
                if (FindNeighbor(relativePosition, out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    int nbIndex = neighborChunk.IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z);
                    red = (byte)((neighborChunk.LightData[nbIndex] >> 8) & 0xF);
                    green = (byte)((neighborChunk.LightData[nbIndex] >> 4) & 0xF);
                    blue = (byte)(neighborChunk.LightData[nbIndex] & 0xF);
                }
                else
                {
                    //Debug.LogError("Not found this chunk");
                    red = 0;
                    green = 0;
                    blue = 0;
                }
            }
        }

        public void SetBlockLight(int x, int y, int z, byte red, byte green, byte blue)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                int index = IndexOf(x, y, z);
                //LightData[index] = (ushort)((LightData[index] & 0xF0) | intensity);

                LightData[index] = (ushort)((LightData[index] & 0xF0FF) | (red << 8));
                LightData[index] = (ushort)((LightData[index] & 0xFF0F) | (green << 4));
                LightData[index] = (ushort)((LightData[index] & 0xFFF0) | blue);
            }
            else
            {
                if (FindNeighbor(new Vector3Int(x, y, z), out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    int nbindex = IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z);
                    //neighborChunk.LightData[nbindex] = (ushort)((neighborChunk.LightData[nbindex] & 0xF0) | intensity);

                    neighborChunk.LightData[nbindex] = (ushort)((neighborChunk.LightData[nbindex] & 0xF0FF) | (red << 8));
                    neighborChunk.LightData[nbindex] = (ushort)((neighborChunk.LightData[nbindex] & 0xFF0F) | (green << 4));
                    neighborChunk.LightData[nbindex] = (ushort)((neighborChunk.LightData[nbindex] & 0xFFF0) | blue);
                }
            }
        }



        public byte GetRedLight(int x, int y, int z)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                return (byte)((LightData[IndexOf(x, y, z)] >> 8) & 0xF);
            }
            else
            {
                if (FindNeighbor(new Vector3Int(x, y, z), out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    return (byte)((neighborChunk.LightData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)] >> 8) & 0xF);
                }
                else
                {
                    //Debug.LogError("Not found this chunk");
                    return 0;
                }
            }
        }

        public byte GetGreenLight(int x, int y, int z)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                return (byte)((LightData[IndexOf(x, y, z)] >> 4) & 0xF);
            }
            else
            {
                if (FindNeighbor(new Vector3Int(x, y, z), out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    return (byte)((neighborChunk.LightData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)] >> 4) & 0xF);
                }
                else
                {
                    //Debug.LogError("Not found this chunk");
                    return 0;
                }
            }
        }

        public byte GetBlueLight(int x, int y, int z)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                return (byte)(LightData[IndexOf(x, y, z)] & 0xF);
            }
            else
            {
                if (FindNeighbor(new Vector3Int(x, y, z), out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    return (byte)(neighborChunk.LightData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)] & 0xF);
                }
                else
                {
                    //Debug.LogError("Not found this chunk");
                    return 0;
                }
            }
        }



        // 0xF0FF: 1111_0000_1111_1111
        public void SetRedLight(int x, int y, int z, byte intensity)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                int index = IndexOf(x, y, z);
                LightData[index] = (ushort)((LightData[index] & 0xF0FF) | (intensity << 8));
            }
            else
            {
                if (FindNeighbor(new Vector3Int(x, y, z), out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    int nbindex = IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z);
                    neighborChunk.LightData[nbindex] = (ushort)((neighborChunk.LightData[nbindex] & 0xF0FF) | (intensity << 8));
                }
            }
        }

        // 0xFF0F: 1111_1111_0000_1111
        public void SetGreenLight(int x, int y, int z, byte intensity)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                int index = IndexOf(x, y, z);
                LightData[index] = (ushort)((LightData[index] & 0xFF0F) | (intensity << 4));
            }
            else
            {
                if (FindNeighbor(new Vector3Int(x, y, z), out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    int nbindex = IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z);
                    neighborChunk.LightData[nbindex] = (ushort)((neighborChunk.LightData[nbindex] & 0xFF0F) | (intensity << 4));
                }
            }
        }

        // 0xFFF0: 1111_1111_1111_0000
        public void SetBlueLight(int x, int y, int z, byte intensity)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                int index = IndexOf(x, y, z);
                LightData[index] = (ushort)((LightData[index] & 0xFFF0) | intensity);
            }
            else
            {
                if (FindNeighbor(new Vector3Int(x, y, z), out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    int nbindex = IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z);
                    neighborChunk.LightData[nbindex] = (ushort)((neighborChunk.LightData[nbindex] & 0xFFF0) | intensity);
                }
            }
        }




        public byte GetAmbientLight(int x, int y, int z)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                //return AmbientLightData[IndexOf(x, y, z)];
                return (byte)((LightData[IndexOf(x, y, z)] >> 12) & 0xF);
            }
            else
            {
                if (FindNeighbor(new Vector3Int(x, y, z), out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    //return neighborChunk.AmbientLightData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)];
                    return (byte)((neighborChunk.LightData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)] >> 12) & 0xF);
                }
                else
                {
                    Debug.LogError($"Not found this chunk {x} {y} {z}");
                    return LightUtils.MAX_LIGHT_INTENSITY;
                }
            }

            throw new System.Exception($"Currently we not calculate height of chunk. {x} {y} {z}");
        }
        public byte GetAmbientLight(Vector3Int relativePosition)
        {
            if (IsValidRelativePosition(relativePosition))
            {
                //return AmbientLightData[IndexOf(relativePosition.x, relativePosition.y, relativePosition.z)];
                return (byte)((LightData[IndexOf(relativePosition.x, relativePosition.y, relativePosition.z)] >> 12) & 0xF);
            }
            else
            {
                if (FindNeighbor(relativePosition, out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    //return neighborChunk.AmbientLightData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)];
                    return (byte)((neighborChunk.LightData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)] >> 12) & 0xF);
                }
                else
                {
                    //Debug.LogError($"Not found this chunk {relativePosition}");
                    return LightUtils.MAX_LIGHT_INTENSITY;
                }
            }

            throw new System.Exception($"Currently we not calculate height of chunk. {relativePosition}");
        }


        // 0x0FFF: 0000_1111_1111_1111
        public void SetAmbientLight(Vector3Int relativePosition, byte intensity)
        {
            if (IsValidRelativePosition(relativePosition))
            {
                //AmbientLightData[IndexOf(relativePosition.x, relativePosition.y, relativePosition.z)] = intensity;

                int index = IndexOf(relativePosition.x, relativePosition.y, relativePosition.z);
                LightData[index] = (ushort)((LightData[index] & 0x0FFF) | (intensity << 12));
                return;
            }
            else
            {

                if (FindNeighbor(relativePosition, out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    //neighborChunk.AmbientLightData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)] = intensity;
                    int nbIndex = neighborChunk.IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z);
                    neighborChunk.LightData[nbIndex] = (ushort)((neighborChunk.LightData[nbIndex] & 0x0FFF) & (intensity << 12));
                }
                else
                {
                    Debug.LogError($"Not found this chunk {relativePosition}");
                }
            }
        }
        public void SetAmbientLight(int x, int y, int z, byte intensity)
        {
            if (IsValidRelativePosition(x, y, z))
            {
                //AmbientLightData[IndexOf(x, y, z)] = intensity;
                int index = IndexOf(x, y, z);
                LightData[index] = (ushort)((LightData[index] & 0x0FFF) | (intensity << 12));
                return;
            }
            else
            {

                if (FindNeighbor(new Vector3Int(x, y, z), out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                {
                    //neighborChunk.AmbientLightData[IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z)] = intensity;

                    int nbIndex = neighborChunk.IndexOf(nbRelativePosition.x, nbRelativePosition.y, nbRelativePosition.z);
                    neighborChunk.LightData[nbIndex] = (ushort)((neighborChunk.LightData[nbIndex] & 0x0FFF) & (intensity << 12));
                }
                else
                {
                    Debug.LogError($"Not found this chunk {x} {y} {z}");
                }
            }
        }
        #endregion













        #region Utilities
        public bool IsValidRelativePosition(Vector3Int relativePosition)
        {
            return relativePosition.x >= 0 && relativePosition.x < Width &&
                   relativePosition.y >= 0 && relativePosition.y < Height &&
                   relativePosition.z >= 0 && relativePosition.z < Depth;
        }
        public bool IsValidRelativePosition(int x, int y, int z)
        {
            return x >= 0 && x < Width &&
                   y >= 0 && y < Height &&
                   z >= 0 && z < Depth;
        }

        public int IndexOf(int x, int y, int z)
        {
            return x + Width * (y + Height * z);
        }
        public int IndexOf(int x, int z)
        {
            return x + z * Width;
        }

        public Vector3Int GlobalPosition => new Vector3Int(FrameX * Dimensions[0], FrameY * Dimensions[1], FrameZ * Dimensions[2]);
        public Vector3Int RelativePosition => new Vector3Int(FrameX, FrameY, FrameZ);
        public Vector3Int GetGlobalPosition(Vector3Int relativePosition)
        {
            if (IsValidRelativePosition(relativePosition))
            {
                return GlobalPosition + relativePosition;
            }
            else
            {
                throw new System.ArgumentOutOfRangeException($"{relativePosition}", "Relative position is out of bounds.");
            }
        }
        public Vector3 GetGlobalPosition(int x, int y, int z)
        {
            if (IsValidRelativePosition(x,y,z))
            {
                return new Vector3(GlobalPosition.x + x, GlobalPosition.y + y, GlobalPosition.z + z);
            }
            else
            {
                throw new System.ArgumentOutOfRangeException($"Relative position is out of bounds.");
            }
        }

        public Vector3Int GetRelativePosition(Vector3Int globalPosition)
        {
            Vector3Int relativePosition = new Vector3Int(Mathf.FloorToInt(globalPosition[0] % Dimensions[0]),
                                                         Mathf.FloorToInt(globalPosition[1] % Dimensions[1]),
                                                         Mathf.FloorToInt(globalPosition[2] % Dimensions[2]));

            // Ensure relative positions are positive
            if (relativePosition.x < 0) relativePosition.x += Dimensions[0];
            if (relativePosition.y < 0) relativePosition.y += Dimensions[1];
            if (relativePosition.z < 0) relativePosition.z += Dimensions[2];

            return relativePosition;
        }

        public Bounds GetBounds()
        {
            Vector3 center = GlobalPosition + new Vector3(Width / 2.0f, Height / 2.0f, Depth / 2.0f);
            return new Bounds(center, new Vector3(Width, Height, Depth));
        }

        public bool OnGroundLevel(Vector3Int relativePosition)
        {
            if (IsValidRelativePosition(relativePosition))
            {
                Vector3Int belowRelativePos = relativePosition + Vector3Int.down;
                Vector3Int upperRelativePos = relativePosition + Vector3Int.up;

                if (GetBlock(relativePosition) == BlockID.Air)
                {
                    if (GetBlock(belowRelativePos) != BlockID.Air)
                    {
                        if (GetBlock(upperRelativePos) == BlockID.Air)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            Debug.LogError($"Out of chunk volume range. {relativePosition}");
            return false;
        }

        public bool IsEdgeOfChunk(Vector3Int relativePosition, ref HashSet<Chunk> neighborChunks)
        {
            if (relativePosition.x == 0 ||
                relativePosition.x == Width - 1 ||
                relativePosition.y == 0 ||
                relativePosition.y == Height - 1 ||
                relativePosition.z == 0 ||
                relativePosition.z == Depth - 1)
            {
                for(int z = relativePosition.z - 1; z <= relativePosition.z + 1; z++)
                {
                    for (int y = relativePosition.y - 1; y <= relativePosition.y + 1; y++)
                    {
                        for (int x = relativePosition.x - 1; x <= relativePosition.x + 1; x++)
                        {
                            if(FindNeighbor(new Vector3Int(x,y,z), out Chunk nb, out Vector3Int nbRelPosition))
                            {
                                if(nb != this && neighborChunks.Contains(nb) == false)
                                {
                                    neighborChunks.Add(nb);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            return false;
        }

        #endregion
    }

    public struct RiverNode
    {
        public Vector3Int RelativePosition;
        public int Density;
    }

    [System.Flags]
    public enum UpdateChunkMask : ushort
    {
        None = 0,
        SolidOpaqueVoxel = 1 << 0,
        SolidTransparentVoxel = 1 << 1,
        SolidOpaqueNonvoxel = 1 << 2,
        SolidTransparentNonvoxel = 1 << 3,
        Grass = 1 << 4,
        Water = 1 << 5,
        RenderLightCorssChunk = 1 << 6,


        RenderAll = SolidOpaqueVoxel |
                    SolidTransparentVoxel |
                    SolidOpaqueNonvoxel |
                    SolidTransparentNonvoxel |
                    Grass |
                    Water |
                    RenderLightCorssChunk,
    }
}
