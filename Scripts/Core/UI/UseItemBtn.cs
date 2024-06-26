using UnityEngine;
using UnityEngine.EventSystems;
using PixelMiner.Core;

namespace PixelMiner.Core.UI
{
    public class UseItemBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public static event System.Action OnPressed;
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

        private void Update()
        {
            if (_isPressed)
            {
                OnPressed?.Invoke();
            }
        }

        private void SetupPlayer()
        {
            _playerController = Main.Instance.Players[0].GetComponent<PlayerController>();
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
        }

    }
}

