using UnityEngine;
using UnityEngine.UI;
using TMPro; // Added TMP
using System.Collections;

/// <summary>
/// Game Over screen UI.
/// Updated to use TextMeshPro and animations.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _highScoreText;
    [SerializeField] private TextMeshProUGUI _distanceText;
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private CanvasGroup _canvasGroup; // For fading
    
    private void Awake()
    {
        // Subscribe in Awake BEFORE the object might get disabled
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
            _subscribed = true;
        }
        
        // Hide panel immediately to prevent flicker
        if (_panel != null)
            _panel.SetActive(false);
            
        // Get or add CanvasGroup if missing for fade effects
        if (_panel != null)
        {
            if (_canvasGroup == null)
                _canvasGroup = _panel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = _panel.AddComponent<CanvasGroup>();
        }
    }
    
    private void Start()
    {
        if (_restartButton != null)
            _restartButton.onClick.AddListener(OnRestartClicked);
        
        // If we didn't subscribe in Awake (GameManager wasn't ready), try again
        if (GameManager.Instance != null && !_subscribed)
        {
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
            OnGameStateChanged(GameManager.Instance.CurrentState);
            _subscribed = true;
        }
    }
    
    private bool _subscribed = false;
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            
        if (_restartButton != null)
            _restartButton.onClick.RemoveListener(OnRestartClicked);
    }
    
    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (_panel == null) return;
        
        if (state == GameManager.GameState.GameOver)
        {
            // DELAY slightly to let screen shake finish or just look better
            StartCoroutine(ShowSequence());
        }
        else
        {
            _panel.SetActive(false);
        }
    }
    
    private IEnumerator ShowSequence()
    {
        yield return new WaitForSeconds(0.5f); // Waiting for death explosion to settle
        
        _panel.SetActive(true);
        
        // Animate Entry
        if (_canvasGroup != null)
        {
            StartCoroutine(UIAnimator.FadeIn(_canvasGroup, 0.5f));
        }
        
        // Slide Up panel if it has a rect transform
        // Assuming panel is centered, we can punch scale it
        StartCoroutine(UIAnimator.ScaleIn(_panel.transform, 0.4f));

        UpdateUI();
        
        // Pulse the restart button after a moment
        if (_restartButton != null)
        {
            yield return new WaitForSeconds(0.6f);
            StartCoroutine(UIAnimator.Pulse(_restartButton.transform, 0.3f, 1.1f));
        }
    }
    
    private void UpdateUI()
    {
        if (ScoreManager.Instance != null)
        {
            // Start Count-up animation for score
            int finalScore = ScoreManager.Instance.CurrentScore;
            StartCoroutine(ScoreCountUp(finalScore));

            if (_highScoreText != null)
                _highScoreText.text = $"HIGH SCORE: {ScoreManager.Instance.HighScore}";
            
            if (_coinsText != null)
                _coinsText.text = $"COINS: {ScoreManager.Instance.CoinsCollected}";
        }
        
        if (_distanceText != null && GameManager.Instance != null)
            _distanceText.text = $"DISTANCE: {Mathf.FloorToInt(GameManager.Instance.DistanceTraveled)}m";
    }
    
    private IEnumerator ScoreCountUp(int targetScore)
    {
        if (_scoreText == null) yield break;
        
        float duration = 1.0f;
        float elapsed = 0f;
        int startScore = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease Out
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            int current = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, t));
            _scoreText.text = $"SCORE: {current}";
            yield return null;
        }
        _scoreText.text = $"SCORE: {targetScore}";
    }
    
    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }
}
