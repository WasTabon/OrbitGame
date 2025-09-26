using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BoostController : MonoBehaviour
{
    [SerializeField] private GameObject _boostPanel;
    
    [Header("Boost Settings")]
    public float timeSlowDuration = 5f;
    public float timeSlowScale = 0.3f;
    
    [Header("Target Objects")]
    public SpaceObject[] obstacleObjects;
    
    private bool boost1Used = false;
    private bool boost2Used = false;
    private bool boost3Used = false;
    
    private Dictionary<SpaceObject, float> originalSpeeds = new Dictionary<SpaceObject, float>();
    private RocketLaunchSystem rocketSystem;
    
    void Start()
    {
        rocketSystem = FindObjectOfType<RocketLaunchSystem>();
        
        // Сохраняем оригинальные скорости препятствий
        foreach (var obstacle in obstacleObjects)
        {
            if (obstacle != null)
            {
                originalSpeeds[obstacle] = obstacle.orbitSpeedMultiplier;
            }
        }
    }
    
    // Буст 1: Обратить скорость препятствий
    public void UseBoost1()
    {
        if (boost1Used)
        {
            Debug.Log("BoostController: Буст 1 уже использован!");
            return;
        }
        
        boost1Used = true;

        _boostPanel.SetActive(false);
        
        foreach (var obstacle in obstacleObjects)
        {
            if (obstacle != null)
            {
                obstacle.orbitSpeedMultiplier = -originalSpeeds[obstacle];
            }
        }
        
        Debug.Log("BoostController: Буст 1 активирован - препятствия движутся в обратную сторону!");
    }
    
    // Буст 2: Замедление времени
    public void UseBoost2()
    {
        if (boost2Used)
        {
            Debug.Log("BoostController: Буст 2 уже использован!");
            return;
        }
        
        boost2Used = true;
        
        _boostPanel.SetActive(false);
        
        StartCoroutine(SlowTimeCoroutine());
        
        Debug.Log("BoostController: Буст 2 активирован - замедление времени!");
    }
    
    // Буст 3: Уменьшить количество ракет
    public void UseBoost3()
    {
        if (boost3Used)
        {
            Debug.Log("BoostController: Буст 3 уже использован!");
            return;
        }
        
        if (rocketSystem == null)
        {
            Debug.LogError("BoostController: RocketLaunchSystem не найден!");
            return;
        }
        
        if (rocketSystem.requiredRocketsCount <= 1)
        {
            Debug.Log("BoostController: Нельзя уменьшить количество ракет ниже 1!");
            return;
        }
        
        boost3Used = true;
        
        _boostPanel.SetActive(false);
        
        rocketSystem.requiredRocketsCount -= 1;
        
        Debug.Log($"BoostController: Буст 3 активирован - требуется ракет: {rocketSystem.requiredRocketsCount}!");
    }
    
    private IEnumerator SlowTimeCoroutine()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = timeSlowScale;
        
        yield return new WaitForSecondsRealtime(timeSlowDuration);
        
        Time.timeScale = originalTimeScale;
        Debug.Log("BoostController: Замедление времени закончилось!");
    }
    
    // Методы для проверки статуса бустов (для UI)
    public bool IsBoost1Used() { return boost1Used; }
    public bool IsBoost2Used() { return boost2Used; }
    public bool IsBoost3Used() { return boost3Used; }
    
    // Сброс всех бустов (для перезапуска игры)
    public void ResetAllBoosts()
    {
        boost1Used = false;
        boost2Used = false;
        boost3Used = false;
        
        // Восстанавливаем оригинальные скорости препятствий
        foreach (var obstacle in obstacleObjects)
        {
            if (obstacle != null && originalSpeeds.ContainsKey(obstacle))
            {
                obstacle.orbitSpeedMultiplier = originalSpeeds[obstacle];
            }
        }
        
        // Восстанавливаем время
        Time.timeScale = 1f;
        
        Debug.Log("BoostController: Все бусты сброшены!");
    }
}