using UnityEngine;

/// <summary>
/// Manages particle effects and audio for game events.
/// Provides static access to spawn VFX at world positions.
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }
    
    [Header("Coin Effects")]
    [SerializeField] private ParticleSystem _coinPickupPrefab;
    [SerializeField] private AudioClip _coinPickupSound;
    
    [Header("Power-Up Effects")]
    [SerializeField] private ParticleSystem _powerUpPickupPrefab;
    [SerializeField] private AudioClip _powerUpPickupSound;
    
    [Header("Player Effects")]
    [SerializeField] private ParticleSystem _deathExplosionPrefab;
    [SerializeField] private ParticleSystem _shieldBreakPrefab;
    [SerializeField] private AudioClip _deathSound;
    [SerializeField] private AudioClip _shieldBreakSound;
    [SerializeField] private AudioClip _jumpSound;
    
    [Header("Speed Effects")]
    [SerializeField] private ParticleSystem _speedLinesPrefab;
    
    [Header("Audio")]
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 0.7f;
    
    private ParticleSystem _activeSpeedLines;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Create audio source if not assigned
        if (_sfxSource == null)
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
        }
    }
    
    #region Coin Effects
    
    public void PlayCoinPickup(Vector3 position)
    {
        SpawnParticle(_coinPickupPrefab, position);
        PlaySound(_coinPickupSound);
    }
    
    #endregion
    
    #region Power-Up Effects
    
    public void PlayPowerUpPickup(Vector3 position, PowerUpType type)
    {
        SpawnParticle(_powerUpPickupPrefab, position);
        PlaySound(_powerUpPickupSound);
    }
    
    #endregion
    
    #region Player Effects
    
    public void PlayDeathEffect(Vector3 position)
    {
        SpawnParticle(_deathExplosionPrefab, position);
        PlaySound(_deathSound);
    }
    
    public void PlayShieldBreak(Vector3 position)
    {
        SpawnParticle(_shieldBreakPrefab, position);
        PlaySound(_shieldBreakSound);
    }
    
    public void PlayJump()
    {
        PlaySound(_jumpSound);
    }
    
    #endregion
    
    #region Speed Effects
    
    public void StartSpeedLines(Transform parent)
    {
        if (_speedLinesPrefab == null || _activeSpeedLines != null) return;
        
        _activeSpeedLines = Instantiate(_speedLinesPrefab, parent);
        _activeSpeedLines.transform.localPosition = Vector3.zero;
        _activeSpeedLines.Play();
    }
    
    public void StopSpeedLines()
    {
        if (_activeSpeedLines != null)
        {
            _activeSpeedLines.Stop();
            Destroy(_activeSpeedLines.gameObject, 2f);
            _activeSpeedLines = null;
        }
    }
    
    #endregion
    
    #region Helpers
    
    private void SpawnParticle(ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null) return;
        
        var ps = Instantiate(prefab, position, Quaternion.identity);
        ps.Play();
        
        // Auto-destroy after particle lifetime
        float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, lifetime);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(clip, _sfxVolume);
    }
    
    #endregion
}
