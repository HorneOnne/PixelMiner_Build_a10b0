using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelMiner.UI
{
    public class UIMenuManager : MonoBehaviour
    {
        public static UIMenuManager Instance { get; private set; }

        public UIMainmenu UIMainmenu{get; private set;}
        public UISettings UISettings { get; private set; }
        public UISettingAudio UISettingAudio { get; private set; }
        public UISelectWorld UISelectWorld { get; private set; }

        [SerializeField] public Canvas _fakeLoadSceneCanvas;


        private void Awake()
        {
            Instance = this;

            _fakeLoadSceneCanvas.enabled = false;

            #region Find ui references.
            UIMainmenu = FindAnyObjectByType<UIMainmenu>();
            if (UIMainmenu == null)
                Debug.LogError("Missing UIMainmenu reference.");

            UISettings = FindAnyObjectByType<UISettings>();
            if (UISettings == null)
                Debug.LogError("Missing UISettings reference.");

            UISettingAudio = FindAnyObjectByType<UISettingAudio>();
            if (UISettingAudio == null)
                Debug.LogError("Missing UISettingAudio reference.");

            UISelectWorld = FindAnyObjectByType<UISelectWorld>();
            if (UISelectWorld == null)
                Debug.LogError("Missing UISelectWorld reference.");


            #endregion
        }


        private void Start()
        {
            CloseAll();
            DisplayMainMenu(true);

        }

        public void CloseAll()
        {
            DisplayMainMenu(false);
            DisplaySettingsMenu(false);
            DisplaySettingAudioMenu(false);
            DisplaySelectWorldMenu(false);
        }


        public void DisplayMainMenu(bool isActive)
        {
            UIMainmenu.DisplayCanvas(isActive);
        }

        public void DisplaySettingsMenu(bool isActive)
        {
            UISettings.DisplayCanvas(isActive);

            if(!isActive)
            {
                DisplaySettingAudioMenu(false);
            }
        }

        public void DisplaySettingAudioMenu(bool isActive)
        {
            UISettingAudio.DisplayCanvas(isActive);
        }

        public void DisplaySelectWorldMenu(bool isActive)
        {
            UISelectWorld.DisplayCanvas(isActive);
        }

        public void DisplayFakeLoadingScene()
        {
            _fakeLoadSceneCanvas.enabled = true;
        }

    }
}
