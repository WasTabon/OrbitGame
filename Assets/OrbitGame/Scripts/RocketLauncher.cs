using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [Header("Rocket Settings")]
    public GameObject rocketPrefab;
    public float launchSpeed = 10f;
    public float rocketMass = 1f;
    
    [Header("Launch Position")]
    public Transform launchPoint;
    public float screenEdgeOffset = 1f;
    
    [Header("Input")]
    public KeyCode launchKey = KeyCode.Space;
    
    void Start()
    {
        if (launchPoint == null)
        {
            GameObject launchObj = new GameObject("LaunchPoint");
            launchPoint = launchObj.transform;
            launchPoint.parent = transform;
            SetupLaunchPosition();
        }
    }
    
    void SetupLaunchPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 screenBottom = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, 0, 10f));
            launchPoint.position = new Vector3(screenBottom.x, screenBottom.y - screenEdgeOffset, screenBottom.z);
        }
    }
    
    void Update()
    {
        // Keyboard input
        if (Input.GetKeyDown(launchKey))
        {
            LaunchRocket();
        }
        
        // Touch/Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            LaunchRocket();
        }
    }
    
    public void LaunchRocket()
    {
        if (rocketPrefab == null)
        {
            Debug.LogError("Rocket prefab is not assigned!");
            return;
        }
        
        GameObject rocket = Instantiate(rocketPrefab, launchPoint.position, Quaternion.identity);
        
        Rigidbody rb = rocket.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = rocket.AddComponent<Rigidbody>();
        }
        
        // Setup rigidbody for space physics
        rb.mass = rocketMass;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Launch upward
        rb.velocity = Vector3.up * launchSpeed;
        
        // Add SpaceObject component if not present
        SpaceObject spaceObj = rocket.GetComponent<SpaceObject>();
        if (spaceObj == null)
        {
            rocket.AddComponent<SpaceObject>();
        }
    }
    
    void OnDrawGizmos()
    {
        if (launchPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(launchPoint.position, 0.5f);
            Gizmos.DrawRay(launchPoint.position, Vector3.up * 2f);
        }
    }
}