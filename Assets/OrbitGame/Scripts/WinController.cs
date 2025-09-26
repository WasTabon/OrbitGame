using UnityEngine;

public class WinController : MonoBehaviour
{
    [Header("Win Conditions")]
    public float requiredOrbitTime = 1.5f;
    
    private GameObject rocketObject;
    private bool isRocketLaunched = false;
    private bool isRocketInOrbit = false;
    private float orbitStartTime;
    private float orbitExitTime;
    private bool gameEnded = false;
    private bool hasExitedOrbit = false;
    
    void Start()
    {
        // Найти ракету в сцене
        RocketLauncher[] launchers = FindObjectsOfType<RocketLauncher>();
        if (launchers.Length > 0)
        {
            rocketObject = launchers[0].gameObject;
        }
        else
        {
            Debug.LogWarning("WinController: Не найдена ракета с компонентом RocketLauncher!");
        }
    }
    
    void Update()
    {
        if (gameEnded || rocketObject == null)
            return;
        
        // Проверяем запущена ли ракета
        if (!isRocketLaunched)
        {
            RocketLauncher launcher = rocketObject.GetComponent<RocketLauncher>();
            if (launcher != null && !launcher.enabled)
            {
                isRocketLaunched = true;
                Debug.Log("WinController: Ракета запущена!");
            }
        }
        
        if (isRocketLaunched)
        {
            CheckOrbitStatus();
            
            // Проверяем условие поражения после выхода из орбиты
            if (hasExitedOrbit && !gameEnded && Time.time - orbitExitTime >= requiredOrbitTime)
            {
                Defeat("Ракета покинула орбиту и не смогла вернуться за отведенное время.");
            }
        }
    }
    
    void CheckOrbitStatus()
    {
        // Проверяем находится ли ракета на орбите
        OrbitController[] orbitControllers = FindObjectsOfType<OrbitController>();
        bool currentlyInOrbit = false;
        
        Rigidbody rocketRb = rocketObject.GetComponent<Rigidbody>();
        if (rocketRb != null)
        {
            foreach (var controller in orbitControllers)
            {
                // Проверяем есть ли ракета в списке орбитальных объектов
                if (IsRocketInOrbitController(controller, rocketRb))
                {
                    currentlyInOrbit = true;
                    break;
                }
            }
        }
        
        // Обрабатываем изменение статуса орбиты
        if (currentlyInOrbit && !isRocketInOrbit)
        {
            // Ракета только что попала на орбиту
            isRocketInOrbit = true;
            orbitStartTime = Time.time;
            Debug.Log("WinController: Ракета попала на орбиту!");
        }
        else if (!currentlyInOrbit && isRocketInOrbit)
        {
            // Ракета покинула орбиту - это автоматическое поражение
            isRocketInOrbit = false;
            hasExitedOrbit = true;
            orbitExitTime = Time.time;
            Debug.Log("WinController: Ракета покинула орбиту");
        }
        
        // Проверяем условие победы
        if (isRocketInOrbit && Time.time - orbitStartTime >= requiredOrbitTime)
        {
            Victory();
        }
    }
    
    bool IsRocketInOrbitController(OrbitController controller, Rigidbody rocketRb)
    {
        float distanceToCenter = Vector3.Distance(rocketRb.transform.position, controller.transform.position);
        float captureThreshold = 43.8f; // Capture Threshold из настроек
        
        if (distanceToCenter <= captureThreshold)
        {
            // Вектор от центра орбиты к ракете
            Vector3 directionToRocket = (rocketRb.transform.position - controller.transform.position).normalized;
            
            // Проверяем направление скорости ракеты
            Vector3 velocity = rocketRb.velocity;
            
            // Если ракета движется по орбите, её скорость должна быть примерно перпендикулярна 
            // направлению к центру (т.е. тангенциальная скорость)
            float dotProduct = Vector3.Dot(velocity.normalized, directionToRocket);
            
            // dotProduct близкий к 0 означает что скорость перпендикулярна радиусу (орбитальное движение)
            // dotProduct > 0 означает движение от центра, < 0 - к центру
            bool isMovingTangentially = Mathf.Abs(dotProduct) < 0.5f; // Допуск для орбитального движения
            
            // Также проверим что у ракеты достаточная скорость
            bool hasOrbitVelocity = velocity.magnitude > 1f;
            
            return isMovingTangentially && hasOrbitVelocity;
        }
        
        return false;
    }
    
    void Victory()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("ПОБЕДА! Ракета успешно продержалась на орбите " + requiredOrbitTime + " секунд!");
    }
    
    void Defeat(string reason)
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("ПОРАЖЕНИЕ! " + reason);
    }
    
    public void OnRocketCollision()
    {
        if (gameEnded) return;
        
        Defeat("Ракета столкнулась с другим объектом до завершения орбитального полета.");
    }
    
    void OnEnable()
    {
        // Подписываемся на события столкновений ракеты
        if (rocketObject != null)
        {
            SpaceObject rocketSpaceObject = rocketObject.GetComponent<SpaceObject>();
            if (rocketSpaceObject != null)
            {
                // Поскольку SpaceObject не имеет событий, мы будем проверять статус через Update
            }
        }
    }
}