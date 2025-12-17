using UnityEngine;

/// <summary>
/// Manages game states: Menu, Playing, GameOver
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum GameState { Menu, Playing, GameOver }
    
    [Header("References")]
    [SerializeField] private RunnerSettings _settings;
    
    [Header("Debug")]
    [SerializeField] private bool _autoStart = false; // Set true for testing without UI
    
    public RunnerSettings Settings => _settings;
    public GameState CurrentState { get; private set; } = GameState.Menu;
    public float CurrentSpeed { get; private set; }
    public float EffectiveSpeed => CurrentSpeed * (PowerUpManager.Instance?.SpeedMultiplier ?? 1f);
    public float DistanceTraveled { get; private set; }
    
    public event System.Action<GameState> OnStateChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        CurrentSpeed = _settings.runSpeed;
        
        if (_autoStart)
        {
            StartGame();
        }
        else
        {
            SetState(GameState.Menu);
        }
    }
    
    private void Update()
    {
        if (CurrentState == GameState.Menu)
        {
            if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                StartGame();
            }
        }
        else if (CurrentState == GameState.GameOver)
        {
            if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                RestartGame();
            }
        }
        else if (CurrentState == GameState.Playing)
        {
            // Update distance using effective speed (includes power-up multiplier)
            DistanceTraveled += EffectiveSpeed * Time.deltaTime;
            
            // Increase speed over distance
            float speedIncrease = (DistanceTraveled / 100f) * _settings.speedIncreaseRate;
            CurrentSpeed = Mathf.Min(_settings.runSpeed + speedIncrease, _settings.maxRunSpeed);
        }
    }
    
    public void StartGame()
    {
        DistanceTraveled = 0f;
        CurrentSpeed = _settings.runSpeed;
        SetState(GameState.Playing);
    }
    
    public void GameOver()
    {
        SetState(GameState.GameOver);
    }
    
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"[GameManager] State: {newState}");
    }
}
