using UnityEngine;

/// <summary>
/// Power-up types available in the game.
/// </summary>
public enum PowerUpType
{
    Magnet,     // Auto-collect nearby coins
    Shield,     // Ignore 1 hit
    DoubleScore,// 2x points
    SpeedBoost  // Faster running
}

/// <summary>
/// Collectible power-up.
/// </summary>
public class PowerUp : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private PowerUpType _type;
    
    [Header("Visual")]
    [SerializeField] private float _rotationSpeed = 90f;
    [SerializeField] private float _pulseSpeed = 2f;
    [SerializeField] private float _pulseScale = 0.2f;
    
    public PowerUpType Type => _type;
    
    private Vector3 _baseScale;
    
    private void Start()
    {
        _baseScale = transform.localScale;
        
        if (!gameObject.CompareTag("PowerUp"))
            gameObject.tag = "PowerUp";
    }
    
    private void Update()
    {
        // Rotate
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        
        // Pulse scale
        float pulse = 1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseScale;
        transform.localScale = _baseScale * pulse;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }
    
    private void Collect()
    {
        PowerUpManager.Instance?.ActivatePowerUp(_type);
        
        // Play VFX and sound
        VFXManager.Instance?.PlayPowerUpPickup(transform.position, _type);
        
        Destroy(gameObject);
    }
}
