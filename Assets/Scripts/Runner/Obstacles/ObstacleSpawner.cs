using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns obstacles on chunks ahead of player.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunnerSettings _settings;
    [SerializeField] private Transform _player;
    
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject _barrierLowPrefab;
    [SerializeField] private GameObject _barrierHighPrefab;
    [SerializeField] private GameObject _carPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private float _minSpawnDistance = 60f;  // Give player time to see
    [SerializeField] private float _maxSpawnDistance = 100f;
    [SerializeField] private float _minGapBetweenObstacles = 15f;
    
    [Header("Difficulty")]
    [SerializeField] private float _baseSpawnChance = 0.3f;
    [SerializeField] private float _maxSpawnChance = 0.7f;
    [SerializeField] private float _difficultyIncreasePerMeter = 0.001f;
    
    private float _nextSpawnZ;
    private List<GameObject> _activeObstacles = new List<GameObject>();
    
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
        
        // Spawn obstacles ahead
        while (_nextSpawnZ < playerZ + _maxSpawnDistance)
        {
            TrySpawnObstacle(_nextSpawnZ);
            _nextSpawnZ += Random.Range(_minGapBetweenObstacles, _minGapBetweenObstacles * 2f);
        }
        
        // Cleanup obstacles behind player
        CleanupObstacles(playerZ - 20f);
    }
    
    private void TrySpawnObstacle(float zPosition)
    {
        // Calculate spawn chance based on distance
        float distance = GameManager.Instance?.DistanceTraveled ?? 0f;
        float spawnChance = Mathf.Lerp(_baseSpawnChance, _maxSpawnChance, 
            distance * _difficultyIncreasePerMeter);
        
        if (Random.value > spawnChance) return;
        
        // Pick random lane
        int lane = Random.Range(-1, 2);
        float xPosition = lane * _settings.laneWidth;
        
        // Pick random obstacle type
        Obstacle.ObstacleType type = (Obstacle.ObstacleType)Random.Range(0, 3);
        
        // Get prefab for type
        GameObject prefab = GetPrefabForType(type);
        if (prefab == null)
        {
            Debug.LogWarning($"[ObstacleSpawner] No prefab assigned for obstacle type: {type}");
            return;
        }
        
        // Instantiate obstacle
        GameObject obstacle = Instantiate(prefab, transform);
        obstacle.transform.position = new Vector3(xPosition, GetObstacleY(type), zPosition);
        
        _activeObstacles.Add(obstacle);
    }
    
    private GameObject GetPrefabForType(Obstacle.ObstacleType type)
    {
        return type switch
        {
            Obstacle.ObstacleType.JumpOver => _barrierLowPrefab,
            Obstacle.ObstacleType.SlideUnder => _barrierHighPrefab,
            Obstacle.ObstacleType.LaneBlock => _carPrefab,
            _ => _carPrefab
        };
    }
    
    private float GetObstacleY(Obstacle.ObstacleType type)
    {
        switch (type)
        {
            case Obstacle.ObstacleType.JumpOver:
                return 0.4f; // Half height - jump over
            case Obstacle.ObstacleType.SlideUnder:
                return 2.0f; // High enough to slide under (bottom at Y=1.5)
            case Obstacle.ObstacleType.LaneBlock:
            default:
                return 0.75f; // Half height - change lane
        }
    }
    
    private void CleanupObstacles(float behindZ)
    {
        for (int i = _activeObstacles.Count - 1; i >= 0; i--)
        {
            if (_activeObstacles[i] == null)
            {
                _activeObstacles.RemoveAt(i);
                continue;
            }
            
            if (_activeObstacles[i].transform.position.z < behindZ)
            {
                Destroy(_activeObstacles[i]);
                _activeObstacles.RemoveAt(i);
            }
        }
    }
}
