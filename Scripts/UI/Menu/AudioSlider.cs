using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PixelMiner.UI
{
    public class AudioSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Slider _slider;
        private Image _backgroundImage;
        private Image _fillImage;
        private Image _handleImage;

        private void Awake()
        {
            _slider = GetComponent<Slider>();   
            _backgroundImage = transform.Find("Background").GetComponent<Image>();
            _fillImage = transform.Find("Fill Area/Fill").GetComponent<Image>();
            _handleImage = transform.Find("Handle Slide Area/Handle").GetComponent<Image>();

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
            _backgroundImage.sprite = Resources.Load<Sprite>("gui/slider/slider_bg");
            _fillImage.sprite = Resources.Load<Sprite>("gui/slider/slider_filled");
            _handleImage.sprite = Resources.Load<Sprite>("gui/slider/slider_handle");
        }

        private void LoadSelected()
        {
            _backgroundImage.sprite = Resources.Load<Sprite>("gui/slider/slider_bg_selected");
            _fillImage.sprite = Resources.Load<Sprite>("gui/slider/slider_filled_selected");
            _handleImage.sprite = Resources.Load<Sprite>("gui/slider/slider_handle_slected");
        }
    }
}
