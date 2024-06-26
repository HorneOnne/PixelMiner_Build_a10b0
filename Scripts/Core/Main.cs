using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using PixelMiner.Enums;
using System.Threading.Tasks;
using System.Threading;
using PixelMiner.DataStructure;
using PixelMiner.Extensions;
using System.Linq;
using System.Buffers;
using UnityEngine.Pool;
namespace PixelMiner.Core
{
    public class Main : MonoBehaviour
    {
        public static Main Instance { get; private set; }
        public event System.Action OnCharacterInitialize;

        private const long PRIME1 = 73856093;
        private const long PRIME2 = 19349663;
        private const long PRIME3 = 83492791;

        [Header("Initialize")]
        public bool CreatePlayerWhenStartGame = false;
        [SerializeField] private CharacterModelType _modelType;
        [SerializeField] private bool _randomPosition;
        [SerializeField] private Vector3Int _initPlayerPosition;


        [Header("Game data"), Space(10)]
        [SerializeField] private List<Transform> _players = new(1);

        // Chunk data
        [Header("Data Cached")]
        public Dictionary<long, Chunk> Chunks;
        public HashSet<Chunk> ActiveChunks;
        public Queue<Chunk> ActiveChunksQueue = new();
        public Queue<Chunk> DeativeChunksEqueue = new();
        public string SeedInput = "7";


        public Vector3Int ChunkDimension;
        private LayerMask _itemLayer;

        public bool AutoLoadChunk = true;
        public bool AutoUnloadChunk = true;


        // limit time execution
        private float _updateTimer = 0.0f;
        private float _updateTime = 0.04f;
        private float _fallingBlockUpdateTimer = 0.0f;
        private float _fallingBlockUpdateTime = 0.075f;
        private float _waterUpdateTimer = 0.0f;
        private float _waterUpdateTime = 0.2f;
        private float _lavaUpdateTimer = 0.0f;
        private float _lavaUpdateTime = 2.5f;


        // render
        private bool _isRenderingChunks = false;


        // falling sand
        private List<Chunk> _fallingChunks = new();
        private List<Task> _fallingSandTaskList = new();


        // Liquids
        // ---------------------
        // water
        public List<WaterSource> WaterSources = new(4);
        private bool _isHandleWaterLogics = false;
        // lava
        public List<LavaSource> LavaSources = new(4);
        private bool _isHandleLavaLogics = false;




        // ambient occlusion
        private HashSet<Chunk> crossChunkNbAOSet = new(6);   // set of chunks need render when place/remove block at edge of chunk

        // lighting
        private bool _isHandleLight = false;
        public List<LightSource> LightSources = new(4);
        private float _updateLightTime = 0.02f;
        private float _updateLightTimer = 0.02f;

        // Destroy, place block cached data
        private List<Vector3Int> _removedBlockBfsPositions = new(50);
        private Queue<Vector3Int> _destroyMultipleBlockBfsQueue = new(50);
        private Vector3Int[] _destroyMultipleBlocksNeighbors = new Vector3Int[5];



        // order of execution (render)
        private List<Task> _renderChunkTask = new();
        int _maxChunkRenderEachFrame = 12;


        // Testing
        //[Range(0, 15)] public byte Red;
        //[Range(0, 15)] public byte Green;
        //[Range(0, 15)] public byte Blue;
        //public Color32 ColorTesting;


        #region  Properties
        public List<Transform> Players { get => _players; }
        public Vector3Int InitWorldPosition { get => _initPlayerPosition; }
        #endregion





        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);

            // Initialize the chunks data
            Chunks = new();
            ActiveChunks = new();
            _itemLayer = 0;
            _itemLayer |= 1 << 23;


            if (_randomPosition)
            {
                //_initPlayerPosition = new Vector3Int(Random.Range(-1000, 1000), 15, Random.Range(-1000, 1000));
                _initPlayerPosition = new Vector3Int(0, 15,0);
            }
        }

        private void Start()
        {
            //InvokeRepeating(nameof(SpawnRandomZombie), 5f, 45f);
            //int count = 0;
            //for (int y = 0; y < 48; y++)
            //{
            //    for (int z = -50 + y; z <= 50 - y; z++)
            //    {
            //        for (int x = -50 + y; x <= 50 - y; x++)
            //        {
            //            count++;

            //        }
            //    }
            //}

            WorldGeneration.Instance.OnWorldLoadFinished += AddNewPlayer;
        }

        private void AddNewPlayer()
        {
            if (CreatePlayerWhenStartGame)
            {
                AddNewPlayer(_modelType);
                if (_players.Count > 1)
                {
                    Debug.LogWarning("have more than 1 player in scnene.");
                }

                OnCharacterInitialize?.Invoke();
            }
        }

        private void Update()
        {
            //Red = (byte)(ColorTesting.r / 17);
            //Green = (byte)(ColorTesting.g / 17);
            //Blue = (byte)(ColorTesting.b / 17);

            // Falling block
            //if (UnityEngine.Time.time - _fallingBlockUpdateTimer > _fallingBlockUpdateTime)
            //{
            //    _fallingBlockUpdateTimer = UnityEngine.Time.time;
            //    await FallingChunks();
            //}


            if (WorldLoading.Instance.IsLoadingChunk) return;

            // Water spreading
            _waterUpdateTimer += UnityEngine.Time.deltaTime;
            if (_waterUpdateTimer > _waterUpdateTime)
            {
                _waterUpdateTimer = 0f;
                if (NeedHandleWaterLogic() && !_isHandleWaterLogics)
                {
                    WaterLogicHandlerAsync();
                }
            }



            // Lava spreading
            _lavaUpdateTimer += UnityEngine.Time.deltaTime;
            if (_lavaUpdateTimer > _lavaUpdateTime)
            {
                _lavaUpdateTimer = 0f;
                if (NeedHandleLavaLogic() && !_isHandleLavaLogics)
                {
                    LavaLogicHandler();
                }
            }



            // Lighting
            _updateLightTimer += UnityEngine.Time.deltaTime;
            if (_updateLightTimer > _updateLightTime)
            {
                _updateLightTimer -= _updateLightTime;
                if (LightSources.Count > 0 && !_isHandleLight)
                {
                    LightingLogicHandlerAsync();
                }

            }



            // Rendering
            _updateTimer += UnityEngine.Time.deltaTime;
            if (_updateTimer > _updateTime)
            {
                _updateTimer -= _updateTime;
                if (!_isRenderingChunks)
                {
                    bool needRenderChunk = false;
                    foreach (var chunk in ActiveChunks)
                    {
                        if ((chunk.UpdateMask & UpdateChunkMask.RenderAll) != 0)
                        {
                            needRenderChunk = true;
                            break;
                        }
                    }
                    if (needRenderChunk)
                    {
                        try
                        {
                            RenderChunks(Application.exitCancellationToken);
                        }
                        catch
                        {
                            Debug.LogWarning("Render chunks was cancelled.");
                        }
                    }
                }

            }



            //if (Input.GetKeyDown(KeyCode.V))
            //{
            //    //WorldGeneration.Instance.GenerateVein(new Vector3Int((int)Players[0].transform.position.x, (int)Players[0].transform.position.y + 7, (int)Players[0].transform.position.z), 0.3f, 64);
            //    Debug.Log("Build lava pool");
            //    StructureBuilder.Instance.BuildLavaPool3(new Vector3Int((int)Players[0].transform.position.x + 3, (int)Players[0].transform.position.y, (int)Players[0].transform.position.z));

            //}

            //if (Input.GetKeyDown(KeyCode.P))
            //{
            //    StartCoroutine(Pyramid());
            //}
        }



#if false
        // Testing
        private void SpawnRandomZombie()
        {
            float randomX = Random.Range(-25f, 25f);
            float randomZ = Random.Range(-25f, 25f);

            Vector3 randomPosition = new Vector3(Players[0].transform.position.x + randomX, Players[0].transform.position.y + 3f, Players[0].transform.position.z + randomZ);
            Zombie zombie = GameFactory.CreateZombie(randomPosition);
            zombie.EnablePhysics();
        }
        private IEnumerator Pyramid()
        {
            int count = 0;
            for (int y = 0; y < 48; y++)
            {
                for (int z = -50 + y; z <= 50 - y; z++)
                {
                    for (int x = -50 + y; x <= 50 - y; x++)
                    {
                        Vector3Int voxelPosition = new Vector3Int(x, 50, z);
                        count++;
                        if (TryGetChunk(voxelPosition, out Chunk c))
                        {
                            c.SetBlock(c.GetRelativePosition(voxelPosition), BlockID.SandMine);
                        }

                        if (count == 7)
                        {
                            count = 0;
                            yield return null;
                        }


                    }
                }
            }
        }
#endif



        #region Players
        private Transform AddNewPlayer(CharacterModelType type)
        {
            Transform playerPrefab;
            switch (type)
            {
                default:
                case CharacterModelType.Gameplay:
                    playerPrefab = Resources.Load<Player>($"Player").transform;
                    break;
                case CharacterModelType.ViewOnly:
                    playerPrefab = Resources.Load<Transform>($"ViewOnlyCharacter").transform;
                    break;
            }


            //Vector3 position = _randomPosition == true ? new Vector3(Random.Range(-100000, 100000), 8, Random.Range(-100000, 100000)) : _startPlayerPosition;
            var playerInstance = Instantiate(playerPrefab, _initPlayerPosition, Quaternion.identity);
            _players.Add(playerInstance);
            return playerInstance.transform;
        }
        #endregion




        #region Get, Set Chunk
        public bool HasChunk(Vector3Int relativePosition)
        {
            return Chunks.ContainsKey(relativePosition.ToSpatialHashing());
        }

        public Chunk GetChunk(Vector3 worldPosition)
        {
            Vector3Int frame = new Vector3Int(Mathf.FloorToInt(worldPosition.x / ChunkDimension[0]),
                Mathf.FloorToInt(worldPosition.y / ChunkDimension[1]), Mathf.FloorToInt(worldPosition.z / ChunkDimension[2]));
            return GetChunk(frame);
        }

        public bool TryAddChunks(Vector3Int relativePosition, Chunk chunk)
        {
            var spatialHashingKey = relativePosition.ToSpatialHashing();
            if (!Chunks.ContainsKey(spatialHashingKey))
            {
                Chunks.Add(spatialHashingKey, chunk);
                return true;
            }
            return false;
        }

        public bool TryGetChunk(Vector3 globalPosition, out Chunk chunk)
        {
            GlobalToRelativeChunkPosition(globalPosition, out int x, out int y, out int z);
            return Chunks.TryGetValue(ToSpatialHashing(x, y, z), out chunk);
        }
        public bool TryGetChunk(int x, int y, int z, out Chunk chunk)
        {
            return Chunks.TryGetValue(ToSpatialHashing(x, y, z), out chunk);
        }

        public Chunk GetChunk(Vector3Int relativePosition)
        {
            Chunks.TryGetValue(relativePosition.ToSpatialHashing(), out Chunk chunk);
            return chunk;
        }
        public Chunk GetChunk(int x, int y, int z)
        {
            Chunks.TryGetValue(ToSpatialHashing(x, y, z), out Chunk chunk);
            return chunk;
        }
        #endregion


        #region Block
        public BlockID GetBlock(Vector3 globalPosition)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);

            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                return chunk.GetBlock(x, y, z);
            }
            else
            {
                return BlockID.Air;
            }
        }

        public void SetBlock(Vector3 globalPosition, BlockID blockID)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                chunk.SetBlock(x, y, z, blockID);
            }
        }
        #endregion









        #region Render
        //private async void DrawChunksAtOnce(HashSet<Chunk> chunks)
        //{
        //    List<Task> drawChunkTasks = new List<Task>();
        //    foreach (var chunk in chunks)
        //    {
        //        drawChunkTasks.Add(chunk.RenderChunkTask());
        //    }
        //    await Task.WhenAll(drawChunkTasks);
        //    chunks.Clear();
        //}

        private void HandleAmbientOcclusionAfterPlaceBlock(Chunk chunk, Vector3Int relativePosition)
        {
            crossChunkNbAOSet.Clear();
            if (chunk.IsEdgeOfChunk(relativePosition, ref crossChunkNbAOSet))
            {
                foreach (var c in crossChunkNbAOSet)
                {
                    c.UpdateMask |= UpdateChunkMask.RenderAll;
                }
            }
        }
        private void HandleAmbientOcclusionAfterRemoveBlock(Chunk chunk, Vector3Int relativePosition)
        {
            crossChunkNbAOSet.Clear();
            if (chunk.IsEdgeOfChunk(relativePosition, ref crossChunkNbAOSet))
            {
                foreach (var c in crossChunkNbAOSet)
                {
                    c.UpdateMask |= UpdateChunkMask.RenderAll;
                }
            }
        }
        private async void RenderChunks(CancellationToken token)
        {
            _isRenderingChunks = true;
            int renderChunkCount = 0;
            _renderChunkTask.Clear();
            foreach (var chunk in ActiveChunks)
            {
                if ((chunk.UpdateMask & UpdateChunkMask.RenderAll) != 0)
                {
                    if (chunk.HasDrawnFirstTime == false)
                    {
                        HandleLightSpreadingCrossChunkFirstTimeDrawn(chunk);
                        await AddLightToLightBlockForChunkFirstTimeDrawn(chunk);
                    }
                    chunk.UpdateMask &= ~UpdateChunkMask.RenderAll;

                    _renderChunkTask.Add(chunk.RenderChunkTask(token));
                    renderChunkCount++;

                    if (renderChunkCount >= _maxChunkRenderEachFrame)
                    {
                        break;
                    }
                }
            }

            await Task.WhenAll(_renderChunkTask);
            _isRenderingChunks = false;
        }
        #endregion











        #region Light
        /*
         * Asynchronous for multiple light at once.
         * Single thread for place or remove single block.
         */

        public float GetAmbientLightIntensity()
        {
            return DayNightCycle.Instance.AmbientlightIntensity;
            //return LightUtils.CalculateSunlightIntensity(hour + _worldTime.Minutes / 60f, SunLightIntensityCurve));
        }

        public ushort GetLightData(Vector3 globalPosition)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                return chunk.GetLightData(x, y, z);
            }
            return byte.MinValue;
        }

        public byte GetRedLight(Vector3 globalPosition)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                return chunk.GetRedLight(x, y, z);
            }
            return byte.MinValue;
        }
        public byte GetGreenLight(Vector3 globalPosition)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                return chunk.GetGreenLight(x, y, z);
            }
            return byte.MinValue;
        }
        public byte GetBlueLight(Vector3 globalPosition)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                return chunk.GetBlueLight(x, y, z);
            }
            return byte.MinValue;
        }


        public void SetRedLight(Vector3 globalPosition, byte intensity)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                chunk.SetRedLight(x, y, z, intensity);
            }
        }

        public byte GetAmbientLight(Vector3 globalPosition)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                return chunk.GetAmbientLight(x, y, z);
            }
            return byte.MinValue;
        }

        public void SetAmbientLight(Vector3 globalPosition, byte insensity)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                chunk.SetAmbientLight(x, y, z, insensity);
            }
        }


        private async void LightingLogicHandlerAsync()
        {
            //Debug.Log("LightingLogicHandler");
            _isHandleLight = true;
            for (int i = 0; i < LightSources.Count; i++)
            {
                if (LightSources[i].AmbientLightBfsQueue.Count > 0)
                {
                    await LightCalculator.PropagateAmbientLightTask(LightSources[i].AmbientLightBfsQueue, LightSources[i].SpreadingChunkEffected);
                }
                if (LightSources[i].RedLightSpreadingBfsQueue.Count > 0)
                {
                    await LightCalculator.PropagateRedLightAsync(LightSources[i].RedLightSpreadingBfsQueue, LightSources[i].SpreadingChunkEffected);
                }
                if (LightSources[i].GreenLightSpreadingBfsQueue.Count > 0)
                {
                    await LightCalculator.PropagateGreenLightAsync(LightSources[i].GreenLightSpreadingBfsQueue, LightSources[i].SpreadingChunkEffected);
                }
                if (LightSources[i].BlueLightSpreadingBfsQueue.Count > 0)
                {
                    await LightCalculator.PropagateBlueLightAsync(LightSources[i].BlueLightSpreadingBfsQueue, LightSources[i].SpreadingChunkEffected);
                }
            }

            for (int i = 0; i < LightSources.Count; i++)
            {
                if (LightSources[i].AmbientLightRemovalBfsQueue.Count > 0)
                {
                    await LightCalculator.RemoveAmbientLightTask(LightSources[i].AmbientLightRemovalBfsQueue, LightSources[i].AmbientLightBfsQueue, LightSources[i].RemovalChunkEffected);
                }
                if (LightSources[i].RedLightRemovalBfsQueue.Count > 0)
                {
                    await LightCalculator.RemoveRedLightAsync(LightSources[i].RedLightRemovalBfsQueue, LightSources[i].RedLightSpreadingBfsQueue, LightSources[i].RemovalChunkEffected);
                }
                if (LightSources[i].GreenLightRemovalBfsQueue.Count > 0)
                {
                    await LightCalculator.RemoveGreenLightAsync(LightSources[i].GreenLightRemovalBfsQueue, LightSources[i].GreenLightSpreadingBfsQueue, LightSources[i].RemovalChunkEffected);
                }
                if (LightSources[i].BlueLightRemovalBfsQueue.Count > 0)
                {
                    await LightCalculator.RemoveBlueLightAsync(LightSources[i].BlueLightRemovalBfsQueue, LightSources[i].BlueLightSpreadingBfsQueue, LightSources[i].RemovalChunkEffected);
                }
            }

            for (int i = 0; i < LightSources.Count; i++)
            {
                foreach (var chunk in LightSources[i].SpreadingChunkEffected)
                {
                    chunk.UpdateMask |= UpdateChunkMask.RenderAll;
                }
                LightSources[i].SpreadingChunkEffected.Clear();
            }
            for (int i = 0; i < LightSources.Count; i++)
            {
                foreach (var chunk in LightSources[i].RemovalChunkEffected)
                {
                    chunk.UpdateMask |= UpdateChunkMask.RenderAll;
                }
                LightSources[i].RemovalChunkEffected.Clear();
            }

            for (int i = 0; i < LightSources.Count; i++)
            {
                if (LightSources[i].IsEmpty())
                {
                    LightSourcePool.Pool.Release(LightSources[i]);
                    LightSources.RemoveAt(i);
                }
            }
            _isHandleLight = false;
        }

        private void HandleLightAfterPlaceBlock(Chunk chunk, Vector3Int blockGPosition, Vector3Int relativePosition, BlockID blockID)
        {
            byte redLight = chunk.GetRedLight(relativePosition.x, relativePosition.y, relativePosition.z);
            byte greenLight = chunk.GetGreenLight(relativePosition.x, relativePosition.y, relativePosition.z);
            byte blueLight = chunk.GetBlueLight(relativePosition.x, relativePosition.y, relativePosition.z);

            LightSource removalLightSource = LightSourcePool.Pool.Get();
            if (redLight > 0)
            {
                removalLightSource.RedLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGPosition, Invensity = redLight });
            }
            if (greenLight > 0)
            {
                removalLightSource.GreenLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGPosition, Invensity = greenLight });
            }
            if (blueLight > 0)
            {
                removalLightSource.BlueLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGPosition, Invensity = blueLight });
            }
            LightSources.Add(removalLightSource);



            if (blockID.IsLightSource())
            {
                LightSource spreadingLightSource = LightSourcePool.Pool.Get();
                LightColor blockLightColor = LightUtils.BlocksLight[(ushort)blockID];
                if (blockLightColor.Red > 0)
                {
                    spreadingLightSource.RedLightSpreadingBfsQueue.Enqueue(
                        new LightNode()
                        {
                            GlobalPosition = blockGPosition,
                            Invensity = blockLightColor.Red
                        });
                }

                // green
                if (blockLightColor.Green > 0)
                {
                    spreadingLightSource.GreenLightSpreadingBfsQueue.Enqueue(
                    new LightNode()
                    {
                        GlobalPosition = blockGPosition,
                        Invensity = blockLightColor.Green
                    });
                }


                // blue
                if (blockLightColor.Blue > 0)
                {
                    spreadingLightSource.BlueLightSpreadingBfsQueue.Enqueue(
                        new LightNode()
                        {
                            GlobalPosition = blockGPosition,
                            Invensity = blockLightColor.Blue
                        });
                }

                LightSources.Add(spreadingLightSource);
            }
        }
        private void HandleLightAfterRemoveBlock(Chunk chunk, Vector3Int blockGPosition, Vector3Int relativePosition)
        {
            byte sunLight = chunk.GetAmbientLight(relativePosition);
            byte redLight = chunk.GetRedLight(relativePosition.x, relativePosition.y, relativePosition.z);
            byte greenLight = chunk.GetGreenLight(relativePosition.x, relativePosition.y, relativePosition.z);
            byte blueLight = chunk.GetBlueLight(relativePosition.x, relativePosition.y, relativePosition.z);


            if (redLight > 0 || greenLight > 0 || blueLight > 0)
            {
                LightSource removalLightSource = LightSourcePool.Pool.Get();

                if (sunLight > 0)
                {
                    removalLightSource.AmbientLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGPosition, Invensity = sunLight });
                }
                if (redLight > 0)
                {
                    removalLightSource.RedLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGPosition, Invensity = redLight });
                }
                if (greenLight > 0)
                {
                    removalLightSource.GreenLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGPosition, Invensity = greenLight });
                }
                if (blueLight > 0)
                {
                    removalLightSource.BlueLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGPosition, Invensity = blueLight });
                }

                LightSources.Add(removalLightSource);
            }


            LightSource spreadingLightSource = LightSourcePool.Pool.Get();
            for (int z = blockGPosition.z - 1; z <= blockGPosition.z + 1; z++)
            {
                for (int y = blockGPosition.y - 1; y <= blockGPosition.y + 1; y++)
                {
                    for (int x = blockGPosition.x - 1; x <= blockGPosition.x + 1; x++)
                    {
                        if (x == blockGPosition.x && y == blockGPosition.y && z == blockGPosition.z) continue;
                        Vector3Int nbGlobalPosition = new Vector3Int(x, y, z);
                        if (TryGetChunk(nbGlobalPosition, out Chunk nbChunk))
                        {
                            Vector3Int nbRelPosition = nbChunk.GetRelativePosition(nbGlobalPosition);
                            byte nbSunLight = nbChunk.GetAmbientLight(nbRelPosition.x, nbRelPosition.y, nbRelPosition.z);
                            byte nbRedLight = nbChunk.GetRedLight(nbRelPosition.x, nbRelPosition.y, nbRelPosition.z);
                            byte nbGreenLight = nbChunk.GetGreenLight(nbRelPosition.x, nbRelPosition.y, nbRelPosition.z);
                            byte nbBlueLight = nbChunk.GetBlueLight(nbRelPosition.x, nbRelPosition.y, nbRelPosition.z);

                            if (nbSunLight > 0)
                            {
                                spreadingLightSource.AmbientLightBfsQueue.Enqueue(new LightNode() { GlobalPosition = nbGlobalPosition, Invensity = nbSunLight });
                            }
                            if (nbRedLight > 0)
                            {
                                spreadingLightSource.RedLightSpreadingBfsQueue.Enqueue(new LightNode() { GlobalPosition = nbGlobalPosition, Invensity = nbRedLight });
                            }
                            if (nbGreenLight > 0)
                            {
                                spreadingLightSource.GreenLightSpreadingBfsQueue.Enqueue(new LightNode() { GlobalPosition = nbGlobalPosition, Invensity = nbGreenLight });
                            }
                            if (nbBlueLight > 0)
                            {
                                spreadingLightSource.BlueLightSpreadingBfsQueue.Enqueue(new LightNode() { GlobalPosition = nbGlobalPosition, Invensity = nbBlueLight });
                            }
                        }


                    }
                }
            }

            if (spreadingLightSource.IsEmpty() == false)
            {
                LightSources.Add(spreadingLightSource);
            }
            else
            {
                LightSourcePool.Pool.Release(spreadingLightSource);
            }
        }
        private void HandleLightAfterRemoveMultipleBlocksAsync(List<Vector3Int> blockGlobalPositions)
        {
            LightSource removalLightSource = LightSourcePool.Pool.Get();
            for (int i = 0; i < blockGlobalPositions.Count; i++)
            {
                if (TryGetChunk(blockGlobalPositions[i], out Chunk chunk))
                {
                    Vector3Int relativePosition = chunk.GetRelativePosition(blockGlobalPositions[i]);

                    byte sunLight = chunk.GetAmbientLight(relativePosition);
                    byte redLight = chunk.GetRedLight(relativePosition.x, relativePosition.y, relativePosition.z);
                    byte greenLight = chunk.GetGreenLight(relativePosition.x, relativePosition.y, relativePosition.z);
                    byte blueLight = chunk.GetBlueLight(relativePosition.x, relativePosition.y, relativePosition.z);


                    if (redLight > 0 || greenLight > 0 || blueLight > 0)
                    {


                        if (sunLight > 0)
                        {
                            removalLightSource.AmbientLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGlobalPositions[i], Invensity = sunLight });
                        }
                        if (redLight > 0)
                        {
                            removalLightSource.RedLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGlobalPositions[i], Invensity = redLight });
                        }
                        if (greenLight > 0)
                        {
                            removalLightSource.GreenLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGlobalPositions[i], Invensity = greenLight });
                        }
                        if (blueLight > 0)
                        {
                            removalLightSource.BlueLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGlobalPositions[i], Invensity = blueLight });
                        }
                    }
                }
            }
            LightSources.Add(removalLightSource);



            LightSource spreadingLightSource = LightSourcePool.Pool.Get();
            for (int i = 0; i < blockGlobalPositions.Count; i++)
            {
                for (int z = blockGlobalPositions[i].z - 1; z <= blockGlobalPositions[i].z + 1; z++)
                {
                    for (int y = blockGlobalPositions[i].y - 1; y <= blockGlobalPositions[i].y + 1; y++)
                    {
                        for (int x = blockGlobalPositions[i].x - 1; x <= blockGlobalPositions[i].x + 1; x++)
                        {
                            if (x == blockGlobalPositions[i].x && y == blockGlobalPositions[i].y && z == blockGlobalPositions[i].z) continue;
                            Vector3Int nbGlobalPosition = new Vector3Int(x, y, z);
                            if (TryGetChunk(nbGlobalPosition, out Chunk nbChunk))
                            {
                                Vector3Int nbRelPosition = nbChunk.GetRelativePosition(nbGlobalPosition);
                                byte nbSunLight = nbChunk.GetAmbientLight(nbRelPosition.x, nbRelPosition.y, nbRelPosition.z);
                                byte nbRedLight = nbChunk.GetRedLight(nbRelPosition.x, nbRelPosition.y, nbRelPosition.z);
                                byte nbGreenLight = nbChunk.GetGreenLight(nbRelPosition.x, nbRelPosition.y, nbRelPosition.z);
                                byte nbBlueLight = nbChunk.GetBlueLight(nbRelPosition.x, nbRelPosition.y, nbRelPosition.z);

                                if (nbSunLight > 0)
                                {
                                    spreadingLightSource.AmbientLightBfsQueue.Enqueue(new LightNode() { GlobalPosition = nbGlobalPosition, Invensity = nbSunLight });
                                }
                                if (nbRedLight > 0)
                                {
                                    spreadingLightSource.RedLightSpreadingBfsQueue.Enqueue(new LightNode() { GlobalPosition = nbGlobalPosition, Invensity = nbRedLight });
                                }
                                if (nbGreenLight > 0)
                                {
                                    spreadingLightSource.GreenLightSpreadingBfsQueue.Enqueue(new LightNode() { GlobalPosition = nbGlobalPosition, Invensity = nbGreenLight });
                                }
                                if (nbBlueLight > 0)
                                {
                                    spreadingLightSource.BlueLightSpreadingBfsQueue.Enqueue(new LightNode() { GlobalPosition = nbGlobalPosition, Invensity = nbBlueLight });
                                }
                            }


                        }
                    }
                }
            }
            if (spreadingLightSource.IsEmpty() == false)
            {
                LightSources.Add(spreadingLightSource);
            }
            else
            {
                LightSourcePool.Pool.Release(spreadingLightSource);
            }

        }

        //private async void HandleLightingAfterRemoveMultipleBlocks(List<Vector3Int> blockGlobalPositions)
        //{        
        //    for (int i = 0; i < blockGlobalPositions.Count; i++)
        //    {
        //        if (Main.Instance.TryGetChunk(blockGlobalPositions[i], out var chunk))
        //        {
        //            Vector3Int relativePosition = chunk.GetRelativePosition(blockGlobalPositions[i]);
        //            _ambientLightBfsQueue.Enqueue(
        //                new LightNode() 
        //                { 
        //                    GlobalPosition = blockGlobalPositions[i], 
        //                    Invensity = chunk.GetAmbientLight(relativePosition.x, relativePosition.y, relativePosition.z) 
        //                });


        //            byte redLight = chunk.GetRedLight(relativePosition.x, relativePosition.y, relativePosition.z);
        //            byte greenLight = chunk.GetGreenLight(relativePosition.x, relativePosition.y, relativePosition.z);
        //            byte blueLight = chunk.GetBlueLight(relativePosition.x, relativePosition.y, relativePosition.z);
        //            if (redLight != 0)
        //            {
        //                _redLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGlobalPositions[i], Invensity = redLight });

        //            }
        //            if (greenLight != 0)
        //            {
        //                _greenLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGlobalPositions[i], Invensity = greenLight });

        //            }
        //            if (blueLight != 0)
        //            {
        //                _blueLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGlobalPositions[i], Invensity = blueLight });
        //            }
        //        }
        //    }

        //    if(_ambientLightBfsQueue.Count > 0)
        //    {
        //        await LightCalculator.RemoveAmbientLightAsync(_ambientLightBfsQueue);
        //    }

        //    if (_redLightRemovalBfsQueue.Count > 0)
        //    {
        //        await LightCalculator.RemoveRedLightAsync(_redLightRemovalBfsQueue, _redLightSpreadingBfsQueue, _chunksEffectedByLight);
        //    }

        //    if (_greenLightRemovalBfsQueue.Count > 0)
        //    {
        //        await LightCalculator.RemoveGreenLightAsync(_greenLightRemovalBfsQueue, _greenLightSpreadingBfsQueue, _chunksEffectedByLight);
        //    }

        //    if (_blueLightRemovalBfsQueue.Count > 0)
        //    {
        //        await LightCalculator.RemoveBlueLightAsync(_blueLightRemovalBfsQueue, _blueLightSpreadingBfsQueue, _chunksEffectedByLight);
        //    }

        //    foreach (var c in _lightingRemovalUpdateChunks)
        //    {
        //        c.UpdateMask |= UpdateChunkMask.RenderAll;
        //    }
        //    foreach (var c in _lightingSpreadingUpdateChunks)
        //    {
        //        c.UpdateMask |= UpdateChunkMask.RenderAll;
        //    }
        //    foreach (var c in _chunksEffectedByLight)
        //    {
        //        c.UpdateMask |= UpdateChunkMask.RenderAll;
        //    }
        //    _lightingRemovalUpdateChunks.Clear();
        //    _lightingSpreadingUpdateChunks.Clear();
        //    _chunksEffectedByLight.Clear();
        //}



        public void HandleLightingAfterPlaceMultipleLava(LavaSource lavaSource)
        {
            LightSource spreadingLightSource = LightSourcePool.Pool.Get();
            for (int i = 0; i < lavaSource.NewLavaSpreadingPositions.Count; i++)
            {
                if (TryGetChunk(lavaSource.NewLavaSpreadingPositions[i], out Chunk chunk))
                {
                    Vector3Int relativePosition = chunk.GetRelativePosition(lavaSource.NewLavaSpreadingPositions[i]);
                    // red
                    BlockID blockID = chunk.GetBlock(relativePosition);
                    LightColor blockLightColor = LightUtils.BlocksLight[(ushort)blockID];
                    if (blockLightColor.Red > 0)
                    {
                        spreadingLightSource.RedLightSpreadingBfsQueue.Enqueue(
                        new LightNode()
                        {
                            GlobalPosition = lavaSource.NewLavaSpreadingPositions[i],
                            Invensity = blockLightColor.Red
                        });
                    }


                    // green
                    if (blockLightColor.Green > 0)
                    {
                        spreadingLightSource.GreenLightSpreadingBfsQueue.Enqueue(
                        new LightNode()
                        {
                            GlobalPosition = lavaSource.NewLavaSpreadingPositions[i],
                            Invensity = blockLightColor.Green
                        });
                    }



                    // blue
                    if (blockLightColor.Blue > 0)
                    {
                        spreadingLightSource.BlueLightSpreadingBfsQueue.Enqueue(
                        new LightNode()
                        {
                            GlobalPosition = lavaSource.NewLavaSpreadingPositions[i],
                            Invensity = blockLightColor.Blue
                        });
                    }
                }
            }
            LightSources.Add(spreadingLightSource);
            lavaSource.NewLavaSpreadingPositions.Clear();
        }

        public void HandleLightingAfterRemoveLava(LavaSource lavaSource)
        {
            LightSource removalLightSource = LightSourcePool.Pool.Get();
            foreach (var removalNode in lavaSource.LavaRemovalBfsQueue)
            {
                if (TryGetChunk(removalNode.GlobalPosition, out Chunk chunk))
                {
                    Vector3Int relativePosition = chunk.GetRelativePosition(removalNode.GlobalPosition);
                    byte redLight = chunk.GetRedLight(relativePosition.x, relativePosition.y, relativePosition.z);
                    byte greenLight = chunk.GetGreenLight(relativePosition.x, relativePosition.y, relativePosition.z);
                    byte blueLight = chunk.GetBlueLight(relativePosition.x, relativePosition.y, relativePosition.z);

                    if (redLight > 0)
                    {
                        removalLightSource.RedLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = removalNode.GlobalPosition, Invensity = redLight });
                    }

                    if (greenLight > 0)
                    {
                        removalLightSource.GreenLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = removalNode.GlobalPosition, Invensity = greenLight });
                    }

                    if (blueLight > 0)
                    {
                        removalLightSource.BlueLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = removalNode.GlobalPosition, Invensity = blueLight });
                    }
                }
            }
            LightSources.Add(removalLightSource);

            //if (lavaSource.RedLightRemovalBfsQueueAsync.Count > 0)
            //{
            //    await LightCalculator.RemoveRedLightAsync(lavaSource.RedLightRemovalBfsQueueAsync, lavaSource.RedLightSpreadingBfsQueueAsync, lavaSource.RemovalChunkEffected);
            //}
            //if (lavaSource.GreenLightRemovalBfsQueueAsync.Count > 0)
            //{
            //    await LightCalculator.RemoveGreenLightAsync(lavaSource.GreenLightRemovalBfsQueueAsync, lavaSource.GreenLightSpreadingBfsQueueAsync, lavaSource.RemovalChunkEffected);
            //}
            //if (lavaSource.BlueLightRemovalBfsQueueAsync.Count > 0)
            //{
            //    await LightCalculator.RemoveBlueLightAsync(lavaSource.BlueLightRemovalBfsQueueAsync, lavaSource.BlueLightSpreadingBfsQueueAsync, lavaSource.RemovalChunkEffected);
            //}

            //foreach(var chunk in lavaSource.RemovalChunkEffected)
            //{
            //    chunk.UpdateMask |= UpdateChunkMask.RenderAll;
            //}
            //lavaSource.RemovalChunkEffected.Clear();
        }


        /// <summary>
        /// Current only update north, south, west and east chunk neighbors.
        /// </summary>
        /// <param name="chunk"></param>
        public void HandleLightSpreadingCrossChunkFirstTimeDrawn(Chunk chunk)
        {
            Chunk north = chunk.North;
            Chunk south = chunk.South;
            Chunk west = chunk.West;
            Chunk east = chunk.East;
            LightSource spreadingLightSource = LightSourcePool.Pool.Get();

            // neihbor chunk is north neighbor.
            if (north != null && north.HasDrawnFirstTime)
            {
                for (int y = 0; y < north.Height; y++)
                {
                    for (int x = 0; x < north.Width; x++)
                    {
                        byte redLight = north.GetRedLight(x, y, 0);
                        byte greenLight = north.GetGreenLight(x, y, 0);
                        byte blueLight = north.GetBlueLight(x, y, 0);
                        Vector3Int relativePosition = new Vector3Int(x, y, 0);
                        if (north.GetRedLight(x, y, 0) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = north.GetGlobalPosition(relativePosition), Invensity = redLight };
                            spreadingLightSource.RedLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                        if (north.GetGreenLight(x, y, 0) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = north.GetGlobalPosition(relativePosition), Invensity = greenLight };
                            spreadingLightSource.GreenLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                        if (north.GetBlueLight(x, y, 0) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = north.GetGlobalPosition(relativePosition), Invensity = blueLight };
                            spreadingLightSource.BlueLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                    }
                }
            }


            // neihbor chunk is south neighbor.
            if (south != null && south.HasDrawnFirstTime)
            {
                int maxDepth = south.Depth - 1;
                for (int y = 0; y < south.Height; y++)
                {
                    for (int x = 0; x < south.Width; x++)
                    {
                        byte redLight = south.GetRedLight(x, y, maxDepth);
                        byte greenLight = south.GetGreenLight(x, y, maxDepth);
                        byte blueLight = south.GetBlueLight(x, y, maxDepth);
                        Vector3Int relativePosition = new Vector3Int(x, y, maxDepth);
                        if (south.GetRedLight(x, y, 0) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = south.GetGlobalPosition(relativePosition), Invensity = redLight };
                            spreadingLightSource.RedLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                        if (south.GetGreenLight(x, y, 0) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = south.GetGlobalPosition(relativePosition), Invensity = greenLight };
                            spreadingLightSource.GreenLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                        if (south.GetBlueLight(x, y, 0) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = south.GetGlobalPosition(relativePosition), Invensity = blueLight };
                            spreadingLightSource.BlueLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                    }
                }
            }

            // neihbor chunk is west neighbor.
            if (west != null && west.HasDrawnFirstTime)
            {
                int maxWidth = west.Width - 1;
                for (int y = 0; y < west.Height; y++)
                {
                    for (int z = 0; z < west.Depth; z++)
                    {
                        byte redLight = west.GetRedLight(maxWidth, y, z);
                        byte greenLight = west.GetGreenLight(maxWidth, y, z);
                        byte blueLight = west.GetBlueLight(maxWidth, y, z);
                        Vector3Int relativePosition = new Vector3Int(maxWidth, y, z);
                        if (west.GetRedLight(maxWidth, y, z) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = west.GetGlobalPosition(relativePosition), Invensity = redLight };
                            spreadingLightSource.RedLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                        if (west.GetGreenLight(maxWidth, y, z) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = west.GetGlobalPosition(relativePosition), Invensity = greenLight };
                            spreadingLightSource.GreenLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                        if (west.GetBlueLight(maxWidth, y, z) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = west.GetGlobalPosition(relativePosition), Invensity = blueLight };
                            spreadingLightSource.BlueLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                    }
                }
            }


            // neihbor chunk is east neighbor.
            if (east != null && east.HasDrawnFirstTime)
            {
                for (int y = 0; y < east.Height; y++)
                {
                    for (int z = 0; z < east.Depth; z++)
                    {
                        byte redLight = east.GetRedLight(0, y, z);
                        byte greenLight = east.GetGreenLight(0, y, z);
                        byte blueLight = east.GetBlueLight(0, y, z);
                        Vector3Int relativePosition = new Vector3Int(0, y, z);
                        if (east.GetRedLight(0, y, z) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = east.GetGlobalPosition(relativePosition), Invensity = redLight };
                            spreadingLightSource.RedLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                        if (east.GetGreenLight(0, y, z) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = east.GetGlobalPosition(relativePosition), Invensity = greenLight };
                            spreadingLightSource.GreenLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                        if (east.GetBlueLight(0, y, z) > 0)
                        {
                            LightNode lightNode = new LightNode() { GlobalPosition = east.GetGlobalPosition(relativePosition), Invensity = blueLight };
                            spreadingLightSource.BlueLightSpreadingBfsQueue.Enqueue(lightNode);
                        }
                    }
                }
            }



            if (!spreadingLightSource.IsEmpty())
            {
                LightSources.Add(spreadingLightSource);
            }
            else
            {
                LightSourcePool.Pool.Release(spreadingLightSource);
            }
        }

        public async Task AddLightToLightBlockForChunkFirstTimeDrawn(Chunk chunk)
        {
            //Debug.Log("AddLightToLightBlockForChunkFirstTimeDrawn");
            LightSource lightSource = LightSourcePool.Pool.Get();
            await Task.Run(() =>
            {
                for (int i = 0; i < chunk.ChunkData.Length; i++)
                {
                    BlockID block = chunk.ChunkData[i];
                    if (block.IsLightSource() == false) continue;


                    int x = i % chunk.Width;
                    int y = (i / chunk.Width) % chunk.Height;
                    int z = i / (chunk.Width * chunk.Height);
                    LightColor blockLightColor = LightUtils.BlocksLight[(ushort)block];
                    Vector3Int blockGlobalPosition = chunk.GetGlobalPosition(new Vector3Int(x, y, z));
                    if (blockLightColor.Red > 0)
                    {
                        lightSource.RedLightSpreadingBfsQueue.Enqueue(
                            new LightNode()
                            {
                                GlobalPosition = blockGlobalPosition,
                                Invensity = blockLightColor.Red
                            });
                    }

                    // green
                    if (blockLightColor.Green > 0)
                    {
                        lightSource.GreenLightSpreadingBfsQueue.Enqueue(
                        new LightNode()
                        {
                            GlobalPosition = blockGlobalPosition,
                            Invensity = blockLightColor.Green
                        });
                    }


                    // blue
                    if (blockLightColor.Blue > 0)
                    {
                        lightSource.BlueLightSpreadingBfsQueue.Enqueue(
                            new LightNode()
                            {
                                GlobalPosition = blockGlobalPosition,
                                Invensity = blockLightColor.Blue
                            });
                    }
                }
            });



            //Debug.Log(lightSource.RedLightSpreadingBfsQueue.Count);
            if (lightSource.IsEmpty())
            {
                LightSourcePool.Pool.Release(lightSource);
            }
            else
            {
                LightSources.Add(lightSource);
            }
        }


        #endregion






        #region Liquid
        /// <summary>
        /// Pre-calculate nearest fall down direction for flow lava and water.
        /// </summary>
        /// <param name="relPosition"></param>
        /// <returns></returns>
        public int GetNearestFlowDirection(Vector3Int globalPosition)
        {
            if (TryGetChunk(globalPosition, out var chunk))
            {
                Vector3Int relPosition = chunk.GetRelativePosition(globalPosition);
                return chunk.GetNearestFlowDirection(relPosition);
            }

            return 0;
        }
        public bool CanSpread(Chunk chunk, Vector3Int relativePosition)
        {
            BlockID down = chunk.GetBlock(relativePosition.x, relativePosition.y - 1, relativePosition.z);
            BlockID west = chunk.GetBlock(relativePosition.x - 1, relativePosition.y, relativePosition.z);
            BlockID east = chunk.GetBlock(relativePosition.x + 1, relativePosition.y, relativePosition.z);
            BlockID north = chunk.GetBlock(relativePosition.x, relativePosition.y, relativePosition.z + 1);
            BlockID south = chunk.GetBlock(relativePosition.x, relativePosition.y, relativePosition.z - 1);


            return (down == BlockID.Air ||
               west == BlockID.Air ||
               east == BlockID.Air ||
               north == BlockID.Air ||
               south == BlockID.Air);

            //Debug.Log(east);
            //return down == BlockID.Air || west == BlockID.Air;

        }
        #endregion


        #region Water
        public byte GetLiquidLevel(Vector3 globalPosition)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                return chunk.GetLiquidLevel(x, y, z);
            }
            return byte.MinValue;
        }
        public void SetLiquidLevel(Vector3 globalPosition, byte level)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                chunk.SetLiquidLevel(x, y, z, level);
            }
        }

        public Vector3 GetWaterPushForce(Vector3 globalPosition)
        {
            GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                if (chunk.GetBlock(x, y, z) == BlockID.Water)
                {
                    return chunk.GetLiquidFlowDirection(new Vector3Int(x, y, z), LiquidType.Water);
                }
            }
            return Vector3.zero;
        }

        private void HandleWaterLogicAfterPlaceBlock(Chunk chunk, Vector3Int globalPosition, Vector3Int relativePosition)
        {
            BlockID currBlock = chunk.GetBlock(relativePosition);
            if (currBlock == BlockID.Water)
            {
                FluidNode waterNode = new FluidNode()
                {
                    GlobalPosition = globalPosition,
                    Level = chunk.GetLiquidLevel(relativePosition)
                };

                WaterSource waterSource = WaterSourcePool.Pool.Get();
                waterSource.WaterRemovalBfsQueue.Enqueue(waterNode);
                WaterSources.Add(waterSource);
            }

            // if above block is water -> spread above block water to another direction
            Vector3Int upperRelPosition = new Vector3Int(relativePosition.x, relativePosition.y + 1, relativePosition.z);
            BlockID upperBlock = chunk.GetBlock(upperRelPosition);
            if (upperBlock == BlockID.Water)
            {
                FluidNode aboveWaterNode = new FluidNode()
                {
                    GlobalPosition = new Vector3Int(globalPosition.x, globalPosition.y + 1, globalPosition.z),
                    Level = chunk.GetLiquidLevel(upperRelPosition)
                };

                WaterSource waterSource = WaterSourcePool.Pool.Get();
                waterSource.WaterSpreadingBfsQueue.Enqueue(aboveWaterNode);
                WaterSources.Add(waterSource);
            }
        }

        private bool NeedHandleWaterLogic()
        {
            if (WaterSources.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private async void WaterLogicHandlerAsync()
        {
            _isHandleWaterLogics = true;
            for (int i = 0; i < WaterSources.Count; i++)
            {
                if (WaterSources[i].WaterSpreadingBfsQueue.Count > 0)
                {
                    Debug.Log("Propagate water source");
                    await Water.PropagateWaterTask(WaterSources[i]);
                }
            }
            for (int i = 0; i < WaterSources.Count; i++)
            {
                if (WaterSources[i].WaterRemovalBfsQueue.Count > 0)
                {
                    await Water.RemoveWaterTask(WaterSources[i]);
                }
            }


            for (int i = 0; i < WaterSources.Count; i++)
            {
                foreach (var chunk in WaterSources[i].ChunkEffected)
                {
                    chunk.UpdateMask |= UpdateChunkMask.RenderAll;
                }
                WaterSources[i].ChunkEffected.Clear();
            }

            for (int i = 0; i < WaterSources.Count; i++)
            {
                if (WaterSources[i].IsEmpty())
                {
                    Debug.Log("Remove water source");
                    WaterSourcePool.Pool.Release(WaterSources[i]);
                    WaterSources.RemoveAt(i);
                }
            }
            _isHandleWaterLogics = false;

        }
        #endregion





        #region Lava

        private bool NeedHandleLavaLogic()
        {
            if (LavaSources.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void LavaLogicHandler()
        {
            _isHandleLavaLogics = true;
            for (int i = 0; i < LavaSources.Count; i++)
            {
                if (LavaSources[i].LavaSpreadingBfsQueue.Count > 0)
                {
                    Lava.PropagateLava(LavaSources[i]);
                    HandleLightingAfterPlaceMultipleLava(LavaSources[i]);
                }
            }
            for (int i = 0; i < LavaSources.Count; i++)
            {
                if (LavaSources[i].LavaRemovalBfsQueue.Count > 0)
                {
                    Lava.RemoveLava(LavaSources[i]);
                    HandleLightingAfterRemoveLava(LavaSources[i]);
                }
            }

            for (int i = 0; i < LavaSources.Count; i++)
            {
                foreach (var chunk in LavaSources[i].SpreadingChunkEffected)
                {
                    chunk.UpdateMask |= UpdateChunkMask.RenderAll;
                }
                LavaSources[i].SpreadingChunkEffected.Clear();
            }


            for (int i = 0; i < LavaSources.Count; i++)
            {
                if (LavaSources[i].IsEmpty())
                {
                    LavaSourcePool.Pool.Release(LavaSources[i]);
                    LavaSources.RemoveAt(i);
                }
            }
            _isHandleLavaLogics = false;
        }

        private void HandleLavaLogicAfterPlaceBlock(Chunk chunk, Vector3Int globalPosition, Vector3Int relativePosition)
        {
            BlockID currBlock = chunk.GetBlock(relativePosition);
            if (currBlock == BlockID.Lava)
            {
                FluidNode lavaNode = new FluidNode()
                {
                    GlobalPosition = globalPosition,
                    Level = chunk.GetLiquidLevel(relativePosition)
                };

                LavaSource lavaSource = LavaSourcePool.Pool.Get();
                lavaSource.LavaRemovalBfsQueue.Enqueue(lavaNode);
                LavaSources.Add(lavaSource);
                Debug.Log("Detected laval");
            }

            // if above block is water -> spread above block water to another direction
            Vector3Int upperRelPosition = new Vector3Int(relativePosition.x, relativePosition.y + 1, relativePosition.z);
            BlockID upperBlock = chunk.GetBlock(upperRelPosition);
            if (upperBlock == BlockID.Lava)
            {
                FluidNode aboveLavaNode = new FluidNode()
                {
                    GlobalPosition = new Vector3Int(globalPosition.x, globalPosition.y + 1, globalPosition.z),
                    Level = chunk.GetLiquidLevel(upperRelPosition)
                };

                LavaSource lavaSource = LavaSourcePool.Pool.Get();
                lavaSource.LavaSpreadingBfsQueue.Enqueue(aboveLavaNode);
                LavaSources.Add(lavaSource);
            }
        }
        #endregion






        #region World 
        public float GetHeatData(Vector3 globalPosition)
        {
            Vector3Int iGlobalPosition = GetBlockGPos(globalPosition);
            if (TryGetChunk(iGlobalPosition, out var chunk))
            {
                Vector3Int relativePosition = chunk.GetRelativePosition(iGlobalPosition);
                return chunk.HeatValues[chunk.IndexOf(relativePosition.x, relativePosition.z)];
            }
            return 0;
        }

        private Collider[] PreventPlaceBlockItems = new Collider[3];


        public bool PlaceBlock(Vector3 globalPosition, BlockID blockID)
        {
            if (TryGetChunk(globalPosition, out Chunk targetChunk))
            {
                if (targetChunk.HasDrawnFirstTime == false) return false;

                GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
                BlockID currBlock = targetChunk.GetBlock(x, y, z);
                if (currBlock.IsSolidOpaqueVoxel()) return false;
                if (currBlock.IsSolidTransparentVoxel()) return false;
                Vector3Int blockGlobalPosition = GetBlockGPos(globalPosition);
                Vector3Int blockRelativePosition = new Vector3Int(x, y, z);

                if (Physics.OverlapBoxNonAlloc(blockGlobalPosition + new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.4f, 0.4f, 0.4f), PreventPlaceBlockItems, Quaternion.identity) == 0)
                {
                    HandleWaterLogicAfterPlaceBlock(targetChunk, blockGlobalPosition, blockRelativePosition);
                    HandleLavaLogicAfterPlaceBlock(targetChunk, blockGlobalPosition, blockRelativePosition);

                    targetChunk.SetBlock(x, y, z, blockID);

                    HandleAmbientOcclusionAfterPlaceBlock(targetChunk, blockRelativePosition);
                    HandleLightAfterPlaceBlock(targetChunk, blockGlobalPosition, blockRelativePosition, blockID);
                    targetChunk.UpdateMask = UpdateChunkMask.RenderAll;
                    return true;
                }
            }

            return false;
        }

        public bool TryRemoveBlock(Vector3 globalPosition, out BlockID removedBlock, byte strength = 1)
        {
            if (TryGetChunk(globalPosition, out Chunk targetChunk))
            {
                Vector3Int blockRelativePosition = GlobalToRelativeBlockPosition(globalPosition);
                BlockID currBlock = targetChunk.GetBlock(blockRelativePosition);
                removedBlock = currBlock;

                if (targetChunk.HasDrawnFirstTime == false)
                    return false;
                Vector3Int blockGPosition = GetBlockGPos(globalPosition);
                if (currBlock != BlockID.Air)
                {
                    if (targetChunk.TryDestroyBlock(blockRelativePosition, strength))
                    {
                        // Sound
                        AudioManager.Instance.PlayEndDigSfx(globalPosition);

                        int maxItemCreated = 10;
                        int itemCount = 0;



                        switch (removedBlock)
                        {
                            case BlockID.Grass:
                            case BlockID.TallGrass:
                                DestroyMultipleBlocksBfs(GetBlockGPos(globalPosition), ref this._removedBlockBfsPositions, (b) => b.IsGrassType());
                                break;
                            case BlockID.Wood:
                            case BlockID.PineWood:
                                DestroyMultipleBlocksBfs(GetBlockGPos(globalPosition), ref this._removedBlockBfsPositions, (b) => b.IsTree());
                                itemCount = this._removedBlockBfsPositions.Count > maxItemCreated ? maxItemCreated : this._removedBlockBfsPositions.Count;
                                for (int i = 0; i < itemCount; i++)
                                {
                                    var item = GameFactory.CreateItem(removedBlock.ToString(), new Vector3(this._removedBlockBfsPositions[i].x + 0.5f, this._removedBlockBfsPositions[i].y + 0.5f, this._removedBlockBfsPositions[i].z + 0.5f), Vector3.zero);
                                    item.AddPhysicAfter(0.2f);
                                }
                                //var seed = GameFactory.CreateItem("OakTreeSampling", new Vector3(this._removedBlockBfsPositions[maxItemCreated].x + 0.5f, this._removedBlockBfsPositions[maxItemCreated].y + 0.5f, this._removedBlockBfsPositions[maxItemCreated].z + 0.5f), Vector3.zero);
                                //seed.AddPhysicAfter(0.2f);

                                HandleLightAfterRemoveMultipleBlocksAsync(this._removedBlockBfsPositions);
                                break;
                            case BlockID.Cactus:
                                DestroyMultipleBlocksBfs(GetBlockGPos(globalPosition), ref _removedBlockBfsPositions, (b) => b == BlockID.Cactus);
                                itemCount = this._removedBlockBfsPositions.Count > maxItemCreated ? maxItemCreated : this._removedBlockBfsPositions.Count;
                                for (int i = 0; i < itemCount; i++)
                                {
                                    var item = GameFactory.CreateItem(removedBlock.ToString(), new Vector3(this._removedBlockBfsPositions[i].x + 0.5f, this._removedBlockBfsPositions[i].y + 0.5f, this._removedBlockBfsPositions[i].z + 0.5f), Vector3.zero);
                                    item.AddPhysicAfter(0.2f);
                                }

                                HandleLightAfterRemoveMultipleBlocksAsync(this._removedBlockBfsPositions);
                                break;
                            case BlockID.Dirt:
                            case BlockID.DirtGrass:
                                targetChunk.SetBlock(blockRelativePosition, BlockID.Air);
                                Vector3 upperPosition = new Vector3(globalPosition.x, globalPosition.y + 1, globalPosition.z);
                                BlockID upperBlock = GetBlock(upperPosition);
                                if (upperBlock.IsGrassType())
                                {
                                    DestroyMultipleBlocksBfs(GetBlockGPos(upperPosition), ref this._removedBlockBfsPositions, (b) => b.IsGrassType());
                                }

                                //ItemID dirtItemID = (ItemID)removedBlock;
                                //var dirtLootableItem = GameFactory.CreateItem(((ItemID)removedBlock).ToItemString(),
                                //                   new Vector3(blockGPosition.x + 0.5f, blockGPosition.y + 0.5f, blockGPosition.z + 0.5f), Vector3.zero);

                                //if (dirtLootableItem != null)
                                //{
                                //    dirtLootableItem.AddPhysics();
                                //    dirtLootableItem.EnableFloatingRotateEffect(true);
                                //}
                                HandleAmbientOcclusionAfterRemoveBlock(targetChunk, blockRelativePosition);
                                HandleLightAfterRemoveBlock(targetChunk, blockGPosition, blockRelativePosition);
                                break;
                            default:
                                targetChunk.SetBlock(blockRelativePosition, BlockID.Air);

                                ItemID itemID = (ItemID)removedBlock;
                                var lootableItem = GameFactory.CreateItem(((ItemID)removedBlock).ToItemString(),
                                                   new Vector3(blockGPosition.x + 0.5f, blockGPosition.y + 0.5f, blockGPosition.z + 0.5f), Vector3.zero);

                                if (lootableItem != null)
                                {
                                    lootableItem.AddPhysics();
                                    lootableItem.EnableFloatingRotateEffect(true);
                                }

                                HandleAmbientOcclusionAfterRemoveBlock(targetChunk, blockRelativePosition);
                                HandleLightAfterRemoveBlock(targetChunk, blockGPosition, blockRelativePosition);
                                break;
                        }

                        if (currBlock != BlockID.Water && currBlock != BlockID.Lava)
                        {
                            for (int z = blockGPosition.z - 1; z <= blockGPosition.z + 1; z++)
                            {
                                for (int y = blockGPosition.y - 1; y <= blockGPosition.y + 1; y++)
                                {
                                    for (int x = blockGPosition.x - 1; x <= blockGPosition.x + 1; x++)
                                    {
                                        Vector3Int neighborsGPos = new Vector3Int(x, y, z);
                                        if (Main.Instance.GetBlock(neighborsGPos) == BlockID.Water)
                                        {
                                            FluidNode waterNode = new FluidNode()
                                            {
                                                GlobalPosition = neighborsGPos,
                                                Level = Main.Instance.GetLiquidLevel(neighborsGPos)
                                            };

                                            WaterSource waterSource = WaterSourcePool.Pool.Get();
                                            waterSource.WaterSpreadingBfsQueue.Enqueue(waterNode);
                                            WaterSources.Add(waterSource);
                                        }
                                        else if (Main.Instance.GetBlock(neighborsGPos) == BlockID.Lava)
                                        {
                                            FluidNode lavaNode = new FluidNode()
                                            {
                                                GlobalPosition = neighborsGPos,
                                                Level = Main.Instance.GetLiquidLevel(neighborsGPos)
                                            };

                                            LavaSource lavaSource = LavaSourcePool.Pool.Get();
                                            lavaSource.LavaSpreadingBfsQueue.Enqueue(lavaNode);
                                            LavaSources.Add(lavaSource);
                                        }
                                    }
                                }
                            }
                        }

                        targetChunk.UpdateMask = UpdateChunkMask.RenderAll;
                        return true;
                    }
                    else
                    {
                        //Debug.Log("Try remove block");
                        AudioManager.Instance.PlayDiggingSfx(globalPosition);

                        // Render crack texture when try remove block
                        targetChunk.UpdateMask |= UpdateChunkMask.RenderAll;
                        return false;
                    }
                }
            }

            removedBlock = BlockID.Air;
            return false;
        }

        public void RemoveGrassBlocks(Vector3Int globalPosition)
        {
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                var grassRemovalList = ListPool<Vector3Int>.Get();
                DestroyMultipleBlocksBfs(GetBlockGPos(globalPosition), ref grassRemovalList, (b) => b.IsGrassType());
                AudioManager.Instance.PlayEndDigSfx(globalPosition, BlockID.Grass);
                ListPool<Vector3Int>.Release(grassRemovalList);
            }
        }
        public Bounds GetBlockBounds(Vector3 globalPosition, Vector3 size)
        {
            int x = Mathf.FloorToInt(globalPosition.x);
            int y = Mathf.FloorToInt(globalPosition.y);
            int z = Mathf.FloorToInt(globalPosition.z);


            return new Bounds(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), Vector3.one);
            //AABB bound = new AABB()
            //{
            //    x = x,
            //    y = y,
            //    z = z,
            //    w = size.x,
            //    h = size.y,
            //    d = size.z,
            //};
            //return bound;
        }



        private void DestroyTree(Vector3Int globalPosition, ref List<Vector3Int> removedPositions)
        {
            removedPositions.Clear();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            Vector3Int[] neighbors = new Vector3Int[5];
            queue.Enqueue(globalPosition);
            removedPositions.Add(globalPosition);
            SetBlock(globalPosition, BlockID.Air);
            int attempts = 0;

            while (queue.Count > 0)
            {
                Vector3Int currentPosition = queue.Dequeue();
                GetVoxelNeighborPosition(currentPosition, ref neighbors);
                for (int i = 0; i < neighbors.Length; i++)
                {
                    BlockID blockID = GetBlock(neighbors[i]);
                    if (blockID.IsTree())
                    {
                        queue.Enqueue(neighbors[i]);
                        removedPositions.Add(neighbors[i]);
                        SetBlock(neighbors[i], BlockID.Air);
                    }
                }


                attempts++;
                if (attempts > 1000)
                {
                    Debug.LogWarning("Max attempt");
                    break;
                }

                //yield return null;
            }



            Debug.Log($"Attemp: {attempts}");

            void GetVoxelNeighborPosition(Vector3Int position, ref Vector3Int[] neighborPosition)
            {
                neighborPosition[0] = new Vector3Int(position.x + 1, position.y, position.z);
                neighborPosition[1] = new Vector3Int(position.x - 1, position.y, position.z);
                neighborPosition[2] = new Vector3Int(position.x, position.y, position.z + 1);
                neighborPosition[3] = new Vector3Int(position.x, position.y, position.z - 1);
                neighborPosition[4] = new Vector3Int(position.x, position.y + 1, position.z);
                //neighborPosition[5] = new Vector3Int(position.x, position.y - 1, position.z);
            }
        }

        private void DestroyMultipleBlocksBfs(Vector3Int globalPosition, ref List<Vector3Int> removedPositions, System.Func<BlockID, bool> condition)
        {
            removedPositions.Clear();
            _destroyMultipleBlockBfsQueue.Enqueue(globalPosition);
            removedPositions.Add(globalPosition);
            SetBlock(globalPosition, BlockID.Air);
            int attempts = 0;

            while (_destroyMultipleBlockBfsQueue.Count > 0)
            {
                Vector3Int currentPosition = _destroyMultipleBlockBfsQueue.Dequeue();
                GetVoxelNeighborPosition(currentPosition, ref _destroyMultipleBlocksNeighbors);
                for (int i = 0; i < _destroyMultipleBlocksNeighbors.Length; i++)
                {
                    BlockID blockID = GetBlock(_destroyMultipleBlocksNeighbors[i]);

                    if (condition(blockID))
                    {
                        _destroyMultipleBlockBfsQueue.Enqueue(_destroyMultipleBlocksNeighbors[i]);
                        removedPositions.Add(_destroyMultipleBlocksNeighbors[i]);
                        SetBlock(_destroyMultipleBlocksNeighbors[i], BlockID.Air);
                    }
                }


                attempts++;
                if (attempts > 1000)
                {
                    Debug.LogWarning("Max attempt");
                    break;
                }
            }

            //Debug.Log($"Attemp: {attempts}");

            void GetVoxelNeighborPosition(Vector3Int position, ref Vector3Int[] neighborPosition)
            {
                neighborPosition[0] = new Vector3Int(position.x + 1, position.y, position.z);
                neighborPosition[1] = new Vector3Int(position.x - 1, position.y, position.z);
                neighborPosition[2] = new Vector3Int(position.x, position.y, position.z + 1);
                neighborPosition[3] = new Vector3Int(position.x, position.y, position.z - 1);
                neighborPosition[4] = new Vector3Int(position.x, position.y + 1, position.z);
                //neighborPosition[5] = new Vector3Int(position.x, position.y - 1, position.z);
            }
        }






        // Use to detect block that has height > 1. like(door, tall grass, cactus,...)
        // This method only check downward
        public int GetBlockHeightFromOrigin(Chunk chunk, Vector3Int relativePosition)
        {
            int heightFromOrigin = 0;   // At origin
            int attempt = 0;
            BlockID blockNeedCheck = chunk.GetBlock(relativePosition);
            Vector3Int currBlockPos = relativePosition;
            while (true)
            {
                Vector3Int nextRelativePosition = new Vector3Int(currBlockPos.x, currBlockPos.y - 1, currBlockPos.z);
                if (blockNeedCheck == chunk.GetBlock(nextRelativePosition))
                {
                    currBlockPos = nextRelativePosition;
                    heightFromOrigin++;
                }
                else
                {
                    break;
                }

                if (attempt++ > 100)
                {
                    Debug.Log("Infinite loop");
                    break;
                }
            }
            return heightFromOrigin;
        }
        public int GetBlockHeightFromOrigin(Vector3Int globalPosition)
        {
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                Vector3Int relativePosition = chunk.GetRelativePosition(globalPosition);
                return GetBlockHeightFromOrigin(chunk, relativePosition);
            }
            return ChunkDimension.y;
        }
        #endregion







        #region Pathfinding
        //public void ClearAStarNodeData()
        //{
        //    AstarNodeDict.Clear();
        //}

        //public bool TryGetPathfindingNode(Vector3Int globalPosition, out AStarNode node)
        //{
        //    if (AstarNodeDict.ContainsKey(globalPosition))
        //    {
        //        node = AstarNodeDict[globalPosition];
        //        return true;
        //    }
        //    else
        //    {
        //        if (TryGetChunk(globalPosition, out var chunk))
        //        {
        //            BlockID blockID = chunk.GetBlock(GlobalToRelativeBlockPosition(globalPosition));
        //            bool walkable = blockID.Walkable();


        //            node = new AStarNode()
        //            {
        //                GlobalPosition = globalPosition,
        //                Walkable = walkable,
        //            };
        //            AstarNodeDict.Add(globalPosition, node);
        //            return true;
        //        }
        //    }

        //    node = default;
        //    return false;
        //}


        //public void SetPathfindingNode(AStarNode node)
        //{
        //    if (AstarNodeDict.ContainsKey(node.GlobalPosition))
        //    {
        //        AstarNodeDict[node.GlobalPosition] = node;
        //    }
        //    else
        //    {
        //        if (TryGetChunk(node.GlobalPosition, out var chunk))
        //        {
        //            AstarNodeDict.Add(node.GlobalPosition, node);
        //        }
        //    }
        //}
        #endregion






        #region Physics for blocks
        private async Task FallingChunks()
        {
            _fallingChunks.Clear();
            _fallingChunks = ActiveChunks.ToList();

            for (int i = 0; i < _fallingChunks.Count; i++)
            {
                if (_fallingChunks[i].FallingBlockCount > 0)
                {
                    //await SandFallingTask(_fallingChunks[i]);
                    _fallingSandTaskList.Add(SandFallingTask(_fallingChunks[i]));
                }
            }

            await Task.WhenAll(_fallingSandTaskList);
            _fallingSandTaskList.Clear();
        }
        private async Task SandFallingTask(Chunk chunk)
        {
            //Debug.Log("SandFallingTask");
            bool _hasFallingBlock = false;
            await Task.Run(() =>
            {
                for (int y = 0; y < chunk.Height; y++)
                {
                    for (int z = 0; z < chunk.Depth; z++)
                    {
                        for (int x = 0; x < chunk.Width; x++)
                        {
                            if (chunk.GetBlock(x, y, z) == BlockID.SandMine)
                            {
                                BlockID blockBelow = chunk.GetBlock(x, y - 1, z);
                                if (blockBelow == BlockID.Air || blockBelow.IsGrassType())// || chunk.GetBlock(x, y - 1, z) == BlockID.Water)
                                {
                                    _hasFallingBlock = true;
                                    chunk.RemoveBlock(x, y, z);
                                    chunk.SetBlock(x, y - 1, z, BlockID.SandMine);
                                }

                            }
                        }

                    }
                }
            });

            if (_hasFallingBlock)
                chunk.UpdateMask |= UpdateChunkMask.RenderAll;
        }
        #endregion






        #region Utilities
        public Vector3Int GlobalToRelativeBlockPosition(Vector3 globalPosition)
        {
            // Calculate the relative position within the chunk
            int relativeX = Mathf.FloorToInt(globalPosition.x) % ChunkDimension.x;
            int relativeY = Mathf.FloorToInt(globalPosition.y) % ChunkDimension.y;
            int relativeZ = Mathf.FloorToInt(globalPosition.z) % ChunkDimension.z;

            // Ensure that the result is within the chunk's dimensions
            if (relativeX < 0) relativeX += ChunkDimension.x;
            if (relativeY < 0) relativeY += ChunkDimension.y;
            if (relativeZ < 0) relativeZ += ChunkDimension.z;

            return new Vector3Int(relativeX, relativeY, relativeZ);
        }
        public void GlobalToRelativeBlockPosition(Vector3 globalPosition, out int x, out int y, out int z)
        {
            // Calculate the relative position within the chunk
            x = Mathf.FloorToInt(globalPosition.x) % ChunkDimension.x;
            y = Mathf.FloorToInt(globalPosition.y) % ChunkDimension.y;
            z = Mathf.FloorToInt(globalPosition.z) % ChunkDimension.z;

            // Ensure that the result is within the chunk's dimensions
            if (x < 0) x += ChunkDimension.x;
            if (y < 0) y += ChunkDimension.y;
            if (z < 0) z += ChunkDimension.z;
        }


        public Vector3Int GlobalToRelativeChunkPosition(Vector3 globalPosition)
        {
            int relativeX = Mathf.FloorToInt(globalPosition.x / ChunkDimension.x);
            int relativeY = Mathf.FloorToInt(globalPosition.y / ChunkDimension.y);
            int relativeZ = Mathf.FloorToInt(globalPosition.z / ChunkDimension.z);

            return new Vector3Int(relativeX, relativeY, relativeZ);
        }

        public void GlobalToRelativeChunkPosition(Vector3 globalPosition, out int x, out int y, out int z)
        {
            x = Mathf.FloorToInt(globalPosition.x / ChunkDimension.x);
            y = Mathf.FloorToInt(globalPosition.y / ChunkDimension.y);
            z = Mathf.FloorToInt(globalPosition.z / ChunkDimension.z);
        }

        public bool InSideChunkBound(Chunk chunk, Vector3 globalPosition)
        {
            //if(chunk == null) return false;
            return (globalPosition.x >= chunk.MinXGPos && globalPosition.x < chunk.MaxXGPos &&
                    globalPosition.y >= chunk.MinYGPos && globalPosition.y < chunk.MaxYGPos &&
                    globalPosition.z >= chunk.MinZGPos && globalPosition.z < chunk.MaxZGPos);
        }


        //If change this method please make sure change the same method at UnityExtension.cs.
        public long ToSpatialHashing(int x, int y, int z)
        {
            long hash = x * PRIME1 + y * PRIME2 + z * PRIME3;
            return hash;


            //return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
        }


        public Chunk GetChunkPerformance(Chunk chunk, Vector3 globalPosition)
        {
            int xOffset = (globalPosition.x < chunk.MinXGPos) ? -1 :
                (globalPosition.x >= chunk.MaxXGPos) ? 1 : 0;

            int yOffset = (globalPosition.y < chunk.MinYGPos) ? -1 :
               (globalPosition.y >= chunk.MaxYGPos) ? 1 : 0;

            int zOffset = (globalPosition.z < chunk.MinZGPos) ? -1 :
                           (globalPosition.z >= chunk.MaxZGPos) ? 1 : 0;

            Vector3Int offset = new Vector3Int(xOffset, yOffset, zOffset);
            if (offset == Vector3Int.zero)
            {
                return chunk;
            }

            return chunk.FindNeighbor(offset);
        }

        public Vector3Int GetBlockGPos(Vector3 globalPosition)
        {
            return new Vector3Int(Mathf.FloorToInt(globalPosition.x),
                                  Mathf.FloorToInt(globalPosition.y),
                                  Mathf.FloorToInt(globalPosition.z));
        }


        // Get first air block count from (MAX_CHUNK_HEIGHT to MIN_CHUNK_HEIGHT).
        public Vector3Int GetFirstAirBLockPosition(Vector3 globalPosition)
        {
            Vector3Int iGlobalPosition = globalPosition.ToVector3Int();
            if (TryGetChunk(globalPosition, out Chunk chunk))
            {
                Vector3Int relativePosition = chunk.GetRelativePosition(iGlobalPosition);
                for (int y = ChunkDimension.y - 1; y >= 0; y--)
                {
                    if (chunk.GetBlock(relativePosition.x, y, relativePosition.z) != BlockID.Air)
                    {
                        return chunk.GetGlobalPosition(new Vector3Int(relativePosition.x, y + 1, relativePosition.z));
                    }
                }
            }
            return iGlobalPosition;
        }
        #endregion



        #region Testing
        //public bool PlaceRandomLightBlock(Vector3 globalPosition)
        //{
        //    Chunk targetChunk = GetChunk(globalPosition);
        //    if (targetChunk.HasDrawnFirstTime == false)
        //        return false;

        //    GlobalToRelativeBlockPosition(globalPosition, out int x, out int y, out int z);
        //    BlockID currBlock = targetChunk.GetBlock(x, y, z);
        //    if (currBlock.IsSolidOpaqueVoxel()) return false;
        //    if (currBlock.IsSolidTransparentVoxel()) return false;
        //    Vector3Int blockGPosition = GetBlockGPos(globalPosition);

        //    if (Physics.OverlapBoxNonAlloc(blockGPosition + new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.4f, 0.4f, 0.4f), PreventPlaceBlockItems, Quaternion.identity) == 0)
        //    {
        //        if (currBlock == BlockID.Water)
        //        {
        //            FluidNode waterNode = new FluidNode()
        //            {
        //                GlobalPosition = blockGPosition,
        //                Level = targetChunk.GetLiquidLevel(x, y, z)
        //            };
        //            targetChunk.WaterRemovalBfsQueue.Enqueue(waterNode);
        //        }

        //        // if above block is water -> spread above block water to another direction
        //        if (targetChunk.GetBlock(new Vector3Int(x, y + 1, z)) == BlockID.Water)
        //        {
        //            FluidNode aboveWaterNode = new FluidNode()
        //            {
        //                GlobalPosition = blockGPosition + Vector3Int.up,
        //                Level = targetChunk.GetLiquidLevel(x, y + 1, z)
        //            };
        //            targetChunk.WaterSpreadingBfsQueue.Enqueue(aboveWaterNode);
        //        }


        //        targetChunk.SetBlock(x, y, z, BlockID.RedLight);
        //        AfterPlaceRandomLightBlock(blockGPosition, BlockID.RedLight);
        //        return true;
        //    }


        //    return false;
        //}

        //private async void AfterPlaceRandomLightBlock(Vector3Int blockGPosition, BlockID blockID)
        //{
        //    Debug.Log($"After place block: {blockID}");

        //    _blockLightRemovalBfsQueue.Enqueue(new LightNode() { GlobalPosition = blockGPosition, Invensity = GetLightData(blockGPosition) });
        //    await LightCalculator.RemoveBlockLightAsync(_blockLightRemovalBfsQueue, chunksNeedUpdate);

        //    int randValue = Random.Range(0, 3);
        //    byte red;
        //    byte green;
        //    byte blue;
        //    switch (randValue)
        //    {
        //        default:
        //        case 0:
        //            red = 15;
        //            green = (byte)Random.Range(0, 16);
        //            blue = (byte)Random.Range(0, 16);
        //            break;
        //        case 1:
        //            red = (byte)Random.Range(0, 16);
        //            green = 15;
        //            blue = (byte)Random.Range(0, 16);
        //            break;
        //        case 2:
        //            red = (byte)Random.Range(0, 16);
        //            green = (byte)Random.Range(0, 16);
        //            blue = 15;
        //            break;
        //    }


        //    _blockLightBfsQueue.Enqueue(
        //        new LightNode()
        //        {
        //            GlobalPosition = blockGPosition,
        //            Invensity = LightUtils.GetLightData(0, Red, Green, Blue)
        //        });
        //    await LightCalculator.PropagateBlockLightAsync(_blockLightBfsQueue, chunksNeedUpdate);


        //    foreach (var c in chunksNeedUpdate)
        //    {
        //        c.UpdateMask |= UpdateChunkMask.RenderAll;
        //    }
        //    chunksNeedUpdate.Clear();
        //}

        #endregion
    }

    public struct PlaceBlockData
    {
        public Vector3Int GlobalPosition;
        public BlockID BlockID;

        public PlaceBlockData(Vector3Int globalPosition, BlockID blockID)
        {
            this.GlobalPosition = globalPosition;
            this.BlockID = blockID;
        }
    }
}

