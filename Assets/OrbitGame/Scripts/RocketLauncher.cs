using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [Header("Rocket Settings")]
    public GameObject rocketPrefab;
    public float launchSpeed = 10f;
    
    [Header("Input")]
    public KeyCode launchKey = KeyCode.Space;
    
    void Update()
    {
        if (Input.GetKeyDown(launchKey) || Input.GetMouseButtonDown(0))
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
        
        GameObject rocket = Instantiate(rocketPrefab, transform.position, Quaternion.identity);
        
        Rigidbody rb = rocket.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.up * launchSpeed;
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.up * 2f);
    }
}