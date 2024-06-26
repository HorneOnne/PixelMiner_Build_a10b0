using UnityEngine;
using PixelMiner.Core;
using System.Collections.Generic;

namespace PixelMiner.Core
{
    public abstract class Mob : Entity
    {
        private Rigidbody _rb;
        [SerializeField] private List<Vector3> _movePath;


        #region Properties
        public Rigidbody Rigidbody { get { return _rb; } }
        public List<Vector3> MovePath { get { return _movePath; } }
        #endregion


        #region Unity callbacks
        protected virtual void Awake()
        {
            _rb= GetComponent<Rigidbody>();
            _movePath = new();
            // stop object fall when chunk not already finish loading.
            _rb.isKinematic = true;
        }

        protected virtual void Start()
        {
            WorldLoading.Instance.OnLoadingGameFinish += EnablePhysics;
        }

        protected virtual void OnDestroy()
        {
            WorldLoading.Instance.OnLoadingGameFinish -= EnablePhysics;
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                if (_movePath.Count > 0)
                {
                    for (int i = 0; i < _movePath.Count - 1; i++)
                    {
                        Gizmos.DrawLine(_movePath[i], _movePath[i + 1]);
                    }

                    Gizmos.color = Color.cyan;
                    for (int i = 0; i < _movePath.Count; i++)
                    {
                        Gizmos.DrawCube(_movePath[i], new Vector3(0.5f, 0.5f, 0.5f));
                    }
                }
            }
        }


        #endregion


        #region Entity callback
        //public override void Attack(Entity target)
        //{
           
        //}

        //public override void TakeDamge(byte damage)
        //{
           
        //}

        //public override void Die()
        //{
           
        //}
        #endregion

        public void SetMovePath(IEnumerable<Vector3> path)
        {
            _movePath.Clear();
            _movePath.AddRange(path);
        }


        public void EnablePhysics()
        {
            _rb.isKinematic = false;
        }

      
    }
}
