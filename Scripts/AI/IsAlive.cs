using PixelMiner.Core;
using BehaviorDesigner.Runtime.Tasks;

namespace PixelMiner.AI
{
    public class IsAlive : Conditional
    {
        public SharedEntity ThisEntity;
        private Zombie _zombie;

        public override void OnStart()
        {
            UnityEngine.Debug.Log("OnStart");
            _zombie = ThisEntity.Value.GetComponent<Zombie>();
        }


        public override TaskStatus OnUpdate()
        {
            if (_zombie.IsAlive)
            {
                return TaskStatus.Running;
            }
            return TaskStatus.Failure;
        }
    }
}

