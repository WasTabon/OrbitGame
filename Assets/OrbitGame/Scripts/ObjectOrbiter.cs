using System.Collections.Generic;
using UnityEngine;

public class ObjectOrbiter : MonoBehaviour
{
    [Header("Center Object")]
    [SerializeField] private Transform centerObject;

    [Header("Objects to Orbit")]
    [SerializeField] private List<GameObject> orbitingObjects = new List<GameObject>();

    [Header("Orbit Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float rotationSpeed = 30f;

    [Header("Orbit Visualization")]
    [SerializeField] private bool showOrbitLine = true;
    [SerializeField] private int orbitSegments = 48;
    [SerializeField] private float orbitLineWidth = 0.15f;
    [SerializeField] private Material orbitMaterial;
    
    [Header("Visual Effects")]
    [SerializeField] private bool useGradient = true;
    [SerializeField] private Color primaryColor = new Color(0.4f, 0.85f, 1f, 0.7f);
    [SerializeField] private Color secondaryColor = new Color(0.6f, 0.95f, 1f, 0.3f);
    [SerializeField] private bool animateColors = true;
    [SerializeField] private float colorAnimationSpeed = 1f;
    
    [Header("Pulsing Effect")]
    [SerializeField] private bool enablePulsing = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;
    [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Glow Effect")]
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private float glowIntensity = 1.5f;
    
    [Header("Wave Effect")]
    [SerializeField] private bool enableWave = true;
    [SerializeField] private float waveSpeed = 3f;
    [SerializeField] private float waveAmplitude = 0.2f;
    [SerializeField] private int waveFrequency = 2;

    private List<float> initialAngles = new List<float>();
    private GameObject orbitTrigger;
    private SphereCollider orbitCollider;
    private LineRenderer orbitLine;
    private float timeOffset;

    private void Start()
    {
        timeOffset = Random.Range(0f, 100f);
        
        CreateOrbitTrigger();
        CreateOrbitVisualization();

        float angleStep = 360f / orbitingObjects.Count;
        
        for (int i = 0; i < orbitingObjects.Count; i++)
        {
            initialAngles.Add(angleStep * i);
        }
    }

    private void CreateOrbitTrigger()
    {
        orbitTrigger = new GameObject("OrbitTrigger");
        orbitTrigger.transform.SetParent(transform);
        orbitTrigger.transform.localPosition = Vector3.zero;

        orbitCollider = orbitTrigger.AddComponent<SphereCollider>();
        orbitCollider.radius = radius;
        orbitCollider.isTrigger = true;
    }

    private void CreateOrbitVisualization()
    {
        if (!showOrbitLine) return;

        GameObject lineObj = new GameObject("OrbitLine");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;

        orbitLine = lineObj.AddComponent<LineRenderer>();
        
        if (orbitMaterial != null)
        {
            orbitLine.material = orbitMaterial;
        }
        else
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            if (enableGlow)
            {
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            orbitLine.material = mat;
        }

        orbitLine.startWidth = orbitLineWidth;
        orbitLine.endWidth = orbitLineWidth;
        orbitLine.loop = true;
        orbitLine.positionCount = orbitSegments;
        orbitLine.useWorldSpace = true;
        orbitLine.numCornerVertices = 5;
        orbitLine.numCapVertices = 5;
        
        UpdateGradient();

        orbitLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        orbitLine.receiveShadows = false;
    }

    private void Update()
    {
        if (centerObject == null) return;

        for (int i = 0; i < orbitingObjects.Count; i++)
        {
            if (orbitingObjects[i] == null) continue;

            float angle = initialAngles[i] + (Time.time * rotationSpeed);
            float radians = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(radians) * radius,
                Mathf.Sin(radians) * radius,
                0f
            );

            orbitingObjects[i].transform.position = centerObject.position + offset;
        }

        UpdateOrbitVisualization();
    }

    private void UpdateOrbitVisualization()
    {
        if (!showOrbitLine || orbitLine == null || centerObject == null) return;

        float currentTime = Time.time + timeOffset;
        
        if (animateColors)
        {
            UpdateGradient();
        }

        float pulseValue = 1f;
        if (enablePulsing)
        {
            float pulse = Mathf.PingPong(currentTime * pulseSpeed, 1f);
            pulseValue = 1f + pulseCurve.Evaluate(pulse) * pulseIntensity;
        }

        for (int i = 0; i < orbitSegments; i++)
        {
            float t = i / (float)orbitSegments;
            float angle = t * 360f * Mathf.Deg2Rad;
            
            float currentRadius = radius;
            
            if (enableWave)
            {
                float wave = Mathf.Sin(currentTime * waveSpeed + t * Mathf.PI * 2f * waveFrequency);
                currentRadius += wave * waveAmplitude;
            }
            
            currentRadius *= pulseValue;
            
            Vector3 position = centerObject.position + new Vector3(
                Mathf.Cos(angle) * currentRadius,
                Mathf.Sin(angle) * currentRadius,
                0f
            );

            orbitLine.SetPosition(i, position);
        }
        
        orbitLine.startWidth = orbitLineWidth * pulseValue;
        orbitLine.endWidth = orbitLineWidth * pulseValue;
    }

    private void UpdateGradient()
    {
        if (orbitLine == null) return;

        Gradient gradient = new Gradient();
        
        if (useGradient)
        {
            Color col1 = primaryColor;
            Color col2 = secondaryColor;
            
            if (animateColors)
            {
                float colorShift = Mathf.PingPong((Time.time + timeOffset) * colorAnimationSpeed, 1f);
                col1 = Color.Lerp(primaryColor, secondaryColor, colorShift);
                col2 = Color.Lerp(secondaryColor, primaryColor, colorShift);
            }
            
            if (enableGlow)
            {
                col1 *= glowIntensity;
                col2 *= glowIntensity;
            }
            
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(col1, 0.0f),
                    new GradientColorKey(col2, 0.25f),
                    new GradientColorKey(col1, 0.5f),
                    new GradientColorKey(col2, 0.75f),
                    new GradientColorKey(col1, 1.0f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(col1.a, 0.0f),
                    new GradientAlphaKey(col2.a * 0.5f, 0.25f),
                    new GradientAlphaKey(col1.a, 0.5f),
                    new GradientAlphaKey(col2.a * 0.5f, 0.75f),
                    new GradientAlphaKey(col1.a, 1.0f)
                }
            );
        }
        else
        {
            Color col = enableGlow ? primaryColor * glowIntensity : primaryColor;
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(col, 0.0f),
                    new GradientColorKey(col, 1.0f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(col.a, 0.0f),
                    new GradientAlphaKey(col.a, 1.0f)
                }
            );
        }
        
        orbitLine.colorGradient = gradient;
    }

    public void DisableOrbitCollider()
    {
        if (orbitCollider != null)
        {
            orbitCollider.enabled = false;
        }
    }

    public void EnableOrbitCollider()
    {
        if (orbitCollider != null)
        {
            orbitCollider.enabled = true;
        }
    }

    public void ShowOrbitLine()
    {
        if (orbitLine != null)
        {
            orbitLine.enabled = true;
        }
    }

    public void HideOrbitLine()
    {
        if (orbitLine != null)
        {
            orbitLine.enabled = false;
        }
    }
}