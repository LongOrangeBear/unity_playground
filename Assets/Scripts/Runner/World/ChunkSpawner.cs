using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns and manages world chunks ahead of player.
/// </summary>
public class ChunkSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunnerSettings _settings;
    [SerializeField] private Transform _player;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject _chunkPrefab;
    [SerializeField] private GameObject[] _buildingPrefabs;
    [SerializeField] private GameObject _streetLightPrefab;
    
    [Header("Materials")]
    [SerializeField] private Material _groundMaterial;
    [SerializeField] private Material _buildingMaterial;
    [SerializeField] private Material _laneMarkerMaterial;
    
    [Header("Debug")]
    [SerializeField] private bool _useProceduralChunks = true;
    
    private List<Chunk> _activeChunks = new List<Chunk>();
    private float _nextSpawnZ = 0f;
    
    private void Start()
    {
        Debug.Log("[ChunkSpawner] Start called");
        
        if (_settings == null)
        {
            Debug.LogError("[ChunkSpawner] _settings is NULL! Assign RunnerSettings in Inspector.");
            return;
        }
        
        if (_player == null)
        {
            var playerRunner = FindFirstObjectByType<PlayerRunner>();
            if (playerRunner != null)
            {
                _player = playerRunner.transform;
                Debug.Log($"[ChunkSpawner] Found player: {_player.name}");
            }
            else
            {
                Debug.LogWarning("[ChunkSpawner] Player not found, will spawn chunks anyway");
            }
        }
        
        Debug.Log($"[ChunkSpawner] Spawning {_settings.chunksAhead + 1} initial chunks");
        
        // Spawn initial chunks
        for (int i = 0; i < _settings.chunksAhead + 1; i++)
        {
            SpawnChunk();
        }
        
        Debug.Log($"[ChunkSpawner] Spawned {_activeChunks.Count} chunks");
    }
    
    private void Update()
    {
        if (_player == null) return;
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing) return;
        
        float playerZ = _player.position.z;
        
        // Spawn new chunks ahead
        while (_nextSpawnZ < playerZ + (_settings.chunksAhead * _settings.chunkLength))
        {
            SpawnChunk();
        }
        
        // Despawn chunks behind
        for (int i = _activeChunks.Count - 1; i >= 0; i--)
        {
            Chunk chunk = _activeChunks[i];
            if (chunk.EndZ < playerZ - (_settings.chunksBehind * _settings.chunkLength))
            {
                DespawnChunk(chunk);
                _activeChunks.RemoveAt(i);
            }
        }
    }
    
    private void SpawnChunk()
    {
        Chunk chunk;
        
        if (_useProceduralChunks || _chunkPrefab == null)
        {
            // Create procedural chunk
            chunk = CreateProceduralChunk();
        }
        else
        {
            // Use prefab
            GameObject chunkObj = Instantiate(_chunkPrefab, transform);
            chunk = chunkObj.GetComponent<Chunk>();
        }
        
        chunk.Initialize(_nextSpawnZ);
        _activeChunks.Add(chunk);
        _nextSpawnZ += _settings.chunkLength;
    }
    
    private void DespawnChunk(Chunk chunk)
    {
        chunk.Reset();
        Destroy(chunk.gameObject);
    }
    
    /// <summary>
    /// Creates a procedural chunk using prefabs when available, or primitives as fallback.
    /// </summary>
    private Chunk CreateProceduralChunk()
    {
        GameObject chunkObj = new GameObject($"Chunk_{_nextSpawnZ}");
        chunkObj.transform.SetParent(transform);
        
        Chunk chunk = chunkObj.AddComponent<Chunk>();
        
        // Ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(chunkObj.transform);
        ground.transform.localPosition = new Vector3(0, -0.5f, _settings.chunkLength / 2f);
        ground.transform.localScale = new Vector3(12f, 1f, _settings.chunkLength);
        ground.tag = "Ground";
        if (_groundMaterial != null)
            ground.GetComponent<Renderer>().sharedMaterial = _groundMaterial;
        
        // Lane markers (3 thin lines)
        for (int lane = -1; lane <= 1; lane++)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = $"LaneMarker_{lane}";
            marker.transform.SetParent(chunkObj.transform);
            marker.transform.localPosition = new Vector3(lane * _settings.laneWidth, 0.01f, _settings.chunkLength / 2f);
            marker.transform.localScale = new Vector3(0.2f, 0.02f, _settings.chunkLength);
            
            // Remove collider from markers
            Destroy(marker.GetComponent<Collider>());
            
            if (_laneMarkerMaterial != null)
                marker.GetComponent<Renderer>().sharedMaterial = _laneMarkerMaterial;
        }
        
        // Buildings on sides (use prefabs if available)
        SpawnBuildings(chunkObj.transform, -8f); // Left side
        SpawnBuildings(chunkObj.transform, 8f);  // Right side
        
        return chunk;
    }
    
    private void SpawnBuildings(Transform parent, float xPosition)
    {
        float z = 0;
        while (z < _settings.chunkLength)
        {
            float buildingDepth = Random.Range(5f, 15f);
            float buildingHeight = Random.Range(10f, 40f);
            float buildingWidth = Random.Range(4f, 8f);
            
            GameObject building;
            
            // Use prefab if available
            if (_buildingPrefabs != null && _buildingPrefabs.Length > 0)
            {
                int prefabIndex = Random.Range(0, _buildingPrefabs.Length);
                if (_buildingPrefabs[prefabIndex] != null)
                {
                    building = Instantiate(_buildingPrefabs[prefabIndex], parent);
                    building.transform.localPosition = new Vector3(
                        xPosition + (xPosition > 0 ? buildingWidth / 2f : -buildingWidth / 2f),
                        0,
                        z + buildingDepth / 2f
                    );
                    // Randomize scale slightly
                    float scaleMultiplier = Random.Range(0.8f, 1.5f);
                    building.transform.localScale *= scaleMultiplier;
                }
                else
                {
                    building = CreatePrimitiveBuilding(parent, xPosition, z, buildingWidth, buildingHeight, buildingDepth);
                }
            }
            else
            {
                building = CreatePrimitiveBuilding(parent, xPosition, z, buildingWidth, buildingHeight, buildingDepth);
            }
            
            z += buildingDepth + Random.Range(0f, 3f); // Gap between buildings
        }
    }
    
    private GameObject CreatePrimitiveBuilding(Transform parent, float xPosition, float z, float width, float height, float depth)
    {
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = "Building";
        building.transform.SetParent(parent);
        building.transform.localPosition = new Vector3(
            xPosition + (xPosition > 0 ? width / 2f : -width / 2f),
            height / 2f,
            z + depth / 2f
        );
        building.transform.localScale = new Vector3(width, height, depth);
        
        // Remove collider to save performance (player doesn't collide with buildings)
        Destroy(building.GetComponent<Collider>());
        
        if (_buildingMaterial != null)
            building.GetComponent<Renderer>().sharedMaterial = _buildingMaterial;
        
        return building;
    }
}
