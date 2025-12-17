using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns enemies on the track.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunnerSettings _settings;
    [SerializeField] private Transform _player;
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject _groundEnemyPrefab;
    [SerializeField] private GameObject _airEnemyPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private float _minSpawnDistance = 50f;
    [SerializeField] private float _spawnAheadDistance = 60f;
    [SerializeField] private float _minGapBetweenEnemies = 30f;
    
    [Header("Spawn Chances")]
    [SerializeField] private float _groundEnemyChance = 0.15f;
    [SerializeField] private float _airEnemyChance = 0.1f;
    
    private float _nextSpawnZ;
    private List<GameObject> _activeEnemies = new List<GameObject>();
    
    private void Start()
    {
        if (_player == null)
        {
            var playerRunner = FindFirstObjectByType<PlayerRunner>();
            if (playerRunner != null)
                _player = playerRunner.transform;
        }
        
        _nextSpawnZ = _minSpawnDistance;
    }
    
    private void Update()
    {
        if (_player == null) return;
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing) return;
        
        float playerZ = _player.position.z;
        
        // Spawn enemies ahead
        while (_nextSpawnZ < playerZ + _spawnAheadDistance)
        {
            TrySpawnEnemy(_nextSpawnZ);
            _nextSpawnZ += _minGapBetweenEnemies;
        }
        
        // Cleanup
        CleanupEnemies(playerZ - 20f);
    }
    
    private void TrySpawnEnemy(float zPosition)
    {
        float roll = Random.value;
        
        if (roll < _groundEnemyChance)
        {
            SpawnEnemy(_groundEnemyPrefab, zPosition, 1f);
        }
        else if (roll < _groundEnemyChance + _airEnemyChance)
        {
            SpawnEnemy(_airEnemyPrefab, zPosition, 2f);
        }
    }
    
    private void SpawnEnemy(GameObject prefab, float zPosition, float yPosition)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[EnemySpawner] Enemy prefab not assigned!");
            return;
        }
        
        int lane = Random.Range(-1, 2);
        float xPosition = lane * _settings.laneWidth;
        
        var enemy = Instantiate(prefab, transform);
        enemy.transform.position = new Vector3(xPosition, yPosition, zPosition);
        
        _activeEnemies.Add(enemy);
    }
    
    private void CleanupEnemies(float behindZ)
    {
        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            if (_activeEnemies[i] == null)
            {
                _activeEnemies.RemoveAt(i);
                continue;
            }
            
            if (_activeEnemies[i].transform.position.z < behindZ)
            {
                Destroy(_activeEnemies[i]);
                _activeEnemies.RemoveAt(i);
            }
        }
    }
}
