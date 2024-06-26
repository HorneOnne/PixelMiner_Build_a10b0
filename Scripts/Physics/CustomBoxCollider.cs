using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelMiner.Core;

namespace PixelMiner.Physics
{
    [System.Serializable]
    public class CustomBoxCollider : MonoBehaviour
    {
        [SerializeField] private PhysicEntities _attachedEntity;
        public Vector3 Center;
        public Vector3 Size;

        public Bounds Bounds
        {
            get
            {
                GetBounds(out Bounds bounds);   
                return bounds;
            }
        }

        public PhysicEntities AttachedEntity { get => _attachedEntity; }
        public System.Guid InstanceID { get; private set; }

        private void Awake()
        {
            GetPhysicEntity(ref _attachedEntity);
            InstanceID = System.Guid.NewGuid();

        }

        private void Start()
        {
            _attachedEntity.Position = this.transform.position + Center;
            GamePhysics.Instance.AddBox(this);
        }

        private void GetBounds(out Bounds bounds)
        {
            bounds = new Bounds(AttachedEntity.Position + Center, Size);
        }

        private void GetPhysicEntity(ref PhysicEntities physicEntity)
        {
            if(!TryGetComponent<PhysicEntities>(out physicEntity))
            {
                physicEntity = GetComponentInParent<PhysicEntities>();
            }
        }

        private void OnDestroy()
        {
            _attachedEntity = null;
        }

#if UNITY_EDITOR
        //private void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawWireCube(transform.position + Center, Size);
        //}
#endif
    }
}
