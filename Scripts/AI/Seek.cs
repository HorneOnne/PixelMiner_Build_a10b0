using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using PixelMiner.Core;
using UnityEngine;
namespace PixelMiner.AI
{

    public class Seek : Action
    {
        public float Speed;

        public float AngularSpeed = 10f;

        public float ArrivalDistance = 1f;

        public SharedEntity TargetEntity;
        private Vector3 _headLookOffset = new Vector3(0,1.5f,0f);

        private Zombie _zombie;
        private Vector3Int _previousTargetPosition;
        private int _currentWaypointIndex = 1;
        private float _arrivalWaypointDistanceSquare = 0.01f;

        public override void OnAwake()
        {
            _zombie = GetComponent<Zombie>();

            if (_zombie == null)
            {
                Debug.LogWarning("Not found zombie.cs reference.");
            }


        }
        public override void OnStart()
        {
            _currentWaypointIndex = 1;
        }


        public override TaskStatus OnUpdate()
        {
            //return TaskStatus.Running;
            if (HasArrived())
            {
                return TaskStatus.Success;
            }
            if(Vector3.Distance(transform.position, TargetEntity.Value.transform.position) < 1.0f)
            {
                return TaskStatus.Success;
            }

            // If target move or this object move out of seek pathfinding line -> Request new path.
            //if (IsTargetMoved())
            //{


            _zombie.HeadLookTarget.position = TargetEntity.Value.transform.position + _headLookOffset;
            //}
            return TaskStatus.Running;
        }


        public override void OnFixedUpdate()
        {
            if (_currentWaypointIndex >= _zombie.MovePath.Count) return;

            //Debug.Log("Movement");
            Vector3 targetPosition = _zombie.MovePath[_currentWaypointIndex];
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0;
 
            _zombie.Rigidbody.MovePosition(_zombie.Rigidbody.position + (direction * Speed * Time.deltaTime));
            //_zombie.Model.LookAt(transform.position + direction);
            RotateTowardDirection(direction);
            if (Vector3.SqrMagnitude(transform.position - targetPosition) < _arrivalWaypointDistanceSquare)
            {
                _currentWaypointIndex++;
                //if (_currentWaypointIndex >= _zombie.MovePath.Count)
                //{
                //    _currentWaypointIndex = 0;
                //}
            }
        }

        /// <summary>
        /// The task has ended. Stop moving.
        /// </summary>
        public override void OnEnd()
        {
            _currentWaypointIndex = 1;
            Stop();
        }

        /// <summary>
        /// The behavior tree has ended. Stop moving.
        /// </summary>
        public override void OnBehaviorComplete()
        {
            Stop();
        }


        private bool HasArrived()
        {
            return Vector3.SqrMagnitude(TargetEntity.Value.transform.position - _zombie.transform.position) < ArrivalDistance;
        }
        public void Stop()
        {
            _zombie.Rigidbody.velocity = new Vector3(0, _zombie.Rigidbody.velocity.y, 0);
        }

        private void RotateTowardDirection(Vector3 direction)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _zombie.Model.rotation = Quaternion.Slerp(_zombie.Model.rotation, targetRotation, AngularSpeed * Time.deltaTime);
        }
    }
}