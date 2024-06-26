using UnityEngine;
using DG.Tweening;

namespace PixelMiner.UI
{
    public class UICharacterAnimation : MonoBehaviour
    {
        [Header("Scale")]
        [SerializeField] private float _scale = 1.0f;
        [SerializeField] private float _scaleTime = 0.25f;
        [SerializeField] private Ease _ease;

        private Tween _showTween;
        private Tween _hideTween;
        private void Start()
        {
            UISettings.OnUISettingsShown += HideCharacter;
            UISettings.OnUISettingsHide += ShowCharacter;


        }

        private void OnDestroy()
        {
            UISettings.OnUISettingsShown -= HideCharacter;
            UISettings.OnUISettingsHide -= ShowCharacter;

            // Kill tweens
            if (_showTween.IsActive())
                _showTween.Kill();

            if (_hideTween.IsActive())
                _hideTween.Kill();
        }


        private void HideCharacter()
        {
            _hideTween = transform.
                DOScale(0, _scaleTime).
                SetEase(_ease)
                ;
        }

        private void ShowCharacter()
        {
            _showTween = transform.
                DOScale(1, _scaleTime).
                SetEase(_ease)
                ;
        }
    }
}

