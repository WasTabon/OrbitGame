using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceObject : MonoBehaviour
{
    [Header("Physics Settings")]
    public float mass = 1f;
    public float drag = 0f;
    public float angularDrag = 0.05f;
    
    [Header("Collision Settings")]
    public float bounciness = 0.8f;
    public float minCollisionForce = 0.1f;
    
    [Header("Visual")]
    public bool showVelocity = true;
    public Color velocityColor = Color.yellow;
    public bool enableTrail = true;
    
    private Rigidbody rb;
    private TrailRenderer trail;
    
    void Start()
    {
        SetupRigidbody();
        if (enableTrail)
        {
            SetupTrail();
        }
        
        // Если есть RocketLauncher, то этот объект начинает неактивным
        RocketLauncher launcher = GetComponent<RocketLauncher>();
        if (launcher != null)
        {
            enabled = false;
        }
    }
    
    void SetupRigidbody()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }
    
    void SetupTrail()
    {
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }
        
        trail.time = 2f;
        trail.startWidth = 0.3f;
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
            Vector3 relativeVelocity = rb.velocity - otherSpace.rb.velocity;
            Vector3 collisionNormal = collision.contacts[0].normal;
            
            float velocityAlongNormal = Vector3.Dot(relativeVelocity, collisionNormal);
            
            if (velocityAlongNormal > 0)
                return;
            
            float mass1 = rb.mass;
            float mass2 = otherSpace.rb.mass;
            float totalMass = mass1 + mass2;
            
            float impulse = 2 * velocityAlongNormal / totalMass;
            impulse *= bounciness;
            
            if (Mathf.Abs(impulse) > minCollisionForce)
            {
                Vector3 impulseVector = impulse * collisionNormal;
                rb.velocity -= impulseVector * mass2;
                otherSpace.rb.velocity += impulseVector * mass1;
                
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