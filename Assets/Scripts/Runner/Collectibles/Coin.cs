using UnityEngine;

/// <summary>
/// Collectible coin that gives points when collected.
/// </summary>
public class Coin : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private float _bobAmplitude = 0.2f;
    [SerializeField] private float _bobFrequency = 2f;
    
    private float _baseY;
    private float _timeOffset;
    private float _lastBobOffset;
    
    private void Start()
    {
        _baseY = transform.position.y;
        _timeOffset = Random.Range(0f, Mathf.PI * 2f);
        _lastBobOffset = 0f;
        
        if (!gameObject.CompareTag("Coin"))
            gameObject.tag = "Coin";
    }
    
    private void Update()
    {
        // Rotate
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        
        // Bob up and down (relative to base Y, preserves XZ movement from magnet)
        float newBobOffset = Mathf.Sin((Time.time + _timeOffset) * _bobFrequency) * _bobAmplitude;
        float bobDelta = newBobOffset - _lastBobOffset;
        transform.position += new Vector3(0, bobDelta, 0);
        _lastBobOffset = newBobOffset;
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
        ScoreManager.Instance?.AddCoin();
        
        // Play VFX and sound
        VFXManager.Instance?.PlayCoinPickup(transform.position);
        
        Destroy(gameObject); // Or return to pool
    }
}
