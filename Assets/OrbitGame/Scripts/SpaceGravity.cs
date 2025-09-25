using UnityEngine;

public class SpaceGravity : MonoBehaviour
{
    [Header("Gravity Settings")]
    public float gravityStrength = 100f;
    public float minDistance = 2f;
    public float maxDistance = 100f;
    public float captureDistance = 50f;
    
    [Header("Gizmo")]
    public bool showGravityField = true;
    public Color gravityFieldColor = new Color(1f, 1f, 0f, 0.2f);
    
    private OrbitController orbitController;
    
    void Start()
    {
        orbitController = GetComponent<OrbitController>();
        if (orbitController == null)
        {
            Debug.LogWarning("OrbitController not found on the same GameObject!");
        }
    }
    
    void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxDistance);
        
        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            
            if (rb != null && rb.gameObject != gameObject)
            {
                ApplyGravity(rb);
            }
        }
    }
    
    void ApplyGravity(Rigidbody rb)
    {
        Vector3 direction = transform.position - rb.position;
        float distance = direction.magnitude;
        
        if (distance < minDistance || distance > maxDistance)
            return;
        
        // Newton's law of gravity (simplified)
        float forceMagnitude = gravityStrength * rb.mass / (distance * distance);
        Vector3 gravityForce = direction.normalized * forceMagnitude;
        
        rb.AddForce(gravityForce, ForceMode.Force);
        
        // Check for orbit capture
        if (orbitController != null && distance <= captureDistance)
        {
            float speed = rb.velocity.magnitude;
            float orbitalSpeed = Mathf.Sqrt(gravityStrength / distance);
            
            // If speed is close to orbital speed, capture into orbit
            if (Mathf.Abs(speed - orbitalSpeed) < orbitalSpeed * 0.5f)
            {
                int orbitIndex = orbitController.GetNearestOrbitIndex(distance);
                orbitController.CaptureObject(rb, orbitIndex);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (showGravityField)
        {
            Gizmos.color = gravityFieldColor;
            Gizmos.DrawWireSphere(transform.position, maxDistance);
            
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, captureDistance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minDistance);
        }
    }
}