using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using PixelMiner.Core;
using UnityEngine;
namespace PixelMiner.AI
{
    public class Attack : Action
    {
        public SharedEntity ThisEntity;
        public SharedEntity TargetEntity;
       
        public override TaskStatus OnUpdate()
        {
            ThisEntity.Value.Attack(TargetEntity.Value);
            return TaskStatus.Success;
        }

    }
}