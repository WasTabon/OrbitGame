using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameResultUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    
    [Header("Animation Settings")]
    public float animationDuration = 0.8f;
    public Ease animationEase = Ease.OutBounce;
    
    private RocketLaunchSystem rocketSystem;
    private bool gameResultShown = false;
    
    void Start()
    {
        rocketSystem = FindObjectOfType<RocketLaunchSystem>();
        
        if (rocketSystem == null)
        {
            Debug.LogError("GameResultUI: RocketLaunchSystem не найден!");
        }
        
        // Автоматически найти панели если не заданы
        if (victoryPanel == null)
            victoryPanel = GameObject.Find("VictoryPanel");
        
        if (defeatPanel == null)
            defeatPanel = GameObject.Find("DefeatPanel");
        
        // Убеждаемся что панели отключены в начале
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        
        if (defeatPanel != null)
            defeatPanel.SetActive(false);
    }
    
    void Update()
    {
        if (!gameResultShown && rocketSystem != null)
        {
            CheckGameState();
        }
    }
    
    void CheckGameState()
    {
        // Проверяем победу
        if (rocketSystem.GetLaunchedRocketsCount() >= rocketSystem.requiredRocketsCount)
        {
            bool allRocketsWin = true;
            
            // Здесь нужна более детальная проверка, но для простоты используем таймер
            // В реальной реализации лучше добавить событие в RocketLaunchSystem
            if (HasAllRocketsInOrbit())
            {
                ShowVictory();
                return;
            }
        }
        
        // Проверяем поражение (упрощенная версия)
        if (HasAnyRocketFailed())
        {
            ShowDefeat();
        }
    }
    
    bool HasAllRocketsInOrbit()
    {
        // Упрощенная проверка - в реальности нужно интегрироваться с RocketLaunchSystem
        // Здесь можно добавить более точную логику проверки орбит
        return false; // Временная заглушка
    }
    
    bool HasAnyRocketFailed()
    {
        // Упрощенная проверка - в реальности нужно интегрироваться с RocketLaunchSystem  
        // Здесь можно добавить более точную логику проверки поражений
        return false; // Временная заглушка
    }
    
    public void ShowVictory()
    {
        if (gameResultShown || victoryPanel == null) return;
        
        gameResultShown = true;
        
        victoryPanel.SetActive(true);
        
        // Анимация появления
        RectTransform panelRect = victoryPanel.GetComponent<RectTransform>();
        Transform contentPanel = victoryPanel.transform.Find("ContentPanel");
        
        // Начальное состояние
        panelRect.GetComponent<Image>().color = new Color(0.1f, 0.8f, 0.1f, 0f);
        if (contentPanel != null)
        {
            contentPanel.localScale = Vector3.zero;
        }
        
        // Анимация фона
        panelRect.GetComponent<Image>().DOFade(0.9f, animationDuration * 0.5f);
        
        // Анимация панели контента
        if (contentPanel != null)
        {
            contentPanel.DOScale(Vector3.one, animationDuration)
                .SetEase(animationEase)
                .SetDelay(animationDuration * 0.3f);
        }
        
        Debug.Log("Показана панель победы!");
    }
    
    public void ShowDefeat()
    {
        if (gameResultShown || defeatPanel == null) return;
        
        gameResultShown = true;
        
        defeatPanel.SetActive(true);
        
        // Анимация появления
        RectTransform panelRect = defeatPanel.GetComponent<RectTransform>();
        Transform contentPanel = defeatPanel.transform.Find("ContentPanel");
        
        // Начальное состояние
        panelRect.GetComponent<Image>().color = new Color(0.8f, 0.1f, 0.1f, 0f);
        if (contentPanel != null)
        {
            contentPanel.localScale = Vector3.zero;
        }
        
        // Анимация фона
        panelRect.GetComponent<Image>().DOFade(0.9f, animationDuration * 0.5f);
        
        // Анимация панели контента с тряской для поражения
        if (contentPanel != null)
        {
            contentPanel.DOScale(Vector3.one, animationDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(animationDuration * 0.3f)
                .OnComplete(() => {
                    // Легкая тряска после появления
                    contentPanel.DOShakePosition(0.5f, 10f, 10, 90f);
                });
        }
        
        Debug.Log("Показана панель поражения!");
    }
    
    public void RestartGame()
    {
        // Останавливаем все анимации DOTween
        DOTween.KillAll();
        
        // Перезагружаем сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Публичные методы для вызова из RocketLaunchSystem
    public void OnGameVictory()
    {
        ShowVictory();
    }
    
    public void OnGameDefeat()
    {
        ShowDefeat();
    }
}