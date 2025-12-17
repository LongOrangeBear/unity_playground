using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages active power-ups and their effects.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }
    
    [Header("Durations")]
    [SerializeField] private float _magnetDuration = 5f;
    [SerializeField] private float _doubleScoreDuration = 10f;
    [SerializeField] private float _speedBoostDuration = 3f;
    
    [Header("Effects")]
    [SerializeField] private float _magnetRadius = 15f;
    [SerializeField] private float _speedBoostMultiplier = 1.5f;
    
    // Active power-up timers
    private Dictionary<PowerUpType, float> _activeTimers = new Dictionary<PowerUpType, float>();
    
    // Events
    public event Action<PowerUpType, float> OnPowerUpActivated;
    public event Action<PowerUpType> OnPowerUpExpired;
    
    // State queries
    public bool HasShield { get; private set; }
    public bool HasMagnet => _activeTimers.ContainsKey(PowerUpType.Magnet);
    public bool HasDoubleScore => _activeTimers.ContainsKey(PowerUpType.DoubleScore);
    public bool HasSpeedBoost => _activeTimers.ContainsKey(PowerUpType.SpeedBoost);
    public float SpeedMultiplier => HasSpeedBoost ? _speedBoostMultiplier : 1f;
    public int ScoreMultiplier => HasDoubleScore ? 2 : 1;
    
    [Header("Debug")]
    [SerializeField] private bool _testMode = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }
    }
    
    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Playing && _testMode)
        {
            // Activate all power-ups for testing
            ActivatePowerUp(PowerUpType.Magnet);
            ActivatePowerUp(PowerUpType.DoubleScore);
            ActivatePowerUp(PowerUpType.SpeedBoost);
            ActivatePowerUp(PowerUpType.Shield);
            Debug.Log("[PowerUpManager] TEST MODE: All power-ups activated!");
        }
    }
    
    private void Update()
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing) return;
        
        UpdateTimers();
        
        if (HasMagnet)
            ApplyMagnetEffect();
    }
    
    public void ActivatePowerUp(PowerUpType type)
    {
        float duration = GetDuration(type);
        
        if (type == PowerUpType.Shield)
        {
            HasShield = true;
            Debug.Log("[PowerUpManager] Shield activated!");
        }
        else
        {
            _activeTimers[type] = duration;
            Debug.Log($"[PowerUpManager] {type} activated for {duration}s!");
        }
        
        OnPowerUpActivated?.Invoke(type, duration);
    }
    
    public bool UseShield()
    {
        if (HasShield)
        {
            HasShield = false;
            OnPowerUpExpired?.Invoke(PowerUpType.Shield);
            Debug.Log("[PowerUpManager] Shield used!");
            return true;
        }
        return false;
    }
    
    private float GetDuration(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.Magnet => _magnetDuration,
            PowerUpType.DoubleScore => _doubleScoreDuration,
            PowerUpType.SpeedBoost => _speedBoostDuration,
            _ => 0f
        };
    }
    
    private void UpdateTimers()
    {
        List<PowerUpType> expired = new List<PowerUpType>();
        
        // Get keys as list to avoid modifying dictionary during iteration
        var keys = new List<PowerUpType>(_activeTimers.Keys);
        
        foreach (var type in keys)
        {
            _activeTimers[type] -= Time.deltaTime;
            if (_activeTimers[type] <= 0)
                expired.Add(type);
        }
        
        foreach (var type in expired)
        {
            _activeTimers.Remove(type);
            OnPowerUpExpired?.Invoke(type);
            Debug.Log($"[PowerUpManager] {type} expired!");
        }
    }
    
    private void ApplyMagnetEffect()
    {
        var player = FindFirstObjectByType<PlayerRunner>();
        if (player == null) return;
        
        var coins = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        
        // Debug: check if coins are found
        if (coins.Length > 0 && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Magnet] Found {coins.Length} coins, player at {player.transform.position}");
        }
        
        foreach (var coin in coins)
        {
            float distance = Vector3.Distance(player.transform.position, coin.transform.position);
            if (distance < _magnetRadius)
            {
                // Pull coin towards player
                Vector3 direction = (player.transform.position - coin.transform.position).normalized;
                coin.transform.position += direction * 30f * Time.deltaTime;
                
                // Debug: log when pulling
                if (Time.frameCount % 30 == 0)
                {
                    Debug.Log($"[Magnet] Pulling coin at distance {distance:F1}m");
                }
            }
        }
    }
    
    public float GetRemainingTime(PowerUpType type)
    {
        return _activeTimers.TryGetValue(type, out float time) ? time : 0f;
    }
}
