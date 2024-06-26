using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelMiner.Core
{
    public class WorldSimulations : MonoBehaviour
    {
        private Main _main;
        [Range(1,10)]
        [SerializeField] private byte _simulationDistance = 2;

        [SerializeField] private List<Chunk> _simulationChunks = new();
        private float _simulationTime = 0.5f;
        private float _simulationTimer = 0.0f;
        private bool _canSimulate = false;

        // Detect where is the player in world
        [SerializeField] private Transform _centerPosition;
        private Vector3Int _lastChunkFrame = default;
        private Vector3Int _currentFrame = default;


        private bool _enableSound = true;

        private void Start()
        {
            _main = Main.Instance;
            WorldLoading.Instance.OnLoadingGameFinish += OnWorldLoadingFinished;
        }
        private void OnDestroy()
        {
            WorldLoading.Instance.OnLoadingGameFinish -= OnWorldLoadingFinished;
        }


        private void Update()
        {
            if (!_canSimulate) return;

            _simulationTimer += UnityEngine.Time.deltaTime;
            if (_simulationTimer > _simulationTime)
            {
                _simulationTimer -= _simulationTime;

                _currentFrame = new Vector3Int(
                       Mathf.FloorToInt(_centerPosition.position.x / _main.ChunkDimension[0]),
                       Mathf.FloorToInt(0),
                       Mathf.FloorToInt(_centerPosition.position.z / _main.ChunkDimension[2]));


                if (_currentFrame != _lastChunkFrame)
                {
                    _lastChunkFrame = _currentFrame;
                    GetSimulationChunks(_currentFrame, _simulationChunks);
                }

                HandleLavaParticles();
            }
        }


        private void GetSimulationChunks(Vector3Int wFrame, List<Chunk> simulationChunks)
        {
            _simulationChunks.Clear();
            //for (int x = wFrame.x - _simulationDistance; x <= wFrame.x + _simulationDistance; x++)
            //{
            //    for (int y = wFrame.y - _simulationDistance; y <= wFrame.y + _simulationDistance; y++)
            //    {
            //        for (int z = wFrame.z - _simulationDistance; z <= wFrame.z + _simulationDistance; z++)
            //        {
            //            if(Main.Instance.TryGetChunk(x,y,z, out Chunk chunk))
            //            {
            //                _simulationChunks.Add(chunk);
            //            }
            //        }
            //    }
            //}

            for (int x = wFrame.x - _simulationDistance; x <= wFrame.x + _simulationDistance; x++)
            {
                for (int z = wFrame.z - _simulationDistance; z <= wFrame.z + _simulationDistance; z++)
                {
                    if (Main.Instance.TryGetChunk(x, wFrame.y, z, out Chunk chunk))
                    {
                        _simulationChunks.Add(chunk);
                    }
                }
            }
        }


        private void HandleLavaParticles()
        {
            Vector3 _lastParticlePosition = default;
            int maxParticleCount = Random.Range(0, 3);
            int particleCount = Random.Range(0, 3);
            if (maxParticleCount > 0)
            {
                for (int i = 0; i < _simulationChunks.Count; i++)
                {
                    PlayLavaParticles(_simulationChunks[i], ref _lastParticlePosition, ref particleCount, ref maxParticleCount);
                    if (particleCount >= maxParticleCount)
                        break;
                }
            }
        }

        private void PlayLavaParticles(Chunk chunk, ref Vector3 lastParticlePosition, ref int particleCount, ref int maxParticleCount)
        {
            for(int i = 0; i < chunk.ChunkData.Length; i++)
            {
                int x = i % chunk.Width;
                int y = (i / chunk.Width) % chunk.Height;
                int z = i / (chunk.Width * chunk.Height);
                Vector3 globalPosition = chunk.GetGlobalPosition(x, y, z);
     

                if (Vector3.Distance(globalPosition, lastParticlePosition) > 10f)
                {
                    if (EligibleToPlayParticles(chunk, x, y, z))
                    {
                        if (Random.Range(0f, 1f) > 0.1f) continue;

                        Projectile projectileInstance = LavaProjectilePool.Pool.Get();
                        projectileInstance.transform.position = new Vector3(globalPosition.x, globalPosition.y + 1f, globalPosition.z);
                        projectileInstance.gameObject.SetActive(true);
                        projectileInstance.Release(new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.8f, 1.25f), Random.Range(-0.5f, 0.5f)) * 350);
                        lastParticlePosition = globalPosition;

                        if(_enableSound)
                        {
                            AudioManager.Instance.PlayLavaParticle(projectileInstance.transform.position);
                        }

                        particleCount++;
                        if (particleCount > maxParticleCount)
                        {
                            break;
                        }
                    }
                }
               
            }


            // If current block is lava and above block is air -> eligible
            bool EligibleToPlayParticles(Chunk chunk, int x, int y, int z)
            {
                return (chunk.GetBlock(x, y, z) == Enums.BlockID.Lava && 
                        chunk.GetBlock(x,y+1,z) == Enums.BlockID.Air &&
                        chunk.GetLiquidLevel(x,y,z) == Lava.MAX_LAVA_LEVEL);

            }
        }

        private void OnWorldLoadingFinished()
        {
            _canSimulate = true;

            _centerPosition = Main.Instance.Players[0].transform;
            _currentFrame = new Vector3Int(
                      Mathf.FloorToInt(_centerPosition.position.x / _main.ChunkDimension[0]),
                      Mathf.FloorToInt(0),
                      Mathf.FloorToInt(_centerPosition.position.z / _main.ChunkDimension[2]));


            if (_currentFrame != _lastChunkFrame)
            {
                _lastChunkFrame = _currentFrame;
                GetSimulationChunks(_currentFrame, _simulationChunks);
            }
        }
    }
}
