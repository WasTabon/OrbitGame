using UnityEngine;
using System.Collections.Generic;

public class WinController : MonoBehaviour
{
    [Header("Win Conditions")]
    public float requiredOrbitTime = 1.5f;
    public int requiredRocketsCount = 3;
    
    [Header("Rocket Spawning")]
    public GameObject rocketPrefab;
    public Transform spawnPoint;
    
    private List<GameObject> launchedRockets = new List<GameObject>();
    private Dictionary<GameObject, bool> rocketOrbitStatus = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, float> rocketOrbitStartTime = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> rocketExitedOrbit = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, float> rocketExitTime = new Dictionary<GameObject, float>();
    private bool gameEnded = false;
    private GameObject currentActiveRocket;
    
    void Start()
    {
        // Находим первую ракету на сцене
        RocketLauncher firstRocket = FindObjectOfType<RocketLauncher>();
        if (firstRocket != null)
        {
            currentActiveRocket = firstRocket.gameObject;
            if (rocketPrefab == null)
                rocketPrefab = currentActiveRocket;
            if (spawnPoint == null)
                spawnPoint = currentActiveRocket.transform;
        }
    }
    
    void Update()
    {
        if (gameEnded)
            return;
        
        CheckAllRocketsOrbitStatus();
        CheckWinCondition();
        CheckDefeatCondition();
        CheckForNextRocketSpawn();
    }
    
    void CheckForNextRocketSpawn()
    {
        // Проверяем нужно ли создать новую ракету
        if (currentActiveRocket == null && launchedRockets.Count < requiredRocketsCount)
        {
            SpawnNewRocket();
        }
    }
    
    void SpawnNewRocket()
    {
        if (rocketPrefab != null && spawnPoint != null)
        {
            GameObject newRocket = Instantiate(rocketPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Сбрасываем состояние ракеты
            RocketLauncher launcher = newRocket.GetComponent<RocketLauncher>();
            if (launcher != null)
            {
                launcher.enabled = true;
            }
            
            Rigidbody rb = newRocket.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            SpaceObject spaceObj = newRocket.GetComponent<SpaceObject>();
            if (spaceObj != null)
            {
                spaceObj.enabled = false;
            }
            
            TrailRenderer trail = newRocket.GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.Clear();
            }
            
            currentActiveRocket = newRocket;
            Debug.Log($"WinController: Создана новая ракета. Ожидает запуск {launchedRockets.Count + 1}/{requiredRocketsCount}");
        }
    }
    
    public void OnRocketLaunched(RocketLauncher launcher)
    {
        GameObject rocket = launcher.gameObject;
        if (!launchedRockets.Contains(rocket))
        {
            launchedRockets.Add(rocket);
            rocketOrbitStatus[rocket] = false;
            rocketExitedOrbit[rocket] = false;
            Debug.Log($"WinController: Ракета {launchedRockets.Count} запущена!");
            
            // Убираем ссылку на активную ракету, чтобы в Update создалась новая
            if (currentActiveRocket == rocket)
            {
                currentActiveRocket = null;
            }
        }
    }
    
    public int GetLaunchedRocketsCount()
    {
        return launchedRockets.Count;
    }
    
    void CheckAllRocketsOrbitStatus()
    {
        for (int i = launchedRockets.Count - 1; i >= 0; i--)
        {
            GameObject rocket = launchedRockets[i];
            
            if (rocket == null)
            {
                launchedRockets.RemoveAt(i);
                continue;
            }
            
            CheckSingleRocketOrbitStatus(rocket);
        }
    }
    
    void CheckSingleRocketOrbitStatus(GameObject rocket)
    {
        OrbitController[] orbitControllers = FindObjectsOfType<OrbitController>();
        bool currentlyInOrbit = false;
        
        Rigidbody rocketRb = rocket.GetComponent<Rigidbody>();
        if (rocketRb != null)
        {
            foreach (var controller in orbitControllers)
            {
                if (IsRocketInOrbitController(controller, rocketRb))
                {
                    currentlyInOrbit = true;
                    break;
                }
            }
        }
        
        bool wasInOrbit = rocketOrbitStatus.ContainsKey(rocket) && rocketOrbitStatus[rocket];
        
        // Обрабатываем изменение статуса орбиты
        if (currentlyInOrbit && !wasInOrbit)
        {
            // Ракета только что попала на орбиту
            rocketOrbitStatus[rocket] = true;
            rocketOrbitStartTime[rocket] = Time.time;
            Debug.Log($"WinController: Ракета попала на орбиту! ({GetRocketsInOrbitCount()}/{requiredRocketsCount})");
        }
        else if (!currentlyInOrbit && wasInOrbit)
        {
            // Ракета покинула орбиту
            rocketOrbitStatus[rocket] = false;
            rocketExitedOrbit[rocket] = true;
            rocketExitTime[rocket] = Time.time;
            Debug.Log("WinController: Ракета покинула орбиту");
        }
    }
    
    int GetRocketsInOrbitCount()
    {
        int count = 0;
        foreach (var status in rocketOrbitStatus.Values)
        {
            if (status) count++;
        }
        return count;
    }
    
    void CheckWinCondition()
    {
        if (launchedRockets.Count < requiredRocketsCount)
            return;
        
        // Проверяем что все ракеты на орбите и продержались нужное время
        bool allRocketsWin = true;
        
        foreach (var rocket in launchedRockets)
        {
            if (!rocketOrbitStatus.ContainsKey(rocket) || !rocketOrbitStatus[rocket])
            {
                allRocketsWin = false;
                break;
            }
            
            if (Time.time - rocketOrbitStartTime[rocket] < requiredOrbitTime)
            {
                allRocketsWin = false;
                break;
            }
        }
        
        if (allRocketsWin)
        {
            Victory();
        }
    }
    
    void CheckDefeatCondition()
    {
        foreach (var rocket in launchedRockets)
        {
            if (rocketExitedOrbit.ContainsKey(rocket) && rocketExitedOrbit[rocket])
            {
                if (Time.time - rocketExitTime[rocket] >= requiredOrbitTime)
                {
                    Defeat("Одна из ракет покинула орбиту и не смогла вернуться за отведенное время.");
                    return;
                }
            }
        }
    }
    
    bool IsRocketInOrbitController(OrbitController controller, Rigidbody rocketRb)
    {
        float distanceToCenter = Vector3.Distance(rocketRb.transform.position, controller.transform.position);
        float captureThreshold = 43.8f;
        
        if (distanceToCenter <= captureThreshold)
        {
            Vector3 directionToRocket = (rocketRb.transform.position - controller.transform.position).normalized;
            Vector3 velocity = rocketRb.velocity;
            float dotProduct = Vector3.Dot(velocity.normalized, directionToRocket);
            bool isMovingTangentially = Mathf.Abs(dotProduct) < 0.5f;
            bool hasOrbitVelocity = velocity.magnitude > 1f;
            
            return isMovingTangentially && hasOrbitVelocity;
        }
        
        return false;
    }
    
    void Victory()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log($"ПОБЕДА! Все {requiredRocketsCount} ракеты успешно продержались на орбите {requiredOrbitTime} секунд!");
    }
    
    void Defeat(string reason)
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("ПОРАЖЕНИЕ! " + reason);
    }
    
    public void OnRocketCollision(GameObject rocket)
    {
        if (gameEnded) return;
        
        Defeat($"Ракета столкнулась с другим объектом до завершения орбитального полета.");
    }
}