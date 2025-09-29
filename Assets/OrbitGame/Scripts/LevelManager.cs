using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    
    private int currentLevelIndex;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void SetCurrentLevelIndex(int index)
    {
        currentLevelIndex = index;
        Debug.Log($"LevelManager: Установлен levelIndex = {index}");
    }
    
    public void OnLevelCompleted()
    {
        PlayerPrefs.SetInt($"LevelWin_{currentLevelIndex}", 1);
        PlayerPrefs.Save();
        Debug.Log($"LevelManager: Level {currentLevelIndex} marked as completed!");
    }
    
    public void ReturnToLevelSelection()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Levels");
    }
}