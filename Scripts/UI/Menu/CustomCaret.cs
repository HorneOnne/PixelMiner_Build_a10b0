using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace PixelMiner.UI
{
    public class CustomCaret : MonoBehaviour
    {
        public TMP_InputField inputField;
        public TextMeshProUGUI text;
        public RectTransform Caret;

        public Vector2 offset;

        private Image image; // Reference to the Image component
        [SerializeField] private float _blinkSpeed = 1f; // Speed of the blink effect
        private float _blinkSpeedCounter = 0.0f; // Speed of the blink effect
        private bool _isOn = false;
        private bool _showCustomCaret = true;

        private void Start()
        {
            inputField.ActivateInputField();
            image = Caret.GetComponent<Image>();
        }


        private void Update()
        {
            //Debug.Log($"{inputField.caretPosition}  {inputField.text.Length}");
            Debug.Log($"{text.text.Length}  {text.GetRenderedValues(true)}");
            if(inputField.caretPosition == inputField.text.Length)
            {
                _showCustomCaret = true;
            }
            else
            {
                _showCustomCaret = false;
            }


            if (_showCustomCaret)
            {
                inputField.caretColor = Color.clear;
                GetCaretPosition();
                _blinkSpeedCounter += UnityEngine.Time.deltaTime;
                if (_blinkSpeedCounter > _blinkSpeed)
                {
                    _blinkSpeedCounter -= _blinkSpeed;
                    _isOn = !_isOn;
                    if (_isOn)
                    {
                        image.enabled = true;
                    }
                    else
                    {
                        image.enabled = false;
                    }
                }
            }
            else
            {
                inputField.caretColor = Color.white;
                image.enabled = false;
            }          
        }



        public Vector2 GetCaretPosition()
        {
            if (text.text.Length == 1)
            {
                Caret.anchoredPosition = offset;
                return Caret.anchoredPosition;
            }
            else
            {
                Vector2 caretPosition = text.GetRenderedValues();
             
                Caret.anchoredPosition = new Vector2(caretPosition.x + offset.x , offset.y);
                return caretPosition;
            }
           
        
        }

      



   

      
    }
}
