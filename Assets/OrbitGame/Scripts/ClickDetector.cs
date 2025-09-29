using System.Collections.Generic;
using UnityEngine;

public class ClickDetector : MonoBehaviour
{
    [Header("Parent Containers")]
    [SerializeField] private List<GameObject> parentContainers = new List<GameObject>();

    [Header("Orbit Settings")]
    [SerializeField] private float orbitRadius = 5f;

    private bool canDetectClicks = true;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
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
                    //canDetectClicks = false;
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
                    //canDetectClicks = false;
                    return;
                }
            }
        }
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