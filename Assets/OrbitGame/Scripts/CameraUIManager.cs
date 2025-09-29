using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CameraUIManager : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;

    private CanvasGroup backButtonCanvasGroup;
    private CanvasGroup moveLeftCanvasGroup;
    private CanvasGroup moveRightCanvasGroup;

    private void Start()
    {
        SetupButton(backButton, out backButtonCanvasGroup, false);
        SetupButton(moveLeftButton, out moveLeftCanvasGroup, true);
        SetupButton(moveRightButton, out moveRightCanvasGroup, true);
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