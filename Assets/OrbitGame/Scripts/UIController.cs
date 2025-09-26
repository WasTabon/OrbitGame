using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI blinkingText;
    public float blinkDuration = 1f;
    
    private RocketLaunchSystem rocketSystem;
    private Tween blinkTween;
    private bool allRocketsLaunched = false;
    
    void Start()
    {
        rocketSystem = FindObjectOfType<RocketLaunchSystem>();
        
        if (blinkingText == null)
        {
            blinkingText = GetComponent<TextMeshProUGUI>();
        }
        
        if (blinkingText != null)
        {
            UpdateText();
            StartBlinking();
        }
    }
    
    void Update()
    {
        if (!allRocketsLaunched && rocketSystem != null)
        {
            UpdateText();
            CheckAllRocketsLaunched();
        }
    }
    
    void UpdateText()
    {
        if (rocketSystem != null && blinkingText != null)
        {
            int remainingRockets = rocketSystem.requiredRocketsCount - rocketSystem.GetLaunchedRocketsCount();
            blinkingText.text = $"TAP TO LAUNCH THE ROCKET ({remainingRockets})";
        }
    }
    
    void StartBlinking()
    {
        blinkTween = blinkingText.DOFade(0f, blinkDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    
    void CheckAllRocketsLaunched()
    {
        if (rocketSystem.GetLaunchedRocketsCount() >= rocketSystem.requiredRocketsCount)
        {
            StopBlinking();
            allRocketsLaunched = true;
        }
    }
    
    void StopBlinking()
    {
        if (blinkTween != null)
        {
            blinkTween.Kill();
        }
        
        if (blinkingText != null)
        {
            blinkingText.gameObject.SetActive(false);
        }
    }
    
    void OnDestroy()
    {
        if (blinkTween != null)
        {
            blinkTween.Kill();
        }
    }
}