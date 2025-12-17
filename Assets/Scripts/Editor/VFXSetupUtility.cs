using UnityEngine;
using UnityEditor;

/// <summary>
/// Sets up VFXManager in the scene with default particle prefabs.
/// </summary>
public static class VFXSetupUtility
{
    [MenuItem("Runner/Setup VFX Manager")]
    public static void SetupVFXManager()
    {
        // Find or create VFXManager
        var vfxManager = Object.FindFirstObjectByType<VFXManager>();
        if (vfxManager == null)
        {
            var go = new GameObject("VFXManager");
            vfxManager = go.AddComponent<VFXManager>();
            Debug.Log("[VFXSetup] Created VFXManager GameObject");
        }
        
        // Create VFX prefabs folder
        string vfxFolder = "Assets/Prefabs/VFX";
        if (!AssetDatabase.IsValidFolder(vfxFolder))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "VFX");
        }
        
        // Create particle prefabs
        CreateCoinPickupVFX(vfxFolder);
        CreatePowerUpPickupVFX(vfxFolder);
        CreateDeathExplosionVFX(vfxFolder);
        CreateShieldBreakVFX(vfxFolder);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Assign to VFXManager via SerializedObject
        AssignVFXPrefabs(vfxManager);
        
        EditorUtility.SetDirty(vfxManager);
        Debug.Log("[VFXSetup] VFX setup complete!");
    }
    
    private static void CreateCoinPickupVFX(string folder)
    {
        string path = $"{folder}/CoinPickupVFX.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
        
        var go = new GameObject("CoinPickupVFX");
        var ps = go.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.duration = 0.3f;
        main.startLifetime = 0.4f;
        main.startSpeed = 5f;
        main.startSize = 0.2f;
        main.startColor = new Color(1f, 0.85f, 0.2f); // Gold
        main.maxParticles = 20;
        main.loop = false;
        main.playOnAwake = false;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 15) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = GetDefaultParticleMaterial();
        
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log($"[VFXSetup] Created {path}");
    }
    
    private static void CreatePowerUpPickupVFX(string folder)
    {
        string path = $"{folder}/PowerUpPickupVFX.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
        
        var go = new GameObject("PowerUpPickupVFX");
        var ps = go.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.6f;
        main.startSpeed = 8f;
        main.startSize = 0.3f;
        main.startColor = new Color(0.4f, 1f, 0.4f); // Green
        main.maxParticles = 30;
        main.loop = false;
        main.playOnAwake = false;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 25) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = GetDefaultParticleMaterial();
        
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log($"[VFXSetup] Created {path}");
    }
    
    private static void CreateDeathExplosionVFX(string folder)
    {
        string path = $"{folder}/DeathExplosionVFX.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
        
        var go = new GameObject("DeathExplosionVFX");
        var ps = go.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.duration = 0.8f;
        main.startLifetime = 1f;
        main.startSpeed = 12f;
        main.startSize = 0.5f;
        main.startColor = new Color(1f, 0.3f, 0.1f); // Orange-red
        main.maxParticles = 50;
        main.loop = false;
        main.playOnAwake = false;
        main.gravityModifier = 1f;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 40) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.8f;
        
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = GetDefaultParticleMaterial();
        
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log($"[VFXSetup] Created {path}");
    }
    
    private static void CreateShieldBreakVFX(string folder)
    {
        string path = $"{folder}/ShieldBreakVFX.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
        
        var go = new GameObject("ShieldBreakVFX");
        var ps = go.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.duration = 0.4f;
        main.startLifetime = 0.5f;
        main.startSpeed = 10f;
        main.startSize = 0.25f;
        main.startColor = new Color(0.5f, 0.8f, 1f); // Light blue
        main.maxParticles = 30;
        main.loop = false;
        main.playOnAwake = false;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 25) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;
        
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = GetDefaultParticleMaterial();
        
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log($"[VFXSetup] Created {path}");
    }
    
    private static Material GetDefaultParticleMaterial()
    {
        // Try to find Default-Particle material
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Particle.mat");
        if (mat != null) return mat;
        
        // Create one
        mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        mat.name = "Particle";
        AssetDatabase.CreateAsset(mat, "Assets/Materials/Particle.mat");
        return mat;
    }
    
    private static void AssignVFXPrefabs(VFXManager manager)
    {
        var so = new SerializedObject(manager);
        
        var coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/CoinPickupVFX.prefab");
        var powerUpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/PowerUpPickupVFX.prefab");
        var deathPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/DeathExplosionVFX.prefab");
        var shieldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/ShieldBreakVFX.prefab");
        
        if (coinPrefab != null)
            so.FindProperty("_coinPickupPrefab").objectReferenceValue = coinPrefab.GetComponent<ParticleSystem>();
        if (powerUpPrefab != null)
            so.FindProperty("_powerUpPickupPrefab").objectReferenceValue = powerUpPrefab.GetComponent<ParticleSystem>();
        if (deathPrefab != null)
            so.FindProperty("_deathExplosionPrefab").objectReferenceValue = deathPrefab.GetComponent<ParticleSystem>();
        if (shieldPrefab != null)
            so.FindProperty("_shieldBreakPrefab").objectReferenceValue = shieldPrefab.GetComponent<ParticleSystem>();
        
        so.ApplyModifiedProperties();
    }
}
