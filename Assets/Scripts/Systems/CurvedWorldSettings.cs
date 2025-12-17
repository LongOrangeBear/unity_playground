using UnityEngine;

[ExecuteAlways]
public class CurvedWorldSettings : MonoBehaviour
{
    [Range(-0.1f, 0.1f)]
    public float curveStrength = -0.01f; // Negative for "horizon drop" effect

    [Range(-0.1f, 0.1f)]
    public float curveXStrength = 0.0f;

    private static readonly int CurveStrengthID = Shader.PropertyToID("_CurveStrength");
    private static readonly int CurveXStrengthID = Shader.PropertyToID("_CurveXStrength");

    void Update()
    {
        Shader.SetGlobalFloat(CurveStrengthID, curveStrength);
        Shader.SetGlobalFloat(CurveXStrengthID, curveXStrength);
    }
}
