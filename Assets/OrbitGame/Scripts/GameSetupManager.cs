using UnityEngine;

public class GameSetupManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject starPrefab;
    public GameObject rocketPrefab;
    
    [Header("Star Settings")]
    public Vector3 starPosition = Vector3.zero;
    public float starGravityStrength = 100f;
    
    [Header("Orbit Configuration")]
    public int numberOfOrbits = 3;
    public float minOrbitRadius = 10f;
    public float maxOrbitRadius = 40f;
    public float baseOrbitSpeed = 30f;
    
    void Start()
    {
        SetupGame();
    }
    
    public void SetupGame()
    {
        // Create star if not exists
        GameObject star = GameObject.Find("Star");
        if (star == null)
        {
            if (starPrefab != null)
            {
                star = Instantiate(starPrefab, starPosition, Quaternion.identity);
                star.name = "Star";
            }
            else
            {
                star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                star.name = "Star";
                star.transform.position = starPosition;
                star.transform.localScale = Vector3.one * 3f;
                
                // Visual setup
                Renderer renderer = star.GetComponent<Renderer>();
                renderer.material.color = Color.yellow;
                renderer.material.SetFloat("_Metallic", 0f);
                renderer.material.SetFloat("_Glossiness", 0.8f);
            }
        }
        
        // Add gravity component
        SpaceGravity gravity = star.GetComponent<SpaceGravity>();
        if (gravity == null)
        {
            gravity = star.AddComponent<SpaceGravity>();
        }
        gravity.gravityStrength = starGravityStrength;
        gravity.maxDistance = maxOrbitRadius * 2f;
        gravity.captureDistance = maxOrbitRadius + 5f;
        
        // Add orbit controller
        OrbitController orbitController = star.GetComponent<OrbitController>();
        if (orbitController == null)
        {
            orbitController = star.AddComponent<OrbitController>();
        }
        
        // Setup orbits
        orbitController.orbits.Clear();
        for (int i = 0; i < numberOfOrbits; i++)
        {
            OrbitData orbit = new OrbitData();
            float t = (float)i / (numberOfOrbits - 1);
            orbit.radius = Mathf.Lerp(minOrbitRadius, maxOrbitRadius, t);
            orbit.orbitSpeed = baseOrbitSpeed / (1 + i * 0.3f); // Slower for outer orbits
            
            // Color gradient from inner to outer
            orbit.gizmoColor = Color.Lerp(Color.red, Color.blue, t);
            orbit.gizmoColor = new Color(orbit.gizmoColor.r, orbit.gizmoColor.g, orbit.gizmoColor.b, 0.7f);
            
            orbitController.orbits.Add(orbit);
        }
        
        // Create launcher if not exists
        GameObject launcher = GameObject.Find("RocketLauncher");
        if (launcher == null)
        {
            launcher = new GameObject("RocketLauncher");
            RocketLauncher rocketLauncher = launcher.AddComponent<RocketLauncher>();
            
            // Setup rocket prefab
            if (rocketPrefab != null)
            {
                rocketLauncher.rocketPrefab = rocketPrefab;
            }
            else
            {
                // Create simple rocket prefab
                GameObject rocket = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                rocket.name = "RocketPrefab";
                rocket.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                
                // Add components
                Rigidbody rb = rocket.AddComponent<Rigidbody>();
                rb.mass = 1f;
                rocket.AddComponent<SpaceObject>();
                
                // Visual
                Renderer rend = rocket.GetComponent<Renderer>();
                rend.material.color = Color.white;
                
                // Save as prefab reference
                rocketLauncher.rocketPrefab = rocket;
                rocket.SetActive(false);
            }
            
            rocketLauncher.launchSpeed = 15f;
        }
        
        // Setup camera if needed
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0, 30, -50);
            mainCamera.transform.LookAt(starPosition);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(starPosition, 2f);
        
        // Draw planned orbits
        for (int i = 0; i < numberOfOrbits; i++)
        {
            float t = (float)i / (numberOfOrbits - 1);
            float radius = Mathf.Lerp(minOrbitRadius, maxOrbitRadius, t);
            Gizmos.color = new Color(1f - t, 0.5f, t, 0.3f);
            DrawCircle(starPosition, radius, 32);
        }
    }
    
    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);
            
            Gizmos.DrawLine(point1, point2);
        }
    }
}