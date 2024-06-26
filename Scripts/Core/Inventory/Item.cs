using UnityEngine;
using PixelMiner.DataStructure;
using PixelMiner.Miscellaneous;
using PixelMiner.Core;
using PixelMiner.Enums;
namespace PixelMiner.Core
{
    public abstract class Item : MonoBehaviour
    {
        [field: SerializeField] public ItemData Data { get; private set; }
        protected BoxCollider boxCollider;
        protected Rigidbody rb;
        protected Renderer[] _renderers;

        protected Main main;
        protected bool AppliedPhysics { get; private set; } = false;



        protected virtual void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
     
        }

        protected virtual void Start()
        {
            // Add texture to material
            for(int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].material.SetTexture("_MainTex", Data.Texture);
            }
           
            main = Main.Instance;
        }


        public virtual void AddPhysics()
        {
            boxCollider = this.gameObject.AddComponent<BoxCollider>();
            rb = this.gameObject.AddComponent<Rigidbody>();
            this.gameObject.AddComponent<Floater>();
            rb.mass = Data.Mass;
            rb.drag = 1f;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            boxCollider.center = Data.Center;
            boxCollider.size = Data.BoundSize;

            AppliedPhysics = true;
        }

        /// <summary>
        /// time in seconds.
        /// </summary>
        /// <param name="time"></param>
        public void AddPhysicAfter(float time)
        {
            Invoke(nameof(AddPhysics), time);
        }


        public virtual void RemovePhysics()
        {
            AppliedPhysics = false;

            if (boxCollider != null)
            {
                Destroy(boxCollider);
            }
            else
            {
                Debug.LogWarning("No BoxCollider component found on GameObject " + gameObject.name);
            }

            if (rb != null)
            {
                Destroy(rb);
            }
            else
            {
                Debug.LogWarning("No Rigidbody component found on GameObject " + gameObject.name);
            }
        }

        public void EnablePhysics(bool enabled)
        {
            if (enabled)
            {
                rb.isKinematic = true;
                boxCollider.enabled = true;
            }
            else
            {
                rb.isKinematic = false;
                boxCollider.enabled = false;

            }
        }

        public void EnableFloatingRotateEffect(bool enable)
        {

            if (_renderers != null)
            {
                if (enable)
                {
                    for(int i = 0; i < _renderers.Length; i++)
                    {
                        _renderers[i].material.SetFloat("_ApplyFloatingRotate", 1);
                    }
                   
                }
                else
                {
                    for (int i = 0; i < _renderers.Length; i++)
                    {
                        _renderers[i].material.SetFloat("_ApplyFloatingRotate", 0);
                    }               
                }
            }
        }
    }
}
