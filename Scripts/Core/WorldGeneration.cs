using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using PixelMiner.Utilities;
using PixelMiner.Enums;
using System.Buffers;
using UnityEngine.SceneManagement;

namespace PixelMiner.Core
{
    /* 
     * 1. Generate Chunk (Instantiate, Init data, 
     * 2. Generate and load data into chunk (data include: Height map, heat map, moisture map,...)
     * 3. Draw chunk - Create mesh object can be shown in Unity Engine.
     */
    public class WorldGeneration : MonoBehaviour
    {
        public static WorldGeneration Instance { get; private set; }
        public event System.Action OnWorldLoadFinished;
        public System.Action<float> OnFirstTimeGenerationProgress;

        #region Fileds and Variables
        [FoldoutGroup("References"), SerializeField] private Chunk _chunkPrefab;
        [FoldoutGroup("References"), SerializeField] private Transform _chunkParent;

        // Height map
        // ==========
        [InfoBox("Height map noise settings")]
        [FoldoutGroup("Height map"), Indent(1)] public int Octaves = 6;
        [FoldoutGroup("Height map"), Indent(1)] public float Frequency = 0.02f;
        [FoldoutGroup("Height map"), Indent(1)] public float Lacunarity = 2.0f;
        [FoldoutGroup("Height map"), Indent(1)] public float Persistence = 0.5f;

        [InfoBox("Height map threshold"), Space(10)]
        [FoldoutGroup("Height map")]
        public float DeepWater = 0.2f;
        [FoldoutGroup("Height map")]
        public float Water = 0.4f;
        [FoldoutGroup("Height map")]
        public float Sand = 0.5f;
        [FoldoutGroup("Height map")]
        public float Grass = 0.7f;
        [FoldoutGroup("Height map")]
        public float Forest = 0.8f;
        [FoldoutGroup("Height map")]
        public float Rock = 0.9f;
        [FoldoutGroup("Height map")]
        public float Snow = 1;
        private FastNoiseLite _heightSimplex;
        private FastNoiseLite _heightVoronoi;



        // Heapmap
        // ======
        [InfoBox("Heat map noise settings")]
        [FoldoutGroup("Heatmap"), Indent(1)] public int HeatOctaves = 4;
        [FoldoutGroup("Heatmap"), Indent(1)] public float HeatFrequency = 0.02f;
        [FoldoutGroup("Heatmap"), Indent(1)] public float HeatLacunarity = 2.0f;
        [FoldoutGroup("Heatmap"), Indent(1)] public float HeatPersistence = 0.5f;

        [InfoBox("Gradient map"), Space(10)]
        [FoldoutGroup("Heatmap"), Indent(1)] public ushort GradientHeatmapSize = 256;
        [FoldoutGroup("Heatmap"), Indent(1), Range(0f, 1f)] public float HeatMapBlendFactor = 0.5f;

        [InfoBox("Heat map threshold"), Space(10)]
        [FoldoutGroup("Heatmap")] public float ColdestValue = 0.1f;
        [FoldoutGroup("Heatmap")] public float ColderValue = 0.2f;
        [FoldoutGroup("Heatmap")] public float ColdValue = 0.4f;
        [FoldoutGroup("Heatmap")] public float WarmValue = 0.6f;
        [FoldoutGroup("Heatmap")] public float WarmerValue = 0.8f;
        [FoldoutGroup("Heatmap")] public float WarmestValue = 1.0f;
        private FastNoiseLite _heatSimplex;
        private FastNoiseLite _heatVoronoi;



        // Moisture map
        // ======
        [InfoBox("Moisture map noise settings")]
        [FoldoutGroup("Moisture map"), Indent(1)] public int MoistureOctaves = 4;
        [FoldoutGroup("Moisture map"), Indent(1)] public float MoistureFrequency = 0.03f;
        [FoldoutGroup("Moisture map"), Indent(1)] public float MoistureLacunarity = 2.0f;
        [FoldoutGroup("Moisture map"), Indent(1)] public float MoisturePersistence = 0.5f;
        [InfoBox("Moisture map noise settings"), Space(10)]
        [FoldoutGroup("Moisture map")] public float DryestValue = 0.22f;
        [FoldoutGroup("Moisture map")] public float DryerValue = 0.35f;
        [FoldoutGroup("Moisture map")] public float DryValue = 0.55f;
        [FoldoutGroup("Moisture map")] public float WetValue = 0.75f;
        [FoldoutGroup("Moisture map")] public float WetterValue = 0.85f;
        [FoldoutGroup("Moisture map")] public float WettestValue = 1.0f;
        private FastNoiseLite _moistureSimplex;
        private FastNoiseLite _moistureVoronoi;



        // River
        // =====
        [FoldoutGroup("River"), Indent(1)] public int RiverOctaves = 4;
        [FoldoutGroup("River"), Indent(1)] public float RiverFrequency = 0.02f;
        [FoldoutGroup("River"), Indent(1)] public float RiverLacunarity = 2.0f;
        [FoldoutGroup("River"), Indent(1)] public float RiverPersistence = 0.5f;
        private FastNoiseLite _riverSimplex;
        private FastNoiseLite _riverVoronoi;



        [Header("Color")]
        // Height
        public static Color RiverColor = new Color(30 / 255f, 120 / 255f, 200 / 255f, 1);
        public static Color DeepColor = new Color(0, 0, 0.5f, 1);
        public static Color ShallowColor = new Color(25 / 255f, 25 / 255f, 150 / 255f, 1);
        public static Color SandColor = new Color(240 / 255f, 240 / 255f, 64 / 255f, 1);
        public static Color GrassColor = new Color(50 / 255f, 220 / 255f, 20 / 255f, 1);
        public static Color ForestColor = new Color(16 / 255f, 160 / 255f, 0, 1);
        public static Color RockColor = new Color(0.5f, 0.5f, 0.5f, 1);
        public static Color SnowColor = new Color(1, 1, 1, 1);
        // Heat
        public static Color ColdestColor = new Color(0, 1, 1, 1);
        public static Color ColderColor = new Color(170 / 255f, 1, 1, 1);
        public static Color ColdColor = new Color(0, 229 / 255f, 133 / 255f, 1);
        public static Color WarmColor = new Color(1, 1, 100 / 255f, 1);
        public static Color WarmerColor = new Color(1, 100 / 255f, 0, 1);
        public static Color WarmestColor = new Color(241 / 255f, 12 / 255f, 0, 1);
        // Moisture
        public static Color Dryest = new Color(255 / 255f, 139 / 255f, 17 / 255f, 1);
        public static Color Dryer = new Color(245 / 255f, 245 / 255f, 23 / 255f, 1);
        public static Color Dry = new Color(80 / 255f, 255 / 255f, 0 / 255f, 1);
        public static Color Wet = new Color(85 / 255f, 255 / 255f, 255 / 255f, 1);
        public static Color Wetter = new Color(20 / 255f, 70 / 255f, 255 / 255f, 1);
        public static Color Wettest = new Color(0 / 255f, 0 / 255f, 100 / 255f, 1);


        // Biomes
        public static Color Ice = Color.white;
        public static Color Desert = new Color(238 / 255f, 218 / 255f, 130 / 255f, 1);
        public static Color Savanna = new Color(177 / 255f, 209 / 255f, 110 / 255f, 1);
        public static Color TropicalRainforest = new Color(66 / 255f, 123 / 255f, 25 / 255f, 1);
        public static Color Tundra = new Color(96 / 255f, 131 / 255f, 112 / 255f, 1);
        public static Color TemperateRainforest = new Color(29 / 255f, 73 / 255f, 40 / 255f, 1);
        public static Color Grassland = new Color(164 / 255f, 225 / 255f, 99 / 255f, 1);
        public static Color SeasonalForest = new Color(73 / 255f, 100 / 255f, 35 / 255f, 1);
        public static Color BorealForest = new Color(95 / 255f, 115 / 255f, 62 / 255f, 1);
        public static Color Woodland = new Color(139 / 255f, 175 / 255f, 90 / 255f, 1);



        // BIOMES
        public BiomeType[,] BiomeTable = new BiomeType[6, 6]
        {
              //COLDEST         //COLDER            //COLD                  //HOT                          //HOTTER                       //HOTTEST
		    { BiomeType.Snow,    BiomeType.Snow,   BiomeType.Plains,      BiomeType.Plains,             BiomeType.Desert,              BiomeType.Desert },              //DRYEST
		    { BiomeType.Snow,    BiomeType.Snow,   BiomeType.Plains,      BiomeType.Plains,             BiomeType.Desert,              BiomeType.Desert },              //DRYER
		    { BiomeType.Snow,    BiomeType.Snow,   BiomeType.Plains,      BiomeType.Plains,             BiomeType.Desert,              BiomeType.Desert },              //DRY
		    { BiomeType.Snow,    BiomeType.Snow,   BiomeType.Plains,      BiomeType.Plains,             BiomeType.Desert,              BiomeType.Desert },              //WET
		    { BiomeType.Snow,    BiomeType.Snow,   BiomeType.Plains,      BiomeType.Forest,             BiomeType.Forest,              BiomeType.Desert },              //WETTER
		    { BiomeType.Snow,    BiomeType.Snow,   BiomeType.Forest,      BiomeType.Forest,             BiomeType.Forest,              BiomeType.Forest }               //WETTEST
        };



        // Lava pool
        private FastNoiseLite _lavaPoolSimplex;



        // Mine noise
        private int _mineOctave = 4;
        private float _mineFrequency = 0.02f;
        private float _mineLacunarity = 2.0f;
        private float _minePersistence = 0.5f;
        private FastNoiseLite _mineHeightNoise;




        // Density noise
        private FastNoiseLite _grassNoiseDistribute;
        private float _grassNoiseFrequency = 0.0045f;

        private FastNoiseLite _treeNoiseDistribute;
        private float _treeNoiseFrequency = 0.0085f;

        private FastNoiseLite _shrubNoiseDistribute;
        private float _shrubNoiseFrequency = 0.0085f;


        private FastNoiseLite _cactusNoiseDistribute;
        private float _cactusNoiseFrequency = 0.0295f;


        // Cached
        [HideInInspector] private int _groundLevel = 7;
        [HideInInspector] private int _underGroundLevel = 0;
        private Vector3Int _chunkDimension;
        private byte _calculateNoiseRangeSampleMultiplier = 15;  // 50 times. 50 * 100000 = 5 mil times.
        private int _calculateNoiseRangeCount = 1000000;    // 1 mil times.
        private float _minWorldNoiseValue = float.MaxValue;
        private float _maxWorldNoiseValue = float.MinValue;
        private Main _main;
        private WorldLoading _worldLoading;
        private Vector3 _initializeWorldPosition;
        #endregion


        private float _voronoiFrequency = 0.006f * 2;
        public AnimationCurve LightAnimCurve;


        #region Properties
        [FoldoutGroup("World Properties"), Indent(1), ReadOnly, ShowInInspector] public int Seed { get; private set; }
        public bool OnLoadingFinished { get; private set; } = false;
        #endregion
        /*
            World generation slider range.
            0.0f -> 0.1f: Calculate noise range from specific seed.
            0.1f -> 1.0f: Loading chunk data.
         */


        #region Unity life cycle methods
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);
    
   

            _minWorldNoiseValue = float.MaxValue;
            _maxWorldNoiseValue = float.MinValue;

        

        }

        private async void Start()
        {
            // Get seed
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name == Loader.Scene.MenuScene.ToString())
            {
                Seed = GameRules.MENU_SCENE_SEED;
            }
            else if (currentScene.name == Loader.Scene.GameplayScene.ToString())
            {
                Seed = WorldGenUtilities.StringToSeed(PlayerPrefs.GetString(GameRules.SEED));
            }


            _main = Main.Instance;
            _worldLoading = WorldLoading.Instance;
            _chunkDimension = _main.ChunkDimension;
            //Seed = WorldGenUtilities.StringToSeed(_main.SeedInput);
            UnityEngine.Random.InitState(Seed);
            if (_main.Players.Count > 0)
            {
                _initializeWorldPosition = _main.Players[0].transform.position;
            }
            else
            {
                //_initializeWorldPosition = Camera.main.transform.position;
                _initializeWorldPosition = Main.Instance.InitWorldPosition;
            }


            // Init noise module
            // HEIGHT
            // ------
            _heightSimplex = new FastNoiseLite(Seed);
            _heightSimplex.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _heightSimplex.SetFractalType(FastNoiseLite.FractalType.FBm);
            _heightSimplex.SetFrequency(Frequency);
            _heightSimplex.SetFractalLacunarity(Lacunarity);
            _heightSimplex.SetFractalGain(Persistence);

            _heightVoronoi = new FastNoiseLite(Seed);
            _heightVoronoi.SetFrequency(_voronoiFrequency);
            _heightVoronoi.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            _heightVoronoi.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);



            // HEAT
            // ----
            _heatSimplex = new FastNoiseLite(Seed);
            _heatSimplex.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _heatSimplex.SetFractalType(FastNoiseLite.FractalType.FBm);
            _heatSimplex.SetFrequency(HeatFrequency);
            _heatSimplex.SetFractalLacunarity(HeatLacunarity);
            _heatSimplex.SetFractalGain(HeatPersistence);

            _heatVoronoi = new FastNoiseLite(Seed);
            _heatVoronoi.SetFrequency(_voronoiFrequency);
            _heatVoronoi.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            _heatVoronoi.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);




            // MOISTURE
            // --------
            _moistureSimplex = new FastNoiseLite(Seed);
            _moistureSimplex.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _moistureSimplex.SetFractalType(FastNoiseLite.FractalType.FBm);
            _moistureSimplex.SetFrequency(MoistureFrequency);
            _moistureSimplex.SetFractalLacunarity(MoistureLacunarity);
            _moistureSimplex.SetFractalGain(MoisturePersistence);

            _moistureVoronoi = new FastNoiseLite(Seed);
            _moistureVoronoi.SetFrequency(_voronoiFrequency);
            _moistureVoronoi.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            _moistureVoronoi.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);




            // RIVER
            // -----
            _riverSimplex = new FastNoiseLite(Seed + 3);
            _riverSimplex.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _riverSimplex.SetFractalType(FastNoiseLite.FractalType.FBm);
            _riverSimplex.SetFrequency(RiverFrequency);
            _riverSimplex.SetFractalOctaves(RiverOctaves);
            _riverSimplex.SetFractalLacunarity(RiverLacunarity);
            _riverSimplex.SetFractalGain(RiverPersistence);

            _riverVoronoi = new FastNoiseLite(Seed + 3);
            _riverVoronoi.SetFrequency(RiverFrequency);
            _riverVoronoi.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            _riverVoronoi.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);





            // LAVA POOL
            // ------
            _lavaPoolSimplex = new FastNoiseLite(Seed);
            _lavaPoolSimplex.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
            _lavaPoolSimplex.SetFractalType(FastNoiseLite.FractalType.FBm);
            _lavaPoolSimplex.SetFractalOctaves(1);
            _lavaPoolSimplex.SetFrequency(0.05f);
            _lavaPoolSimplex.SetFractalLacunarity(2.0f);
            _lavaPoolSimplex.SetFractalGain(0.5f);



            // MINE
            // -----
            _mineHeightNoise = new FastNoiseLite(Seed * 15777547);
            _mineHeightNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _mineHeightNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
            _mineHeightNoise.SetFrequency(_mineFrequency);
            _mineHeightNoise.SetFractalOctaves(_mineOctave);
            _mineHeightNoise.SetFractalLacunarity(_mineLacunarity);
            _mineHeightNoise.SetFractalGain(_minePersistence);




            // DISTRIBUTION
            _grassNoiseDistribute = new FastNoiseLite(Seed);
            _grassNoiseDistribute.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _grassNoiseDistribute.SetFractalType(FastNoiseLite.FractalType.FBm);
            _grassNoiseDistribute.SetFrequency(_grassNoiseFrequency);
            _grassNoiseDistribute.SetFractalOctaves(1);


            _treeNoiseDistribute = new FastNoiseLite(Seed);
            _treeNoiseDistribute.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _treeNoiseDistribute.SetFractalType(FastNoiseLite.FractalType.FBm);
            _treeNoiseDistribute.SetFrequency(_treeNoiseFrequency);
            _treeNoiseDistribute.SetFractalOctaves(1);



            _shrubNoiseDistribute = new FastNoiseLite(Seed);
            _shrubNoiseDistribute.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _shrubNoiseDistribute.SetFractalType(FastNoiseLite.FractalType.FBm);
            _shrubNoiseDistribute.SetFrequency(_shrubNoiseFrequency);
            _shrubNoiseDistribute.SetFractalOctaves(1);


            _cactusNoiseDistribute = new FastNoiseLite(Seed + 1);
            _cactusNoiseDistribute.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _cactusNoiseDistribute.SetFractalType(FastNoiseLite.FractalType.FBm);
            _cactusNoiseDistribute.SetFrequency(_cactusNoiseFrequency);
            _cactusNoiseDistribute.SetFractalOctaves(1);

            // World Initialization
            try
            {
                await InitWorldAsyncInSequence(_worldLoading.LastChunkFrame.x,
                                 _worldLoading.LastChunkFrame.y,
                                 _worldLoading.LastChunkFrame.z,
                                 widthInit: _worldLoading.InitWorldWidth,
                                 heightInit: _worldLoading.InitWorldHeight,
                                 depthInit: _worldLoading.InitWorldDepth, () =>
                                 {

                                 }, Application.exitCancellationToken);

                OnWorldLoadFinished?.Invoke();
                Vector3Int _currentFrame = new Vector3Int(Mathf.FloorToInt(_initializeWorldPosition.x / Main.Instance.ChunkDimension[0]), 0,
                Mathf.FloorToInt(_initializeWorldPosition.z / Main.Instance.ChunkDimension[2]));
                Vector3Int LastChunkFrame = _currentFrame;
                await WorldLoading.Instance.InitializeLoadChunksAroundPositionTask(LastChunkFrame.x, LastChunkFrame.y, LastChunkFrame.z,
                                                            offsetWidth: WorldLoading.Instance.LoadChunkOffsetWidth,
                                                            offsetHeight: WorldLoading.Instance.LoadChunkOffsetHeight,
                                                            offsetDepth: WorldLoading.Instance.LoadChunkOffsetDepth,
                                                            Application.exitCancellationToken);
                WorldLoading.Instance.OnLoadingGameFinish?.Invoke();
                OnLoadingFinished = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Initialize world was cancelled.");
                Debug.LogWarning($"{ex}");
            }


            //await InitWorldAsyncInSequence(_worldLoading.LastChunkFrame.x,
            //                    _worldLoading.LastChunkFrame.y,
            //                    _worldLoading.LastChunkFrame.z,
            //                    widthInit: _worldLoading.InitWorldWidth,
            //                    heightInit: _worldLoading.InitWorldHeight,
            //                    depthInit: _worldLoading.InitWorldDepth, () =>
            //                    {

            //                    }, Application.exitCancellationToken);

            //OnWorldLoadFinished?.Invoke();
            //Vector3Int _currentFrame = new Vector3Int(Mathf.FloorToInt(_initializeWorldPosition.x / Main.Instance.ChunkDimension[0]), 0,
            //Mathf.FloorToInt(_initializeWorldPosition.z / Main.Instance.ChunkDimension[2]));
            //Vector3Int LastChunkFrame = _currentFrame;
            //await WorldLoading.Instance.InitializeLoadChunksAroundPositionTask(LastChunkFrame.x, LastChunkFrame.y, LastChunkFrame.z,
            //                                            offsetWidth: WorldLoading.Instance.LoadChunkOffsetWidth,
            //                                            offsetHeight: WorldLoading.Instance.LoadChunkOffsetHeight,
            //                                            offsetDepth: WorldLoading.Instance.LoadChunkOffsetDepth,
            //                                            Application.exitCancellationToken);
            //WorldLoading.Instance.OnLoadingGameFinish?.Invoke();
            //OnLoadingFinished = true;

        }

        private void OnDestroy()
        {
            
        }
        #endregion






        #region Compute noise range [min, max] when start
        public async Task ComputeNoiseRangeAsyncInSequence(CancellationToken token)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            float minNoiseValue = float.MaxValue;
            float maxNoiseValue = float.MinValue;
            System.Random rand = new System.Random(Seed);

            await Task.Run(() =>
            {
                for (int i = 0; i < _calculateNoiseRangeCount; i++)
                {
                    float x = rand.Next();
                    float y = rand.Next();
                    float noiseValue = (float)_heightSimplex.GetNoise(x, y); // Generate noise value

                    // Update min and max values
                    if (noiseValue < minNoiseValue)
                        minNoiseValue = noiseValue;
                    if (noiseValue > maxNoiseValue)
                        maxNoiseValue = noiseValue;

                    // 46655 = 6**6 - 1 (use & operator compare to improve performance)
                    if ((i & 46655) == 0)
                    {
                        float progress = (float)i / _calculateNoiseRangeCount;
                        float mapProgress = MathHelper.Map(progress, 0f, 1f, 0.0f, 0.3f);
                        //UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        //{
                        //    //UIGameManager.Instance.CanvasWorldGen.SetWorldGenSlider(mapProgress);
                        //});
                    }
                }

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
            }, token);


            _minWorldNoiseValue = minNoiseValue;
            _maxWorldNoiseValue = maxNoiseValue;

            sw.Stop();
            Debug.Log($"Compute noise time: {sw.ElapsedMilliseconds / 1000f} s");
        }
        public async Task ComputeNoiseRangeAsyncInParallel()
        {
            float minNoiseValue = float.MaxValue;
            float maxNoiseValue = float.MinValue;
            int completedTaskCount = 0;

            //UIGameManager.Instance.CanvasWorldGen.SetWorldGenSlider(0);

            Task<MinMax>[] tasks = new Task<MinMax>[_calculateNoiseRangeSampleMultiplier];
            List<Task> continuationTasks = new List<Task>();
            for (int i = 0; i < tasks.Length; i++)
            {
                int newSeed = WorldGenUtilities.GenerateNewSeed(Seed + i);
                bool updateLoadingUI = i == 0 ? true : false;
                tasks[i] = ComputeNoiseRangeAsync(newSeed);

                Task continuationTask = tasks[i].ContinueWith(task =>
                {
                    // Increment the completed tasks counter
                    Interlocked.Increment(ref completedTaskCount);

                    // Play Slider UI
                    float progress = (float)completedTaskCount / tasks.Length;
                    float mapProgress = MathHelper.Map(progress, 0f, 1f, 0.0f, 0.1f);
                    //UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    //{
                    //    //UIGameManager.Instance.CanvasWorldGen.SetWorldGenSlider(mapProgress);
                    //});
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

                continuationTasks.Add(continuationTask);
            }
            await Task.WhenAll(tasks);
            await Task.WhenAll(continuationTasks);

            for (int i = 0; i < tasks.Length; i++)
            {
                float minValue = tasks[i].Result.MinValue;
                float maxValue = tasks[i].Result.MaxValue;

                if (minNoiseValue > minValue)
                    minNoiseValue = minValue;
                if (maxNoiseValue < maxValue)
                    maxNoiseValue = maxValue;
            }
            _minWorldNoiseValue = minNoiseValue;
            _maxWorldNoiseValue = maxNoiseValue;
        }
        struct MinMax
        {
            public float MinValue;
            public float MaxValue;
        }
        private async Task<MinMax> ComputeNoiseRangeAsync(int seed)
        {
            MinMax minMax = new MinMax()
            {
                MinValue = float.MaxValue,
                MaxValue = float.MinValue
            };
            System.Random rand = new System.Random(seed);
            int sampleCount = 100000;

            await Task.Run(() =>
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    float x = rand.Next();
                    float y = rand.Next();
                    float noiseValue = (float)_heightSimplex.GetNoise(x, y); // Generate noise value

                    // Update min and max values
                    if (noiseValue < minMax.MinValue)
                        minMax.MinValue = noiseValue;
                    if (noiseValue > minMax.MaxValue)
                        minMax.MaxValue = noiseValue;
                }
            });

            return minMax;
        }
        #endregion Compute Noise Range


        #region INIT CHUNK DATA
        private async Task InitWorldAsyncInSequence(int initFrameX, int initFrameY, int initFrameZ, byte widthInit, byte heightInit, byte depthInit, System.Action onFinished, CancellationToken token)
        {
            Debug.Log("InitWorldAsyncInSequence");
            await ComputeNoiseRangeAsyncInParallel();

            //Debug.Log($"min: {_minWorldNoiseValue}");
            //Debug.Log($"max: {_maxWorldNoiseValue}");
            int totalChunkLoad = 0;
            List<Task<Chunk>> loadChunkTask = new List<Task<Chunk>>();
            int totalChunkNeedLoad = ((widthInit * 2 + 1) * heightInit * (depthInit * 2 + 1)) * 2;
            int chunkLoaded = 0;
            float progress = 0.0f;
            //Debug.Log($"total: {totalChunkNeedLoad}");

            for (int x = initFrameX - widthInit; x <= initFrameX + widthInit; x++)
            {
                for (int y = initFrameY - heightInit; y <= initFrameY + heightInit; y++)
                {
                    for (int z = initFrameZ - depthInit; z <= initFrameZ + depthInit; z++)
                    {
                        if (y < 0) continue;


                        loadChunkTask.Add(GenerateNewChunkTask(x, y, z, _main.ChunkDimension, token));
                   
                        if (totalChunkLoad > 10)
                        {
                            totalChunkLoad = 0;
                            await Task.WhenAll(loadChunkTask);
                        }

                        chunkLoaded++;
                        progress = chunkLoaded / (float)totalChunkNeedLoad;
                        OnFirstTimeGenerationProgress?.Invoke(progress);
                    }
                }
            }
            await Task.WhenAll(loadChunkTask);



            for (int i = 0; i < loadChunkTask.Count; i++)
            {
                _worldLoading.LoadChunk(loadChunkTask[i].Result);
                _worldLoading.UnloadChunk(loadChunkTask[i].Result);
            }
            onFinished?.Invoke();
        }

        public async Task<Chunk> GenerateNewChunkTask(int frameX, int frameY, int frameZ, Vector3Int chunkDimension, CancellationToken token)
        {
            //Debug.Log($"GenerateNewChunk: {frameX}  {frameY}  {frameZ}");

            Vector3Int frame = new Vector3Int(frameX, frameY, frameZ);
            Vector3 worldPosition = frame * new Vector3Int(chunkDimension[0], chunkDimension[1], chunkDimension[2]);
            Chunk newChunk = Instantiate(_chunkPrefab, worldPosition, Quaternion.identity, _chunkParent.transform);
            newChunk.Init(frameX, frameY, frameZ, chunkDimension[0], chunkDimension[1], chunkDimension[2], _grassNoiseDistribute);

            ChunkGenData chunkData = ChunkDataPool.Pool.Get();
            if (frameY <= 0)
            {
                Task<float[]> heightTask = GetHeightMapDataAsync(newChunk.FrameX, newChunk.FrameZ, chunkDimension[0], chunkDimension[2], token);
                Task<float[]> heatTask = GetFractalHeatMapDataAsync(newChunk.FrameX, newChunk.FrameZ, chunkDimension[0], chunkDimension[2], token);
                Task<float[]> moistureTask = GetMoistureMapDataAsync(newChunk.FrameX, newChunk.FrameZ, chunkDimension[0], chunkDimension[2], token);
                Task<float[]> riverTask = GetRiverDataAsync(newChunk.FrameX, newChunk.FrameZ, chunkDimension[0], chunkDimension[2], token);
                await Task.WhenAll(heightTask, heatTask, moistureTask, riverTask).ConfigureAwait(false);

                chunkData.HeightValues = heightTask.Result;
                newChunk.HeatValues = heatTask.Result;
                chunkData.MoistureValues = moistureTask.Result;
                chunkData.RiverValues = riverTask.Result;

                Task loadHeatTask = LoadHeatMapDataAsync(newChunk, newChunk.HeatValues, token);
                Task loadMoistureTask = LoadMoistureMapDataAsync(newChunk, chunkData.MoistureValues, token);
                await Task.WhenAll(loadHeatTask, loadMoistureTask);
                await GenerateBiomeMapDataAsync(newChunk, chunkData.HeightValues, token);


                // River
                // ----
                //BiomeType[] riverBiomes = new BiomeType[riverValues.Length];
                for (int i = 0; i < chunkData.RiverValues.Length; i++)
                {
                    if (chunkData.RiverValues[i] > 0.6f && newChunk.BiomesData[i] != BiomeType.Ocean)
                    {
                        newChunk.RiverBiomes[i] = BiomeType.River;
                    }
                    else
                    {
                        newChunk.RiverBiomes[i] = BiomeType.Other;
                    }
                }
            }
            else
            {
                //float[] heatValues = await GetFractalHeatMapDataAsync(newChunk.FrameX, newChunk.FrameZ, chunkDimension[0], chunkDimension[2]);
                //float[] moistureValues = await GetMoistureMapDataAsync(newChunk.FrameX, newChunk.FrameZ, chunkDimension[0], chunkDimension[2]);

                await LoadHeightMapDataAsync(newChunk, BlockID.Air, token);
                //await LoadHeatMapDataAsync(newChunk, heatValues);
                //await LoadMoistureMapDataAsync(newChunk, moistureValues);
                //await GenerateBiomeMapDataAsync(newChunk);
            }

            ChunkDataPool.Pool.Release(chunkData);
            return newChunk;
        }
        #endregion





        #region DRAW CHUNK

        public async Task UpdateChunkWhenHasAllNeighborsTask(Chunk chunk, CancellationToken token)
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            // Dig river
            // ---------
            ChunkGenData chunkData = ChunkDataPool.Pool.Get();
            chunkData.HeightValues = await GetHeightMapDataAsync(chunk.FrameX, chunk.FrameZ, chunk.Width, chunk.Depth, token);

            GetRiverBfsNodes(chunk, chunk.Width, chunk.Depth);
            if (chunk.RiverBfsQueue.Count > 0)
            {
                await DigRiverAsync(chunk, chunk.RiverBfsQueue, token);
            }
            await LoadChunkMapDataAsync(chunk, chunkData.HeightValues, token);
            ChunkDataPool.Pool.Release(chunkData);

            // Place lava pool first
            await PlaceLavaPoolAsync(chunk, token);

            await Task.WhenAll(
                 PlaceGrassAsync(chunk, token),
                 PlaceShrubAsync(chunk, token),
                 PlaceTreeAsync(chunk, token),
                 PlaceCactusAsync(chunk, 2, 5, token)
            );
            await PropagateAmbientLightAsync(chunk, token);

            //sw.Stop();
            //Debug.Log($"Elapsed time: {sw.ElapsedMilliseconds / 1000f} s");

        }
        #endregion





        #region Generate noise map data.
        /// <summary>
        /// Return 2D noise height map.
        /// </summary>
        /// <returns></returns>
        public async Task<float[]> GetHeightMapDataAsync(int frameX, int frameZ, int width, int height, CancellationToken token)
        {
            float[] heightValues = new float[width * height];


            await Task.Run(() =>
            {
                Parallel.For(0, height, z =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        float offsetX = frameX * width + x;
                        float offsetZ = frameZ * height + z;
                        float heightValue = float.PositiveInfinity;


                        float heightSimplex = (float)_heightSimplex.GetNoise(offsetX, offsetZ);
                        float heightVoronoi = DomainWarping(offsetX, offsetZ, _heatSimplex, _heatVoronoi);

                        float normalizeSimplexValue = (heightSimplex - _minWorldNoiseValue) / (_maxWorldNoiseValue - _minWorldNoiseValue);
                        float normalizeVoronoiValue = (heightVoronoi - _minWorldNoiseValue) / (_maxWorldNoiseValue - _minWorldNoiseValue);
                        float normalizeHeightValue = float.PositiveInfinity;


                        if (normalizeVoronoiValue < Water)
                        {
                            normalizeHeightValue = ScaleNoise(heightSimplex, -1f, 1f, 0, Water - 0.0001f);
                        }
                        else
                        {
                            heightValue = (heightSimplex - _minWorldNoiseValue) / (_maxWorldNoiseValue - _minWorldNoiseValue);
                            normalizeHeightValue = ScaleNoise(heightValue, 0f, 1f, Water + 0.0001f, 1f);
                        }

                        heightValues[WorldGenUtilities.IndexOf(x, z, width)] = normalizeHeightValue;
                    }

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                });
            }, token);

            return heightValues;
        }
        public async Task<float[]> GetGradientMapDataAsync(int frameX, int frameZ, int width, int height, CancellationToken token)
        {
            float[] gradientData = new float[width * height];

            await Task.Run(() =>
            {
                int gradientFrameX = (int)(frameX * width / (float)GradientHeatmapSize);
                int gradientFrameZ = -Mathf.CeilToInt(frameZ * height / (float)GradientHeatmapSize);

                // Calculate the center of the texture with the offset
                Vector2 gradientOffset = new Vector2(gradientFrameX * GradientHeatmapSize, gradientFrameZ * GradientHeatmapSize);
                Vector2 gradientCenterOffset = gradientOffset + new Vector2(frameX * width, frameZ * height);


                for (int x = 0; x < width; x++)
                {
                    for (int z = 0; z < height; z++)
                    {
                        Vector2 center = new Vector2(Mathf.FloorToInt(x / GradientHeatmapSize), Mathf.FloorToInt(z / GradientHeatmapSize)) * new Vector2(GradientHeatmapSize, GradientHeatmapSize) + new Vector2(GradientHeatmapSize / 2f, GradientHeatmapSize / 2f);
                        Vector2 centerWithOffset = center + gradientCenterOffset;

                        float distance = Mathf.Abs(z - centerWithOffset.y);
                        float normalizedDistance = 1.0f - Mathf.Clamp01(distance / (GradientHeatmapSize / 2f));
                        gradientData[WorldGenUtilities.IndexOf(x, z, width)] = normalizedDistance;

                        if (token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
                        }
                    }
                }
            }, token);

            //Debug.Log("GetGradientMapAsync Finish");
            return gradientData;
        }
        public async Task<float[]> GetFractalHeatMapDataAsync(int frameX, int frameZ, int width, int height, CancellationToken token)
        {
            //Debug.Log("GetFractalHeatMapAsync Start");

            float[] fractalNoiseData = new float[width * height];

            await Task.Run(() =>
            {
                Parallel.For(0, height, (z) =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        float offsetX = frameX * width + x;
                        float offsetZ = frameZ * height + z;

                        float heatValue = DomainWarping(offsetX, offsetZ, _heatSimplex, _heatVoronoi);

                        float normalizeHeatValue = (heatValue - _minWorldNoiseValue) / (_maxWorldNoiseValue - _minWorldNoiseValue);
                        fractalNoiseData[WorldGenUtilities.IndexOf(x, z, width)] = normalizeHeatValue;
                    }

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                });
            }, token);

            //Debug.Log("GetFractalHeatMapAsync Finish");
            return fractalNoiseData;
        }
        public async Task<float[]> GetHeatMapDataAysnc(int frameX, int frameZ, int width, int height, CancellationToken token)
        {
            /*
             * Heatmap created by blend gradient map and fractal noise map.
             */
            Task<float[]> gradientTask = GetGradientMapDataAsync(frameX, frameZ, width, height, token);
            Task<float[]> fractalNoiseTask = GetFractalHeatMapDataAsync(frameX, frameZ, width, height, token);

            // Await for both tasks to complete
            await Task.WhenAll(gradientTask, fractalNoiseTask);
            float[] gradientValues = gradientTask.Result;
            float[] fractalNoiseValues = fractalNoiseTask.Result;

            // Blend the maps
            float[] heatValues = WorldGenUtilities.BlendMapData(gradientValues, fractalNoiseValues, HeatMapBlendFactor);
            return heatValues;
        }
        public async Task<float[]> GetMoistureMapDataAsync(int frameX, int frameZ, int width, int height, CancellationToken token)
        {
            float[] moistureData = new float[width * height];

            await Task.Run(() =>
            {
                Parallel.For(0, height, (z) =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        float offsetX = frameX * width + x;
                        float offsetZ = frameZ * height + z;

                        //float moisetureValue = _moistureNoise.GetNoise(offsetX, offsetZ);
                        float moisetureValue = DomainWarping(offsetX, offsetZ, _moistureSimplex, _moistureVoronoi);
                        float normalizeMoistureValue = (moisetureValue - _minWorldNoiseValue) / (_maxWorldNoiseValue - _minWorldNoiseValue);

                        moistureData[WorldGenUtilities.IndexOf(x, z, width)] = normalizeMoistureValue;
                    }

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                });
            }, token);

            return moistureData;
        }
        public async Task<float[]> GetRiverDataAsync(int frameX, int frameZ, int width, int height, CancellationToken token)
        {
            float[] riverValues = new float[width * height];

            await Task.Run(() =>
            {
                Parallel.For(0, width, x =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        float offsetX = frameX * width + x;
                        float offsetZ = frameZ * height + y;

                        //float riverValue = (float)_riverSimplex.GetNoise(offsetX, offsetZ);
                        float riverValue = DomainWarping(offsetX, offsetZ, _riverSimplex, _riverVoronoi);


                        float normalizeRiverValue = (riverValue - _minWorldNoiseValue) / (_maxWorldNoiseValue - _minWorldNoiseValue);
                        riverValues[x + y * width] = normalizeRiverValue;
                    }

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                });
            }, token);

            return riverValues;
        }
        public async Task<float[]> GetMineDataAsync(int frameX, int frameZ, int width, int height, CancellationToken token)
        {
            float[] mineNoise = new float[width * height];

            await Task.Run(() =>
            {
                Parallel.For(0, height, z =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        float offsetX = frameX * width + x;
                        float offsetZ = frameZ * height + z;
                        float heightValue = float.PositiveInfinity;


                        float heightSimplex = (float)_mineHeightNoise.GetNoise(offsetX, offsetZ);
                        float heightVoronoi = DomainWarping(offsetX, offsetZ, _heatSimplex, _heatVoronoi);

                        float normalizeSimplexValue = (heightSimplex - _minWorldNoiseValue) / (_maxWorldNoiseValue - _minWorldNoiseValue);
                        float normalizeVoronoiValue = (heightVoronoi - _minWorldNoiseValue) / (_maxWorldNoiseValue - _minWorldNoiseValue);
                        float normalizeHeightValue = float.PositiveInfinity;


                        heightValue = (heightSimplex - _minWorldNoiseValue) / (_maxWorldNoiseValue - _minWorldNoiseValue);
                        normalizeHeightValue = ScaleNoise(heightValue, 0f, 1f, Water + 0.0001f, 1f);

                        mineNoise[WorldGenUtilities.IndexOf(x, z, width)] = normalizeHeightValue;
                    }
                });

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
            }, token);

            return mineNoise;
        }

        public async Task<float[]> GetLavaPoolNoiseDataAsync(int frameX, int frameZ, int width, int height, CancellationToken token)
        {
            float[] noiseValues = new float[width * height];
            await Task.Run(() =>
            {
                Parallel.For(0, height, (z) =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        float offsetX = frameX * width + x;
                        float offsetZ = frameZ * height + z;

                        float noiseValue = _lavaPoolSimplex.GetNoise(offsetX, offsetZ);
                        float normalizeMoistureValue = (noiseValue - _minWorldNoiseValue) / (_maxWorldNoiseValue - _minWorldNoiseValue);
                        noiseValues[WorldGenUtilities.IndexOf(x, z, width)] = normalizeMoistureValue;
                    }
                });

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
            }, token);

            return noiseValues;
        }
        #endregion



        #region GET CHUNK TYPES
        public async Task LoadChunkMapDataAsync(Chunk chunk, float[] heightValues, CancellationToken token)
        {
            await Task.Run(() =>
            {
                int width = chunk.Dimensions[0];
                int height = chunk.Dimensions[1];
                int depth = chunk.Dimensions[2];
                chunk.UpdateMaxBlocksHeight(_groundLevel);
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            float heightValue = heightValues[WorldGenUtilities.IndexOf(x, z, width)];
                            Vector3Int relativePosition = new Vector3Int(x, y, z);
                            int index3D = WorldGenUtilities.IndexOf(x, y, z, width, height);
                            int indexHighestY = WorldGenUtilities.IndexOf(x, height - 1, z, width, height);
                            int averageGroundLayer = Mathf.FloorToInt(Water * height);

                            int terrainHeight = Mathf.FloorToInt(heightValue * _groundLevel);
                            int globalHeight = (chunk.FrameY * chunk.Height) + y;


                            if (chunk.GetBlock(relativePosition) != (BlockID)0)
                            {
                                continue;
                            }
                            if (globalHeight <= _underGroundLevel)
                            {
                                //chunk.ChunkData[index3D] = blockID.Stone;
                                chunk.SetBlock(relativePosition, BlockID.Stone);
                            }
                            else if (globalHeight <= _groundLevel)
                            {
                                switch (chunk.BiomesData[index3D])
                                {
                                    case BiomeType.Desert:
                                        chunk.SetBlock(relativePosition, BlockID.Sand);
                                        break;
                                    case BiomeType.Plains:
                                        if (globalHeight < _groundLevel)
                                        {
                                            //chunk.ChunkData[index3D] = blockID.Dirt;
                                            chunk.SetBlock(relativePosition, BlockID.Dirt);
                                        }
                                        else if (globalHeight == _groundLevel)
                                        {
                                            chunk.SetBlock(relativePosition, BlockID.DirtGrass);
                                            //chunk.ChunkData[index3D] = blockID.DirtGrass;
                                        }
                                        break;
                                    case BiomeType.Forest:
                                        if (globalHeight < _groundLevel)
                                        {
                                            chunk.SetBlock(relativePosition, BlockID.Dirt);
                                            //chunk.ChunkData[index3D] = blockID.Dirt;
                                        }
                                        else if (globalHeight == _groundLevel)
                                        {
                                            //chunk.ChunkData[index3D] = blockID.DirtGrass;
                                            chunk.SetBlock(relativePosition, BlockID.DirtGrass);
                                        }
                                        break;
                                    case BiomeType.Snow:
                                        if (globalHeight < _groundLevel)
                                        {
                                            //chunk.ChunkData[index3D] = blockID.Dirt;
                                            chunk.SetBlock(relativePosition, BlockID.Dirt);
                                        }
                                        else if (globalHeight == _groundLevel)
                                        {
                                            //chunk.ChunkData[index3D] = blockID.SnowDirtGrass;
                                            chunk.SetBlock(relativePosition, BlockID.SnowDirtGrass);
                                        }
                                        break;
                                    case BiomeType.Ocean:
                                        if (heightValues[x + y * width] > (y / (float)height) && heightValues[x + y * width] < Water && y < _groundLevel)
                                        {
                                            //chunk.ChunkData[index3D] = blockID.Gravel;
                                            chunk.SetBlock(relativePosition, BlockID.Gravel);
                                        }
                                        else
                                        {
                                            //chunk.ChunkData[index3D] = blockID.Water;
                                            chunk.SetBlock(relativePosition, BlockID.Water);
                                            chunk.SetLiquidLevel(relativePosition, Core.Water.MAX_WATER_LEVEL);
                                        }
                                        break;
                                    case BiomeType.River:
                                        if (chunk.HeatData[index3D] == HeatType.Coldest || chunk.HeatData[index3D] == HeatType.Colder)
                                        {
                                            //chunk.ChunkData[index3D] = blockID.Ice;
                                            chunk.SetBlock(relativePosition, BlockID.Ice);
                                        }
                                        else
                                        {
                                            //chunk.ChunkData[index3D] = blockID.Water;
                                            chunk.SetBlock(relativePosition, BlockID.Water);
                                            chunk.SetLiquidLevel(relativePosition, Core.Water.MAX_WATER_LEVEL);
                                        }

                                        break;
                                    default:
                                        Debug.LogError($"Not found {chunk.BiomesData[index3D]} biome.");
                                        break;
                                }
                            }
                            else if (heightValue * 12f >= globalHeight)
                            {
                                switch (chunk.BiomesData[index3D])
                                {
                                    case BiomeType.Desert:
                                        //BlockID aboveBlock = chunk.GetBlock(relativePosition.x, relativePosition.y + 1, relativePosition.z);
                                        BlockID blockBelow = chunk.GetBlock(relativePosition.x, relativePosition.y - 1, relativePosition.z);
                                        BlockID belowOfBelowBlock = chunk.GetBlock(relativePosition.x, relativePosition.y - 2, relativePosition.z);
                                        if (blockBelow == BlockID.Sand ||
                                            blockBelow == BlockID.SandMine ||
                                            belowOfBelowBlock == BlockID.Sand ||
                                            belowOfBelowBlock == BlockID.SandMine
                                            )
                                        {
                                            chunk.SetBlock(relativePosition, BlockID.SandMine);
                                        }
                                        else
                                        {
                                            chunk.SetBlock(relativePosition, BlockID.Air);
                                        }
                                        break;
                                    //case BiomeType.Plains:
                                    //    chunk.SetBlock(relativePosition, BlockID.DirtGrass);
                                    //    break;
                                    //case BiomeType.Forest:
                                    //    chunk.SetBlock(relativePosition, BlockID.DirtGrass);
                                    //    break;
                                    //case BiomeType.Snow:
                                    //    chunk.SetBlock(relativePosition, BlockID.SnowDirtGrass);
                                    //    break;
                                    default:
                                        if (chunk.GetBlock(relativePosition) == (BlockID)0)
                                        {
                                            chunk.SetBlock(relativePosition, BlockID.Air);
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                if (chunk.GetBlock(relativePosition) == (BlockID)0)
                                {
                                    chunk.SetBlock(relativePosition, BlockID.Air);
                                }
                            }

                            if (token.IsCancellationRequested)
                            {
                                token.ThrowIfCancellationRequested();
                            }
                        }
                    }
                }
            }, token);
        }
        public async Task LoadHeightMapDataAsync(Chunk chunk, BlockID blockID, CancellationToken token)
        {
            await Task.Run(() =>
            {
                int width = chunk.Dimensions[0];
                int height = chunk.Dimensions[1];
                int depth = chunk.Dimensions[2];

                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            //int index3D = WorldGenUtilities.IndexOf(x, y, z, width, height);
                            //chunk.ChunkData[index3D] = blockID;

                            chunk.SetBlock(new Vector3Int(x, y, z), blockID);
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);
        }
        public async Task LoadHeatMapDataAsync(Chunk chunk, float[] heatValues, CancellationToken token)
        {
            await Task.Run(() =>
            {
                int width = chunk.Dimensions[0];
                int height = chunk.Dimensions[1];
                int depth = chunk.Dimensions[2];

                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int index3D = chunk.IndexOf(x, y, z);
                            int flattenedIndex = chunk.IndexOf(x, z);
                            float heatValue = heatValues[flattenedIndex];


                            float fracIndex2 = heatValue % 0.1f;
                            if (fracIndex2 < 0.015f)
                            {
                                chunk.HeatData[index3D] = HeatType.Coldest;
                            }
                            else if (fracIndex2 < 0.03f)
                            {
                                chunk.HeatData[index3D] = HeatType.Colder;
                            }
                            else if (fracIndex2 < 0.045f)
                            {
                                chunk.HeatData[index3D] = HeatType.Cold;
                            }
                            else if (fracIndex2 < 0.06)
                            {
                                chunk.HeatData[index3D] = HeatType.Warm;
                            }
                            else if (fracIndex2 < 0.08)
                            {
                                chunk.HeatData[index3D] = HeatType.Warmer;
                            }
                            else
                            {
                                chunk.HeatData[index3D] = HeatType.Warmest;
                            }
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);
        }
        public async Task LoadMoistureMapDataAsync(Chunk chunk, float[] moistureValues, CancellationToken token)
        {
            await Task.Run(() =>
            {
                int width = chunk.Dimensions[0];
                int height = chunk.Dimensions[1];
                int depth = chunk.Dimensions[2];

                for (int x = 0; x < width; x++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            int index2D = chunk.IndexOf(x, z);
                            int index3D = chunk.IndexOf(x, y, z);
                            float moistureValue = moistureValues[index2D];

                            if (moistureValue < DryestValue)
                            {
                                chunk.MoistureData[index3D] = MoistureType.Dryest;
                            }
                            else if (moistureValue < DryerValue)
                            {
                                chunk.MoistureData[index3D] = MoistureType.Dryer;
                            }
                            else if (moistureValue < DryValue)
                            {
                                chunk.MoistureData[index3D] = MoistureType.Dry;
                            }
                            else if (moistureValue < WetValue)
                            {
                                chunk.MoistureData[index3D] = MoistureType.Wet;
                            }
                            else if (moistureValue < WetterValue)
                            {
                                chunk.MoistureData[index3D] = MoistureType.Wetter;
                            }
                            else
                            {
                                chunk.MoistureData[index3D] = MoistureType.Wettest;
                            }
                        }
                    }
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);
        }
        public async Task GenerateBiomeMapDataAsync(Chunk chunk, float[] heightValues, CancellationToken token)
        {
            await Task.Run(() =>
            {
                int width = chunk.Dimensions[0];
                int height = chunk.Dimensions[1];
                int depth = chunk.Dimensions[2];

                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int index2D = chunk.IndexOf(x, z);
                            int index3D = chunk.IndexOf(x, y, z);
                            chunk.BiomesData[index3D] = GetBiome(chunk, x, y, z);

                            if (heightValues[index2D] < Water)
                            {
                                chunk.BiomesData[index3D] = BiomeType.Ocean;
                                chunk.HasOceanBiome = true;
                            }
                            else
                            {
                                chunk.BiomesData[index3D] = GetBiome(chunk, x, y, z);
                            }
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);
        }
        #endregion






        #region MODIFY NOISE DATA
        public async Task<float[]> ApplyHeightDataToMoistureDataAsync(float[] heightValues, float[] moistureValues, int width, int height, CancellationToken token)
        {
            await Task.Run(() =>
            {
                for (int x = 0; x < width; x++)
                {
                    for (int z = 0; z < height; z++)
                    {
                        int index = x + z * width;
                        float heightValue = heightValues[index];
                        if (heightValue < DeepWater)
                        {
                            moistureValues[index] += 8f * heightValue;
                        }
                        if (heightValue < Water)
                        {
                            moistureValues[index] += 3f * heightValue;
                        }
                        if (heightValue < Sand)
                        {
                            moistureValues[index] += 0.2f * heightValue;
                        }
                    }
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);

            return moistureValues;
        }

        #endregion







        #region RIVER
        private Queue<RiverNode> GetRiverBfsNodes(BiomeType[] riverBiome, int width, int depth)
        {
            Queue<RiverNode> bfsRiverQueue = new Queue<RiverNode>();

            int size = width * depth;
            for (int i = 0; i < size; i++)
            {
                int x = i % width;
                int z = i / width;

                if (x == 0 || x == width - 1 || z == 0 || z == depth - 1) continue;

                int bitmask = 0;
                if (riverBiome[x + (z + 1) * width] == riverBiome[i])
                    bitmask += 1;
                if (riverBiome[(x + 1) + z * width] == riverBiome[i])
                    bitmask += 2;
                if (riverBiome[x + (z - 1) * width] == riverBiome[i])
                    bitmask += 4;
                if (riverBiome[(x - 1) + z * width] == riverBiome[i])
                    bitmask += 8;

                if (bitmask != 15)
                {
                    RiverNode riverNode = new RiverNode()
                    {
                        RelativePosition = new Vector3Int(x, _groundLevel, z),
                        Density = 5
                    };

                    bfsRiverQueue.Enqueue(riverNode);
                }
            }
            return bfsRiverQueue;
        }
        private void GetRiverBfsNodes(Chunk chunk, int width, int depth)
        {
            int size = width * depth;
            for (int i = 0; i < size; i++)
            {
                int x = i % width;
                int z = i / width;
                int bitmask = 0;

                if (x == 0 || x == width - 1 || z == 0 || z == depth - 1)
                {
                    //continue;
                    if (chunk.FindNeighbor(new Vector3Int(x, _groundLevel, z + 1), out Chunk nbChunk, out Vector3Int nbRelativePosition))
                    {
                        if (nbChunk.RiverBiomes[nbRelativePosition.x + nbRelativePosition.z * width] == chunk.RiverBiomes[i])
                            bitmask += 1;
                    }
                    else
                    {
                        if (chunk.RiverBiomes[x + (z + 1) * width] == chunk.RiverBiomes[i])
                            bitmask += 1;
                    }


                    if (chunk.FindNeighbor(new Vector3Int(x + 1, _groundLevel, z), out Chunk nbChunk2, out Vector3Int nbRelativePosition2))
                    {
                        if (nbChunk2.RiverBiomes[nbRelativePosition2.x + nbRelativePosition2.z * width] == chunk.RiverBiomes[i])
                            bitmask += 2;
                    }
                    else
                    {
                        if (chunk.RiverBiomes[(x + 1) + z * width] == chunk.RiverBiomes[i])
                            bitmask += 2;
                    }



                    if (chunk.FindNeighbor(new Vector3Int(x, _groundLevel, z - 1), out Chunk nbChunk3, out Vector3Int nbRelativePosition3))
                    {
                        if (nbChunk3.RiverBiomes[nbRelativePosition3.x + nbRelativePosition3.z * width] == chunk.RiverBiomes[i])
                            bitmask += 4;
                    }
                    else
                    {
                        if (chunk.RiverBiomes[x + (z - 1) * width] == chunk.RiverBiomes[i])
                            bitmask += 4;
                    }




                    if (chunk.FindNeighbor(new Vector3Int(x - 1, _groundLevel, z), out Chunk nbChunk4, out Vector3Int nbRelativePosition4))
                    {
                        if (nbChunk4.RiverBiomes[nbRelativePosition4.x + nbRelativePosition4.z * width] == chunk.RiverBiomes[i])
                            bitmask += 8;
                    }
                    else
                    {
                        if (chunk.RiverBiomes[(x - 1) + z * width] == chunk.RiverBiomes[i])
                            bitmask += 8;
                    }

                    if (bitmask != 15)
                    {
                        RiverNode riverNode = new RiverNode()
                        {
                            RelativePosition = new Vector3Int(x, _groundLevel, z),
                            Density = 5
                        };

                        chunk.RiverBfsQueue.Enqueue(riverNode);
                    }
                }
                else
                {
                    if (chunk.RiverBiomes[x + (z + 1) * width] == chunk.RiverBiomes[i])
                        bitmask += 1;
                    if (chunk.RiverBiomes[(x + 1) + z * width] == chunk.RiverBiomes[i])
                        bitmask += 2;
                    if (chunk.RiverBiomes[x + (z - 1) * width] == chunk.RiverBiomes[i])
                        bitmask += 4;
                    if (chunk.RiverBiomes[(x - 1) + z * width] == chunk.RiverBiomes[i])
                        bitmask += 8;

                    if (bitmask != 15)
                    {
                        RiverNode riverNode = new RiverNode()
                        {
                            RelativePosition = new Vector3Int(x, _groundLevel, z),
                            Density = 5
                        };

                        chunk.RiverBfsQueue.Enqueue(riverNode);
                    }
                }
            }
        }

        private async Task DigRiverAsync(Chunk chunk, Queue<RiverNode> riverSpreadQueue, CancellationToken token)
        {
            ChunkGenData chunkData = ChunkDataPool.Pool.Get();
            int attempts = 0;
            await Task.Run(() =>
            {
                Array.Fill(chunkData.RiverDensity, 0);
                Parallel.ForEach(riverSpreadQueue, riverNode =>
                {
                    int index = WorldGenUtilities.IndexOf(riverNode.RelativePosition.x, riverNode.RelativePosition.y, riverNode.RelativePosition.z, chunk.Width, chunk.Height);
                    chunkData.RiverDensity[index] = riverNode.Density;
                });

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
            }, token);

            RiverNode startNode = riverSpreadQueue.Peek();
            chunkData.RiverDensity[WorldGenUtilities.IndexOf(startNode.RelativePosition.x, startNode.RelativePosition.y, startNode.RelativePosition.z, chunk.Width, chunk.Height)] = startNode.Density;
            while (riverSpreadQueue.Count > 0)
            {
                RiverNode currentNode = riverSpreadQueue.Dequeue();
                chunkData.RiverDensity[WorldGenUtilities.IndexOf(currentNode.RelativePosition.x, currentNode.RelativePosition.y, currentNode.RelativePosition.z, chunk.Width, chunk.Height)] = currentNode.Density;

                if (chunk.GetBiome(currentNode.RelativePosition) != BiomeType.Ocean)
                {
                    chunk.SetBiome(currentNode.RelativePosition, BiomeType.River);
                }

                GetNeighborsForBfsRiver(currentNode.RelativePosition, ref chunkData.RiverBfsNeighbors);
                for (int i = 0; i < chunkData.RiverBfsNeighbors.Length; i++)
                {
                    if (chunk.GetBiome(chunkData.RiverBfsNeighbors[i]) == BiomeType.Ocean) return;
                    if (chunkData.RiverBfsNeighbors[i] == currentNode.RelativePosition) return;


                    if (chunk.IsValidRelativePosition(chunkData.RiverBfsNeighbors[i]) == false)
                    {
                        if (chunk.FindNeighbor(chunkData.RiverBfsNeighbors[i], out Chunk neighborChunk, out Vector3Int nbRelativePosition))
                        {
                            RiverNode nbRiverNode = new RiverNode()
                            {
                                RelativePosition = nbRelativePosition,
                                Density = currentNode.Density - 1
                            };
                            neighborChunk.RiverBfsQueue.Enqueue(nbRiverNode);
                        }
                        else
                        {
                            //Debug.LogWarning($"Not found this chunk at: {neighbors[i]}");
                        }
                        continue;
                    }

                    if ((chunkData.RiverDensity[WorldGenUtilities.IndexOf(chunkData.RiverBfsNeighbors[i].x, chunkData.RiverBfsNeighbors[i].y, chunkData.RiverBfsNeighbors[i].z, chunk.Width, chunk.Height)] + 1 < currentNode.Density) && currentNode.Density > 0)
                    {
                        RiverNode nbRiverNode = new RiverNode()
                        {
                            RelativePosition = chunkData.RiverBfsNeighbors[i],
                            Density = currentNode.Density - 1
                        };

                        riverSpreadQueue.Enqueue(nbRiverNode);
                        chunkData.RiverDensity[WorldGenUtilities.IndexOf(nbRiverNode.RelativePosition.x, nbRiverNode.RelativePosition.y, nbRiverNode.RelativePosition.z, chunk.Width, chunk.Height)] = nbRiverNode.Density;
                    }
                }

                attempts++;
                if (attempts > 10000)
                {
                    Debug.Log("Infinite loop");
                    break;
                }
            }

            ChunkDataPool.Pool.Release(chunkData);
        }

        private void GetNeighborsForBfsRiver(Vector3Int position, ref Vector3Int[] riverBfsRiver)
        {
            riverBfsRiver[0] = position + new Vector3Int(1, 0, 0);
            riverBfsRiver[1] = position + new Vector3Int(-1, 0, 0);
            riverBfsRiver[2] = position + new Vector3Int(0, 0, 1);
            riverBfsRiver[3] = position + new Vector3Int(0, 0, -1);
            riverBfsRiver[4] = position + new Vector3Int(0, -1, 0);
        }
        #endregion






        #region BIOMES
        public BiomeType GetBiome(Vector3 globalPosition)
        {
            Chunk chunk = _main.GetChunk(globalPosition);
            if (chunk != null)
            {
                int localBlockX = Mathf.FloorToInt(globalPosition.x) % _chunkDimension.x;
                int localBlockY = Mathf.FloorToInt(globalPosition.y) % _chunkDimension.y;
                int localBlockZ = Mathf.FloorToInt(globalPosition.z) % _chunkDimension.z;
                int index = chunk.IndexOf(localBlockX, localBlockY, localBlockZ);

                return BiomeTable[(int)chunk.MoistureData[index], (int)chunk.HeatData[index]];
            }
            return default;
        }
        public BiomeType GetBiome(Chunk chunk, int frameX, int frameY, int frameZ)
        {
            int index = chunk.IndexOf(frameX, frameY, frameZ);
            return BiomeTable[(int)chunk.MoistureData[index], (int)chunk.HeatData[index]];
        }
        #endregion




        #region DECORs
        public async Task PlaceGrassAsync(Chunk chunk, CancellationToken token)
        {
            var poissonDiscData = PoissonDiscDataPool.Pool.Get();
            await PoissonDiscManager.Instance.GetRandom2DPoissonDiscTask(poissonDiscData, PoissonDiscManager.GRASS_DIMENSION, chunk.GlobalPosition, 2, 5);

            await Task.Run(() =>
            {
                for (int i = 0; i < poissonDiscData.SamplesResult.Count; i++)
                {
                    Vector3Int distributeGlobalPos = new Vector3Int(poissonDiscData.SamplesResult[i].x, _groundLevel, poissonDiscData.SamplesResult[i].y);
                    Vector3Int currRelativePos = chunk.GetRelativePosition(distributeGlobalPos);

                    if (chunk.GetBlock(currRelativePos) == BlockID.DirtGrass)
                    {
                        Vector3Int upperRelativePos = new Vector3Int(currRelativePos.x, currRelativePos.y + 1, currRelativePos.z);
                        if (chunk.OnGroundLevel(upperRelativePos))
                        {
                            if (i % 2 == 0)
                            {
                                chunk.SetBlock(upperRelativePos, BlockID.Grass);
                            }
                            else
                            {
                                chunk.SetBlock(upperRelativePos, BlockID.TallGrass);
                                chunk.SetBlock(upperRelativePos + new Vector3Int(0, 1, 0), BlockID.TallGrass);
                            }

                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);

            PoissonDiscDataPool.Pool.Release(poissonDiscData);
        }

        public async Task PlaceTreeAsync(Chunk chunk, CancellationToken token)
        {
            var poissonDiscData = PoissonDiscDataPool.Pool.Get();
            await PoissonDiscManager.Instance.GetRandom2DPoissonDiscTask(poissonDiscData, PoissonDiscManager.TREE_DIMENSION, chunk.GlobalPosition, 20, 25);

            await Task.Run(() =>
            {
                for (int i = 0; i < poissonDiscData.SamplesResult.Count; i++)
                {
                    Vector3Int distributeGlobalPos = new Vector3Int(poissonDiscData.SamplesResult[i].x, _groundLevel, poissonDiscData.SamplesResult[i].y);
                    Vector3Int currRelativePos = chunk.GetRelativePosition(distributeGlobalPos);

                    if (chunk.GetBlock(currRelativePos).IsDirt())
                    {
                        if (chunk.GetBiome(currRelativePos) == BiomeType.Forest)
                        {
                            Vector3Int upperRelativePos = new Vector3Int(currRelativePos.x, currRelativePos.y + 1, currRelativePos.z);
                            int randomHeight = (int)Mathf.Lerp(5f, 7.5f, (_treeNoiseDistribute.GetNoise(currRelativePos.x, currRelativePos.z)));
                            int treeHeight = CreateTree(chunk.GetGlobalPosition(upperRelativePos), randomHeight);
                            chunk.UpdateMaxBlocksHeight(treeHeight);

                        }
                        else if (chunk.GetBiome(currRelativePos) == BiomeType.Snow)
                        {
                            Vector3Int upperRelativePos = new Vector3Int(currRelativePos.x, currRelativePos.y + 1, currRelativePos.z);
                            int randHeight = (int)Mathf.Lerp(9f, 11f, (_treeNoiseDistribute.GetNoise(currRelativePos.x, currRelativePos.z)));
                            int treeHeight = CreatePineTree(chunk.GetGlobalPosition(upperRelativePos), randHeight);
                            chunk.UpdateMaxBlocksHeight(treeHeight);

                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);

            PoissonDiscDataPool.Pool.Release(poissonDiscData);
        }

        public async Task PlaceShrubAsync(Chunk chunk, CancellationToken token)
        {
            PoissonDiscData poissonDiscData = PoissonDiscDataPool.Pool.Get();
            await PoissonDiscManager.Instance.GetRandom2DPoissonDiscTask(poissonDiscData, PoissonDiscManager.GRASS_DIMENSION, chunk.GlobalPosition, 7, 15);

            await Task.Run(() =>
            {
                for (int i = 0; i < poissonDiscData.SamplesResult.Count; i++)
                {
                    try
                    {
                        Vector3Int distributeGlobalPos = new Vector3Int(poissonDiscData.SamplesResult[i].x, _groundLevel, poissonDiscData.SamplesResult[i].y);
                        Vector3Int currRelativePos = chunk.GetRelativePosition(distributeGlobalPos);

                        if (chunk.GetBiome(currRelativePos) == BiomeType.Desert && chunk.GetBlock(currRelativePos) == BlockID.Sand)
                        {
                            Vector3Int upperRelativePos = new Vector3Int(currRelativePos.x, currRelativePos.y + 1, currRelativePos.z);
                            if (chunk.OnGroundLevel(upperRelativePos))
                            {
                                chunk.SetBlock(upperRelativePos, BlockID.Shrub);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Vector3Int distributeGlobalPos = new Vector3Int(poissonDiscData.SamplesResult[i].x, _groundLevel, poissonDiscData.SamplesResult[i].y);
                        Debug.LogError($"Eroror at: {distributeGlobalPos}     {chunk.GetRelativePosition(distributeGlobalPos)}");
                        Debug.LogError(ex);
                    }


                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);

            PoissonDiscDataPool.Pool.Release(poissonDiscData);
        }

        public async Task PlaceCactusAsync(Chunk chunk, int minCactusHeight, int maxCactusHeight, CancellationToken token)
        {
            var poissonDiscData = PoissonDiscDataPool.Pool.Get();
            await PoissonDiscManager.Instance.GetRandom2DPoissonDiscTask(poissonDiscData, PoissonDiscManager.GRASS_DIMENSION, chunk.GlobalPosition, 12, 20);
            await Task.Run(() =>
            {
                for (int i = 0; i < poissonDiscData.SamplesResult.Count; i++)
                {
                    Vector3Int distributeGlobalPos = new Vector3Int(poissonDiscData.SamplesResult[i].x, _groundLevel, poissonDiscData.SamplesResult[i].y);
                    Vector3Int currRelativePos = chunk.GetRelativePosition(distributeGlobalPos);

                    GetGroundAt(chunk, currRelativePos.x, currRelativePos.z, out int relativeY);
                    float noiseValue = (_cactusNoiseDistribute.GetNoise(poissonDiscData.SamplesResult[i].x, poissonDiscData.SamplesResult[i].y) + 1.0f) / 2.0f;
                    int randomHeight = Mathf.RoundToInt(Mathf.Lerp(minCactusHeight, maxCactusHeight, noiseValue));
                    BlockID block = chunk.GetBlock(currRelativePos);
                    if (chunk.GetBiome(currRelativePos) == BiomeType.Desert && (block == BlockID.Sand || block == BlockID.SandMine))
                    {
                        Vector3Int startPos = new Vector3Int(currRelativePos.x, currRelativePos.y + randomHeight, currRelativePos.z);
                        //Main.Instance.SetBlock(chunk.GetGlobalPosition(startPos), BlockID.Cactus);
                        PlaceBlockDownward(chunk.GetGlobalPosition(startPos), BlockID.Cactus);
                        chunk.UpdateMaxBlocksHeight(startPos.y);
                    }

                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }, token);

            PoissonDiscDataPool.Pool.Release(poissonDiscData);
        }
        private void GetGroundAt(Chunk chunk, int relativeX, int relativeZ, out int relativeY)
        {
            for (int y = 0; y < chunk.Height; y++)
            {
                if (chunk.GetBlock(relativeX, y, relativeZ) == BlockID.Air)
                {
                    relativeY = y;
                    return;
                }
            }
            relativeY = chunk.Height - 1;
        }
        #endregion






        #region Structure
        /// <summary>
        /// Make this run. will modify later.
        /// </summary>
        /// <param name="globalPosition"></param>
        /// <returns></returns>
        public bool CanGenerateLavaPool(Vector3Int globalPosition)
        {
            bool canGenerate = true;


            if ((globalPosition.x + globalPosition.z) % 2 == 0)
            {
                for (int i = 0; i < StructureBuilder.Instance.LavaPool.Count; i++)
                {
                    var structureNode = StructureBuilder.Instance.LavaPool[i];
                    BlockID currBlock = Main.Instance.GetBlock(globalPosition + structureNode.GlobalPosition);
                    BlockID belowBlock = Main.Instance.GetBlock(globalPosition + structureNode.GlobalPosition + Vector3Int.down);

                    if (currBlock == BlockID.Water || belowBlock == BlockID.Water || belowBlock == BlockID.Air)
                    {
                        canGenerate = false;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < StructureBuilder.Instance.LavaPool2.Count; i++)
                {
                    var structureNode = StructureBuilder.Instance.LavaPool2[i];
                    BlockID currBlock = Main.Instance.GetBlock(globalPosition + structureNode.GlobalPosition);
                    BlockID belowBlock = Main.Instance.GetBlock(globalPosition + structureNode.GlobalPosition + Vector3Int.down);

                    if (currBlock == BlockID.Water || belowBlock == BlockID.Water || belowBlock == BlockID.Air)
                    {
                        canGenerate = false;
                        break;
                    }
                }
            }

            return canGenerate;
        }

        public async Task PlaceLavaPoolAsync(Chunk chunk, CancellationToken token)
        {
            var poissonDiscData = PoissonDiscDataPool.Pool.Get();
            await PoissonDiscManager.Instance.GetRandom2DPoissonDiscTask(poissonDiscData, PoissonDiscManager.TREE_DIMENSION, chunk.GlobalPosition, 60, 100);
            await Task.Run(() =>
            {
                for (int i = 0; i < poissonDiscData.SamplesResult.Count; i++)
                {
                    Vector3Int distributeGlobalPos = new Vector3Int(poissonDiscData.SamplesResult[i].x, _groundLevel, poissonDiscData.SamplesResult[i].y);
                    Vector3Int currRelativePos = chunk.GetRelativePosition(distributeGlobalPos);

                    GetGroundAt(chunk, currRelativePos.x, currRelativePos.z, out int relativeY);

                    BlockID block = chunk.GetBlock(currRelativePos);
                    if (chunk.GetBiome(currRelativePos) == BiomeType.Desert)
                    {
                        //Debug.Log(chunk.GetBlock(currRelativePos.x, relativeY, currRelativePos.z));
                        Vector3Int poolGlobalPosition = chunk.GetGlobalPosition(new Vector3Int(currRelativePos.x, relativeY - 1, currRelativePos.z));

                        if (CanGenerateLavaPool(poolGlobalPosition))
                        {
                            if ((poolGlobalPosition.x + poolGlobalPosition.z) % 2 == 0)
                            {
                                StructureBuilder.Instance.BuildLavaPool(poolGlobalPosition);
                            }
                            else
                            {
                                StructureBuilder.Instance.BuildLavaPool2(poolGlobalPosition);
                            }
                        }

                    }
                }
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
            }, token);

            PoissonDiscDataPool.Pool.Release(poissonDiscData);
        }

        #endregion









        #region NEIGHBORS
        /// <summary>
        /// Return: First time all neighbors has filled.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public bool UpdateChunkNeighbors(Chunk chunk)
        {
            if (chunk.HasNeighbors()) return false;


            // Face neighbors
            if (_main.HasChunk(chunk.RelativePosition + Vector3Int.left))
            {
                chunk.West = _main.GetChunk(chunk.RelativePosition + Vector3Int.left);
            }
            if (_main.HasChunk(chunk.RelativePosition + Vector3Int.right))
            {
                chunk.East = _main.GetChunk(chunk.RelativePosition + Vector3Int.right);
            }
            if (_main.HasChunk(chunk.RelativePosition + Vector3Int.forward))
            {
                chunk.North = _main.GetChunk(chunk.RelativePosition + Vector3Int.forward);
            }
            if (_main.HasChunk(chunk.RelativePosition + Vector3Int.back))
            {
                chunk.South = _main.GetChunk(chunk.RelativePosition + Vector3Int.back);
            }
            if (_main.HasChunk(chunk.RelativePosition + Vector3Int.up))
            {
                chunk.Up = _main.GetChunk(chunk.RelativePosition + Vector3Int.up);
            }
            if (_main.HasChunk(chunk.RelativePosition + Vector3Int.down))
            {
                chunk.Down = _main.GetChunk(chunk.RelativePosition + Vector3Int.down);
            }


            // Edge neighbors
            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(-1, 0, 1)))
            {
                chunk.Northwest = _main.GetChunk(chunk.RelativePosition + new Vector3Int(-1, 0, 1));
            }
            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(1, 0, 1)))
            {
                chunk.Northeast = _main.GetChunk(chunk.RelativePosition + new Vector3Int(1, 0, 1));
            }
            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(-1, 0, -1)))
            {
                chunk.Southwest = _main.GetChunk(chunk.RelativePosition + new Vector3Int(-1, 0, -1));
            }
            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(1, 0, -1)))
            {
                chunk.Southeast = _main.GetChunk(chunk.RelativePosition + new Vector3Int(1, 0, -1));
            }

            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(-1, 1, 0)))
            {
                chunk.UpWest = _main.GetChunk(chunk.RelativePosition + new Vector3Int(-1, 1, 0));
            }
            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(1, 1, 0)))
            {
                chunk.UpEast = _main.GetChunk(chunk.RelativePosition + new Vector3Int(1, 1, 0));
            }
            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(0, 1, 1)))
            {
                chunk.UpNorth = _main.GetChunk(chunk.RelativePosition + new Vector3Int(0, 1, 1));
            }
            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(0, 1, -1)))
            {
                chunk.UpSouth = _main.GetChunk(chunk.RelativePosition + new Vector3Int(0, 1, -1));
            }




            // Corner neighbors
            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(-1, 1, 1)))
            {
                chunk.UpNorthwest = _main.GetChunk(chunk.RelativePosition + new Vector3Int(-1, 1, 1));
            }
            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(1, 1, 1)))
            {
                chunk.UpNortheast = _main.GetChunk(chunk.RelativePosition + new Vector3Int(1, 1, 1));
            }
            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(-1, 1, -1)))
            {
                chunk.UpSouthwest = _main.GetChunk(chunk.RelativePosition + new Vector3Int(-1, 1, -1));
            }
            if (_main.HasChunk(chunk.RelativePosition + new Vector3Int(1, 1, -1)))
            {
                chunk.UpSoutheast = _main.GetChunk(chunk.RelativePosition + new Vector3Int(1, 1, -1));
            }


            if (chunk.HasNeighbors())
            {
                //Chunk.OnChunkHasNeighbors?.Invoke(chunk);
                //DrawChunk(chunk);
                return true;
            }

            return false;
        }
        #endregion





        #region LIGHTING
        public async Task PropagateAmbientLightAsync(Chunk chunk, CancellationToken token)
        {
            // Apply ambient light
            // I use list instead of queue because this type of light only fall down when start, 
            // use list can help this method can process in parallel. When this light hit block (not air)
            // we'll use normal bfs to spread light like with torch.
            Vector3Int[] faceNeighbors = new Vector3Int[6];
            await Task.Run(() =>
            {
                Parallel.For(0, _chunkDimension[2], (z) =>
                {
                    for (int y = _chunkDimension[1] - 1; y > chunk.MaxBlocksHeightInit; y--)
                    {
                        for (int x = 0; x < _chunkDimension[0]; x++)
                        {
                            Vector3Int relativePos = new Vector3Int(x, y, z);
                            chunk.SetAmbientLight(relativePos, LightUtils.MAX_LIGHT_INTENSITY);
                        }
                    }
                });


                for (int z = 0; z < _chunkDimension[2]; z++)
                {
                    for (int x = 0; x < _chunkDimension[0]; x++)
                    {
                        //Vector3Int lightNodeGlobalPosition = chunk.GlobalPosition + new Vector3Int(x, _chunkDimension[1] - 1, z);
                        //chunk.AmbientLightBfsQueue.Enqueue(new LightNode(lightNodeGlobalPosition, LightUtils.MaxLightIntensity));

                        Vector3Int lightNodeGlobalPosition = chunk.GlobalPosition + new Vector3Int(x, chunk.MaxBlocksHeightInit + 1, z);
                        chunk.AmbientLightBfsQueue.Enqueue(new LightNode(lightNodeGlobalPosition, LightUtils.MAX_LIGHT_INTENSITY));
                    }
                }

                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
            }, token);


            await LightCalculator.Instance.SpreadAmbientLightTask(chunk);

        }
        #endregion








        #region MODELS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootPosition"></param>
        /// <param name="treeHeight"></param>
        /// <returns>Tree height</returns>
        public int CreateTree(Vector3Int rootPosition, int treeHeight)
        {
            int offsetLeavesInY = 0;
            // Wood
            for (int i = 0; i < treeHeight; i++)
            {
                Vector3Int woodPos = new Vector3Int(rootPosition.x, rootPosition.y + i, rootPosition.z);
                Main.Instance.SetBlock(woodPos, BlockID.Wood);
            }


            // Leaves
            float radius = treeHeight / 3f * 2f;
            Vector3 center = new Vector3(rootPosition.x, rootPosition.y + treeHeight - 1, rootPosition.z);
            for (int i = -(int)radius; i < radius; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    for (int k = -(int)radius; k < radius; k++)
                    {
                        Vector3 leavePos = center + new Vector3(i, j, k);

                        float distance = Vector3.Distance(center, leavePos);

                        if (distance < radius)
                        {
                            if (_main.GetBlock(leavePos + Vector3.down) == BlockID.Air)
                            {
                                Main.Instance.SetBlock(leavePos + Vector3.down, BlockID.Leaves);
                            }
                            else
                            {
                                offsetLeavesInY++;
                            }

                        }
                    }
                }
            }

            //Debug.Log($"Oak: {treeHeight}   {Mathf.CeilToInt(radius)}  {offsetLeaves}");
            return rootPosition.y + treeHeight + Mathf.CeilToInt(radius) - offsetLeavesInY;
        }

        //private bool CanGrowthTree(Vector3Int rootPosition)
        //{
        //    Bounds treeBounds = new Bounds(new Vector3(rootPosition.x, 5, rootPosition.z), new Vector3(10, 5, 10));
        //    Vector3 minBP = treeBounds.min;
        //    Vector3 maxBP = treeBounds.max;

        //    for (float y = minBP.y; y <= maxBP.y; y++)
        //    {
        //        for (float z = minBP.z; z <= maxBP.z; z++)
        //        {
        //            for (float x = minBP.x; x <= maxBP.x; x++)
        //            {
        //                BlockID blockID = _main.GetBlock(new Vector3(x, y, z));
        //                if (blockID == BlockID.PineWood ||
        //                    blockID == BlockID.Wood ||
        //                    blockID == BlockID.Leaves ||
        //                    blockID == BlockID.PineLeaves)
        //                {
        //                    return false;
        //                }
        //            }
        //        }
        //    }
        //    return true;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootPosition"></param>
        /// <param name="treeHeight"></param>
        /// <returns>Tree height</returns>
        public int CreatePineTree(Vector3Int rootPosition, int treeHeight)
        {
            // Wood
            for (int i = 0; i < treeHeight; i++)
            {
                Vector3Int woodPos = new Vector3Int(rootPosition.x, rootPosition.y + i, rootPosition.z);
                Main.Instance.SetBlock(woodPos, BlockID.PineWood);
            }


            // Leaves
            Vector3Int highestLeafPos = new Vector3Int(rootPosition.x, rootPosition.y + treeHeight, rootPosition.z);
            Main.Instance.SetBlock(highestLeafPos, BlockID.PineLeaves);
            Main.Instance.SetBlock(highestLeafPos + new Vector3Int(1, -1, 0), BlockID.PineLeaves);
            Main.Instance.SetBlock(highestLeafPos + new Vector3Int(-1, -1, 0), BlockID.PineLeaves);
            Main.Instance.SetBlock(highestLeafPos + new Vector3Int(0, -1, 1), BlockID.PineLeaves);
            Main.Instance.SetBlock(highestLeafPos + new Vector3Int(0, -1, -1), BlockID.PineLeaves);


            int offsetIndex = 1;
            for (int y = highestLeafPos.y - 3; y > highestLeafPos.y - 6; y--)
            {
                for (int x = highestLeafPos.x - offsetIndex; x <= highestLeafPos.x + offsetIndex; x++)
                {
                    for (int z = highestLeafPos.z - offsetIndex; z <= highestLeafPos.z + offsetIndex; z++)
                    {
                        if (y % 2 == 0)
                        {
                            Vector3 blockPos = new Vector3(x, y, z);
                            Main.Instance.SetBlock(blockPos, BlockID.PineLeaves);
                        }

                    }
                }

                if (y % 2 == 0)
                {
                    offsetIndex++;
                }
            }

            return highestLeafPos.y;
        }

        public void PlaceBlockDownward(Vector3Int startGPosition, BlockID blockID)
        {
            int attempt = 0;
            Vector3 currGPos = new Vector3(startGPosition.x, startGPosition.y, startGPosition.z);
            while (true)
            {
                BlockID b = Main.Instance.GetBlock(currGPos);
                if (b == BlockID.Air || b == BlockID.Shrub)
                {
                    Main.Instance.SetBlock(currGPos, blockID);
                    currGPos = new Vector3(currGPos.x, currGPos.y - 1, currGPos.z);
                }
                else
                {
                    break;
                }


                if (attempt++ > 10)
                {
                    Debug.Log("Infinite loop");
                    break;
                }
            }
        }
        #endregion





        #region NOISE
        public float DomainWarping(float x, float y, FastNoiseLite simplex)
        {
            Vector2 p = new Vector2(x, y);

            Vector2 q = new Vector2((float)simplex.GetNoise(p.x, p.y),
                                    (float)simplex.GetNoise(p.x + 42.0f, p.y + 13.0f));


            ////Vector2 l2p1 = (p + 40 * q) + new Vector2(77, 35);
            ////Vector2 l2p2 = (p + 40 * q) + new Vector2(83, 28);

            ////Vector2 r = new Vector3((float)simplex.GetNoise(l2p1.x, l2p1.y),
            ////                        (float)simplex.GetNoise(l2p2.x, l2p2.y));


            //Vector2 l3 = p + 120 * r;
            //Vector2 l3 = p + 40 * q;
            return (float)simplex.GetNoise(q.x, q.y);
        }

        public float DomainWarping(float x, float y, FastNoiseLite simplex, FastNoiseLite voronoi)
        {
            Vector2 p = new Vector2(x, y);

            Vector2 q = new Vector2((float)simplex.GetNoise(p.x, p.y),
                                    (float)simplex.GetNoise(p.x + 52.0f, p.y + 13.0f));


            //Vector2 l2p1 = (p + 40 * q) + new Vector2(77, 35);
            //Vector2 l2p2 = (p + 40 * q) + new Vector2(83, 28);

            //Vector2 r = new Vector3((float)simplex.GetNoise(l2p1.x, l2p1.y),
            //                        (float)simplex.GetNoise(l2p2.x, l2p2.y));


            //Vector2 l3 = p + 120 * r;
            Vector2 l3 = p + 40 * q;
            return voronoi.GetNoise(l3.x, l3.y);
        }

        public float ScaleNoise(float noiseValue, float oldMin, float oldMax, float newMin, float newMax)
        {
            return (noiseValue - oldMin) * (newMax - newMin) / (oldMax - oldMin) + newMin;
        }
        #endregion




        #region Mine generation
        public HashSet<Vector3Int> GenerateVein(Vector3Int globalPosition, float probability, int maxCount)
        {
            HashSet<Vector3Int> veinPositions = new();
            HashSet<Chunk> chunksUpdate = new();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            Vector3Int[] neighbors = new Vector3Int[4];
            queue.Enqueue(globalPosition);
            veinPositions.Add(globalPosition);
            int attempts = 0;
            int count = 0;

            while (queue.Count > 0)
            {
                Debug.Log("A");
                Vector3Int currentPosition = queue.Dequeue();
                GetVoxelNeighborPosition(currentPosition, ref neighbors);

                for (int i = 0; i < neighbors.Length; i++)
                {
                    if (Main.Instance.TryGetChunk(neighbors[i], out Chunk chunk))
                    {
                        if (chunksUpdate.Contains(chunk) == false)
                        {
                            chunksUpdate.Add(chunk);
                        }

                        Debug.Log($"{neighbors[i].x}  {neighbors[i].z}");
                        float noiseValue = _lavaPoolSimplex.GetNoise((float)neighbors[i].x, (float)neighbors[i].z);
                        float normalizeMoistureValue = (noiseValue - _minWorldNoiseValue) / (_maxWorldNoiseValue - _minWorldNoiseValue);
                        Debug.Log(normalizeMoistureValue);
                        if (normalizeMoistureValue < probability)
                        {
                            if (veinPositions.Contains(neighbors[i]) == false)
                            {
                                queue.Enqueue(neighbors[i]);
                                veinPositions.Add(neighbors[i]);
                                chunk.SetBlock(Main.Instance.GlobalToRelativeBlockPosition(neighbors[i]), BlockID.Ice);

                                count++;
                            }
                        }
                    }
                }



                attempts++;
                if (attempts > 1000)
                {
                    Debug.LogWarning("Max attempt");
                    break;
                }

                if (count > maxCount)
                {
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
                //neighborPosition[4] = new Vector3Int(position.x, position.y + 1, position.z);
                //neighborPosition[5] = new Vector3Int(position.x, position.y - 1, position.z);
            }


            foreach (var c in chunksUpdate)
            {
                c.UpdateMask |= UpdateChunkMask.RenderAll;
            }

            return veinPositions;
        }
        #endregion
    }


}

