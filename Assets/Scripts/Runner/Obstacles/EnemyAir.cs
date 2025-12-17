using UnityEngine;

/// <summary>
/// Air enemy that flies at head height.
/// </summary>
public class EnemyAir : EnemyBase
{
    [Header("Flight")]
    [SerializeField] private float _floatAmplitude = 0.5f;
    [SerializeField] private float _floatFrequency = 2f;
    [SerializeField] private float _horizontalDrift = 1f;
    
    private Vector3 _startPosition;
    private float _timeOffset;
    
    private void Start()
    {
        _startPosition = transform.position;
        _timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }
    
    private void Update()
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing) return;
        
        float time = Time.time + _timeOffset;
        
        // Bobbing up and down
        float yOffset = Mathf.Sin(time * _floatFrequency) * _floatAmplitude;
        
        // Slight horizontal drift
        float xOffset = Mathf.Sin(time * _floatFrequency * 0.5f) * _horizontalDrift;
        
        transform.position = _startPosition + new Vector3(xOffset, yOffset, 0);
    }
    
    public override void Reset()
    {
        _timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }
}
