using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelMiner.Core;

namespace PixelMiner.Core
{
    public class CharacterAnimEventDriven : MonoBehaviour
    {
        private Player _player;
        private PlayerController _playerController;
        //private Vector3 _previousPosition;
        private AudioManager _audioManager;

        private void Start()
        {
            //_previousPosition = transform.position;
            _player = GetComponentInParent<Player>();
            _playerController = GetComponentInParent<PlayerController>();   
            _audioManager = AudioManager.Instance;
        }
        public void PlaySound()
        {
            if (_playerController.OnGround())
            {
                if (_playerController.Rigidbody.velocity.x != 0 || _playerController.Rigidbody.velocity.z != 0)
                {
                    //_previousPosition = transform.position;
                    
                    float magOfVelocityXZ = new Vector3(_playerController.Rigidbody.velocity.x, 0f, _playerController.Rigidbody.velocity.z).magnitude;
                    if (magOfVelocityXZ > _playerController.MoveSpeed - 0.01f)
                    {
                        _audioManager.PlayStepSfx(_player.transform.position);
                    }
                  
                }

            }  
        }

   
    }
}
