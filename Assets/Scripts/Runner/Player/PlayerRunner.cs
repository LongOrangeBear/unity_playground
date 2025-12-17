using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Main player controller for endless runner.
/// Handles forward movement, lane switching, jump, and slide.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerRunner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunnerSettings _settings;
    
    private CharacterController _controller;
    private Vector3 _velocity;
    private float _targetX;
    private int _currentLane = 0; // -1 = left, 0 = center, 1 = right
    
    // Jump state
    private bool _isGrounded;
    private float _verticalVelocity;
    
    // Slide state
    // State properties
    public bool IsSliding => _isSliding;
    
    private bool _isSliding;
    private float _slideTimer;
    private float _originalHeight;
    private Vector3 _originalCenter;
    
    // Input
    private float _horizontalInput;
    private bool _jumpPressed;
    private bool _slidePressed;
    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _originalHeight = _controller.height;
        _originalCenter = _controller.center;
    }
    
    private void Update()
    {
        if (GameManager.Instance == null || 
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }
        
        // Tick down invincibility
        if (_invincibilityTimer > 0)
            _invincibilityTimer -= Time.deltaTime;
        
        ReadInput();
        HandleLaneSwitch();
        HandleJump();
        HandleSlide();
        ApplyMovement();
    }
    
    private void ReadInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Horizontal input for lane switching
        _horizontalInput = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            _horizontalInput = -1f;
        else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            _horizontalInput = 1f;
        
        // Jump
        _jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
        
        // Slide
        _slidePressed = keyboard.leftCtrlKey.isPressed || keyboard.sKey.wasPressedThisFrame;
    }
    
    private void HandleLaneSwitch()
    {
        // Tap to switch lanes
        var keyboard = Keyboard.current;
        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            _currentLane = Mathf.Max(_currentLane - 1, -1);
        }
        else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            _currentLane = Mathf.Min(_currentLane + 1, 1);
        }
        
        // Calculate target X based on lane
        _targetX = _currentLane * _settings.laneWidth;
        
        // Allow free movement within bounds when holding
        if (Mathf.Abs(_horizontalInput) > 0.1f)
        {
            _targetX += _horizontalInput * _settings.laneWidth * 0.5f;
        }
        
        // Clamp to max bounds
        _targetX = Mathf.Clamp(_targetX, -_settings.maxX, _settings.maxX);
    }
    
    private void HandleJump()
    {
        // Ground check
        _isGrounded = _controller.isGrounded;
        
        if (_isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f; // Small negative to keep grounded
        }
        
        // Jump input
        if (_jumpPressed && _isGrounded && !_isSliding)
        {
            _verticalVelocity = _settings.jumpForce;
            VFXManager.Instance?.PlayJump();
        }
        
        // Apply gravity
        _verticalVelocity += Physics.gravity.y * _settings.gravityMultiplier * Time.deltaTime;
    }
    
    private void HandleSlide()
    {
        if (_slidePressed && _isGrounded && !_isSliding)
        {
            StartSlide();
        }
        
        if (_isSliding)
        {
            _slideTimer -= Time.deltaTime;
            if (_slideTimer <= 0)
            {
                EndSlide();
            }
        }
    }
    
    private void StartSlide()
    {
        _isSliding = true;
        _slideTimer = _settings.slideDuration;
        
        // Calculate bottom position from original settings
        // This ensures the bottom of the collider stays at the same relative Y, 
        // regardless of where the pivot is (Feet vs Center).
        float bottomY = _originalCenter.y - (_originalHeight / 2f);
        
        // Apply new height
        float newHeight = _settings.slideColliderHeight;
        _controller.height = newHeight;
        
        // Calculate new center to keep bottom aligned
        // Center = Bottom + HalfHeight
        float newCenterY = bottomY + (newHeight / 2f);
        
        _controller.center = new Vector3(_originalCenter.x, newCenterY, _originalCenter.z);
        
        Debug.Log($"[PlayerRunner] SLIDE! Original: H={_originalHeight} C={_originalCenter} | New: H={newHeight} C={_controller.center}");
    }
    
    private void EndSlide()
    {
        _isSliding = false;
        
        // Restore collider
        _controller.height = _originalHeight;
        _controller.center = _originalCenter;
        
        // Force update position if needed to prevent getting stuck
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.2f))
        {
             // Optional: move up slightly if embedded? 
             // Logic: CharacterController usually handles restoration well if we don't penetrate.
        }
    }
    
    private void ApplyMovement()
    {
        float currentSpeed = GameManager.Instance.EffectiveSpeed;
        
        // Forward movement (Z axis)
        float forwardMove = currentSpeed;
        
        // Horizontal movement (X axis) - smooth lerp to target
        float currentX = transform.position.x;
        float newX = Mathf.MoveTowards(currentX, _targetX, _settings.laneSwitchSpeed * Time.deltaTime);
        float horizontalMove = (newX - currentX) / Time.deltaTime;
        
        // Combine movement
        Vector3 move = new Vector3(horizontalMove, _verticalVelocity, forwardMove);
        _controller.Move(move * Time.deltaTime);
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check for obstacle collision - only die if specifically tagged
        if (hit.gameObject.CompareTag("Obstacle") || hit.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"[PlayerRunner] Hit obstacle: {hit.gameObject.name}");
            Die(hit.collider);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("Enemy"))
        {
            Die(other);
        }
        else if (other.CompareTag("Coin"))
        {
            CollectCoin(other.gameObject);
        }
        else if (other.CompareTag("PowerUp"))
        {
            CollectPowerUp(other.gameObject);
        }
    }
    
    // Invincibility after shield use
    private float _invincibilityTimer;
    private const float INVINCIBILITY_DURATION = 1.0f; // 1 second to pass through
    
    private void Die(Collider obstacleCollider = null)
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        
        // Still invincible from shield?
        if (_invincibilityTimer > 0)
        {
            return; // Silent - no log spam
        }
        
        // Check for shield first
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.UseShield())
        {
            Debug.Log("[PlayerRunner] Shield absorbed hit!");
            VFXManager.Instance?.PlayShieldBreak(transform.position);
            _invincibilityTimer = INVINCIBILITY_DURATION;
            
            // Disable the obstacle collider so player can pass through
            if (obstacleCollider != null)
            {
                obstacleCollider.enabled = false;
                Debug.Log($"[PlayerRunner] Disabled collider on {obstacleCollider.gameObject.name}");
            }
            
            return; // Shield saved us!
        }
        
        Debug.Log("[PlayerRunner] Died!");
        VFXManager.Instance?.PlayDeathEffect(transform.position);
        CameraEffects.Instance?.ShakeDeath();
        GameManager.Instance.GameOver();
    }
    
    private void CollectCoin(GameObject coin)
    {
        ScoreManager.Instance?.AddCoin();
        coin.SetActive(false); // Return to pool later
    }
    
    private void CollectPowerUp(GameObject powerUp)
    {
        // Will be handled by PowerUpManager
        powerUp.SetActive(false);
    }
}
