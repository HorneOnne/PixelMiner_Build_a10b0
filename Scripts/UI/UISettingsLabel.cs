using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace PixelMiner.UI
{
    public class UISettingsLabel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        protected Button button;
        protected Image icon;
        protected TextMeshProUGUI nameText;
        protected bool isSelected = false;

        [SerializeField] protected Color defaultTextColor;
        [SerializeField] protected Color hoverTextColor;
        [SerializeField] protected Color selectedTextColor;

        protected virtual void Awake()
        {
            button = GetComponent<Button>();
            if (button == null) Debug.LogError("Missing _button reference.");

            icon = transform.Find("Icon").GetComponent<Image>();
            nameText = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        }

        public virtual void Select(bool isSelect)
        {
            isSelected = isSelect;
            if (isSelected)
            {
                LoadSelected();
            }
            else
            {
                LoadDefault();
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            LoadHover();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if(isSelected)
            {
                LoadSelected();
            }
            else
            {
                LoadDefault();
            }
         
        }

        private void LoadDefault()
        {
            button.image.sprite = Resources.Load<Sprite>("gui/settingbtn");
            nameText.color = defaultTextColor;
        }

        private void LoadSelected()
        {
            button.image.sprite = Resources.Load<Sprite>("gui/settingbtn_selected");
            nameText.color = selectedTextColor;
        }

        private void LoadHover()
        {
            button.image.sprite = Resources.Load<Sprite>("gui/settingbtn_hover");
            nameText.color = hoverTextColor;
        }
    }
}
