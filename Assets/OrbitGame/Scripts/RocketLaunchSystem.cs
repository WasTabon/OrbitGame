using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RocketLaunchSystem : MonoBehaviour
{
    [Header("Rocket Settings")]
    public GameObject rocketPrefab;
    public Transform spawnPoint;
    public float launchSpeed = 10f;
    
    [Header("Win Conditions")]
    public float requiredOrbitTime = 1.5f;
    public int requiredRocketsCount = 3;
    
    [Header("Input")]
    public KeyCode launchKey = KeyCode.Space;
    
    private List<GameObject> launchedRockets = new List<GameObject>();
    private Dictionary<GameObject, bool> rocketOrbitStatus = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, float> rocketOrbitStartTime = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> rocketExitedOrbit = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, float> rocketExitTime = new Dictionary<GameObject, float>();
    private bool gameEnded = false;
    
    void Update()
    {
        if (gameEnded)
            return;
        
        if (CheckLaunchInput() && launchedRockets.Count < requiredRocketsCount)
        {
            SpawnAndLaunchRocket();
        }
        
        CheckAllRocketsOrbitStatus();
        CheckWinCondition();
        CheckDefeatCondition();
    }
    
    bool CheckLaunchInput()
    {
        // Проверка клавиатуры
        if (Input.GetKeyDown(launchKey))
        {
            return true;
        }
        
        // Проверка мыши для PC
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
                return false;
            return true;
        }
        
        // Проверка тачей для мобильных устройств
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (IsPointerOverUI(touch.fingerId))
                    return false;
                return true;
            }
        }
        
        return false;
    }
    
    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    
    bool IsPointerOverUI(int fingerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
    }
    
    void SpawnAndLaunchRocket()
    {
        if (rocketPrefab == null || spawnPoint == null)
        {
            Debug.LogError("RocketLaunchSystem: Не задан префаб ракеты или точка спавна!");
            return;
        }
        
        // Создаем ракету
        GameObject newRocket = Instantiate(rocketPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Сразу запускаем её
        Rigidbody rb = newRocket.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.up * launchSpeed;
        }
        
        // Активируем SpaceObject если есть
        SpaceObject spaceObj = newRocket.GetComponent<SpaceObject>();
        if (spaceObj != null)
        {
            spaceObj.enabled = true;
        }
        
        // Отключаем RocketLauncher если есть (он не нужен в этой системе)
        RocketLauncher launcher = newRocket.GetComponent<RocketLauncher>();
        if (launcher != null)
        {
            launcher.enabled = false;
        }
        
        // Добавляем в список запущенных ракет
        launchedRockets.Add(newRocket);
        rocketOrbitStatus[newRocket] = false;
        rocketExitedOrbit[newRocket] = false;
        
        Debug.Log($"RocketLaunchSystem: Ракета {launchedRockets.Count} запущена!");
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
            Debug.Log($"RocketLaunchSystem: Ракета попала на орбиту! ({GetRocketsInOrbitCount()}/{requiredRocketsCount})");
        }
        else if (!currentlyInOrbit && wasInOrbit)
        {
            // Ракета покинула орбиту
            rocketOrbitStatus[rocket] = false;
            rocketExitedOrbit[rocket] = true;
            rocketExitTime[rocket] = Time.time;
            Debug.Log("RocketLaunchSystem: Ракета покинула орбиту");
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
    
        GameResultUI resultUI = FindObjectOfType<GameResultUI>();
        if (resultUI != null)
            resultUI.OnGameVictory();
    
        Debug.Log($"ПОБЕДА! Все {requiredRocketsCount} ракеты успешно продержались на орбите {requiredOrbitTime} секунд!");
    }

    void Defeat(string reason)
    {
        if (gameEnded) return;
        gameEnded = true;
    
        GameResultUI resultUI = FindObjectOfType<GameResultUI>();
        if (resultUI != null)
            resultUI.OnGameDefeat();
    
        Debug.Log("ПОРАЖЕНИЕ! " + reason);
    }
    
    public void OnRocketCollision(GameObject rocket)
    {
        if (gameEnded) return;
        
        Defeat("Ракета столкнулась с другим объектом до завершения орбитального полета.");
    }
    
    public int GetLaunchedRocketsCount()
    {
        return launchedRockets.Count;
    }
}