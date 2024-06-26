using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace PixelMiner.AI
{
    public class Wait : Action
    {
        public float WaitTime = 1.0f;

        private float _waitDuration;
        private float _startTime;
        private float _pauseTime;

        public override void OnStart()
        {
            _startTime = Time.time;
            _waitDuration = WaitTime;
        }


        public override TaskStatus OnUpdate()
        {
            //Debug.Log($"{_startTime}  {_waitDuration}  {Time.time}");
            if (_startTime + _waitDuration < Time.time)
            {
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }

        //public override void OnPause(bool paused)
        //{
        //    if (paused)
        //    {
        //        _pauseTime = Time.time;
        //    }
        //    else
        //    {
        //        _startTime += (Time.time - _pauseTime);
        //    }
        //}
    }
}

