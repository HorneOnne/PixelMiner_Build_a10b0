using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelMiner.Enums;

namespace PixelMiner.Core
{
    [RequireComponent(typeof(ParticleSystem))]
    public class PhysicParticle : MonoBehaviour
    {
        private Main _main;
        private ParticleSystem _ps;
        [SerializeField] private ParticleSystem.Particle[] _particles;
        public float _gravityForce;
        [SerializeField] private float _frictionCoefficient = 2f;
        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
            _particles = new ParticleSystem.Particle[20];
            
        }

        private void Start()
        {
            _main = Main.Instance;
        }


        private void FixedUpdate()
        {
            if(_ps.isPlaying)
            {
                System.Array.Clear(_particles, 0, _particles.Length);
                  int numParticleAlive = _ps.GetParticles(_particles);
                _ps.Emit(numParticleAlive);
                
                for(int i = 0; i < numParticleAlive; i++)
                {
                    Vector3 particleVel = _particles[i].velocity;

                    // ResolveX
                    Vector3 xOffsetPos = _particles[i].position + new Vector3(0.2f, 0f, 0f);
                    Vector3 xOffsetNeg = _particles[i].position + new Vector3(-0.2f, 0f, 0f);
                    BlockID x1 = _main.GetBlock(xOffsetPos);
                    BlockID x2 = _main.GetBlock(xOffsetNeg);

                    if (x1.IsSolidOpaqueVoxel() || x1.IsSolidTransparentVoxel())
                    {
                        _particles[i].velocity = new Vector3(0, particleVel.y, _particles[i].velocity.z);
                    }
                    else if (x2.IsSolidOpaqueVoxel() || x2.IsSolidTransparentVoxel())
                    {
                        _particles[i].velocity = new Vector3(0, particleVel.y, _particles[i].velocity.z);
                    }


                    // ResolveZ
                    Vector3 zOffsetPos = _particles[i].position + new Vector3(0f, 0f, 0.2f);
                    Vector3 zOffsetNeg = _particles[i].position + new Vector3(0f, 0f, -0.2f);
                    BlockID z1 = _main.GetBlock(zOffsetPos);
                    BlockID z2 = _main.GetBlock(zOffsetNeg);

                    if (z1.IsSolidOpaqueVoxel() || z1.IsSolidTransparentVoxel())
                    {
                        _particles[i].velocity = new Vector3(particleVel.x, particleVel.y, 0);
                    }
                    else if (z2.IsSolidOpaqueVoxel() || z2.IsSolidTransparentVoxel())
                    {
                        _particles[i].velocity = new Vector3(particleVel.x, particleVel.y, 0);
                    }




                    // Resolve Y
                    Vector3 yOffsetPos = _particles[i].position + new Vector3(0f, 0.2f, 0f);
                    Vector3 yOffsetNeg = _particles[i].position + new Vector3(0f, -0.2f, 0f);
                    BlockID y1 = _main.GetBlock(yOffsetPos);
                    BlockID y2 = _main.GetBlock(yOffsetNeg);
                    if (y1.IsSolidOpaqueVoxel() || y1.IsSolidTransparentVoxel())
                    {
                        _particles[i].velocity += new Vector3(particleVel.x, -7 * UnityEngine.Time.deltaTime, particleVel.z);
                    }


                    if (y2.IsSolidOpaqueVoxel() || y2.IsSolidTransparentVoxel())
                    {
                        _particles[i].velocity = new Vector3(particleVel.x, 0, particleVel.z);

                        // Add Friction
                        _particles[i].velocity -= _particles[i].velocity * _frictionCoefficient * UnityEngine.Time.deltaTime;
                    }
                    else
                    {
                        _particles[i].velocity += new Vector3(0, -7 * UnityEngine.Time.deltaTime, 0);
                    }
                }

                _ps.SetParticles(_particles);
            }
        }
    }
}
