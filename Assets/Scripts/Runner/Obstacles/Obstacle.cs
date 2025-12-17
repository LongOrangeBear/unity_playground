using UnityEngine;

/// <summary>
/// Base obstacle component. Attach to any obstacle prefab.
/// </summary>
public class Obstacle : MonoBehaviour
{
    public enum ObstacleType
    {
        JumpOver,   // Low barrier - jump to avoid
        SlideUnder, // High barrier - slide to avoid
        LaneBlock   // Full block - switch lanes to avoid
    }
    
    [Header("Settings")]
    [SerializeField] private ObstacleType _type = ObstacleType.LaneBlock;
    
    public ObstacleType Type => _type;
    
    private void OnEnable()
    {
        // Ensure correct tag
        if (!gameObject.CompareTag("Obstacle"))
            gameObject.tag = "Obstacle";
    }
    
    /// <summary>
    /// Called when returned to pool.
    /// </summary>
    public void Reset()
    {
        // Reset any state if needed
    }
}
