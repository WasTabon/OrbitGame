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
    
    RocketLauncher launcher;
    
    private Tween blinkTween;
    private bool isRocketLaunched = false;
    
    void Start()
    {
        launcher = FindObjectOfType<RocketLauncher>();
        
        if (blinkingText == null)
        {
            blinkingText = GetComponent<TextMeshProUGUI>();
        }
        
        if (blinkingText != null)
        {
            StartBlinking();
        }
    }
    
    void Update()
    {
        if (!isRocketLaunched)
        {
            CheckRocketLaunched();
        }
    }
    
    void StartBlinking()
    {
        blinkTween = blinkingText.DOFade(0f, blinkDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    
    void CheckRocketLaunched()
    {
        if (!launcher.enabled)
        {
            StopBlinking();
            isRocketLaunched = true;
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