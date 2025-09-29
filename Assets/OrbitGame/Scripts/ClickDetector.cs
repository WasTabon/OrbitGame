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
    [SerializeField] private float paddingMultiplier = 1.2f;

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

        Bounds bounds = CalculateContainerBounds(container, orbiter);
        
        float requiredDistance = CalculateCameraDistance(bounds);
        
        Vector3 targetPosition = new Vector3(
            container.transform.position.x,
            container.transform.position.y,
            container.transform.position.z - requiredDistance
        );

        targetCamera.transform.DOMove(targetPosition, cameraMoveSpeed).SetEase(Ease.InOutQuad);
        targetCamera.transform.DORotateQuaternion(Quaternion.identity, cameraMoveSpeed).SetEase(Ease.InOutQuad);
    }

    private Bounds CalculateContainerBounds(GameObject container, ObjectOrbiter orbiter)
    {
        Bounds bounds = new Bounds(container.transform.position, Vector3.zero);
        
        foreach (Transform child in container.transform)
        {
            bounds.Encapsulate(child.position);
            
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        float orbitRadius = orbiter != null ? GetOrbitRadius(orbiter) : 5f;
        bounds.Expand(orbitRadius * 2);

        return bounds;
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

    private float CalculateCameraDistance(Bounds bounds)
    {
        float verticalSize = bounds.size.y * paddingMultiplier;
        float horizontalSize = bounds.size.x * paddingMultiplier;
        
        float aspect = (float)Screen.height / Screen.width;
        float verticalFOV = targetCamera.fieldOfView;
        float horizontalFOV = Camera.VerticalToHorizontalFieldOfView(verticalFOV, 1f / aspect);
        
        float distanceVertical = (verticalSize * 0.5f) / Mathf.Tan(verticalFOV * 0.5f * Mathf.Deg2Rad);
        float distanceHorizontal = (horizontalSize * 0.5f) / Mathf.Tan(horizontalFOV * 0.5f * Mathf.Deg2Rad);
        
        return Mathf.Max(distanceVertical, distanceHorizontal);
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