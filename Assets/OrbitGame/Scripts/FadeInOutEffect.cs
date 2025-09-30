using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FadeInOutEffect : MonoBehaviour
{
    [Header("Objects to Fade")]
    [SerializeField] private List<GameObject> objectsToFade = new List<GameObject>();
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float visibleDuration = 5f;
    [SerializeField] private float invisibleDuration = 3f;
    
    [Header("Fade Curve")]
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Options")]
    [SerializeField] private bool startVisible = true;
    [SerializeField] private bool randomizeTimings = true;
    [SerializeField] private float timingRandomness = 0.5f;
    [SerializeField] private bool fadeTrails = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Dictionary<GameObject, ObjectFadeData> fadeDataMap = new Dictionary<GameObject, ObjectFadeData>();
    
    private class ObjectFadeData
    {
        public List<Renderer> renderers = new List<Renderer>();
        public List<TrailRenderer> trails = new List<TrailRenderer>();
        public Dictionary<Material, Color> originalColors = new Dictionary<Material, Color>();
        public Dictionary<TrailRenderer, Gradient> originalTrailGradients = new Dictionary<TrailRenderer, Gradient>();
        public float currentAlpha = 1f;
        public bool isVisible = true;
        public Coroutine fadeCoroutine;
    }
    
    void Start()
    {
        if (objectsToFade == null || objectsToFade.Count == 0)
        {
            Debug.LogError("FadeInOutEffect: Список объектов пуст! Добавьте объекты в массив objectsToFade.");
            enabled = false;
            return;
        }
        
        InitializeObjects();
        
        if (fadeDataMap.Count == 0)
        {
            Debug.LogError("FadeInOutEffect: Не удалось инициализировать ни один объект!");
            enabled = false;
            return;
        }
        
        foreach (var obj in objectsToFade)
        {
            if (obj != null && fadeDataMap.ContainsKey(obj))
            {
                float delay = randomizeTimings ? Random.Range(0f, visibleDuration * timingRandomness) : 0f;
                StartCoroutine(FadeLoop(obj, delay));
                
                if (showDebugLogs)
                    Debug.Log($"FadeInOutEffect: Запущен цикл fade для {obj.name} с задержкой {delay:F2}с");
            }
        }
    }
    
    void InitializeObjects()
    {
        foreach (var obj in objectsToFade)
        {
            if (obj == null)
            {
                Debug.LogWarning("FadeInOutEffect: Обнаружен null объект в списке!");
                continue;
            }
            
            ObjectFadeData data = new ObjectFadeData();
            
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"FadeInOutEffect: У объекта {obj.name} нет Renderer компонентов!");
                continue;
            }
            
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                
                if (renderer is TrailRenderer)
                {
                    TrailRenderer trail = renderer as TrailRenderer;
                    if (fadeTrails)
                    {
                        data.trails.Add(trail);
                        
                        Gradient originalGradient = new Gradient();
                        GradientColorKey[] colorKeys = trail.colorGradient.colorKeys;
                        GradientAlphaKey[] alphaKeys = trail.colorGradient.alphaKeys;
                        originalGradient.SetKeys(colorKeys, alphaKeys);
                        data.originalTrailGradients[trail] = originalGradient;
                        
                        if (showDebugLogs)
                            Debug.Log($"FadeInOutEffect: Найден TrailRenderer на {obj.name}");
                    }
                }
                else
                {
                    data.renderers.Add(renderer);
                }
                
                foreach (var mat in renderer.materials)
                {
                    if (mat == null) continue;
                    
                    if (mat.HasProperty("_Color"))
                    {
                        if (!data.originalColors.ContainsKey(mat))
                        {
                            data.originalColors[mat] = mat.color;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"FadeInOutEffect: Материал {mat.name} на {obj.name} не имеет свойства _Color!");
                    }
                }
            }
            
            if (data.renderers.Count == 0 && data.trails.Count == 0)
            {
                Debug.LogWarning($"FadeInOutEffect: У объекта {obj.name} нет подходящих рендереров для fade!");
                continue;
            }
            
            data.isVisible = startVisible;
            data.currentAlpha = startVisible ? 1f : 0f;
            
            if (!startVisible)
            {
                SetObjectAlpha(data, 0f);
            }
            
            fadeDataMap[obj] = data;
            
            if (showDebugLogs)
                Debug.Log($"FadeInOutEffect: Инициализирован {obj.name} ({data.renderers.Count} renderers, {data.trails.Count} trails)");
        }
    }
    
    IEnumerator FadeLoop(GameObject obj, float initialDelay)
    {
        if (obj == null || !fadeDataMap.ContainsKey(obj))
        {
            Debug.LogError($"FadeInOutEffect: Объект не найден в fadeDataMap!");
            yield break;
        }
        
        yield return new WaitForSeconds(initialDelay);
        
        ObjectFadeData data = fadeDataMap[obj];
        
        while (obj != null && enabled)
        {
            if (data.isVisible)
            {
                float duration = visibleDuration;
                if (randomizeTimings)
                {
                    duration += Random.Range(-visibleDuration * timingRandomness, visibleDuration * timingRandomness);
                    duration = Mathf.Max(duration, 0.5f);
                }
                
                yield return new WaitForSeconds(duration);
                
                if (obj == null) yield break;
                
                yield return StartCoroutine(FadeOut(data, obj.name));
            }
            else
            {
                float duration = invisibleDuration;
                if (randomizeTimings)
                {
                    duration += Random.Range(-invisibleDuration * timingRandomness, invisibleDuration * timingRandomness);
                    duration = Mathf.Max(duration, 0.5f);
                }
                
                yield return new WaitForSeconds(duration);
                
                if (obj == null) yield break;
                
                yield return StartCoroutine(FadeIn(data, obj.name));
            }
        }
    }
    
    IEnumerator FadeIn(ObjectFadeData data, string objName)
    {
        data.isVisible = true;
        float elapsed = 0f;
        
        if (showDebugLogs)
            Debug.Log($"FadeInOutEffect: Начало fade in для {objName}");
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeCurve.Evaluate(elapsed / fadeInDuration);
            data.currentAlpha = t;
            SetObjectAlpha(data, t);
            yield return null;
        }
        
        SetObjectAlpha(data, 1f);
        data.currentAlpha = 1f;
        
        if (showDebugLogs)
            Debug.Log($"FadeInOutEffect: Завершен fade in для {objName}");
    }
    
    IEnumerator FadeOut(ObjectFadeData data, string objName)
    {
        data.isVisible = false;
        float elapsed = 0f;
        
        if (showDebugLogs)
            Debug.Log($"FadeInOutEffect: Начало fade out для {objName}");
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeCurve.Evaluate(1f - (elapsed / fadeOutDuration));
            data.currentAlpha = t;
            SetObjectAlpha(data, t);
            yield return null;
        }
        
        SetObjectAlpha(data, 0f);
        data.currentAlpha = 0f;
        
        if (showDebugLogs)
            Debug.Log($"FadeInOutEffect: Завершен fade out для {objName}");
    }
    
    void SetObjectAlpha(ObjectFadeData data, float alpha)
    {
        if (data == null)
        {
            Debug.LogError("FadeInOutEffect: ObjectFadeData is null!");
            return;
        }
        
        foreach (var renderer in data.renderers)
        {
            if (renderer == null) continue;
            
            foreach (var mat in renderer.materials)
            {
                if (mat == null) continue;
                
                if (data.originalColors.ContainsKey(mat))
                {
                    Color originalColor = data.originalColors[mat];
                    Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * alpha);
                    mat.color = newColor;
                }
            }
        }
        
        if (fadeTrails)
        {
            foreach (var trail in data.trails)
            {
                if (trail == null) continue;
                
                if (!data.originalTrailGradients.ContainsKey(trail))
                {
                    Debug.LogWarning($"FadeInOutEffect: Оригинальный градиент для trail не найден!");
                    continue;
                }
                
                Gradient originalGradient = data.originalTrailGradients[trail];
                GradientColorKey[] colorKeys = originalGradient.colorKeys;
                GradientAlphaKey[] alphaKeys = originalGradient.alphaKeys;
                
                GradientAlphaKey[] newAlphaKeys = new GradientAlphaKey[alphaKeys.Length];
                for (int i = 0; i < alphaKeys.Length; i++)
                {
                    newAlphaKeys[i] = new GradientAlphaKey(alphaKeys[i].alpha * alpha, alphaKeys[i].time);
                }
                
                Gradient newGradient = new Gradient();
                newGradient.SetKeys(colorKeys, newAlphaKeys);
                trail.colorGradient = newGradient;
            }
        }
    }
    
    void OnDestroy()
    {
        foreach (var kvp in fadeDataMap)
        {
            if (kvp.Value != null)
            {
                SetObjectAlpha(kvp.Value, 1f);
            }
        }
    }
    
    void OnDisable()
    {
        StopAllCoroutines();
        
        foreach (var kvp in fadeDataMap)
        {
            if (kvp.Value != null)
            {
                SetObjectAlpha(kvp.Value, 1f);
            }
        }
    }
}