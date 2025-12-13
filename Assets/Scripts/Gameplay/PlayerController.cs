using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 50f;
    
    private Rigidbody _rb;
    private Vector3 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Auto-adjust speed based on level size (default 20 units, so speed ~50 gives good feel)
        if (_moveSpeed < 10f)
            _moveSpeed = 50f;
    }

    public void Move(Vector3 direction)
    {
        _moveInput = direction;
    }

    private void Update()
    {
        // Keyboard fallback (WASD)
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            Vector3 keyInput = Vector3.zero;
            if (keyboard.wKey.isPressed) keyInput.z += 1;
            if (keyboard.sKey.isPressed) keyInput.z -= 1;
            if (keyboard.aKey.isPressed) keyInput.x -= 1;
            if (keyboard.dKey.isPressed) keyInput.x += 1;
            
            if (keyInput != Vector3.zero)
                _moveInput = keyInput.normalized;
        }
    }

    private void FixedUpdate()
    {
        if (_moveInput != Vector3.zero)
        {
            _rb.AddForce(_moveInput * _moveSpeed, ForceMode.Force);
            _moveInput = Vector3.zero;
        }
    }
}