using UnityEngine;

namespace PixelMiner.Core
{

    /// <summary>
    /// Currently only work on water shader
    /// </summary>
    public class AnimatedTextureBlockManager : MonoBehaviour
    {
        public static AnimatedTextureBlockManager Instance { get; private set; }

        private float _animatedTime = 0.5f;
        private float _animatedTimer;
        private int[] _waterAnimIndex = new int[] { 205, 206, 207, 222, 223};
        private int _currentIndex = 0;
        private void Awake()
        {
            Instance = this;
        }


        private void Update()
        {
            if(UnityEngine.Time.time - _animatedTimer > _animatedTime)
            {
                _animatedTimer = UnityEngine.Time.time;

                _currentIndex = (_currentIndex + 1) % _waterAnimIndex.Length;

                //Debug.Log(_waterAnimIndex[_currentIndex]);
                Shader.SetGlobalInt("AnimatedBlockTextureIndex", _waterAnimIndex[_currentIndex]);
            }
        }

    }
}
