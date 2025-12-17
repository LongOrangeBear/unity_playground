using UnityEngine;

/// <summary>
/// Camera that follows the player from behind.
/// </summary>
public class RunnerCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0, 5, -10);
    [SerializeField] private float _smoothSpeed = 5f;
    
    [Header("Look Settings")]
    [SerializeField] private Vector3 _lookOffset = new Vector3(0, 2, 10);
    
    [Header("Effects")]
    [SerializeField] private float _baseFOV = 60f;
    [SerializeField] private float _maxFOV = 75f;
    [SerializeField] private float _fovLerpSpeed = 2f;
    
    private Camera _camera;
    private Vector3 _currentVelocity;
    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
            _camera = Camera.main;
    }
    
    private void Start()
    {
        if (_target == null)
        {
            var player = FindFirstObjectByType<PlayerRunner>();
            if (player != null)
                _target = player.transform;
        }
    }
    
    private void LateUpdate()
    {
        if (_target == null) return;
        
        FollowTarget();
        UpdateFOV();
    }
    
    private void FollowTarget()
    {
        // Calculate desired position
        Vector3 desiredPosition = _target.position + _offset;
        
        // Keep X centered (don't follow player side-to-side too much)
        desiredPosition.x = Mathf.Lerp(desiredPosition.x, 0, 0.8f);
        
        // Smooth follow
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref _currentVelocity, 
            1f / _smoothSpeed);
        
        // Look at point ahead of player
        Vector3 lookTarget = _target.position + _lookOffset;
        transform.LookAt(lookTarget);
    }
    
    private void UpdateFOV()
    {
        if (_camera == null || GameManager.Instance == null) return;
        
        // Increase FOV with speed
        float speedRatio = GameManager.Instance.CurrentSpeed / GameManager.Instance.Settings.maxRunSpeed;
        float targetFOV = Mathf.Lerp(_baseFOV, _maxFOV, speedRatio);
        
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFOV, _fovLerpSpeed * Time.deltaTime);
    }
}
