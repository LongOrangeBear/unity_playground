using UnityEngine;
using UnityEditor;

/// <summary>
/// Assigns different materials to barrier prefabs for visual distinction.
/// </summary>
public static class BarrierMaterialFixer
{
    [MenuItem("Runner/Fix Barrier Materials")]
    public static void FixBarrierMaterials()
    {
        // Load materials
        var redMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Runner/Obstacle_Red.mat");
        var orangeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Runner/Obstacle_Orange.mat");
        
        if (redMat == null || orangeMat == null)
        {
            Debug.LogError("[BarrierMaterialFixer] Materials not found!");
            return;
        }
        
        // Fix Barrier_High - use orange (need to slide under)
        FixPrefabMaterial("Assets/Prefabs/Obstacles/Barrier_High.prefab", orangeMat);
        
        // Fix Barrier_Low - keep red (need to jump over)
        FixPrefabMaterial("Assets/Prefabs/Obstacles/Barrier_Low.prefab", redMat);
        
        AssetDatabase.SaveAssets();
        Debug.Log("[BarrierMaterialFixer] Barrier materials fixed!");
    }
    
    private static void FixPrefabMaterial(string prefabPath, Material mat)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[BarrierMaterialFixer] Prefab not found: {prefabPath}");
            return;
        }
        
        var renderer = prefab.GetComponent<MeshRenderer>();
        if (renderer == null)
            renderer = prefab.GetComponentInChildren<MeshRenderer>();
        
        if (renderer != null)
        {
            renderer.sharedMaterial = mat;
            EditorUtility.SetDirty(prefab);
            Debug.Log($"[BarrierMaterialFixer] Applied {mat.name} to {prefab.name}");
        }
    }
}
