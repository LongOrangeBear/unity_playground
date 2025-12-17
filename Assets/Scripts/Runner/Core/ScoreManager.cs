using UnityEngine;

/// <summary>
/// Tracks score from distance and coins
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    public int CurrentScore { get; private set; }
    public int HighScore { get; private set; }
    public int CoinsCollected { get; private set; }
    
    private const string HIGH_SCORE_KEY = "HighScore";
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        HighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }
    
    private void Start()
    {
        GameManager.Instance.OnStateChanged += OnGameStateChanged;
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
    }
    
    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            // Score from distance
            int distanceScore = Mathf.FloorToInt(GameManager.Instance.DistanceTraveled) 
                * GameManager.Instance.Settings.pointsPerMeter;
            CurrentScore = distanceScore + (CoinsCollected * GameManager.Instance.Settings.pointsPerCoin);
        }
    }
    
    public void AddCoin()
    {
        CoinsCollected++;
    }
    
    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.GameOver)
        {
            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, HighScore);
                PlayerPrefs.Save();
                Debug.Log($"[ScoreManager] New High Score: {HighScore}");
            }
        }
        else if (state == GameManager.GameState.Playing)
        {
            CurrentScore = 0;
            CoinsCollected = 0;
        }
    }
}
