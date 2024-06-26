using UnityEngine;
using PixelMiner.Core;
using BehaviorDesigner.Runtime.Tasks;

namespace PixelMiner.AI
{
    public class BeAttacked : Conditional
    {
        //public SharedEntity TargetEntity;
        public SharedEntity ThisEntity;

        private Zombie _zombie;
  
        public override void OnStart()
        {
            _zombie = ThisEntity.Value.GetComponent<Zombie>();
        }


        public override TaskStatus OnUpdate()
        {
            if(_zombie.BeAttacked)
            {
                _zombie.BeAttacked = false;
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }
    }
}

