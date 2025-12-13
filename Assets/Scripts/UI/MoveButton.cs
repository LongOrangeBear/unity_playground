using UnityEngine;
using UnityEngine.EventSystems;

public class MoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Vector3 _moveDirection;
    
    private PlayerController _player;
    private bool _isPressed;

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (_player == null)
            Debug.LogError("[MoveButton] PlayerController not found!");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
    }

    private void Update()
    {
        if (_isPressed && _player != null)
        {
            _player.Move(_moveDirection.normalized);
        }
    }
}