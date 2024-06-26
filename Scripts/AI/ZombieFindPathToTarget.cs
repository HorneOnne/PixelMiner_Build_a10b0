using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using PixelMiner.Core;
using UnityEngine;
namespace PixelMiner.AI
{
    public class ZombieFindPathToTarget : Action
    {
        public SharedEntity ThisEntity;
        public SharedEntity Target;
     

        private Zombie _zombie;
        private Vector3Int _previousTargetPosition;
        private PathRequest _request;
        enum FindState
        {
            FINDING,
            SUCCESS,
            FAILURE
        }
        private FindState _state;

        public override void OnAwake()
        {
            _zombie = ThisEntity.Value.GetComponent<Zombie>();

            if (_zombie == null)
            {
                Debug.LogWarning("Not found zombie.cs reference.");
            }


        }
        public override void OnStart()
        {
            _request = PathRequestPool.Pool.Get();
            //_request.SetPath(transform.position, Target.Value.position);
            _request.SetPath(Target.Value.transform.position, this.transform.position);
            PathRequestManager.Instance.RequestPath(_request);
            _state = FindState.FINDING;

            _request.OnRequestCompleted += FindPathCompleted;
        }

    

        public override TaskStatus OnUpdate()
        {
            switch(_state)
            {
                default:
                case FindState.FINDING:
                    return TaskStatus.Running;
                case FindState.SUCCESS:
                    _zombie.SetMovePath(_request.SimplifyPath);
                    return TaskStatus.Success;
                case FindState.FAILURE:
                    return TaskStatus.Failure;
            }
        }


        /// <summary>
        /// The task has ended. Stop moving.
        /// </summary>
        public override void OnEnd()
        {
            _request.OnRequestCompleted -= FindPathCompleted;
            PathRequestPool.Pool.Release(_request);
        }

        /// <summary>
        /// The behavior tree has ended. Stop moving.
        /// </summary>
        public override void OnBehaviorComplete()
        {
           
        }


        private void FindPathCompleted(bool success)
        {
            if(success)
            {
                _state = FindState.SUCCESS;
            }
            else
            {
                _state= FindState.FAILURE;
            }
        }


        private bool IsTargetMoved()
        {
            int targetRoundX = Mathf.FloorToInt(Target.Value.transform.position.x);
            int targetRoundY = Mathf.FloorToInt(Target.Value.transform.position.y);
            int targetRoundZ = Mathf.FloorToInt(Target.Value.transform.position.z);

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