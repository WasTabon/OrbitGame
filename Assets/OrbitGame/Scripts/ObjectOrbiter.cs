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

    private List<float> initialAngles = new List<float>();

    private void Start()
    {
        float angleStep = 360f / orbitingObjects.Count;
        
        for (int i = 0; i < orbitingObjects.Count; i++)
        {
            initialAngles.Add(angleStep * i);
        }
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
    }
}