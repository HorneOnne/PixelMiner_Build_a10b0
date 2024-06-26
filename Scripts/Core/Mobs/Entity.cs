using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelMiner.Core
{
    [System.Serializable]
    public abstract class Entity : MonoBehaviour
    {
        public event System.Action<byte, byte> OnEntityTakeDamaged;
        public event System.Action OnEntityDied;

        public Transform Model { get; set; }
        public byte MaxHealth { get; set; }
        public byte Health { get; set; }
        public byte Armor { get; set; }
        public byte AttackPower { get; set; }
        public float Speed { get; set; }
        public bool IsAlive { get; set; } = true;
        public uint MobSpawnerIndex { get; set; }
        public abstract void Attack(Entity target);
        public abstract void TakeDamge(byte damage, Entity fromEntity);
        public abstract void Die();

        protected void OnEntityTakeDamagedTriggered()
        {
            OnEntityTakeDamaged?.Invoke(Health, MaxHealth);
        }

        protected void OnEntityDiedTriggered()
        {
            OnEntityDied?.Invoke();
        }
    }
}
