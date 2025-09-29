using UnityEngine;

public class PlanetLevel : MonoBehaviour
{
    [Header("Level Settings")]
    public int levelIndex = 1;
    
    private GameObject beaconObject;
    
    void Start()
    {
        beaconObject = transform.Find("SM_Veh_Beacon_01")?.gameObject;
        
        if (beaconObject != null)
        {
            bool isCompleted = PlayerPrefs.GetInt($"LevelWin_{levelIndex}", 0) == 1;
            beaconObject.SetActive(isCompleted);
        }
    }
    
    public void LoadLevel()
    {
        string levelName = GetLevelNameToLoad();
        LevelManager.Instance.SetCurrentPlanet(this);
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }
    
    string GetLevelNameToLoad()
    {
        if (levelIndex <= 5)
        {
            return $"Level {levelIndex}";
        }
        else
        {
            int randomLevel = Random.Range(1, 6);
            return $"Level {randomLevel}";
        }
    }
    
    public void MarkAsCompleted()
    {
        PlayerPrefs.SetInt($"LevelWin_{levelIndex}", 1);
        PlayerPrefs.Save();
        
        if (beaconObject != null)
        {
            beaconObject.SetActive(true);
        }
    }
}