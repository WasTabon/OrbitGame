#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class GameUIEditor : EditorWindow
{
    private Canvas targetCanvas;
    
    [MenuItem("Tools/Create Game Result Panels")]
    public static void ShowWindow()
    {
        GetWindow<GameUIEditor>("Game UI Creator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Game Result Panels Creator", EditorStyles.boldLabel);
        
        targetCanvas = (Canvas)EditorGUILayout.ObjectField("Target Canvas", targetCanvas, typeof(Canvas), true);
        
        if (GUILayout.Button("Create Victory & Defeat Panels"))
        {
            if (targetCanvas != null)
            {
                CreateGamePanels();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Canvas first!", "OK");
            }
        }
    }
    
    void CreateGamePanels()
    {
        // Создаем Victory Panel
        GameObject victoryPanel = CreatePanel("VictoryPanel", new Color(0.1f, 0.8f, 0.1f, 0.9f));
        CreatePanelContent(victoryPanel, "VICTORY!", "All rockets successfully orbited!", new Color(1f, 1f, 1f));
        
        // Создаем Defeat Panel  
        GameObject defeatPanel = CreatePanel("DefeatPanel", new Color(0.8f, 0.1f, 0.1f, 0.9f));
        CreatePanelContent(defeatPanel, "DEFEAT!", "Mission failed. Try again!", new Color(1f, 1f, 1f));
        
        // Отключаем панели по умолчанию
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);
        
        Debug.Log("Victory and Defeat panels created successfully!");
    }
    
    GameObject CreatePanel(string name, Color backgroundColor)
    {
        // Основная панель
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(targetCanvas.transform, false);
        
        // RectTransform для полноэкранного размещения
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Image компонент для фона
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = backgroundColor;
        
        // Content Panel (центральная часть)
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(panel.transform, false);
        
        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.1f, 0.3f);
        contentRect.anchorMax = new Vector2(0.9f, 0.7f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        
        Image contentImage = contentPanel.AddComponent<Image>();
        contentImage.color = new Color(0f, 0f, 0f, 0.7f);
        contentImage.sprite = CreateRoundedSprite();
        
        return panel;
    }
    
    void CreatePanelContent(GameObject panel, string title, string subtitle, Color textColor)
    {
        GameObject contentPanel = panel.transform.Find("ContentPanel").gameObject;
        
        // Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(contentPanel.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.6f);
        titleRect.anchorMax = new Vector2(0.95f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = textColor;
        titleText.alignment = TextAlignmentOptions.Center;
        
        // Subtitle Text
        GameObject subtitleObj = new GameObject("SubtitleText");
        subtitleObj.transform.SetParent(contentPanel.transform, false);
        
        RectTransform subtitleRect = subtitleObj.AddComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.05f, 0.3f);
        subtitleRect.anchorMax = new Vector2(0.95f, 0.6f);
        subtitleRect.offsetMin = Vector2.zero;
        subtitleRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
        subtitleText.text = subtitle;
        subtitleText.fontSize = 24;
        subtitleText.color = new Color(textColor.r * 0.8f, textColor.g * 0.8f, textColor.b * 0.8f);
        subtitleText.alignment = TextAlignmentOptions.Center;
        
        // Restart Button
        CreateRestartButton(contentPanel, textColor);
    }
    
    void CreateRestartButton(GameObject parent, Color textColor)
    {
        GameObject buttonObj = new GameObject("RestartButton");
        buttonObj.transform.SetParent(parent.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.2f, 0.05f);
        buttonRect.anchorMax = new Vector2(0.8f, 0.25f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        buttonImage.sprite = CreateRoundedSprite();
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Button Text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = buttonTextObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "RESTART";
        buttonText.fontSize = 20;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.color = textColor;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        button.targetGraphic = buttonImage;
    }
    
    Sprite CreateRoundedSprite()
    {
        // Создаем простой белый спрайт для кнопок
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }
}
#endif