using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceObject : MonoBehaviour
{
    [Header("Collision Settings")]
    public float bounciness = 0.8f;
    public float minCollisionForce = 0.1f;
    
    [Header("Visual")]
    public bool showVelocity = true;
    public Color velocityColor = Color.yellow;
    
    private Rigidbody rb;
    private TrailRenderer trail;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Ensure proper physics settings
        rb.drag = 0f;
        rb.angularDrag = 0.05f;
        rb.useGravity = false;
        
        // Add trail for visual effect
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            SetupTrail();
        }
        
        // Add collider if missing
        if (GetComponent<Collider>() == null)
        {
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.radius = 0.5f;
        }
    }
    
    void SetupTrail()
    {
        trail.time = 2f;
        trail.startWidth = 0.5f;
        trail.endWidth = 0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0.0f), 
                new GradientColorKey(Color.cyan, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        trail.colorGradient = gradient;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        SpaceObject otherSpace = collision.gameObject.GetComponent<SpaceObject>();
        
        if (otherSpace != null)
        {
            // Realistic collision response
            Vector3 relativeVelocity = rb.velocity - otherSpace.rb.velocity;
            Vector3 collisionNormal = collision.contacts[0].normal;
            
            float velocityAlongNormal = Vector3.Dot(relativeVelocity, collisionNormal);
            
            if (velocityAlongNormal > 0)
                return;
            
            float mass1 = rb.mass;
            float mass2 = otherSpace.rb.mass;
            float totalMass = mass1 + mass2;
            
            // Calculate impulse
            float impulse = 2 * velocityAlongNormal / totalMass;
            impulse *= bounciness;
            
            // Apply forces
            if (Mathf.Abs(impulse) > minCollisionForce)
            {
                Vector3 impulseVector = impulse * collisionNormal;
                rb.velocity -= impulseVector * mass2;
                otherSpace.rb.velocity += impulseVector * mass1;
                
                // Add some rotation for visual effect
                rb.angularVelocity += Random.insideUnitSphere * impulse;
                otherSpace.rb.angularVelocity += Random.insideUnitSphere * impulse;
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (showVelocity && rb != null)
        {
            Gizmos.color = velocityColor;
            Gizmos.DrawRay(transform.position, rb.velocity * 0.5f);
        }
    }
}