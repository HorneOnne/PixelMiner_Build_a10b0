using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using PixelMiner.Core;

namespace PixelMiner.AI
{
    public class Nearby : Conditional
    {
        public SharedEntity ThisEntity;
        public SharedEntity TargetEntity;
    
        private float _squareDistance = 2500; // 50^2

        public override void OnAwake()
        {
            TargetEntity.Value = Main.Instance.Players[0].GetComponent<Player>();
            ThisEntity.Value = this.gameObject.GetComponent<Entity>();
        }


        public override TaskStatus OnUpdate()
        {
            if (Vector3.SqrMagnitude(transform.position - TargetEntity.Value.transform.position) < _squareDistance)
            {
                //Debug.Log("Success");
                return TaskStatus.Success;
            }
            //Debug.Log("Failure");
            return TaskStatus.Failure;
        }
    }


}

