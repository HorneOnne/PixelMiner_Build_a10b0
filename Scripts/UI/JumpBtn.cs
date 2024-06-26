using UnityEngine;
using UnityEngine.EventSystems;
using PixelMiner.Core;
using System;

namespace PixelMiner.UI
{
    public class JumpBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private bool _isPressed;
        private PlayerController _playerController;
        private void Start()
        {
            _isPressed = false;
            Main.Instance.OnCharacterInitialize += SetupPlayer;
        }
        private void OnDestroy()
        {
            Main.Instance.OnCharacterInitialize -= SetupPlayer;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
        }

        private void FixedUpdate()
        {
            if(_isPressed)
            {
                _playerController.JumpLogicHandler();
            }
        }

        private void SetupPlayer()
        {
            _playerController = Main.Instance.Players[0].GetComponent<PlayerController>();
        }
    }
}

