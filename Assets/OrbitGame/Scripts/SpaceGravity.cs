using UnityEngine;

public class SpaceGravity : MonoBehaviour
{
    [Header("Gravity Settings")]
    public float gravityStrength = 100f;
    public float minDistance = 2f;
    public float maxDistance = 100f;
    
    [Header("Gizmo")]
    public bool showGravityField = true;
    public Color gravityFieldColor = new Color(1f, 1f, 0f, 0.2f);
    
    void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxDistance);
        
        foreach (Collider col in colliders)
        {
            if (col.gameObject == gameObject)
                continue;
                
            Rigidbody rb = col.GetComponent<Rigidbody>();
            SpaceObject spaceObj = col.GetComponent<SpaceObject>();
            
            if (rb != null && spaceObj != null)
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
        
        float forceMagnitude = gravityStrength * rb.mass / (distance * distance);
        Vector3 gravityForce = direction.normalized * forceMagnitude;
        
        rb.AddForce(gravityForce, ForceMode.Force);
    }
    
    void OnDrawGizmos()
    {
        if (showGravityField)
        {
            Gizmos.color = gravityFieldColor;
            Gizmos.DrawWireSphere(transform.position, maxDistance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minDistance);
        }
    }
}