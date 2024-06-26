using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelMiner.Core
{
    public class Projectile : MonoBehaviour
    {
        protected Rigidbody _rb;

        [Header("Properties")]
        protected Transform rotationPivot;
        [SerializeField] protected float exitTime;
        protected float exitCountTimer = 0.0f;
        protected LayerMask environmentLayer;


        // Particles 
        protected ParticleSystem[] particles;

        #region Properties
        public Rigidbody Rigidbody { get => _rb; }
        #endregion
   
        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null)
            {
                Debug.LogError("Projectile missing rigidbody component.");
            }
            rotationPivot = transform.Find("RotationPivot");
            environmentLayer = 0;
            environmentLayer |= LayerMask.GetMask("Environment");

            particles = transform.GetComponentsInChildren<ParticleSystem>();
        }

        public void Release(Vector3 direction)
        {
            for(int i = 0; i < particles.Length; ++i)
            {
                particles[i].Play();
            }
            Rigidbody.AddForce(direction);
        }
        
        protected virtual void Update()
        {
            exitCountTimer += UnityEngine.Time.deltaTime;
            if(exitCountTimer > exitTime)
            {
                OnReturnPool();
            }
        }

        protected virtual void FixedUpdate()
        {
            rotationPivot.up = _rb.velocity;
        }

        public virtual void ResetProjectile()
        {
            exitCountTimer = 0.0f;
        }

        public virtual void OnReturnPool()
        {
            Destroy(this.gameObject);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if ((environmentLayer.value & 1 << collision.gameObject.layer) != 0)
            {
                _rb.velocity = Vector3.zero;
            }
        }
    }
}
