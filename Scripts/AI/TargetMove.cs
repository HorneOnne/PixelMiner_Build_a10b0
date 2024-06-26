using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace PixelMiner.AI
{
    public class TargetMove : Conditional
    {
        public SharedEntity TargetEntity;
        private Vector3Int _previousTargetPosition;

        public override void OnStart()
        {
            int targetRoundX = Mathf.FloorToInt(TargetEntity.Value.transform.position.x);
            int targetRoundY = Mathf.FloorToInt(TargetEntity.Value.transform.position.y);
            int targetRoundZ = Mathf.FloorToInt(TargetEntity.Value.transform.position.z);
            _previousTargetPosition = new Vector3Int(targetRoundX, targetRoundY, targetRoundZ);
        }


        public override TaskStatus OnUpdate()
        {
            if (IsTargetMoved())
            {
                //Debug.Log("Move");
                return TaskStatus.Success;
            }
            else
            {
                //Debug.Log("Idle");
                return TaskStatus.Running;
            }
        }

        private bool IsTargetMoved()
        {
            int targetRoundX = Mathf.FloorToInt(TargetEntity.Value.transform.position.x);
            int targetRoundY = Mathf.FloorToInt(TargetEntity.Value.transform.position.y);
            int targetRoundZ = Mathf.FloorToInt(TargetEntity.Value.transform.position.z);

            if (_previousTargetPosition.x != targetRoundX ||
               _previousTargetPosition.y != targetRoundY ||
               _previousTargetPosition.z != targetRoundZ)
            {
                _previousTargetPosition = new Vector3Int(targetRoundX, targetRoundY, targetRoundZ);
                return true;
            }
            else
            {
                return false;
            }
        }
    }


}

