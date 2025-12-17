using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Displays orbiting cubes around player for active power-ups.
/// Attach to Player.
/// </summary>
public class PowerUpOrbitEffect : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private float _orbitRadius = 1.5f;
    [SerializeField] private float _orbitSpeed = 180f; // degrees per second
    [SerializeField] private float _cubeSize = 0.3f;
    [SerializeField] private float _verticalOffset = 1f;
    
    private Dictionary<PowerUpType, GameObject> _orbitingCubes = new Dictionary<PowerUpType, GameObject>();
    private float _currentAngle;
    
    private void Start()
    {
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnPowerUpActivated += OnPowerUpActivated;
            PowerUpManager.Instance.OnPowerUpExpired += OnPowerUpExpired;
        }
    }
    
    private void OnDestroy()
    {
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnPowerUpActivated -= OnPowerUpActivated;
            PowerUpManager.Instance.OnPowerUpExpired -= OnPowerUpExpired;
        }
    }
    
    private void Update()
    {
        _currentAngle += _orbitSpeed * Time.deltaTime;
        if (_currentAngle > 360f) _currentAngle -= 360f;
        
        UpdateOrbitPositions();
    }
    
    private void OnPowerUpActivated(PowerUpType type, float duration)
    {
        if (_orbitingCubes.ContainsKey(type))
        {
            // Already exists, just reset
            return;
        }
        
        // Create orbiting cube
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = $"Orbit_{type}";
        cube.transform.SetParent(transform);
        cube.transform.localScale = Vector3.one * _cubeSize;
        
        // Remove collider
        Destroy(cube.GetComponent<Collider>());
        
        // Set color based on type - use the renderer's existing material
        var renderer = cube.GetComponent<Renderer>();
        Color cubeColor = GetColorForType(type);
        renderer.material.color = cubeColor;
        
        // Make it glow
        if (renderer.material.HasProperty("_EmissionColor"))
        {
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", cubeColor * 0.5f);
        }
        
        _orbitingCubes[type] = cube;
        Debug.Log($"[PowerUpOrbitEffect] Created orbit cube for {type}");
    }
    
    private void OnPowerUpExpired(PowerUpType type)
    {
        if (_orbitingCubes.TryGetValue(type, out GameObject cube))
        {
            Destroy(cube);
            _orbitingCubes.Remove(type);
            Debug.Log($"[PowerUpOrbitEffect] Removed orbit cube for {type}");
        }
    }
    
    private void UpdateOrbitPositions()
    {
        int count = _orbitingCubes.Count;
        if (count == 0) return;
        
        float angleStep = 360f / count;
        int index = 0;
        
        foreach (var kvp in _orbitingCubes)
        {
            float angle = _currentAngle + angleStep * index;
            float rad = angle * Mathf.Deg2Rad;
            
            Vector3 offset = new Vector3(
                Mathf.Cos(rad) * _orbitRadius,
                _verticalOffset + Mathf.Sin(Time.time * 3f + index) * 0.2f, // bobbing
                Mathf.Sin(rad) * _orbitRadius
            );
            
            kvp.Value.transform.position = transform.position + offset;
            kvp.Value.transform.Rotate(Vector3.up * 180f * Time.deltaTime); // spin
            
            index++;
        }
    }
    
    private Color GetColorForType(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.Magnet => Color.blue,
            PowerUpType.DoubleScore => Color.green,
            PowerUpType.SpeedBoost => Color.yellow,
            PowerUpType.Shield => Color.white,
            _ => Color.magenta
        };
    }
}
