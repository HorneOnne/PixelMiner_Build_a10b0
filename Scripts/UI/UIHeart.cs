using UnityEngine;
using UnityEngine.UI;
namespace PixelMiner.UI
{
    /// <summary>
    /// 1 heart = 10 health.
    /// </summary>
    public class UIHeart : MonoBehaviour
    {
        private Image _border;
        private Image _fill;

        private Sprite _fullHearth;
        private Sprite _halfHearth;
        private Sprite _borderHearth;


        private void Awake()
        {
            _border = GetComponent<Image>();
            _fill = transform.Find("Fill").GetComponent<Image>();

            _fullHearth = Resources.Load<Sprite>("gui/full_heart");
            _halfHearth = Resources.Load<Sprite>("gui/half_heart");
            _borderHearth = Resources.Load<Sprite>("gui/border_heart");
        }


        public void UpdateHealth(int health)
        {
            switch (health)
            {
                case 0:            
                    _fill.sprite = _borderHearth;
                    break;
                case 5:           
                    _fill.sprite = _halfHearth;
                    break;
                case 10:
                    _fill.sprite = _fullHearth;
                    break;
            }
        }
    }
}
