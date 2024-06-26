using UnityEngine;
using System.Collections.Generic;
using PixelMiner.Time;
using PixelMiner.Extensions;
namespace PixelMiner.Core
{
    public class EntitySpawner : MonoBehaviour
    {
        private bool _startSpawnEntities = false;
        private Transform _player;
        private List<Entity> _mobs;



        // Spawner timer;
        private const float ZOMBIE_SPAWN_TIME_MIN = 10.0f;
        private const float ZOMBIE_SPAWN_TIME_MAX = 60.0f;
        private float _zombieSpawnTime = 10.0f;
        private float _zombieSpawnTimer = 0.0f;

        private void Awake()
        {
            _mobs = new(100);
        }

        private void Start()
        {
            Main.Instance.OnCharacterInitialize += SetupWhenCharacterAdded;
          
        }
        private void OnDestroy()
        {
            Main.Instance.OnCharacterInitialize -= SetupWhenCharacterAdded;
        }


        private void Update()
        {
            if (_startSpawnEntities == false) return;

            // Night 
            if (WorldTime.Instance.Hours <= 5 || WorldTime.Instance.Hours >= 18)
            {
                // Spawn zombie
                _zombieSpawnTimer += UnityEngine.Time.deltaTime;

                if (_zombieSpawnTimer > _zombieSpawnTime)
                {
                    _zombieSpawnTimer = 0.0f;
                    _zombieSpawnTime = Random.Range(ZOMBIE_SPAWN_TIME_MIN, ZOMBIE_SPAWN_TIME_MAX);
                    var mob = SpawnRandomZombie();
                    _mobs.Add(mob);
                    mob.MobSpawnerIndex = (uint)(_mobs.Count);
                }
            }
        }


        private void SetupWhenCharacterAdded()
        {
            _player = Main.Instance.Players[0];
            _startSpawnEntities = true;
        }

        private Entity SpawnRandomZombie()
        {
            float randomX = Random.Range(-25f, 25f);
            float randomZ = Random.Range(-25f, 25f);

            Vector3 randomPosition = new Vector3(_player.transform.position.x + randomX, _player.transform.position.y + 3f, _player.transform.position.z + randomZ);
            Zombie zombie = GameFactory.CreateZombie(randomPosition);
            zombie.EnablePhysics();

            return zombie;
        }
    }


    public static class ListExtension
    {
        public static void RemoveAtUnordered<T>(this List<T> list, T item) where T : Entity
        {
            if (list.Count == 1)
            {
                list.RemoveAt(0);
            }
            else if (item.MobSpawnerIndex == list.Count - 1)
            {
                list.RemoveAt(list.Count - 1);
            }
            else
            {
                var temp = item;
                item = list[list.Count - 1];
                list[list.Count - 1] = temp;

                list.RemoveAt(list.Count - 1);
            }
        }
    }
}
