using UnityEngine;
using UnityEditor;

/// <summary>
/// Separates the visual components of the player into a 'Visual' child object
/// to allow scaling/rotating without affecting the CharacterController.
/// </summary>
public static class PlayerStructureFixer
{
    [MenuItem("Runner/Fix Player Hierarchy")]
    public static void FixPlayerHierarchy()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[PlayerStructureFixer] Player not found in scene!");
            return;
        }
        
        // Check if Visual already exists
        Transform existingVisual = player.transform.Find("Visual");
        if (existingVisual != null)
        {
            Debug.Log("[PlayerStructureFixer] 'Visual' child already exists. Verifying components...");
            // Optionally we could verify properties here
            return;
        }
        
        // 1. Create Visual Child
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(player.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;
        
        // 2. Move MeshFilter
        MeshFilter rootMF = player.GetComponent<MeshFilter>();
        if (rootMF != null)
        {
            MeshFilter childMF = visual.AddComponent<MeshFilter>();
            childMF.sharedMesh = rootMF.sharedMesh;
            Object.DestroyImmediate(rootMF);
        }
        
        // 3. Move MeshRenderer
        MeshRenderer rootMR = player.GetComponent<MeshRenderer>();
        if (rootMR != null)
        {
            MeshRenderer childMR = visual.AddComponent<MeshRenderer>();
            childMR.sharedMaterials = rootMR.sharedMaterials;
            childMR.shadowCastingMode = rootMR.shadowCastingMode;
            childMR.receiveShadows = rootMR.receiveShadows;
            Object.DestroyImmediate(rootMR);
        }
        
        // 4. Update PlayerVisualEffects
        PlayerVisualEffects vfx = player.GetComponent<PlayerVisualEffects>();
        if (vfx == null) vfx = player.AddComponent<PlayerVisualEffects>();
        
        // We need to set the private _visualTarget field via SerializedObject since it's private
        SerializedObject so = new SerializedObject(vfx);
        SerializedProperty prop = so.FindProperty("_visualTarget");
        if (prop != null)
        {
            prop.objectReferenceValue = visual.transform;
            so.ApplyModifiedProperties();
        }
        
        // 5. Move Trail (if exists) or let script recreate it
        Transform existingTrail = player.transform.Find("PlayerTrail");
        if (existingTrail != null)
        {
            existingTrail.SetParent(visual.transform, false);
        }
        
        EditorUtility.SetDirty(player);
        Debug.Log("[PlayerStructureFixer] Player hierarchy fixed! Visuals moved to child.");
    }
}
