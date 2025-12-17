using UnityEngine;

/// <summary>
/// Camera effects: FOV changes with speed, screen shake on events.
/// Attach to Main Camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraEffects : MonoBehaviour
{
    public static CameraEffects Instance { get; private set; }
    
    [Header("FOV Settings")]
    [SerializeField] private bool _enableFOVEffect = false; // Disabled by default
    [SerializeField] private float _baseFOV = 60f;
    [SerializeField] private float _maxFOV = 75f;
    [SerializeField] private float _speedThreshold = 15f;
    [SerializeField] private float _maxSpeed = 30f;
    [SerializeField] private float _fovLerpSpeed = 3f;
    
    [Header("Screen Shake")]
    [SerializeField] private float _shakeDecay = 5f;
    
    [Header("Speed Boost Effect")]
    [SerializeField] private float _boostFOVBonus = 10f;
    
    private Camera _camera;
    private float _shakeAmount;
    private Vector3 _originalPosition;
    private float _targetFOV;
    
    private void Awake()
    {
        Instance = this;
        _camera = GetComponent<Camera>();
        _baseFOV = _camera.fieldOfView;
        _targetFOV = _baseFOV;
    }
    
    private void LateUpdate()
    {
        if (GameManager.Instance == null) return;
        
        UpdateFOV();
        ApplyShake();
    }
    
    private void UpdateFOV()
    {
        if (!_enableFOVEffect) return;
        
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            _targetFOV = _baseFOV;
        }
        else
        {
            float speed = GameManager.Instance.CurrentSpeed;
            float speedProgress = Mathf.InverseLerp(_speedThreshold, _maxSpeed, speed);
            _targetFOV = Mathf.Lerp(_baseFOV, _maxFOV, speedProgress);
            
            // Add extra FOV during speed boost
            if (PowerUpManager.Instance != null && PowerUpManager.Instance.HasSpeedBoost)
            {
                _targetFOV += _boostFOVBonus;
            }
        }
        
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFOV, _fovLerpSpeed * Time.deltaTime);
    }
    
    private void ApplyShake()
    {
        if (_shakeAmount > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * _shakeAmount;
            shakeOffset.z = 0; // Don't shake forward/back
            transform.localPosition = _originalPosition + shakeOffset;
            
            _shakeAmount = Mathf.Lerp(_shakeAmount, 0, _shakeDecay * Time.deltaTime);
            if (_shakeAmount < 0.01f)
            {
                _shakeAmount = 0;
                transform.localPosition = _originalPosition;
            }
        }
    }
    
    /// <summary>
    /// Trigger screen shake.
    /// </summary>
    public void Shake(float intensity = 0.3f)
    {
        _originalPosition = transform.localPosition;
        _shakeAmount = intensity;
    }
    
    /// <summary>
    /// Strong shake for death.
    /// </summary>
    public void ShakeDeath()
    {
        Shake(0.5f);
    }
    
    /// <summary>
    /// Light shake for near-miss.
    /// </summary>
    public void ShakeNearMiss()
    {
        Shake(0.15f);
    }
}
