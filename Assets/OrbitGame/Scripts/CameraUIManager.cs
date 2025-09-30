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

    [Header("Modifiers Database")]
    [SerializeField] private ModifiersDatabase modifiersDatabase;

    private CanvasGroup backButtonCanvasGroup;
    private CanvasGroup moveLeftCanvasGroup;
    private CanvasGroup moveRightCanvasGroup;
    private CanvasGroup levelInfoPanelCanvasGroup;

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
            levelInfoPanel.SetActive(false);
        }

        if (startLevelButton != null)
        {
            startLevelButton.onClick.AddListener(OnStartLevelButtonClick);
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
            levelInfoPanelCanvasGroup.alpha = 0f;
            levelInfoPanelCanvasGroup.DOFade(1f, fadeDuration);
        }
    }

    public void HideLevelInfoPanel()
    {
        if (levelInfoPanel != null && levelInfoPanelCanvasGroup != null)
        {
            levelInfoPanelCanvasGroup.DOFade(0f, fadeDuration)
                .OnComplete(() => levelInfoPanel.SetActive(false));
        }
    }

    private void OnStartLevelButtonClick()
    {
        HideLevelInfoPanel();
        onStartLevelCallback?.Invoke();
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