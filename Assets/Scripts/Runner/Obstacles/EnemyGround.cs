using UnityEngine;

/// <summary>
/// Ground enemy that patrols within a lane.
/// </summary>
public class EnemyGround : EnemyBase
{
    [Header("Patrol")]
    [SerializeField] private float _patrolRange = 2f;
    
    private Vector3 _startPosition;
    
    private void Start()
    {
        _startPosition = transform.position;
    }
    
    private void Update()
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing) return;
        
        // Smooth side-to-side patrol using PingPong
        float offset = Mathf.PingPong(Time.time * _moveSpeed, _patrolRange * 2f) - _patrolRange;
        transform.position = new Vector3(
            _startPosition.x + offset, 
            transform.position.y, 
            transform.position.z
        );
    }
    
    public override void Reset()
    {
        _startPosition = transform.position;
    }
}
