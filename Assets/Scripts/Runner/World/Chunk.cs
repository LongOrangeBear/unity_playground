using UnityEngine;

/// <summary>
/// Represents a spawnable chunk of the endless world.
/// Contains ground, buildings, decorations.
/// </summary>
public class Chunk : MonoBehaviour
{
    [Header("Chunk Settings")]
    [SerializeField] private float _length = 50f;
    
    public float Length => _length;
    public float EndZ => transform.position.z + _length;
    
    /// <summary>
    /// Called when chunk is spawned from pool.
    /// </summary>
    public void Initialize(float startZ)
    {
        transform.position = new Vector3(0, 0, startZ);
    }
    
    /// <summary>
    /// Called when chunk is returned to pool.
    /// </summary>
    public void Reset()
    {
        // Clear any spawned obstacles/coins (they have their own pools)
        // This is handled by obstacle/coin spawner
    }
}
