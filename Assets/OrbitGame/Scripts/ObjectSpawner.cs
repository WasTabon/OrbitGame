using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private Transform spawnPoint;
    
    [Header("Launch Settings")]
    [SerializeField] private float launchSpeed = 10f;
    [SerializeField] private Vector3 launchDirection = Vector3.right;
    
    [Header("Lifetime Settings")]
    [SerializeField] private float objectLifetime = 5f;
    
    [Header("Auto Spawn")]
    [SerializeField] private bool autoSpawn = false;
    [SerializeField] private float spawnInterval = 1f;
    
    [Header("Input")]
    [SerializeField] private KeyCode spawnKey = KeyCode.Space;
    [SerializeField] private bool allowMouseClick = true;
    [SerializeField] private bool allowTouch = true;
    
    [Header("Gizmos")]
    [SerializeField] private bool showDirection = true;
    [SerializeField] private float directionLength = 3f;
    [SerializeField] private Color directionColor = Color.cyan;
    
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private float nextSpawnTime = 0f;
    
    void Start()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
    }
    
    void Update()
    {
        if (autoSpawn && Time.time >= nextSpawnTime)
        {
            SpawnObject();
            nextSpawnTime = Time.time + spawnInterval;
        }
        
        CleanupDestroyedObjects();
    }
    
    bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null && 
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
    
    bool IsPointerOverUI(int fingerId)
    {
        return UnityEngine.EventSystems.EventSystem.current != null && 
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(fingerId);
    }
    
    public void SpawnObject()
    {
        if (objectPrefab == null)
        {
            Debug.LogError("ObjectSpawner: Префаб объекта не назначен!");
            return;
        }
        
        GameObject spawnedObject = Instantiate(objectPrefab, spawnPoint.position, spawnPoint.rotation);
        
        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = launchDirection.normalized * launchSpeed;
        }
        
        SpaceObject spaceObj = spawnedObject.GetComponent<SpaceObject>();
        if (spaceObj != null)
        {
            spaceObj.enabled = true;
        }
        
        spawnedObjects.Add(spawnedObject);
        
        StartCoroutine(DestroyAfterTime(spawnedObject, objectLifetime));
        
        Debug.Log($"ObjectSpawner: Объект создан и запущен в направлении {launchDirection}");
    }
    
    private IEnumerator DestroyAfterTime(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (obj != null)
        {
            spawnedObjects.Remove(obj);
            Destroy(obj);
        }
    }
    
    private void CleanupDestroyedObjects()
    {
        spawnedObjects.RemoveAll(obj => obj == null);
    }
    
    public void ClearAllSpawnedObjects()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
    }
    
    public int GetSpawnedObjectsCount()
    {
        CleanupDestroyedObjects();
        return spawnedObjects.Count;
    }
    
    void OnDrawGizmos()
    {
        if (!showDirection)
            return;
        
        Vector3 startPos = spawnPoint != null ? spawnPoint.position : transform.position;
        
        Gizmos.color = directionColor;
        Gizmos.DrawRay(startPos, launchDirection.normalized * directionLength);
        Gizmos.DrawWireSphere(startPos, 0.5f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startPos + launchDirection.normalized * directionLength, 0.3f);
    }
}