using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FadeInOutEffect : MonoBehaviour
{
    [Header("Objects to Fade")]
    [SerializeField] private List<GameObject> objectsToFade = new List<GameObject>();
    
    [Header("Fade Settings")]
    [SerializeField] private float visibleDuration = 5f;
    [SerializeField] private float invisibleDuration = 3f;
    
    [Header("Options")]
    [SerializeField] private bool startVisible = true;
    [SerializeField] private bool randomizeTimings = true;
    [SerializeField] private float timingRandomness = 0.5f;
    [SerializeField] private bool fadeTrails = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Dictionary<GameObject, ObjectData> objectDataMap = new Dictionary<GameObject, ObjectData>();
    
    private class ObjectData
    {
        public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        public List<TrailRenderer> trails = new List<TrailRenderer>();
        public bool isVisible = true;
    }
    
    void Start()
    {
        if (objectsToFade == null || objectsToFade.Count == 0)
        {
            Debug.LogError("FadeInOutEffect: Список пуст!");
            enabled = false;
            return;
        }
        
        InitializeObjects();
        
        foreach (var obj in objectsToFade)
        {
            if (obj != null && objectDataMap.ContainsKey(obj))
            {
                float delay = randomizeTimings ? Random.Range(0f, visibleDuration * timingRandomness) : 0f;
                StartCoroutine(FadeLoop(obj, delay));
            }
        }
    }
    
    void InitializeObjects()
    {
        foreach (var obj in objectsToFade)
        {
            if (obj == null) continue;
            
            ObjectData data = new ObjectData();
            
            MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>(true);
            data.meshRenderers.AddRange(meshRenderers);
            
            if (fadeTrails)
            {
                TrailRenderer[] trails = obj.GetComponentsInChildren<TrailRenderer>(true);
                data.trails.AddRange(trails);
            }
            
            if (data.meshRenderers.Count == 0)
            {
                Debug.LogWarning($"FadeInOutEffect: У {obj.name} нет MeshRenderer!");
                continue;
            }
            
            data.isVisible = startVisible;
            
            if (!startVisible)
            {
                SetVisibility(data, false);
            }
            
            objectDataMap[obj] = data;
            
            if (showDebugLogs)
                Debug.Log($"FadeInOutEffect: {obj.name} - {data.meshRenderers.Count} MeshRenderers, {data.trails.Count} Trails");
        }
    }
    
    IEnumerator FadeLoop(GameObject obj, float initialDelay)
    {
        if (!objectDataMap.ContainsKey(obj)) yield break;
        
        yield return new WaitForSeconds(initialDelay);
        
        ObjectData data = objectDataMap[obj];
        
        while (obj != null && enabled)
        {
            if (data.isVisible)
            {
                float duration = visibleDuration;
                if (randomizeTimings)
                    duration = Mathf.Max(0.5f, duration + Random.Range(-duration * timingRandomness, duration * timingRandomness));
                
                yield return new WaitForSeconds(duration);
                
                if (showDebugLogs)
                    Debug.Log($"FadeInOutEffect: Выключаю {obj.name}");
                
                SetVisibility(data, false);
                data.isVisible = false;
            }
            else
            {
                float duration = invisibleDuration;
                if (randomizeTimings)
                    duration = Mathf.Max(0.5f, duration + Random.Range(-duration * timingRandomness, duration * timingRandomness));
                
                yield return new WaitForSeconds(duration);
                
                if (showDebugLogs)
                    Debug.Log($"FadeInOutEffect: Включаю {obj.name}");
                
                SetVisibility(data, true);
                data.isVisible = true;
            }
        }
    }
    
    void SetVisibility(ObjectData data, bool visible)
    {
        foreach (var renderer in data.meshRenderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }
        
        if (fadeTrails)
        {
            foreach (var trail in data.trails)
            {
                if (trail != null)
                    trail.enabled = visible;
            }
        }
    }
    
    void OnDestroy()
    {
        foreach (var data in objectDataMap.Values)
        {
            SetVisibility(data, true);
        }
    }
}