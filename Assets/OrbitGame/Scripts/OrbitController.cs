using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class OrbitData
{
    public float radius = 20f;
    public float orbitSpeed = 30f;
    public Color gizmoColor = Color.cyan;
    public float captureThreshold = 2f;
}

public class OrbitController : MonoBehaviour
{
    [Header("Orbit Settings")]
    public List<OrbitData> orbits = new List<OrbitData>();
    
    [Header("Orbit Force")]
    public float captureForce = 5f;
    public float orbitSmoothness = 2f;
    public float speedMatchThreshold = 0.3f;
    
    private Dictionary<Rigidbody, int> capturedObjects = new Dictionary<Rigidbody, int>();
    private Dictionary<Rigidbody, float> orbitAngles = new Dictionary<Rigidbody, float>();
    
    void FixedUpdate()
    {
        CheckForOrbitCapture();
        UpdateCapturedObjects();
    }
    
    void CheckForOrbitCapture()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, GetMaxOrbitRadius() + 5f);
        
        foreach (Collider col in nearbyObjects)
        {
            if (col.gameObject == gameObject)
                continue;
                
            Rigidbody rb = col.GetComponent<Rigidbody>();
            SpaceObject spaceObj = col.GetComponent<SpaceObject>();
            
            if (rb != null && spaceObj != null && !capturedObjects.ContainsKey(rb))
            {
                float distance = Vector3.Distance(rb.position, transform.position);
                int nearestOrbitIndex = GetNearestOrbitIndex(distance);
                
                if (nearestOrbitIndex >= 0)
                {
                    OrbitData orbit = orbits[nearestOrbitIndex];
                    
                    if (Mathf.Abs(distance - orbit.radius) <= orbit.captureThreshold)
                    {
                        float speed = rb.velocity.magnitude;
                        float expectedOrbitSpeed = orbit.orbitSpeed * orbit.radius * Mathf.Deg2Rad;
                        
                        if (Mathf.Abs(speed - expectedOrbitSpeed) < expectedOrbitSpeed * speedMatchThreshold)
                        {
                            CaptureObject(rb, nearestOrbitIndex);
                        }
                    }
                }
            }
        }
    }
    
    void UpdateCapturedObjects()
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
            
            float distance = Vector3.Distance(rb.position, transform.position);
            OrbitData orbit = orbits[orbitIndex];
            
            if (distance > orbit.radius + orbit.captureThreshold * 2f)
            {
                toRemove.Add(rb);
                continue;
            }
            
            if (!orbitAngles.ContainsKey(rb))
            {
                Vector3 direction = rb.position - transform.position;
                orbitAngles[rb] = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            }
            
            orbitAngles[rb] += orbit.orbitSpeed * Time.fixedDeltaTime;
            
            float angleRad = orbitAngles[rb] * Mathf.Deg2Rad;
            Vector3 targetPosition = transform.position + new Vector3(
                Mathf.Cos(angleRad) * orbit.radius,
                rb.position.y,
                Mathf.Sin(angleRad) * orbit.radius
            );
            
            Vector3 direction2 = (targetPosition - rb.position).normalized;
            rb.velocity = Vector3.Lerp(rb.velocity, direction2 * orbit.orbitSpeed * 0.1f * orbit.radius, Time.fixedDeltaTime * orbitSmoothness);
            
            float currentDistance = Vector3.Distance(new Vector3(rb.position.x, transform.position.y, rb.position.z), transform.position);
            if (Mathf.Abs(currentDistance - orbit.radius) > 0.5f)
            {
                Vector3 pullDirection = (transform.position - rb.position).normalized;
                pullDirection.y = 0;
                float pullForce = (currentDistance - orbit.radius) * captureForce;
                rb.AddForce(pullDirection * pullForce, ForceMode.Acceleration);
            }
        }
        
        foreach (var rb in toRemove)
        {
            ReleaseObject(rb);
        }
    }
    
    public void CaptureObject(Rigidbody rb, int orbitIndex = 0)
    {
        if (rb != null && orbitIndex < orbits.Count)
        {
            capturedObjects[rb] = orbitIndex;
            
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
        if (orbits.Count == 0)
            return -1;
            
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
    
    float GetMaxOrbitRadius()
    {
        if (orbits.Count == 0)
            return 0f;
            
        float max = orbits[0].radius;
        for (int i = 1; i < orbits.Count; i++)
        {
            if (orbits[i].radius > max)
                max = orbits[i].radius;
        }
        return max;
    }
    
    void OnDrawGizmos()
    {
        foreach (var orbit in orbits)
        {
            Gizmos.color = orbit.gizmoColor;
            DrawOrbitGizmo(orbit.radius);
            
            Gizmos.color = new Color(orbit.gizmoColor.r, orbit.gizmoColor.g, orbit.gizmoColor.b, 0.3f);
            DrawOrbitGizmo(orbit.radius + orbit.captureThreshold);
            DrawOrbitGizmo(orbit.radius - orbit.captureThreshold);
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