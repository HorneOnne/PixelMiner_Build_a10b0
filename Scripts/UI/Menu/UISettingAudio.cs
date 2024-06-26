using UnityEngine.UI;
using UnityEngine;
using TMPro;
using PixelMiner.Core;
using System.Collections.Generic;

namespace PixelMiner.UI
{
    public class UISettingAudio : CustomCanvas
    {
        private TextMeshProUGUI _mainText;
        private Slider _mainSlider;

        private TextMeshProUGUI _musicText;
        private Slider _musicSlider;

        private TextMeshProUGUI _soundText;
        private Slider _soundSlider;

        private Button _resetToDefaultBtn;


        private void Start () 
        {        
            // main
            _mainText = transform.Find("AudioPanel/Main/Text").GetComponent<TextMeshProUGUI>();
            if (_mainText == null) Debug.LogError("Missing _mainText reference.");
            _mainSlider = transform.Find("AudioPanel/Main/Slider").GetComponent<Slider>();
            if (_mainSlider == null) Debug.LogError("Missing _mainSlider reference.");

            // music
            _musicText = transform.Find("AudioPanel/Music/Text").GetComponent<TextMeshProUGUI>();
            if (_musicText == null) Debug.LogError("Missing _musicText reference.");
            _musicSlider = transform.Find("AudioPanel/Music/Slider").GetComponent<Slider>();
            if (_musicSlider == null) Debug.LogError("Missing _musicSlider reference.");

            // sound
            _soundText = transform.Find("AudioPanel/Sound/Text").GetComponent<TextMeshProUGUI>();
            if (_soundText == null) Debug.LogError("Missing _soundText reference.");
            _soundSlider = transform.Find("AudioPanel/Sound/Slider").GetComponent<Slider>();
            if (_soundSlider == null) Debug.LogError("Missing _soundSlider reference.");

            _resetToDefaultBtn = transform.Find("AudioPanel/ResetToDefaultBtn").GetComponent<Button>();
            if (_resetToDefaultBtn == null) Debug.LogError("Missing _resetToDefaultBtn reference.");
           

            // set default audio
            SetDefaultAllSound();



            _mainSlider.onValueChanged.AddListener(UpdateMainAudio);
            _musicSlider.onValueChanged.AddListener(UpdateMusicAudio);
            _soundSlider.onValueChanged.AddListener(UpdateSoundAudio);

            _resetToDefaultBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySelectSfx();
                SetDefaultAllSound();
            });
        }

 

        private void OnDestroy()
        {
            _mainSlider.onValueChanged.RemoveAllListeners();
            _musicSlider.onValueChanged.RemoveAllListeners();
            _soundSlider.onValueChanged.RemoveAllListeners();
            _resetToDefaultBtn.onClick.RemoveAllListeners();
        }

        private void UpdateMainAudio(float value)
        {
            AudioManager.Instance.SetMasterVolume(value / 100f);
            _mainText.text = "Main: " + value.ToString();
        }

        private void UpdateMusicAudio(float value)
        {
            AudioManager.Instance.SetMusicVolume(value / 100f);
            _musicText.text = "Music: " + value.ToString();
        }

        private void UpdateSoundAudio(float value)
        {
            AudioManager.Instance.SetSoundVolume(value / 100f);
            _soundText.text = "Sound: " + value.ToString();
        }


        private void SetDefaultAllSound()
        {
            int defaultMasterVolume = Mathf.FloorToInt(AudioManager.Instance.DefaultMasterVolume * _mainSlider.maxValue);
            int defaultMusicVolume = Mathf.FloorToInt(AudioManager.Instance.DefaultMusicVolume * _musicSlider.maxValue);
            int defaultSoundVolume = Mathf.FloorToInt(AudioManager.Instance.DefaultSoundVolume * _soundSlider.maxValue);
            UpdateMainAudio(defaultMasterVolume);
            UpdateMusicAudio(defaultMusicVolume);
            UpdateSoundAudio(defaultSoundVolume);

            _mainSlider.value = defaultMasterVolume;
            _musicSlider.value = defaultMusicVolume;
            _soundSlider.value = defaultSoundVolume;
        }
      
    }
}
