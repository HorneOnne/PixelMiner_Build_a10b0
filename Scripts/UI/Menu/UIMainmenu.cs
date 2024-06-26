using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PixelMiner.Core;

namespace PixelMiner.UI
{
    public class UIMainmenu : CustomCanvas
    {
        private Button _playBtn;
        private Button _settingsBtn;


        private void Start()
        {
            _playBtn = transform.Find("MenuBtns/PlayBtn").GetComponent<Button>();
            _settingsBtn = transform.Find("MenuBtns/SettingsBtn").GetComponent<Button>();

            if (_playBtn == null) Debug.LogError("Missing _playBtn reference.");
            if (_settingsBtn == null) Debug.LogError("Missing _settingsBtn reference.");

            _playBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySelectSfx();
                UIMenuManager.Instance.DisplaySelectWorldMenu(true);
                UIMenuManager.Instance.UISelectWorld.Show();
            });

            _settingsBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySelectSfx();

                UIMenuManager.Instance.DisplaySettingsMenu(true);
                UIMenuManager.Instance.UISettings.Show();
            });
        }

        private void OnDestroy()
        {
            _playBtn.onClick.RemoveAllListeners();
            _settingsBtn.onClick.RemoveAllListeners();
        }
    }
}
