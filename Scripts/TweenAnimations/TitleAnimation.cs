using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TitleAnimation : MonoBehaviour
{
    [Header("Scale")]
    [SerializeField] private float _scale = 1.0f;
    [SerializeField] private float _scaleTime = 0.25f;
    [SerializeField] private Ease _ease;
    private Tween _scaleTween;
    private void Start()
    {
        _scaleTween = transform.
            DOScale(_scale, _scaleTime).
            SetEase(_ease).
            SetLoops(-1, LoopType.Yoyo)
            ;
    }

    private void OnDestroy()
    {
        if(_scaleTween.IsActive())
        {
            _scaleTween.Kill();
        }
    }
}
