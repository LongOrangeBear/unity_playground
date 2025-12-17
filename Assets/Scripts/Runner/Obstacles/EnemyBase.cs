using UnityEngine;

/// <summary>
/// Base class for enemies.
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] protected float _moveSpeed = 2f;
    
    protected virtual void OnEnable()
    {
        if (!gameObject.CompareTag("Enemy"))
            gameObject.tag = "Enemy";
    }
    
    public virtual void Reset()
    {
        // Override in derived classes
    }
}
