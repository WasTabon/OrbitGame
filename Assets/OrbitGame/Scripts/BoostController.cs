using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class BoostController : MonoBehaviour
{
    public static BoostController Instance;
    
    [SerializeField] private GameObject _boostPanel;
    
    [Header("Boost Settings")]
    public float timeSlowDuration = 5f;
    public float timeSlowScale = 0.3f;
    public int boostPrice = 5;
    
    [Header("Target Objects")]
    public SpaceObject[] obstacleObjects;

    [Header("Boost Texts")] 
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI coinsText2;
    
    [SerializeField] private TextMeshProUGUI boost1Text;
    [SerializeField] private TextMeshProUGUI boost2Text;
    [SerializeField] private TextMeshProUGUI boost3Text;
    
    private int boost1Count = 0;
    private int boost2Count = 0;
    private int boost3Count = 0;
    
    private int coins = 0;
    
    private Dictionary<SpaceObject, float> originalSpeeds = new Dictionary<SpaceObject, float>();
    private RocketLaunchSystem rocketSystem;
    
    private const string BOOST1_KEY = "boost1_count";
    private const string BOOST2_KEY = "boost2_count";
    private const string BOOST3_KEY = "boost3_count";
    private const string COINS_KEY = "boost_coins";

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rocketSystem = FindObjectOfType<RocketLaunchSystem>();
        
        LoadData();
        
        foreach (var obstacle in obstacleObjects)
        {
            if (obstacle != null)
            {
                originalSpeeds[obstacle] = obstacle.orbitSpeedMultiplier;
            }
        }
        
        UpdateAllTexts();
    }
    
    void Update()
    {
        UpdateAllTexts();
    }
    
    void LoadData()
    {
        boost1Count = PlayerPrefs.GetInt(BOOST1_KEY, 50);
        boost2Count = PlayerPrefs.GetInt(BOOST2_KEY, 50);
        boost3Count = PlayerPrefs.GetInt(BOOST3_KEY, 50);
        coins = PlayerPrefs.GetInt(COINS_KEY, 50);
        
        Debug.Log($"Loaded: Boost1={boost1Count}, Boost2={boost2Count}, Boost3={boost3Count}, Coins={coins}");
    }
    
    void SaveData()
    {
        PlayerPrefs.SetInt(BOOST1_KEY, boost1Count);
        PlayerPrefs.SetInt(BOOST2_KEY, boost2Count);
        PlayerPrefs.SetInt(BOOST3_KEY, boost3Count);
        PlayerPrefs.SetInt(COINS_KEY, coins);
        PlayerPrefs.Save();
    }
    
    void UpdateAllTexts()
    {
        if (boost1Text != null)
        {
            boost1Text.text = $"REVERSE OBSTACLES SPEED ({boost1Count})";
        }
        if (coinsText != null)
        {
            coinsText.text = coins.ToString();
        }
        if (coinsText2 != null)
        {
            coinsText2.text = coins.ToString();
        }
        
        if (boost2Text != null)
        {
            boost2Text.text = $"SLOW TIME FOR 5 SECONDS ({boost2Count})";
        }
        
        if (boost3Text != null)
        {
            boost3Text.text = $"DECREASE NUMBER OF ROCKETS ({boost3Count})";
        }
    }
    
    public void BuyBoost1()
    {
        if (coins >= boostPrice)
        {
            coins -= boostPrice;
            boost1Count++;
            SaveData();
            Debug.Log($"Bought Boost 1! Count: {boost1Count}, Coins left: {coins}");
        }
        else
        {
            Debug.Log("Not enough coins to buy Boost 1!");
        }
    }
    
    public void BuyBoost2()
    {
        if (coins >= boostPrice)
        {
            coins -= boostPrice;
            boost2Count++;
            SaveData();
            Debug.Log($"Bought Boost 2! Count: {boost2Count}, Coins left: {coins}");
        }
        else
        {
            Debug.Log("Not enough coins to buy Boost 2!");
        }
    }
    
    public void BuyBoost3()
    {
        if (coins >= boostPrice)
        {
            coins -= boostPrice;
            boost3Count++;
            SaveData();
            Debug.Log($"Bought Boost 3! Count: {boost3Count}, Coins left: {coins}");
        }
        else
        {
            Debug.Log("Not enough coins to buy Boost 3!");
        }
    }
    
    public void AddCoins(int amount)
    {
        coins += amount;
        SaveData();
        Debug.Log($"Added {amount} coins. Total: {coins}");
    }
    
    public void UseBoost1()
    {
        if (boost1Count <= 0)
        {
            Debug.Log("No Boost 1 available!");
            return;
        }
        
        boost1Count--;
        SaveData();

        if (_boostPanel != null)
        {
            _boostPanel.SetActive(false);
        }
        
        foreach (var obstacle in obstacleObjects)
        {
            if (obstacle != null)
            {
                obstacle.orbitSpeedMultiplier = -originalSpeeds[obstacle];
            }
        }
        
        Debug.Log($"Boost 1 used! Remaining: {boost1Count}");
    }
    
    public void UseBoost2()
    {
        if (boost2Count <= 0)
        {
            Debug.Log("No Boost 2 available!");
            return;
        }
        
        boost2Count--;
        SaveData();
        
        if (_boostPanel != null)
        {
            _boostPanel.SetActive(false);
        }
        
        StartCoroutine(SlowTimeCoroutine());
        
        Debug.Log($"Boost 2 used! Remaining: {boost2Count}");
    }
    
    public void UseBoost3()
    {
        if (boost3Count <= 0)
        {
            Debug.Log("No Boost 3 available!");
            return;
        }
        
        if (rocketSystem == null)
        {
            Debug.LogError("RocketLaunchSystem not found!");
            return;
        }
        
        if (rocketSystem.requiredRocketsCount <= 1)
        {
            Debug.Log("Cannot decrease rockets below 1!");
            return;
        }
        
        boost3Count--;
        SaveData();
        
        if (_boostPanel != null)
        {
            _boostPanel.SetActive(false);
        }
        
        rocketSystem.requiredRocketsCount -= 1;
        
        Debug.Log($"Boost 3 used! Remaining: {boost3Count}, Rockets required: {rocketSystem.requiredRocketsCount}");
    }
    
    private IEnumerator SlowTimeCoroutine()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = timeSlowScale;
        
        yield return new WaitForSecondsRealtime(timeSlowDuration);
        
        Time.timeScale = originalTimeScale;
        Debug.Log("Time slow ended!");
    }
    
    public int GetBoost1Count() { return boost1Count; }
    public int GetBoost2Count() { return boost2Count; }
    public int GetBoost3Count() { return boost3Count; }
    public int GetCoins() { return coins; }
    
    public void ResetAllBoosts()
    {
        foreach (var obstacle in obstacleObjects)
        {
            if (obstacle != null && originalSpeeds.ContainsKey(obstacle))
            {
                obstacle.orbitSpeedMultiplier = originalSpeeds[obstacle];
            }
        }
        
        Time.timeScale = 1f;
        
        Debug.Log("All boosts reset!");
    }
}