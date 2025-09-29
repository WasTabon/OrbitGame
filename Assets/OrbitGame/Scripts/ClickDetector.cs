using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ClickDetector : MonoBehaviour
{
    [Header("Parent Containers")]
    [SerializeField] private List<GameObject> parentContainers = new List<GameObject>();

    [Header("Orbit Settings")]
    [SerializeField] private float orbitRadius = 5f;

    [Header("Camera Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float cameraMoveSpeed = 1f;
    [SerializeField] private float paddingMultiplier = 1.5f;

    private bool canDetectClicks = true;
    private Camera mainCamera;
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;

    private void Start()
    {
        mainCamera = Camera.main;
        
        if (targetCamera == null)
            targetCamera = mainCamera;

        initialCameraPosition = targetCamera.transform.position;
        initialCameraRotation = targetCamera.transform.rotation;
    }

    private void Update()
    {
        if (!canDetectClicks) return;

        if (Input.GetMouseButtonDown(0))
        {
            DetectClick(Input.mousePosition);
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            DetectClick(Input.GetTouch(0).position);
        }
    }

    private void DetectClick(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            foreach (var container in parentContainers)
            {
                if (container != null && hit.collider.transform.IsChildOf(container.transform))
                { 
                    Debug.Log(container.name);
                    canDetectClicks = false;
                    MoveCameraToContainer(container);
                    return;
                }
            }
        }

        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        float distance;

        if (plane.Raycast(ray, out distance))
        {
            Vector3 clickWorldPos = ray.GetPoint(distance);

            foreach (var container in parentContainers)
            {
                if (container == null) continue;

                float distanceToCenter = Vector2.Distance(
                    new Vector2(clickWorldPos.x, clickWorldPos.y),
                    new Vector2(container.transform.position.x, container.transform.position.y)
                );

                if (distanceToCenter <= orbitRadius)
                {
                    Debug.Log(container.name);
                    canDetectClicks = false;
                    MoveCameraToContainer(container);
                    return;
                }
            }
        }
    }

    private void MoveCameraToContainer(GameObject container)
    {
        ObjectOrbiter orbiter = container.GetComponent<ObjectOrbiter>();
        if (orbiter == null) return;

        float orbitRadiusValue = GetOrbitRadius(orbiter);
        
        float maxObjectSize = 0f;
        
        foreach (Transform child in container.transform)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                float childMaxSize = Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.y);
                maxObjectSize = Mathf.Max(maxObjectSize, childMaxSize);
            }
        }

        float totalDiameter = (orbitRadiusValue + maxObjectSize * 0.5f) * 2f;
        
        float sizeWithPadding = totalDiameter * paddingMultiplier;
        
        float requiredDistance = CalculateCameraDistanceForSize(sizeWithPadding);
        
        Vector3 targetPosition = new Vector3(
            container.transform.position.x,
            container.transform.position.y,
            container.transform.position.z - requiredDistance
        );

        targetCamera.transform.DOMove(targetPosition, cameraMoveSpeed).SetEase(Ease.InOutQuad);
        targetCamera.transform.DORotateQuaternion(Quaternion.identity, cameraMoveSpeed).SetEase(Ease.InOutQuad);
    }

    private float GetOrbitRadius(ObjectOrbiter orbiter)
    {
        var field = orbiter.GetType().GetField("radius", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (field != null)
            return (float)field.GetValue(orbiter);
        
        return 5f;
    }

    private float CalculateCameraDistanceForSize(float size)
    {
        float aspect = (float)Screen.height / Screen.width;
        float verticalFOV = targetCamera.fieldOfView;
        
        float distance = (size * 0.5f) / Mathf.Tan(verticalFOV * 0.5f * Mathf.Deg2Rad);
        
        return distance;
    }

    public void ResetCamera()
    {
        targetCamera.transform.DOMove(initialCameraPosition, cameraMoveSpeed).SetEase(Ease.InOutQuad);
        targetCamera.transform.DORotateQuaternion(initialCameraRotation, cameraMoveSpeed).SetEase(Ease.InOutQuad)
            .OnComplete(() => canDetectClicks = true);
    }

    public void EnableClickDetection()
    {
        canDetectClicks = true;
    }

    public void DisableClickDetection()
    {
        canDetectClicks = false;
    }
}