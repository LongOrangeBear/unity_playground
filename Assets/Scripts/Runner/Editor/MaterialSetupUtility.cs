using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to assign materials to prefabs.
/// </summary>
[InitializeOnLoad]
public class MaterialSetupUtility
{
    private const string SessionKey = "MaterialSetupUtility_HasRun_v2";
    
    static MaterialSetupUtility()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool(SessionKey, false))
            {
                SessionState.SetBool(SessionKey, true);
                AssignMaterialsToPrefabs();
            }
        };
    }
    
    [MenuItem("Tools/Runner/Assign Materials to Prefabs")]
    public static void AssignMaterialsToPrefabs()
    {
        // 1. Update Shaders first
        UpdateMaterialShaders();
        CreateSkyboxMaterial();
        FixScenePlayer();

        // Obstacles - Red
        AssignMaterial("Assets/Prefabs/Obstacles/Barrier_Low.prefab", "Assets/Materials/Runner/Obstacle_Red.mat");
        AssignMaterial("Assets/Prefabs/Obstacles/Barrier_High.prefab", "Assets/Materials/Runner/Obstacle_Red.mat");
        AssignMaterial("Assets/Prefabs/Obstacles/Car.prefab", "Assets/Materials/Runner/Obstacle_Red.mat");
        
        // Enemies - Purple
        AssignMaterial("Assets/Prefabs/Enemies/Enemy_Ground.prefab", "Assets/Materials/Runner/Enemy_Purple.mat");
        AssignMaterial("Assets/Prefabs/Enemies/Enemy_Air.prefab", "Assets/Materials/Runner/Enemy_Purple.mat");
        
        // Collectibles
        AssignMaterial("Assets/Prefabs/Collectibles/Coin.prefab", "Assets/Materials/Runner/Coin_Gold.mat");
        AssignMaterial("Assets/Prefabs/Collectibles/PowerUp_Magnet.prefab", "Assets/Materials/Runner/PowerUp_Magnet.mat");
        AssignMaterial("Assets/Prefabs/Collectibles/PowerUp_Shield.prefab", "Assets/Materials/Runner/PowerUp_Shield.mat");
        AssignMaterial("Assets/Prefabs/Collectibles/PowerUp_DoubleScore.prefab", "Assets/Materials/Runner/PowerUp_DoubleScore.mat");
        AssignMaterial("Assets/Prefabs/Collectibles/PowerUp_SpeedBoost.prefab", "Assets/Materials/Runner/PowerUp_SpeedBoost.mat");
        
        // Player
        AssignMaterial("Assets/Prefabs/Player/Player.prefab", "Assets/Materials/Runner/Player_Blue.mat");
        
        // World - Buildings
        AssignMaterial("Assets/Prefabs/World/Building_Small.prefab", "Assets/Materials/Runner/Building_Gray.mat");
        AssignMaterial("Assets/Prefabs/World/Building_Medium.prefab", "Assets/Materials/Runner/Building_Gray.mat");
        AssignMaterial("Assets/Prefabs/World/Building_Tall.prefab", "Assets/Materials/Runner/Building_Gray.mat");
        AssignMaterial("Assets/Prefabs/World/Chunk_Basic.prefab", "Assets/Materials/Runner/Ground_Dark.mat");
        AssignMaterial("Assets/Prefabs/World/StreetLight.prefab", "Assets/Materials/Runner/LaneMarker_White.mat");
        
        // Setup ChunkSpawner materials
        SetupChunkSpawner();
        
        AssetDatabase.SaveAssets();
        Debug.Log("[MaterialSetup] All materials assigned!");
    }
    
    private static void AssignMaterial(string prefabPath, string materialPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        
        if (prefab == null || material == null) return;
        
        var renderer = prefab.GetComponent<Renderer>();
        if (renderer == null)
            renderer = prefab.GetComponentInChildren<Renderer>();
        
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            EditorUtility.SetDirty(prefab);
            Debug.Log($"[MaterialSetup] Assigned {material.name} to {prefab.name}");
        }
    }
    
    private static void SetupChunkSpawner()
    {
        var spawner = Object.FindFirstObjectByType<ChunkSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("[MaterialSetup] ChunkSpawner not found!");
            return;
        }
        
        var so = new SerializedObject(spawner);
        
        SetMaterialRef(so, "_groundMaterial", "Assets/Materials/Runner/Ground_Dark.mat");
        SetMaterialRef(so, "_buildingMaterial", "Assets/Materials/Runner/Building_Gray.mat");
        SetMaterialRef(so, "_laneMarkerMaterial", "Assets/Materials/Runner/LaneMarker_White.mat");
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spawner);
        Debug.Log("[MaterialSetup] ChunkSpawner materials configured.");
    }
    
    private static void SetMaterialRef(SerializedObject so, string propName, string path)
    {
        var prop = so.FindProperty(propName);
        if (prop != null)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
                prop.objectReferenceValue = mat;
        }
    }

    private static void UpdateMaterialShaders()
    {
        // Environment
        SetShader("Assets/Materials/Runner/Ground_Dark.mat", "Runner/NeonGridRoad");
        SetShader("Assets/Materials/Runner/Building_Gray.mat", "Runner/BuildingFacade");
        
        // Obstacles
        SetShader("Assets/Materials/Runner/Obstacle_Red.mat", "Runner/HolographicObstacle");
        // Enemy uses Purple, maybe holographic too? Or keep Lit? Let's try Holographic for consistent vibe.
        SetShader("Assets/Materials/Runner/Enemy_Purple.mat", "Runner/HolographicObstacle"); 
        
        // Collectibles
        SetShader("Assets/Materials/Runner/Coin_Gold.mat", "Runner/CollectibleGlow");
        SetShader("Assets/Materials/Runner/PowerUp_Magnet.mat", "Runner/CollectibleGlow");
        SetShader("Assets/Materials/Runner/PowerUp_Shield.mat", "Runner/CollectibleGlow");
        SetShader("Assets/Materials/Runner/PowerUp_DoubleScore.mat", "Runner/CollectibleGlow");
        SetShader("Assets/Materials/Runner/PowerUp_SpeedBoost.mat", "Runner/CollectibleGlow");
        
        // Player
        SetShader("Assets/Materials/Runner/Player_Blue.mat", "Runner/PlayerGlitch");
        
        Debug.Log("[MaterialSetup] Shaders updated to Premium Neon versions.");
    }

    private static void SetShader(string materialPath, string shaderName)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (mat == null) return;
        
        var shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.LogWarning($"[MaterialSetup] Shader '{shaderName}' not found. Check if file created.");
            return;
        }
        
        if (mat.shader != shader)
        {
            mat.shader = shader;
            EditorUtility.SetDirty(mat);
        }
    }

    private static void CreateSkyboxMaterial()
    {
        string path = "Assets/Materials/Runner/Skybox_Procedural.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        
        if (mat == null)
        {
            var shader = Shader.Find("Runner/ProceduralSkybox");
            if (shader == null) return;
            
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
            Debug.Log("[MaterialSetup] Created Skybox Material.");
        }
        else
        {
            SetShader(path, "Runner/ProceduralSkybox");
        }
        
        // Assign to RenderSettings?
        // RenderSettings.skybox = mat; // Only works if scene is open. Maybe skip for now or do if active scene matches.
    }

    private static void FixScenePlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            var renderer = player.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Runner/Player_Blue.mat");
                if (mat != null)
                {
                    renderer.sharedMaterial = mat;
                    Debug.Log("[MaterialSetup] Fixed Scene Player Material.");
                }
            }
        }
    }
}
