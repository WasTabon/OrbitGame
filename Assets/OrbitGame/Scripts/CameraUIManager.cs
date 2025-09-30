using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class CameraUIManager : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Level Info Panel")]
    [SerializeField] private GameObject levelInfoPanel;
    [SerializeField] private Image modifierIcon;
    [SerializeField] private TMP_Text modifierNameText;
    [SerializeField] private TMP_Text modifierDescriptionText;
    [SerializeField] private Button startLevelButton;
    [SerializeField] private Button closePanelButton;

    [Header("Panel Animation Settings")]
    [SerializeField] private float panelAnimationDuration = 0.6f;
    [SerializeField] private float iconRotationAmount = 360f;
    [SerializeField] private float iconBounceDuration = 0.4f;

    [Header("Modifiers Database")]
    [SerializeField] private ModifiersDatabase modifiersDatabase;

    private CanvasGroup backButtonCanvasGroup;
    private CanvasGroup moveLeftCanvasGroup;
    private CanvasGroup moveRightCanvasGroup;
    private CanvasGroup levelInfoPanelCanvasGroup;

    private RectTransform panelRectTransform;
    private RectTransform iconRectTransform;
    private RectTransform nameTextRectTransform;
    private RectTransform descriptionTextRectTransform;
    private RectTransform startButtonRectTransform;

    private Vector3 panelOriginalScale;
    private Vector3 iconOriginalScale;

    private int currentLevelIndex;
    private System.Action onStartLevelCallback;

    private void Start()
    {
        SetupButton(backButton, out backButtonCanvasGroup, false);
        SetupButton(moveLeftButton, out moveLeftCanvasGroup, true);
        SetupButton(moveRightButton, out moveRightCanvasGroup, true);

        if (levelInfoPanel != null)
        {
            levelInfoPanelCanvasGroup = levelInfoPanel.GetComponent<CanvasGroup>();
            if (levelInfoPanelCanvasGroup == null)
            {
                levelInfoPanelCanvasGroup = levelInfoPanel.AddComponent<CanvasGroup>();
            }

            panelRectTransform = levelInfoPanel.GetComponent<RectTransform>();
            panelOriginalScale = panelRectTransform.localScale;

            levelInfoPanel.SetActive(false);
        }

        if (modifierIcon != null)
        {
            iconRectTransform = modifierIcon.GetComponent<RectTransform>();
            iconOriginalScale = iconRectTransform.localScale;
        }

        if (modifierNameText != null)
            nameTextRectTransform = modifierNameText.GetComponent<RectTransform>();

        if (modifierDescriptionText != null)
            descriptionTextRectTransform = modifierDescriptionText.GetComponent<RectTransform>();

        if (startLevelButton != null)
        {
            startButtonRectTransform = startLevelButton.GetComponent<RectTransform>();
            startLevelButton.onClick.AddListener(OnStartLevelButtonClick);
        }

        if (closePanelButton != null)
        {
            closePanelButton.onClick.AddListener(OnClosePanelButtonClick);
        }
    }

    private void SetupButton(Button button, out CanvasGroup canvasGroup, bool startVisible)
    {
        if (button == null)
        {
            canvasGroup = null;
            return;
        }

        canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = startVisible ? 1f : 0f;
        button.gameObject.SetActive(startVisible);
    }

    public void ShowLevelInfoPanel(int levelIndex, System.Action onStartLevel)
    {
        currentLevelIndex = levelIndex;
        onStartLevelCallback = onStartLevel;

        int modifierIndex = levelIndex;
        if (levelIndex > 5)
        {
            modifierIndex = Random.Range(1, 6);
        }

        LevelModifierData modifier = modifiersDatabase.GetModifier(modifierIndex);
        
        if (modifier != null)
        {
            if (modifierIcon != null)
                modifierIcon.sprite = modifier.modifierIcon;
            
            if (modifierNameText != null)
                modifierNameText.text = modifier.modifierName;
            
            if (modifierDescriptionText != null)
                modifierDescriptionText.text = modifier.description;
        }

        if (levelInfoPanel != null)
        {
            levelInfoPanel.SetActive(true);
            PlayOpenAnimation();
        }
    }

    private void PlayOpenAnimation()
    {
        DOTween.Kill(panelRectTransform);
        DOTween.Kill(iconRectTransform);
        DOTween.Kill(nameTextRectTransform);
        DOTween.Kill(descriptionTextRectTransform);
        DOTween.Kill(startButtonRectTransform);
        DOTween.Kill(levelInfoPanelCanvasGroup);

        levelInfoPanelCanvasGroup.alpha = 0f;
        panelRectTransform.localScale = Vector3.zero;
        
        if (iconRectTransform != null)
        {
            iconRectTransform.localScale = Vector3.zero;
            iconRectTransform.rotation = Quaternion.Euler(0, 0, -90f);
        }

        if (nameTextRectTransform != null)
        {
            nameTextRectTransform.localScale = Vector3.zero;
        }

        if (descriptionTextRectTransform != null)
        {
            descriptionTextRectTransform.anchoredPosition = new Vector2(0, -50f);
            var canvasGroup = descriptionTextRectTransform.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = descriptionTextRectTransform.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }

        if (startButtonRectTransform != null)
        {
            startButtonRectTransform.localScale = Vector3.zero;
        }

        Sequence openSequence = DOTween.Sequence();

        openSequence.Append(levelInfoPanelCanvasGroup.DOFade(1f, panelAnimationDuration * 0.3f));
        openSequence.Join(panelRectTransform.DOScale(panelOriginalScale, panelAnimationDuration)
            .SetEase(Ease.OutBack, 1.2f));

        openSequence.Append(iconRectTransform.DOScale(iconOriginalScale, iconBounceDuration)
            .SetEase(Ease.OutElastic, 1f, 0.6f));
        openSequence.Join(iconRectTransform.DORotate(Vector3.zero, iconBounceDuration)
            .SetEase(Ease.OutBack));

        openSequence.Append(nameTextRectTransform.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack, 1.5f));

        if (descriptionTextRectTransform != null)
        {
            var canvasGroup = descriptionTextRectTransform.GetComponent<CanvasGroup>();
            openSequence.Append(descriptionTextRectTransform.DOAnchorPos(Vector2.zero, 0.4f)
                .SetEase(Ease.OutCubic));
            openSequence.Join(canvasGroup.DOFade(1f, 0.4f));
        }

        openSequence.Append(startButtonRectTransform.DOScale(Vector3.one, 0.35f)
            .SetEase(Ease.OutBack, 1.7f));

        openSequence.OnComplete(() =>
        {
            startButtonRectTransform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f);
        });
    }

    private void PlayCloseAnimation(System.Action onComplete = null)
    {
        DOTween.Kill(panelRectTransform);
        DOTween.Kill(iconRectTransform);
        DOTween.Kill(nameTextRectTransform);
        DOTween.Kill(descriptionTextRectTransform);
        DOTween.Kill(startButtonRectTransform);
        DOTween.Kill(levelInfoPanelCanvasGroup);

        Sequence closeSequence = DOTween.Sequence();

        if (startButtonRectTransform != null)
        {
            closeSequence.Append(startButtonRectTransform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack, 1.5f));
        }

        if (descriptionTextRectTransform != null)
        {
            var canvasGroup = descriptionTextRectTransform.GetComponent<CanvasGroup>();
            closeSequence.Append(canvasGroup.DOFade(0f, 0.2f));
            closeSequence.Join(descriptionTextRectTransform.DOAnchorPos(new Vector2(0, 50f), 0.2f)
                .SetEase(Ease.InCubic));
        }

        if (nameTextRectTransform != null)
        {
            closeSequence.Append(nameTextRectTransform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack));
        }

        if (iconRectTransform != null)
        {
            closeSequence.Append(iconRectTransform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack));
            closeSequence.Join(iconRectTransform.DORotate(new Vector3(0, 0, iconRotationAmount), 0.3f)
                .SetEase(Ease.InCubic));
        }

        closeSequence.Append(panelRectTransform.DOScale(Vector3.zero, 0.4f)
            .SetEase(Ease.InBack, 1.2f));
        closeSequence.Join(levelInfoPanelCanvasGroup.DOFade(0f, 0.3f));

        closeSequence.OnComplete(() =>
        {
            if (levelInfoPanel != null)
                levelInfoPanel.SetActive(false);
            
            onComplete?.Invoke();
        });
    }

    public void HideLevelInfoPanel()
    {
        PlayCloseAnimation();
    }

    private void OnStartLevelButtonClick()
    {
        PlayCloseAnimation(() => onStartLevelCallback?.Invoke());
    }

    private void OnClosePanelButtonClick()
    {
        PlayCloseAnimation();
    }

    public void ShowBackButton()
    {
        ShowButton(backButton, backButtonCanvasGroup);
    }

    public void HideBackButton()
    {
        HideButton(backButton, backButtonCanvasGroup);
    }

    public void ShowNavigationButtons()
    {
        ShowButton(moveLeftButton, moveLeftCanvasGroup);
        ShowButton(moveRightButton, moveRightCanvasGroup);
    }

    public void HideNavigationButtons()
    {
        HideButton(moveLeftButton, moveLeftCanvasGroup);
        HideButton(moveRightButton, moveRightCanvasGroup);
    }

    private void ShowButton(Button button, CanvasGroup canvasGroup)
    {
        if (button == null || canvasGroup == null) return;

        button.gameObject.SetActive(true);
        canvasGroup.DOFade(1f, fadeDuration);
    }

    private void HideButton(Button button, CanvasGroup canvasGroup)
    {
        if (button == null || canvasGroup == null) return;

        canvasGroup.DOFade(0f, fadeDuration)
            .OnComplete(() => button.gameObject.SetActive(false));
    }

    public void SetBackButtonCallback(System.Action callback)
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => callback?.Invoke());
        }
    }

    public void SetMoveLeftCallback(System.Action callback)
    {
        if (moveLeftButton != null)
        {
            moveLeftButton.onClick.RemoveAllListeners();
            moveLeftButton.onClick.AddListener(() => callback?.Invoke());
        }
    }

    public void SetMoveRightCallback(System.Action callback)
    {
        if (moveRightButton != null)
        {
            moveRightButton.onClick.RemoveAllListeners();
            moveRightButton.onClick.AddListener(() => callback?.Invoke());
        }
    }
}