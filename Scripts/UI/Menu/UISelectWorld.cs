using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using PixelMiner.Core;

namespace PixelMiner.UI
{
    public class UISelectWorld : CustomCanvas
    {
        private RectTransform _container;
        private Button _createNewWorldBtn;
        private Button _backBtn;
        private TMP_InputField _worldNameFiled;
        private TMP_InputField _seedField;

        [Header("Popup Tween")]
        private Ease _ease = Ease.Linear;
        private float _time = 0.2f;
        private Vector3 _initializeContainerPosition;
        private Tween _showTween;
        private Tween _hideTween;

        private void Start()
        {
            _container = transform.Find("Container").GetComponent<RectTransform>();
            _initializeContainerPosition = _container.anchoredPosition;
            _createNewWorldBtn = transform.Find("Container/Lower/CreateNewWorldBtn").GetComponent<Button>();
            _backBtn = transform.Find("Container/Lower/BackBtn").GetComponent<Button>();

            _worldNameFiled = transform.Find("Container/Middle/WorldName/InputField (TMP)").GetComponent<TMP_InputField>();
            _seedField = transform.Find("Container/Middle/Seed/InputField (TMP)").GetComponent<TMP_InputField>();


            _worldNameFiled.text = GameRules.DEFAULT_WORLD_NAME;

            _createNewWorldBtn.onClick.AddListener(() =>
            {
                // Audio
                AudioManager.Instance.PlaySelectSfx();

                // Save data cross scene
                if(_worldNameFiled.text.Length > 0)
                {
                    PlayerPrefs.SetString(GameRules.WORLD_NAME, _worldNameFiled.text);
                }
                else
                {
                    PlayerPrefs.SetString(GameRules.WORLD_NAME, GameRules.DEFAULT_WORLD_NAME);
                }

                if (_seedField.text.Length > 0)
                {
                    PlayerPrefs.SetString(GameRules.SEED, _seedField.text);
                }
                else
                {
                    PlayerPrefs.SetString(GameRules.SEED, GetRandomSeed().ToString());
                }



                // Show fake scene loading (prevent freeze detected by player when this scene unload)
                UIMenuManager.Instance.DisplayFakeLoadingScene();

                // Load scene
                Loader.Load(Loader.Scene.GameplayScene);
            });

            _backBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySelectSfx();
                Hide();
            });
        }

        private void OnDestroy()
        {
            _createNewWorldBtn.onClick.RemoveAllListeners();
            _backBtn.onClick.RemoveAllListeners();

            // Kill tweens
            if (_showTween.IsActive())
                _showTween.Kill();

            if (_hideTween.IsActive())
                _hideTween.Kill();
        }

        private int GetRandomSeed()
        {
            return Random.Range(int.MinValue, int.MaxValue);
        }


        #region Tween animation
        public void Show()
        {
            _showTween = _container.DOLocalMove(new Vector3(0, 0, 0), _time).
                SetEase(_ease).
                OnComplete(OnShowCompleted);
            ;
        }


        public void Hide()
        {
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
            
        }
        #endregion

    }
}
