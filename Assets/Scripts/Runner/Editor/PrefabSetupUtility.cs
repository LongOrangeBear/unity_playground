using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to automatically configure prefab references on spawners.
/// </summary>
[InitializeOnLoad]
public class PrefabSetupUtility : EditorWindow
{
    private const string SessionKey = "PrefabSetupUtility_HasRun";
    
    static PrefabSetupUtility()
    {
        // Run setup once per session after domain reload
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool(SessionKey, false))
            {
                SessionState.SetBool(SessionKey, true);
                SetupPrefabReferences();
            }
        };
    }
    
    [MenuItem("Tools/Runner/Setup Prefab References")]
    public static void SetupPrefabReferences()
    {
        SetupObstacleSpawner();
        SetupEnemySpawner();
        SetupCollectibleSpawner();
        SetupChunkSpawner();
        SetupPowerUpTypes();
        
        AssetDatabase.SaveAssets();
        Debug.Log("[PrefabSetup] All prefab references configured successfully!");
    }
    
    private static void SetupObstacleSpawner()
    {
        var spawner = FindFirstObjectByType<ObstacleSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("[PrefabSetup] ObstacleSpawner not found in scene!");
            return;
        }
        
        var so = new SerializedObject(spawner);
        
        SetPrefabReference(so, "_barrierLowPrefab", "Assets/Prefabs/Obstacles/Barrier_Low.prefab");
        SetPrefabReference(so, "_barrierHighPrefab", "Assets/Prefabs/Obstacles/Barrier_High.prefab");
        SetPrefabReference(so, "_carPrefab", "Assets/Prefabs/Obstacles/Car.prefab");
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spawner);
        Debug.Log("[PrefabSetup] ObstacleSpawner configured.");
    }
    
    private static void SetupEnemySpawner()
    {
        var spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("[PrefabSetup] EnemySpawner not found in scene!");
            return;
        }
        
        var so = new SerializedObject(spawner);
        
        SetPrefabReference(so, "_groundEnemyPrefab", "Assets/Prefabs/Enemies/Enemy_Ground.prefab");
        SetPrefabReference(so, "_airEnemyPrefab", "Assets/Prefabs/Enemies/Enemy_Air.prefab");
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spawner);
        Debug.Log("[PrefabSetup] EnemySpawner configured.");
    }
    
    private static void SetupCollectibleSpawner()
    {
        var spawner = FindFirstObjectByType<CollectibleSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("[PrefabSetup] CollectibleSpawner not found in scene!");
            return;
        }
        
        var so = new SerializedObject(spawner);
        
        SetPrefabReference(so, "_coinPrefab", "Assets/Prefabs/Collectibles/Coin.prefab");
        SetPrefabReference(so, "_powerUpMagnetPrefab", "Assets/Prefabs/Collectibles/PowerUp_Magnet.prefab");
        SetPrefabReference(so, "_powerUpShieldPrefab", "Assets/Prefabs/Collectibles/PowerUp_Shield.prefab");
        SetPrefabReference(so, "_powerUpDoubleScorePrefab", "Assets/Prefabs/Collectibles/PowerUp_DoubleScore.prefab");
        SetPrefabReference(so, "_powerUpSpeedBoostPrefab", "Assets/Prefabs/Collectibles/PowerUp_SpeedBoost.prefab");
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spawner);
        Debug.Log("[PrefabSetup] CollectibleSpawner configured.");
    }
    
    private static void SetupChunkSpawner()
    {
        var spawner = FindFirstObjectByType<ChunkSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("[PrefabSetup] ChunkSpawner not found in scene!");
            return;
        }
        
        var so = new SerializedObject(spawner);
        
        SetPrefabReference(so, "_chunkPrefab", "Assets/Prefabs/World/Chunk_Basic.prefab");
        
        // Setup building prefabs array
        var buildingsProperty = so.FindProperty("_buildingPrefabs");
        if (buildingsProperty != null)
        {
            string[] buildingPaths = {
                "Assets/Prefabs/World/Building_Small.prefab",
                "Assets/Prefabs/World/Building_Medium.prefab",
                "Assets/Prefabs/World/Building_Tall.prefab"
            };
            
            buildingsProperty.arraySize = buildingPaths.Length;
            for (int i = 0; i < buildingPaths.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(buildingPaths[i]);
                buildingsProperty.GetArrayElementAtIndex(i).objectReferenceValue = prefab;
            }
        }
        
        SetPrefabReference(so, "_streetLightPrefab", "Assets/Prefabs/World/StreetLight.prefab");
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spawner);
        Debug.Log("[PrefabSetup] ChunkSpawner configured.");
    }
    
    private static void SetupPowerUpTypes()
    {
        SetPowerUpType("Assets/Prefabs/Collectibles/PowerUp_Magnet.prefab", PowerUpType.Magnet);
        SetPowerUpType("Assets/Prefabs/Collectibles/PowerUp_Shield.prefab", PowerUpType.Shield);
        SetPowerUpType("Assets/Prefabs/Collectibles/PowerUp_DoubleScore.prefab", PowerUpType.DoubleScore);
        SetPowerUpType("Assets/Prefabs/Collectibles/PowerUp_SpeedBoost.prefab", PowerUpType.SpeedBoost);
        
        Debug.Log("[PrefabSetup] PowerUp types configured.");
    }
    
    private static void SetPrefabReference(SerializedObject so, string propertyName, string prefabPath)
    {
        var property = so.FindProperty(propertyName);
        if (property != null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                property.objectReferenceValue = prefab;
            }
            else
            {
                Debug.LogWarning($"[PrefabSetup] Prefab not found at: {prefabPath}");
            }
        }
        else
        {
            Debug.LogWarning($"[PrefabSetup] Property '{propertyName}' not found!");
        }
    }
    
    private static void SetPowerUpType(string prefabPath, PowerUpType type)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[PrefabSetup] PowerUp prefab not found: {prefabPath}");
            return;
        }
        
        var powerUp = prefab.GetComponent<PowerUp>();
        if (powerUp == null)
        {
            Debug.LogWarning($"[PrefabSetup] PowerUp component not found on: {prefabPath}");
            return;
        }
        
        var so = new SerializedObject(powerUp);
        var typeProperty = so.FindProperty("_type");
        if (typeProperty != null)
        {
            typeProperty.enumValueIndex = (int)type;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(prefab);
        }
    }
}
