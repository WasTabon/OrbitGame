using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class OrbitData
{
    public float radius = 20f;
    public float orbitSpeed = 30f; // degrees per second
    public Color gizmoColor = Color.cyan;
}

public class OrbitController : MonoBehaviour
{
    [Header("Orbit Settings")]
    public List<OrbitData> orbits = new List<OrbitData>();
    
    [Header("Orbit Force")]
    public float captureForce = 5f;
    public float orbitSmoothness = 2f;
    
    private Dictionary<Rigidbody, int> capturedObjects = new Dictionary<Rigidbody, int>();
    private Dictionary<Rigidbody, float> orbitAngles = new Dictionary<Rigidbody, float>();
    
    void Start()
    {
        if (orbits.Count == 0)
        {
            orbits.Add(new OrbitData());
        }
    }
    
    void FixedUpdate()
    {
        List<Rigidbody> toRemove = new List<Rigidbody>();
        
        foreach (var kvp in capturedObjects)
        {
            Rigidbody rb = kvp.Key;
            int orbitIndex = kvp.Value;
            
            if (rb == null || orbitIndex >= orbits.Count)
            {
                toRemove.Add(rb);
                continue;
            }
            
            OrbitData orbit = orbits[orbitIndex];
            
            // Update angle
            if (!orbitAngles.ContainsKey(rb))
            {
                Vector3 direction = rb.position - transform.position;
                orbitAngles[rb] = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            }
            
            orbitAngles[rb] += orbit.orbitSpeed * Time.fixedDeltaTime;
            
            // Calculate target position
            float angleRad = orbitAngles[rb] * Mathf.Deg2Rad;
            Vector3 targetPosition = transform.position + new Vector3(
                Mathf.Cos(angleRad) * orbit.radius,
                rb.position.y,
                Mathf.Sin(angleRad) * orbit.radius
            );
            
            // Smooth movement to orbit
            Vector3 direction2 = (targetPosition - rb.position).normalized;
            rb.velocity = Vector3.Lerp(rb.velocity, direction2 * orbit.orbitSpeed * 0.1f * orbit.radius, Time.fixedDeltaTime * orbitSmoothness);
            
            // Keep object at orbit radius
            float currentDistance = Vector3.Distance(new Vector3(rb.position.x, transform.position.y, rb.position.z), transform.position);
            if (Mathf.Abs(currentDistance - orbit.radius) > 0.5f)
            {
                Vector3 pullDirection = (transform.position - rb.position).normalized;
                pullDirection.y = 0;
                float pullForce = (currentDistance - orbit.radius) * captureForce;
                rb.AddForce(pullDirection * pullForce, ForceMode.Acceleration);
            }
        }
        
        // Clean up null references
        foreach (var rb in toRemove)
        {
            capturedObjects.Remove(rb);
            orbitAngles.Remove(rb);
        }
    }
    
    public void CaptureObject(Rigidbody rb, int orbitIndex = 0)
    {
        if (rb != null && orbitIndex < orbits.Count)
        {
            capturedObjects[rb] = orbitIndex;
            
            // Initialize angle based on current position
            Vector3 direction = rb.position - transform.position;
            orbitAngles[rb] = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        }
    }
    
    public void ReleaseObject(Rigidbody rb)
    {
        if (capturedObjects.ContainsKey(rb))
        {
            capturedObjects.Remove(rb);
            orbitAngles.Remove(rb);
        }
    }
    
    public int GetNearestOrbitIndex(float distance)
    {
        int nearestIndex = 0;
        float minDifference = Mathf.Abs(distance - orbits[0].radius);
        
        for (int i = 1; i < orbits.Count; i++)
        {
            float difference = Mathf.Abs(distance - orbits[i].radius);
            if (difference < minDifference)
            {
                minDifference = difference;
                nearestIndex = i;
            }
        }
        
        return nearestIndex;
    }
    
    void OnDrawGizmos()
    {
        foreach (var orbit in orbits)
        {
            Gizmos.color = orbit.gizmoColor;
            DrawOrbitGizmo(orbit.radius);
        }
    }
    
    void DrawOrbitGizmo(float radius)
    {
        int segments = 64;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector3 point1 = transform.position + new Vector3(
                Mathf.Cos(angle1) * radius,
                0,
                Mathf.Sin(angle1) * radius
            );
            
            Vector3 point2 = transform.position + new Vector3(
                Mathf.Cos(angle2) * radius,
                0,
                Mathf.Sin(angle2) * radius
            );
            
            Gizmos.DrawLine(point1, point2);
        }
    }
}