using UnityEngine;

/// <summary>
/// Visual juice effects for player: smooth lane tilt, squash/stretch on jump only.
/// Trail disabled by default.
/// </summary>
public class PlayerVisualEffects : MonoBehaviour
{
    [Header("Lane Tilt")]
    [SerializeField] private float _maxTiltAngle = 12f;
    [SerializeField] private float _tiltSpeed = 4f; // Slower for smoothness
    [SerializeField] private float _tiltReturnSpeed = 6f;
    
    [Header("Squash & Stretch (Jump Only)")]
    [SerializeField] private float _stretchAmount = 1.15f;
    [SerializeField] private float _landSquashAmount = 0.85f;
    [SerializeField] private float _effectSpeed = 12f;
    [SerializeField] private float _landSquashDuration = 0.1f;
    
    [Header("Trail")]
    [SerializeField] private bool _enableTrail = true; // Re-enabled with better settings
    
    [Header("References")]
    [SerializeField] private Transform _visualTarget;
    
    private Transform _visualTransform;
    private Vector3 _baseScale;
    private Vector3 _currentScale;
    private float _currentTilt;
    private float _smoothTiltVelocity;
    private bool _wasGrounded = true;
    private CharacterController _controller;
    private float _landSquashTimer;
    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null)
            _controller = GetComponentInParent<CharacterController>();
            
        // Setup visual target
        if (_visualTarget == null)
        {
            var visualParams = transform.Find("Visual");
            if (visualParams != null)
                _visualTarget = visualParams;
            else
                _visualTarget = transform; // Fallback (warning: will scale/rotate root)
        }
        
        _visualTransform = _visualTarget;
        _baseScale = _visualTransform.localScale;
        _currentScale = _baseScale;
    }
    
    private TrailRenderer _trail;
    
    private void Start()
    {
        // Force enable for debugging if needed, or rely on inspector. 
        // Since we changed default, let's force it once to be sure.
        _enableTrail = true; 
        
        if (_enableTrail)
        {
            CreateTrail();
        }
    }
    
    private void CreateTrail()
    {
        // Check if already exists on visual
        if (_visualTransform.Find("PlayerTrail") != null) return;
        
        var trailGO = new GameObject("PlayerTrail");
        trailGO.transform.SetParent(_visualTransform, false);
        trailGO.transform.localPosition = new Vector3(0, -0.9f, 0); 
        
        _trail = trailGO.AddComponent<TrailRenderer>();
        _trail.time = 0.2f; 
        _trail.startWidth = 0.4f; 
        _trail.endWidth = 0.05f;
        _trail.startColor = new Color(0.2f, 0.5f, 1f, 0.5f); 
        _trail.endColor = new Color(0.2f, 0.5f, 1f, 0f);
        
        // Use Sprites/Default as it is always included and visible
        Material mat = new Material(Shader.Find("Sprites/Default"));
        _trail.material = mat;
        
        _trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _trail.receiveShadows = false;
        
        Debug.Log($"[PlayerVisualEffects] Trail created on {trailGO.name} parented to {_visualTransform.name}");
    }
    
    private void Update()
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing) return;
        
        UpdateLaneTilt();
        UpdateSquashStretch();
        UpdateTrailColor();
    }

    private void UpdateTrailColor()
    {
        if (_trail == null || PowerUpManager.Instance == null) return;
        
        Color targetColor = new Color(0.2f, 0.5f, 1f, 0.5f); // Default Blue
        
        if (PowerUpManager.Instance.HasSpeedBoost)
            targetColor = new Color(1f, 0.8f, 0.2f, 0.6f); // Yellow/Gold
        else if (PowerUpManager.Instance.HasShield)
            targetColor = new Color(0f, 1f, 1f, 0.6f); // Cyan
            
        _trail.startColor = Color.Lerp(_trail.startColor, targetColor, 5f * Time.deltaTime);
        _trail.endColor = Color.Lerp(_trail.endColor, new Color(targetColor.r, targetColor.g, targetColor.b, 0f), 5f * Time.deltaTime);
    }
    
    private void UpdateLaneTilt()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null) return;
        
        // Target tilt based on input
        float targetTilt = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            targetTilt = _maxTiltAngle; // Lean right when going left
        else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            targetTilt = -_maxTiltAngle; // Lean left when going right
        
        // Smooth damp for very smooth transitions
        float speed = Mathf.Abs(targetTilt) > 0.1f ? _tiltSpeed : _tiltReturnSpeed;
        _currentTilt = Mathf.SmoothDamp(_currentTilt, targetTilt, ref _smoothTiltVelocity, 1f / speed);
        
        // Apply rotation
        Vector3 euler = _visualTransform.localEulerAngles;
        euler.z = _currentTilt;
        _visualTransform.localEulerAngles = euler;
    }
    
    private void UpdateSquashStretch()
    {
        if (_controller == null) return;
        
        Vector3 targetScale = _baseScale;

        // Check for Slide first
        PlayerRunner runner = GetComponent<PlayerRunner>();
        if (runner == null) runner = GetComponentInParent<PlayerRunner>();
        
        if (runner != null && runner.IsSliding)
        {
            // Squash vertically, stretch horizontally
            targetScale = new Vector3(_baseScale.x * 1.3f, _baseScale.y * 0.5f, _baseScale.z * 1.3f);
            
            // Fast transition for slide
            _currentScale = Vector3.Lerp(_currentScale, targetScale, 20f * Time.deltaTime);
            _visualTransform.localScale = _currentScale;
            return;
        }
        
        // --- Normal Jump logic ---
        
        bool isGrounded = _controller.isGrounded;
        float velocityY = _controller.velocity.y;
        
        // In air going up - stretch vertically
        if (!isGrounded && velocityY > 1f)
        {
            float t = Mathf.Clamp01(velocityY / 10f);
            float yStretch = Mathf.Lerp(1f, _stretchAmount, t);
            float xzSquash = 1f / Mathf.Sqrt(yStretch); // Preserve volume
            targetScale = new Vector3(_baseScale.x * xzSquash, _baseScale.y * yStretch, _baseScale.z * xzSquash);
        }
        // Just landed - brief squash
        else if (isGrounded && !_wasGrounded)
        {
            _landSquashTimer = _landSquashDuration;
        }
        
        // Apply landing squash
        if (_landSquashTimer > 0)
        {
            float t = _landSquashTimer / _landSquashDuration;
            float ySquash = Mathf.Lerp(1f, _landSquashAmount, t);
            float xzStretch = 1f / Mathf.Sqrt(ySquash);
            targetScale = new Vector3(_baseScale.x * xzStretch, _baseScale.y * ySquash, _baseScale.z * xzStretch);
            _landSquashTimer -= Time.deltaTime;
        }
        
        _currentScale = Vector3.Lerp(_currentScale, targetScale, _effectSpeed * Time.deltaTime);
        _visualTransform.localScale = _currentScale;
        _wasGrounded = isGrounded;
    }
}
