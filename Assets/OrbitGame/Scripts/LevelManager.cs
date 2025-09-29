    using UnityEngine;

    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }
        
        private PlanetLevel currentPlanet;
        
        void Awake()
        {
            Instance = this;
        }
        
        public void SetCurrentPlanet(PlanetLevel planet)
        {
            currentPlanet = planet;
        }
        
        public void OnLevelCompleted()
        {
            if (currentPlanet != null)
            {
                currentPlanet.MarkAsCompleted();
            }
        }
        
        public void ReturnToLevelSelection()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Levels");
        }
    }