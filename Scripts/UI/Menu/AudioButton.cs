using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace PixelMiner.UI
{
    public class AudioButton: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Button _button;
        private TextMeshProUGUI _btnText;

        [Header("Text Color")]
        [SerializeField] private Color _defaultTextColor;
        [SerializeField] private Color _hoverTextColor;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _btnText = _button.GetComponentInChildren<TextMeshProUGUI>();

            LoadDefault();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            LoadSelected();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            LoadDefault();
        }

        private void LoadDefault()
        {
            _btnText.color = _defaultTextColor;
            _button.image.sprite = Resources.Load<Sprite>("gui/button");
        }

        private void LoadSelected()
        {
            _btnText.color = _hoverTextColor;
            _button.image.sprite = Resources.Load<Sprite>("gui/button_selected");
        }
    }
}
