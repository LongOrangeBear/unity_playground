using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessSetup
{
    [MenuItem("Tools/Runner/Setup Post-Processing")]
    public static void SetupPostProcessing()
    {
        // 1. Find or Create Volume Object
        GameObject volumeGO = GameObject.Find("Global Volume");
        if (volumeGO == null)
        {
            volumeGO = new GameObject("Global Volume");
            volumeGO.layer = LayerMask.NameToLayer("Default"); // Ensure it's rendered
        }

        // 2. Setup Volume Component
        Volume volume = volumeGO.GetComponent<Volume>();
        if (volume == null)
            volume = volumeGO.AddComponent<Volume>();

        volume.isGlobal = true;

        // 3. Create/Load Profile
        if (volume.sharedProfile == null)
        {
            // Try to find existing
            string path = "Assets/Data/RunnerGlobalVolume.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
                AssetDatabase.SaveAssets();
                Debug.Log("[PostProcess] Created new Volume Profile");
            }
            
            volume.sharedProfile = profile;
        }
        
        ConfigureProfile(volume.sharedProfile);
        Debug.Log("[PostProcess] Global Volume configured!");
    }

    private static void ConfigureProfile(VolumeProfile profile)
    {
        // Bloom
        if (!profile.TryGet(out Bloom bloom))
            bloom = profile.Add<Bloom>(true);
            
        bloom.intensity.Override(1.5f);
        bloom.threshold.Override(0.9f);
        bloom.scatter.Override(0.7f);
        bloom.active = true;

        // Chromatic Aberration
        if (!profile.TryGet(out ChromaticAberration chrome))
            chrome = profile.Add<ChromaticAberration>(true);
            
        chrome.intensity.Override(0.3f);
        chrome.active = true;

        // Tonemapping
        if (!profile.TryGet(out Tonemapping tone))
            tone = profile.Add<Tonemapping>(true);
            
        tone.mode.Override(TonemappingMode.ACES);
        tone.active = true;

        // Color Adjustments
        if (!profile.TryGet(out ColorAdjustments colorAdj))
            colorAdj = profile.Add<ColorAdjustments>(true);
            
        colorAdj.postExposure.Override(0.5f);
        colorAdj.contrast.Override(15f);
        colorAdj.saturation.Override(20f);
        colorAdj.active = true;
        
        EditorUtility.SetDirty(profile);
    }
}
