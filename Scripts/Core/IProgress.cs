using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelMiner.Core
{
    public interface IProgress
    {
       public float CurrentProgress { get; }
        public void IncrementProgress(float amount);
        public void ResetProgress();
    }
}
