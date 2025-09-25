using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [Header("Rocket Settings")]
    public float launchSpeed = 10f;
    
    [Header("Input")]
    public KeyCode launchKey = KeyCode.Space;
    
    private Rigidbody rb;
    private bool isLaunched = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Отключаем SpaceObject до запуска
        SpaceObject spaceObj = GetComponent<SpaceObject>();
        if (spaceObj != null)
        {
            spaceObj.enabled = false;
        }
    }
    
    void Update()
    {
        if (!isLaunched && (Input.GetKeyDown(launchKey) || Input.GetMouseButtonDown(0)))
        {
            LaunchRocket();
        }
    }
    
    public void LaunchRocket()
    {
        if (isLaunched || rb == null)
            return;
        
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
    
    void OnDrawGizmos()
    {
        if (!isLaunched)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawRay(transform.position, Vector3.up * 2f);
        }
    }
}