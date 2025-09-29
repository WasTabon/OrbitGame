using UnityEngine;

public class ReturnButton : MonoBehaviour
{
    public void ReturnToLevels()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ReturnToLevelSelection();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Levels");
        }
    }
}