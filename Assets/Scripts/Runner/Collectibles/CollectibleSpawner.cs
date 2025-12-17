using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns coins and power-ups along the track.
/// </summary>
public class CollectibleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunnerSettings _settings;
    [SerializeField] private Transform _player;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private GameObject _powerUpMagnetPrefab;
    [SerializeField] private GameObject _powerUpShieldPrefab;
    [SerializeField] private GameObject _powerUpDoubleScorePrefab;
    [SerializeField] private GameObject _powerUpSpeedBoostPrefab;
    
    [Header("Coin Spawn Settings")]
    [SerializeField] private float _coinSpawnDistance = 40f;
    [SerializeField] private float _coinGap = 3f;
    [SerializeField] private float _coinLineChance = 0.4f;
    [SerializeField] private int _minCoinsInLine = 3;
    [SerializeField] private int _maxCoinsInLine = 8;
    
    [Header("PowerUp Spawn Settings")]
    [SerializeField] private float _powerUpMinDistance = 30f;
    [SerializeField] private float _powerUpChance = 0.25f;
    
    private float _nextCoinSpawnZ;
    private float _nextPowerUpCheckZ;
    private List<GameObject> _activeCollectibles = new List<GameObject>();
    
    private void Start()
    {
        if (_player == null)
        {
            var playerRunner = FindFirstObjectByType<PlayerRunner>();
            if (playerRunner != null)
                _player = playerRunner.transform;
        }
        
        _nextCoinSpawnZ = 10f;
        _nextPowerUpCheckZ = _powerUpMinDistance;
    }
    
    private void Update()
    {
        if (_player == null) return;
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing) return;
        
        float playerZ = _player.position.z;
        
        // Spawn coins
        while (_nextCoinSpawnZ < playerZ + _coinSpawnDistance)
        {
            TrySpawnCoins(_nextCoinSpawnZ);
        }
        
        // Spawn power-ups
        while (_nextPowerUpCheckZ < playerZ + _coinSpawnDistance)
        {
            TrySpawnPowerUp(_nextPowerUpCheckZ);
            _nextPowerUpCheckZ += 20f; // Check every 20m
        }
        
        // Cleanup
        CleanupCollectibles(playerZ - 10f);
    }
    
    private void TrySpawnCoins(float startZ)
    {
        if (Random.value > _coinLineChance)
        {
            _nextCoinSpawnZ += _coinGap * 2f;
            return;
        }
        
        int lane = Random.Range(-1, 2);
        float xPosition = lane * _settings.laneWidth;
        int coinCount = Random.Range(_minCoinsInLine, _maxCoinsInLine + 1);
        
        for (int i = 0; i < coinCount; i++)
        {
            SpawnCoin(new Vector3(xPosition, 1f, startZ + (i * _coinGap)));
        }
        
        _nextCoinSpawnZ = startZ + (coinCount * _coinGap) + _coinGap;
    }
    
    private void SpawnCoin(Vector3 position)
    {
        if (_coinPrefab == null)
        {
            Debug.LogWarning("[CollectibleSpawner] Coin prefab not assigned!");
            return;
        }
        
        var coin = Instantiate(_coinPrefab, position, Quaternion.identity, transform);
        _activeCollectibles.Add(coin);
    }
    
    private void TrySpawnPowerUp(float zPosition)
    {
        if (Random.value > _powerUpChance) return;
        
        int lane = Random.Range(-1, 2);
        float xPosition = lane * _settings.laneWidth;
        
        PowerUpType type = (PowerUpType)Random.Range(0, 4);
        SpawnPowerUp(new Vector3(xPosition, 1.5f, zPosition), type);
    }
    
    private void SpawnPowerUp(Vector3 position, PowerUpType type)
    {
        GameObject prefab = GetPowerUpPrefab(type);
        if (prefab == null)
        {
            Debug.LogWarning($"[CollectibleSpawner] PowerUp prefab for {type} not assigned!");
            return;
        }
        
        var powerUp = Instantiate(prefab, position, Quaternion.identity, transform);
        _activeCollectibles.Add(powerUp);
    }
    
    private GameObject GetPowerUpPrefab(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.Magnet => _powerUpMagnetPrefab,
            PowerUpType.Shield => _powerUpShieldPrefab,
            PowerUpType.DoubleScore => _powerUpDoubleScorePrefab,
            PowerUpType.SpeedBoost => _powerUpSpeedBoostPrefab,
            _ => _powerUpMagnetPrefab
        };
    }
    
    private void CleanupCollectibles(float behindZ)
    {
        for (int i = _activeCollectibles.Count - 1; i >= 0; i--)
        {
            if (_activeCollectibles[i] == null)
            {
                _activeCollectibles.RemoveAt(i);
                continue;
            }
            
            if (_activeCollectibles[i].transform.position.z < behindZ)
            {
                Destroy(_activeCollectibles[i]);
                _activeCollectibles.RemoveAt(i);
            }
        }
    }
}
