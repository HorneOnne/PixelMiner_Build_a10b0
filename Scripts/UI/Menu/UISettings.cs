using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using PixelMiner.Core;
using System;

namespace PixelMiner.UI
{
    public class UISettings : CustomCanvas
    {        
        public static event System.Action OnUISettingsShown;
        public static event System.Action OnUISettingsHide;

        private RectTransform _container;
        private Button _backBtn;

        [SerializeField] private UISettingsLabel[] _settingLabels;
        private int _currentContentActiveIndex = 0;


        [Header("Popup Tween")]
        private Ease _ease = Ease.Linear;
        private float _time = 0.2f;
        private Tween _showTween;
        private Tween _hideTween;

        // Cached
        private Vector3 _initializeContainerPosition;
        private void Start()
        {
            _container = transform.Find("Container").GetComponent<RectTransform>();
            if (_container == null) Debug.LogError("Missing _container reference.");
            _initializeContainerPosition = _container.anchoredPosition;

            _backBtn = transform.Find("Container/Upper/BackBtn").GetComponent<Button>();
            if (_backBtn == null) Debug.LogError("Missing _backBtn reference.");


            if (Camera.main.aspect >= 1.7)
            {
                Debug.Log("16:9");
                _container.anchoredPosition = new Vector2(1920, _container.anchoredPosition.y);
            }       
            else if (Camera.main.aspect >= 1.5)
            {
                Debug.Log("3:2");
                _container.anchoredPosition = new Vector2(1800, _container.anchoredPosition.y);
            }              
            else
            {
                Debug.Log("4:3");
                _container.anchoredPosition = new Vector2(1800, _container.anchoredPosition.y);
            }

            _settingLabels = transform.GetComponentsInChildren<UISettingsLabel>();


            _backBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySelectSfx();
                Hide();
            });

            _settingLabels[_currentContentActiveIndex].Select(true);
        }

        private void OnDestroy()
        {
            _backBtn.onClick.RemoveAllListeners();

            // Kill tweens
            if (_showTween.IsActive())
                _showTween.Kill();

            if (_hideTween.IsActive())
                _hideTween.Kill();
        }

        public void Show()
        {
            _settingLabels[_currentContentActiveIndex].Select(true);
            OnUISettingsShown?.Invoke();
            _showTween = _container.DOLocalMove(new Vector3(0, 0, 0), _time).
                SetEase(_ease).
                OnComplete(OnShowCompleted);
                ;
        }

     
        public void Hide()
        {
            OnUISettingsHide?.Invoke();
            _hideTween = _container.DOLocalMove(_initializeContainerPosition, _time).
                SetEase(_ease).
                OnComplete(OnHideCompleted)
                ;
   
        }

        private void OnShowCompleted()
        {
    
        }

        private void OnHideCompleted()
        {
            UIMenuManager.Instance.DisplaySettingsMenu(false);
        }

    }
}
