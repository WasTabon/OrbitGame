using UnityEngine;
using UnityEngine.EventSystems;

public class RocketLauncher : MonoBehaviour
{
    [Header("Rocket Settings")]
    public float launchSpeed = 10f;
    
    [Header("Input")]
    public KeyCode launchKey = KeyCode.Space;
    
    private Rigidbody rb;
    private bool isLaunched = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private static int rocketCount = 0;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Сохраняем начальную позицию и поворот
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        // Отключаем SpaceObject до запуска
        SpaceObject spaceObj = GetComponent<SpaceObject>();
        if (spaceObj != null)
        {
            spaceObj.enabled = false;
        }
        
        rocketCount++;
    }
    
    void Update()
    {
        if (!isLaunched && CheckLaunchInput())
        {
            LaunchRocket();
        }
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
    
    public void LaunchRocket()
    {
        if (isLaunched || rb == null)
            return;
        
        // Уведомляем WinController о запуске ПЕРЕД запуском
        WinController winController = FindObjectOfType<WinController>();
        if (winController != null)
        {
            winController.OnRocketLaunched(this);
        }
        
        // Активируем физику
        rb.isKinematic = false;
        rb.velocity = Vector3.up * launchSpeed;
        
        // Активируем SpaceObject
        SpaceObject spaceObj = GetComponent<SpaceObject>();
        if (spaceObj != null)
        {
            spaceObj.enabled = true;
        }
        
        isLaunched = true;
        
        // Отключаем этот компонент после запуска
        this.enabled = false;
    }
    
    void SpawnNewRocket()
    {
        GameObject newRocket = Instantiate(gameObject, initialPosition, initialRotation);
        
        // Сбрасываем состояние нового экземпляра
        RocketLauncher newLauncher = newRocket.GetComponent<RocketLauncher>();
        if (newLauncher != null)
        {
            newLauncher.isLaunched = false;
            newLauncher.enabled = true;
            newLauncher.initialPosition = initialPosition;
            newLauncher.initialRotation = initialRotation;
        }
        
        Rigidbody newRb = newRocket.GetComponent<Rigidbody>();
        if (newRb != null)
        {
            newRb.isKinematic = true;
            newRb.velocity = Vector3.zero;
            newRb.angularVelocity = Vector3.zero;
        }
        
        SpaceObject newSpaceObj = newRocket.GetComponent<SpaceObject>();
        if (newSpaceObj != null)
        {
            newSpaceObj.enabled = false;
        }
        
        // Сбрасываем TrailRenderer если есть
        TrailRenderer trail = newRocket.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.Clear();
        }
    }
    
    void OnDrawGizmos()
    {
        if (!isLaunched)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawRay(transform.position, Vector3.up * 2f);
        }
    }
    
    void OnDestroy()
    {
        rocketCount--;
    }
}