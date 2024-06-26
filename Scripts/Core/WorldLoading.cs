using UnityEngine;
using Sirenix.OdinInspector;
using PixelMiner.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace PixelMiner.Core
{
    public class WorldLoading : MonoBehaviour
    {
        public static WorldLoading Instance { get; private set; }
        public System.Action OnLoadingGameFinish;
        public System.Action<float> OnFirstTimePreRenderProgress;
        public System.Action<float> OnFirstTimeRenderProgress;
        private Main _main;
        private WorldGeneration _worldGen;

        [FoldoutGroup("World Settings"), Indent(1)] public byte InitWorldWidth = 3;
        [FoldoutGroup("World Settings"), Indent(1)] public byte InitWorldHeight = 3;
        [FoldoutGroup("World Settings"), Indent(1)] public byte InitWorldDepth = 3;
        [FoldoutGroup("World Settings"), Indent(1)] public byte LoadChunkOffsetWidth = 1;
        [FoldoutGroup("World Settings"), Indent(1)] public byte LoadChunkOffsetHeight = 1;
        [FoldoutGroup("World Settings"), Indent(1)] public byte LoadChunkOffsetDepth = 1;


        // Cached
        [SerializeField] private Transform _playerTrans;
        public Vector3Int LastChunkFrame { get; private set; }
        private Vector3Int _currentFrame;
        // Performance
        private float _updateTimer = 0.0f;
        private float _updateTime = 0.1f;
        private float _unloadChunkDistance = 200f;


        private List<Chunk> _preDrawChunkList = new();
        private List<Task> _preDrawChunkTaskList = new();
        private List<Task> _drawChunkTaskList = new();
        private List<Chunk> _loadChunkList = new();
        private List<Chunk> _unloadChunkList = new();
        private bool _finishLoadChunk = true;

        public bool IsLoadingChunk { get; private set; } = false;


        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);
        }

        private void Start()
        {
            _main = Main.Instance;
            _worldGen = WorldGeneration.Instance;


            // detect target where the world loading from.
            if (Main.Instance.Players.Count > 0)
            {
                _currentFrame = new Vector3Int(Mathf.FloorToInt(_playerTrans.position.x / Main.Instance.ChunkDimension[0]), 0,
                  Mathf.FloorToInt(_playerTrans.position.z / Main.Instance.ChunkDimension[2]));
            }
            else
            {
                //_playerTrans = Camera.main.transform;

                _currentFrame = new Vector3Int(Mathf.FloorToInt(Main.Instance.InitWorldPosition.x / Main.Instance.ChunkDimension[0]), 0,
                  Mathf.FloorToInt(Main.Instance.InitWorldPosition.z / Main.Instance.ChunkDimension[2]));
            }


            LastChunkFrame = _currentFrame;
            WorldGeneration.Instance.OnWorldLoadFinished += () =>
            {
                LastChunkFrame = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            };

            Main.Instance.OnCharacterInitialize += SetUpCharacter;
        }

     
        private void OnDestroy()
        {
            Main.Instance.OnCharacterInitialize -= SetUpCharacter;
        }

        private void SetUpCharacter()
        {
            _playerTrans = Main.Instance.Players[0];
        }


        private void Update()
        {
            // Temp
            if (_playerTrans == null) return;


            if (UnityEngine.Time.time - _updateTime > _updateTimer)
            {
                _updateTimer = UnityEngine.Time.time;
                //if (GameManager.Instance.CurrentState != GameState.Playing) return;
                if (_main.AutoLoadChunk && WorldGeneration.Instance.OnLoadingFinished)
                {
                    _currentFrame = new Vector3Int(
                        Mathf.FloorToInt(_playerTrans.position.x / _main.ChunkDimension[0]),
                        Mathf.FloorToInt(0),
                        Mathf.FloorToInt(_playerTrans.position.z / _main.ChunkDimension[2]));


                    if (_currentFrame != LastChunkFrame && _finishLoadChunk && IsLoadingChunk == false)
                    {
                        _finishLoadChunk = false;
                        LastChunkFrame = _currentFrame;
                        UpdateChunksAsync();
                    }
                }
            }
        }

        private async void UpdateChunksAsync()
        {
            IsLoadingChunk = true;
            await LoadChunksAroundPositionTask(LastChunkFrame.x, LastChunkFrame.y, LastChunkFrame.z,
                                             offsetWidth: LoadChunkOffsetWidth,
                                             offsetHeight: LoadChunkOffsetHeight,
                                             offsetDepth: LoadChunkOffsetDepth,
                                             Application.exitCancellationToken);
            IsLoadingChunk = false;
        }


        /// <summary>
        /// Load each chunk in sequence and draw each chunk in sequence. -> Less drop FPS but slow.
        /// </summary>
        /// <param name="frameX"></param>
        /// <param name="frameZ"></param>
        /// <param name="offsetWidth"></param>
        /// <param name="offsetDepth"></param>
        public async Task LoadChunksAroundPositionTask(int frameX, int frameY, int frameZ, byte offsetWidth, byte offsetHeight, byte offsetDepth, CancellationToken token)
        {
            for (int x = frameX - offsetWidth; x <= frameX + offsetWidth; x++)
            {
                for (int y = frameY - offsetHeight; y <= frameY + offsetHeight; y++)
                {
                    for (int z = frameZ - offsetDepth; z <= frameZ + offsetDepth; z++)
                    {
                        // Only need 1 down neighbor
                        if (y < 0)
                        {
                            //if (!(x == frameX && z == frameZ))
                            //{
                            //    continue;
                            //}
                            y = 0;
                        }


                        Vector3Int nbFrame = new Vector3Int(x, y, z);
                        Chunk chunk = _main.GetChunk(nbFrame);
                        if (chunk == null)   // Create new chunk
                        {
                            if (x == frameX && z == frameZ)
                            {
                                // Center
                                // ......
                            }

                            Chunk newChunk = await _worldGen.GenerateNewChunkTask(x, y, z, _main.ChunkDimension, token);
                            _loadChunkList.Add(newChunk);
                            //LoadChunk(newChunk);
                            if (newChunk.HasDrawnFirstTime == false)
                            {

                            }


                        }
                        else // Load chunk cached.
                        {
                            if (x == frameX && z == frameZ)
                            {
                                // Center
                                // ......
                            }
                            //LoadChunk(_main.Chunks[nbFrame]);
                            Chunk targetChunk = _main.GetChunk(nbFrame);
                            _loadChunkList.Add(targetChunk);
                            if (targetChunk.HasDrawnFirstTime == false)
                            {

                            }
                        }
                    }
                }
            }


            int preDrawChunkCount = 0;
            int maxChunkPreDrawInStage = 8;

            for (int i = 0; i < _loadChunkList.Count; i++)
            {
                LoadChunk(_loadChunkList[i]);
            }

            // Pre-draw chunk
            foreach (var activeChunk in _main.ActiveChunks)
            {
                if (preDrawChunkCount > maxChunkPreDrawInStage)
                {
                    await Task.WhenAll(_preDrawChunkTaskList);
                    preDrawChunkCount = 0;
                    _preDrawChunkTaskList.Clear();
                }

                if (_worldGen.UpdateChunkNeighbors(activeChunk))
                {
                    _preDrawChunkTaskList.Add(_worldGen.UpdateChunkWhenHasAllNeighborsTask(activeChunk, token));
                    //await _worldGen.UpdateChunkWhenHasAllNeighborsTask(activeChunk);
                    _preDrawChunkList.Add(activeChunk);
                    preDrawChunkCount++;
                }
            }


            if (_preDrawChunkTaskList.Count > 0)
            {
                //Debug.Log($"Pre-Draw last: {_preDrawChunkTaskList.Count}");
                await Task.WhenAll(_preDrawChunkTaskList);
                _preDrawChunkTaskList.Clear();
            }





            // Draw chunk
            for (int i = 0; i < _preDrawChunkList.Count; i++)
            {
                _preDrawChunkList[i].UpdateMask |= UpdateChunkMask.RenderAll;

                // Redraw fix ao artifact (Only need redraw chunk at ground level)
                if (_preDrawChunkList[i].FrameY == 0)
                {
                    if (_preDrawChunkList[i].West.HasDrawnFirstTime)
                        _preDrawChunkList[i].West.UpdateMask |= UpdateChunkMask.RenderAll;
                    if (_preDrawChunkList[i].East.HasDrawnFirstTime)
                        _preDrawChunkList[i].East.UpdateMask |= UpdateChunkMask.RenderAll;
                    if (_preDrawChunkList[i].South.HasDrawnFirstTime)
                        _preDrawChunkList[i].South.UpdateMask |= UpdateChunkMask.RenderAll;
                    if (_preDrawChunkList[i].North.HasDrawnFirstTime)
                        _preDrawChunkList[i].North.UpdateMask |= UpdateChunkMask.RenderAll;
                }
            }



            // Unload chunk
            if (Main.Instance.AutoUnloadChunk)
            {
                foreach (var activeChunk in _main.ActiveChunks)
                {
                    if (Vector3.Distance(_playerTrans.position, activeChunk.transform.position) > _unloadChunkDistance)
                    {
                        _unloadChunkList.Add(activeChunk);
                    }
                }

                for (int i = 0; i < _unloadChunkList.Count; i++)
                {
                    UnloadChunk(_unloadChunkList[i]);
                }
            }


            _preDrawChunkTaskList.Clear();
            _preDrawChunkList.Clear();
            _unloadChunkList.Clear();
            _loadChunkList.Clear();

            _finishLoadChunk = true;
        }



        public async Task InitializeLoadChunksAroundPositionTask(int frameX, int frameY, int frameZ, byte offsetWidth, byte offsetHeight, byte offsetDepth, CancellationToken token)
        {
            for (int x = frameX - offsetWidth; x <= frameX + offsetWidth; x++)
            {
                for (int y = frameY - offsetHeight; y <= frameY + offsetHeight; y++)
                {
                    for (int z = frameZ - offsetDepth; z <= frameZ + offsetDepth; z++)
                    {
                        // Only need 1 down neighbor
                        if (y < 0)
                        {
                            //if (!(x == frameX && z == frameZ))
                            //{
                            //    continue;
                            //}
                            y = 0;
                        }


                        Vector3Int nbFrame = new Vector3Int(x, y, z);
                        Chunk chunk = _main.GetChunk(nbFrame);
                        if (chunk == null)   // Create new chunk
                        {
                            if (x == frameX && z == frameZ)
                            {
                                // Center
                                // ......
                            }


                            Chunk newChunk = await _worldGen.GenerateNewChunkTask(x, y, z, _main.ChunkDimension, token);
                            _loadChunkList.Add(newChunk);
                            //LoadChunk(newChunk);
                            if (newChunk.HasDrawnFirstTime == false)
                            {

                            }


                        }
                        else // Load chunk cached.
                        {
                            if (x == frameX && z == frameZ)
                            {
                                // Center
                                // ......
                            }
                            //LoadChunk(_main.Chunks[nbFrame]);
                            Chunk targetChunk = _main.GetChunk(nbFrame);
                            _loadChunkList.Add(targetChunk);
                            if (targetChunk.HasDrawnFirstTime == false)
                            {

                            }
                        }
                    }
                }
            }

        

            int preDrawChunkCount = 0;
            int maxChunkPreDrawInStage = 8;
            for (int i = 0; i < _loadChunkList.Count; i++)
            {
                LoadChunk(_loadChunkList[i]);
            }


            // Pre-Render progress
            // ---------------
            int totalPreRenderChunk = _main.ActiveChunks.Count;
            int preRenderCount = 0;
            float progress = 0f;
            Debug.Log($"Pre-render: {totalPreRenderChunk}");
            // Pre-draw chunk
            foreach (var activeChunk in _main.ActiveChunks)
            {
                if (preDrawChunkCount >= maxChunkPreDrawInStage)
                {
                    await Task.WhenAll(_preDrawChunkTaskList);
                    preDrawChunkCount = 0;
                    _preDrawChunkTaskList.Clear();         
                }

                if (_worldGen.UpdateChunkNeighbors(activeChunk))
                {
                    _preDrawChunkTaskList.Add(_worldGen.UpdateChunkWhenHasAllNeighborsTask(activeChunk, token));
                    _preDrawChunkList.Add(activeChunk);
                    preDrawChunkCount++;           
                }
                preRenderCount += 1;
                progress = preRenderCount / (float)totalPreRenderChunk;
                OnFirstTimePreRenderProgress?.Invoke(progress);
            }


            if (_preDrawChunkTaskList.Count > 0)
            {
                await Task.WhenAll(_preDrawChunkTaskList);
            }
 

            // Render progress
            // ---------------
            int totalRenderChunk = _preDrawChunkList.Count;
            int renderCount = 0;
            Debug.Log($"total render chunk: {totalRenderChunk}");


            // Draw chunk
            int maxRenderChunkCount = 8;
            _drawChunkTaskList.Clear();
            for (int i = 0; i < _preDrawChunkList.Count; i++)
            {
                Main.Instance.HandleLightSpreadingCrossChunkFirstTimeDrawn(_preDrawChunkList[i]);
                await Main.Instance.AddLightToLightBlockForChunkFirstTimeDrawn(_preDrawChunkList[i]);
                _drawChunkTaskList.Add(_preDrawChunkList[i].RenderChunkTask(token));
                if (_drawChunkTaskList.Count >= maxRenderChunkCount)
                {
                    await Task.WhenAll(_drawChunkTaskList);
                    _drawChunkTaskList.Clear();


                    // Render progress
                    // ---------------
                    renderCount += maxRenderChunkCount;
                    progress = renderCount / (float)totalRenderChunk;
                    OnFirstTimeRenderProgress?.Invoke(progress);
                }
            }
            await Task.WhenAll(_drawChunkTaskList);


            for (int i = _preDrawChunkList.Count - _drawChunkTaskList.Count; i < _preDrawChunkList.Count; i++)
            {
                //Debug.Log($"i :  {i}");
                Main.Instance.HandleLightSpreadingCrossChunkFirstTimeDrawn(_preDrawChunkList[i]);
                await Main.Instance.AddLightToLightBlockForChunkFirstTimeDrawn(_preDrawChunkList[i]);
            }

            // Render progress
            // ---------------
            renderCount += _drawChunkTaskList.Count;
            progress = renderCount / (float)totalRenderChunk;
            OnFirstTimeRenderProgress?.Invoke(progress);





            // Unload chunk
            if (Main.Instance.AutoUnloadChunk)
            {
                foreach (var activeChunk in _main.ActiveChunks)
                {
                    if (Vector3.Distance(_playerTrans.position, activeChunk.transform.position) > _unloadChunkDistance)
                    {
                        _unloadChunkList.Add(activeChunk);
                    }
                }

                for (int i = 0; i < _unloadChunkList.Count; i++)
                {
                    UnloadChunk(_unloadChunkList[i]);
                }
            }


            _preDrawChunkTaskList.Clear();
            _preDrawChunkList.Clear();
            _unloadChunkList.Clear();
            _loadChunkList.Clear();
            _drawChunkTaskList.Clear();
            _finishLoadChunk = true;

            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }
        }


        public void LoadChunk(Chunk chunk)
        {
            Vector3Int frame = new Vector3Int(chunk.FrameX, chunk.FrameY, chunk.FrameZ);
            _main.TryAddChunks(frame, chunk);
            _main.ActiveChunks.Add(chunk);
            chunk.gameObject.SetActive(true);
        }
        public void UnloadChunk(Chunk chunk)
        {
            _main.ActiveChunks.Remove(chunk);
            chunk.gameObject.SetActive(false);
        }
    }
}

