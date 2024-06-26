using UnityEngine.UI;
using PixelMiner.Core;
using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace PixelMiner.UI
{
    public class UIMenuLoading : CustomCanvas
    {
        private Slider _loadingSlider;
        private Image _loadingIcon;
        private TextMeshProUGUI _loadingProgresText;

        private const int MAX_WORLD_GENERATION_PROGRESS = 10; // in percent
        private const int MAX_PRE_RENDER_PROGRESS = 40; // in percent
        private const int MAX_RENDER_PROGRESS = 100; // in percent

        private int _currentProgress = 0;
        private int _targetProgress = 0;

        public Sprite[] _loadingSprites;
        private int _currentLoadingSpriteIndex = 0;
        private int _nextLoadingSpriteIndex = 0;

        private void Start()
        {
            _loadingSlider = transform.Find("LoadingSlider").GetComponent<Slider>();
            _loadingIcon = transform.Find("LoadingIcon").GetComponent<Image>();
            _loadingProgresText = transform.Find("LoadingPercentText").GetComponent<TextMeshProUGUI>();

            WorldGeneration.Instance.OnFirstTimeGenerationProgress += UpdateWorldGenerationProgress;
            WorldLoading.Instance.OnFirstTimePreRenderProgress += UpdatePreRenderProgress;
            WorldLoading.Instance.OnFirstTimeRenderProgress += UpdateRenderProgress;
            WorldLoading.Instance.OnLoadingGameFinish += OnLoadingGameFinish;


            _loadingSprites = Resources.LoadAll<Sprite>("gui/loading/LoadingIcon");
        }


        private void FixedUpdate()
        {
            if(_currentProgress < _targetProgress)
            {
                _currentProgress++;
                _loadingSlider.value = _currentProgress;
                _loadingProgresText.text = _currentProgress + "%";

                _nextLoadingSpriteIndex = Mathf.Clamp(_currentProgress / _loadingSprites.Length, 0, _loadingSprites.Length - 1);
                if(_currentLoadingSpriteIndex != _nextLoadingSpriteIndex)
                {
                    _currentLoadingSpriteIndex = _nextLoadingSpriteIndex;
                    _loadingIcon.sprite = _loadingSprites[_currentLoadingSpriteIndex];
                }
               
                if (_currentProgress == 100)
                {
                    StartCoroutine(HideLoadingSceneCoroutine());
                }
            }
        }


        private void OnDestroy()
        {
            WorldGeneration.Instance.OnFirstTimeGenerationProgress -= UpdateWorldGenerationProgress;
            WorldLoading.Instance.OnFirstTimePreRenderProgress -= UpdatePreRenderProgress;
            WorldLoading.Instance.OnFirstTimeRenderProgress -= UpdateRenderProgress;
            WorldLoading.Instance.OnLoadingGameFinish -= OnLoadingGameFinish;
        }
        private void OnLoadingGameFinish()
        {
            //DisplayCanvas(false);
        }


        // loading 0% - 10%
        private void UpdateWorldGenerationProgress(float progress)
        {
            int sliderProgress = Mathf.FloorToInt(progress * MAX_WORLD_GENERATION_PROGRESS);
            //_loadingSlider.value = sliderProgress;
            _targetProgress = sliderProgress;
        }
        // loading 10% - 40%
        private void UpdatePreRenderProgress(float progress)
        {
            int sliderProgress = Map(Mathf.FloorToInt(progress * 100), 0, 100, 10, 40);
            //_loadingSlider.value = sliderProgress;
            _targetProgress = sliderProgress;
        }
        // loading 40% - 100%
        private void UpdateRenderProgress(float progress)
        {
            int sliderProgress = Map(Mathf.FloorToInt(progress * 100), 0, 100, 40, 100);
            //_loadingSlider.value = sliderProgress;
            _targetProgress = sliderProgress;
        }

        int Map(float s, float a1, float a2, float b1, float b2)
        {
            return Mathf.FloorToInt(b1 + (s - a1) * (b2 - b1) / (a2 - a1));
        }

        private IEnumerator HideLoadingSceneCoroutine()
        {
            yield return new WaitForSeconds(.5f);
            DisplayCanvas(false);
        }
    }
}
