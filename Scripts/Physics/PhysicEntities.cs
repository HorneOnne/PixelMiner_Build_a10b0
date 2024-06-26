using UnityEngine;
using System.Collections.Generic;

namespace PixelMiner.Physics
{
    [System.Serializable]
    public class PhysicEntities : MonoBehaviour
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Mass;
        public bool Simulate;
        public bool OnGround;
        public Constraint Constraint{get; private set;}
        //public Bounds Bounds { get => Collider.bounds; }


        public int PhysicEntityIndex = -1;

        private void Awake()
        {
            
            
        }

        //public CustomRigidbody(Transform transform, AABB bound, Vector3 boxOffset, 
        //                     LayerMask layerMask)
        //{
        //    this.Transform = transform;
        //    this.Position = Transform.position;
        //    this.AABB = bound;
        //    this.BoxOffset = boxOffset;
        //    Velocity = default;
        //    Mass = 1;
        //    Simulate = true;
        //    //this.Layers = layers;
        //    //this.CollideLayers = collideLayers;
        //    this.PhysicLayer = layerMask;

        //    EntitiesIndex = -1;
        //    EntityRootIndex = -1;
        //    EntityNodeIndex = -1;

        //    Rigidbody
        //}

        public void SetVelocity(Vector3 vel)
        {
            Velocity = vel; 
        }
        public void SetVelocityX(float velX)
        {
            Velocity.x = velX;
        }
        public void SetVelocityY(float velY)
        {
            Velocity.y = velY;
        }
        public void SetVelocityZ(float velZ)
        {
            Velocity.z = velZ;
        }

        public void AddVelocity(Vector3 vel)
        {
            Velocity += vel;
        }
        public void AddVelocityX(float velX)
        {
            Velocity.x += velX;
        }
        public void AddVelocityY(float velY)
        {
            Velocity.y += velY;
        }
        public void AddVelocityZ(float velZ)
        {
            Velocity.z += velZ;
        }



        public void SetConstraint(Constraint constraint, bool enable)
        {
            if (enable)
            {
                // Add the flag using bitwise OR
                Constraint |= constraint;
            }
            else
            {
                // Remove the flag using bitwise AND and bitwise complement
                Constraint &= ~constraint;
            }
        }

        public bool GetConstraint(Constraint coordinate)
        {
            return (Constraint & coordinate) != 0;  
        }
    }

    [System.Flags]
    public enum Constraint : byte
    {
        X = 0x1, 
        Y = 0x2, 
        Z = 0x4,
    }

}
