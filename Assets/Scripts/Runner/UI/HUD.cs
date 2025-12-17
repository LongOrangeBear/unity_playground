using UnityEngine;
using UnityEngine.UI;
using TMPro; // Added TMP
using System.Text;

/// <summary>
/// In-game HUD showing score, coins, and power-ups.
/// Updated to use TextMeshPro and simple animations.
/// </summary>
public class HUD : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private TextMeshProUGUI _distanceText;
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private TextMeshProUGUI _powerUpTimerText;
    
    [Header("Power-Up Icons")]
    [SerializeField] private GameObject _magnetIcon;
    [SerializeField] private GameObject _shieldIcon;
    [SerializeField] private GameObject _doubleScoreIcon;
    [SerializeField] private GameObject _speedBoostIcon;
    
    private StringBuilder _sb = new StringBuilder();
    private int _lastCoins = -1;
    
    // Animation Flags
    private bool _scoreAnimated = false;

    private void Start()
    {
        // Initial setup if needed
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        
        UpdateTexts();
        UpdatePowerUpIcons();
        UpdatePowerUpTimers();
    }
    
    private void UpdateTexts()
    {
        if (ScoreManager.Instance != null)
        {
            int currentScore = ScoreManager.Instance.CurrentScore;
            int currentCoins = ScoreManager.Instance.CoinsCollected;

            if (_scoreText != null)
                _scoreText.text = $"SCORE: {currentScore}";
            
            if (_coinsText != null)
            {
                _coinsText.text = $"x {currentCoins}";
                
                // Pop animation on coin change
                if (_lastCoins != -1 && currentCoins > _lastCoins)
                {
                    UIAnimator.Start(this, UIAnimator.Pulse(_coinsText.transform, 0.2f, 1.2f));
                }
                _lastCoins = currentCoins;
            }
        }
        
        if (_distanceText != null)
            _distanceText.text = $"{Mathf.FloorToInt(GameManager.Instance.DistanceTraveled)}m";
        
        if (_speedText != null)
            _speedText.text = $"{GameManager.Instance.CurrentSpeed:F1} m/s";
    }
    
    private void UpdatePowerUpIcons()
    {
        if (PowerUpManager.Instance == null) return;
        
        // This is a bit brute-force every frame, but fine for MVP
        SetIconActive(_magnetIcon, PowerUpManager.Instance.HasMagnet);
        SetIconActive(_shieldIcon, PowerUpManager.Instance.HasShield);
        SetIconActive(_doubleScoreIcon, PowerUpManager.Instance.HasDoubleScore);
        SetIconActive(_speedBoostIcon, PowerUpManager.Instance.HasSpeedBoost);
    }
    
    private void SetIconActive(GameObject icon, bool isActive)
    {
        if (icon == null) return;
        
        // Only trigger animation if state changes from inactive to active
        if (isActive && !icon.activeSelf)
        {
            icon.SetActive(true);
            UIAnimator.Start(this, UIAnimator.Pulse(icon.transform, 0.5f, 1.5f));
        }
        else if (!isActive && icon.activeSelf)
        {
            icon.SetActive(false);
        }
    }
    
    private void UpdatePowerUpTimers()
    {
        if (_powerUpTimerText == null || PowerUpManager.Instance == null) return;
        
        _sb.Clear();
        
        float magnetTime = PowerUpManager.Instance.GetRemainingTime(PowerUpType.Magnet);
        float doubleTime = PowerUpManager.Instance.GetRemainingTime(PowerUpType.DoubleScore);
        float speedTime = PowerUpManager.Instance.GetRemainingTime(PowerUpType.SpeedBoost);
        bool hasShield = PowerUpManager.Instance.HasShield;
        
        if (magnetTime > 0)
            _sb.AppendLine($"<color=blue>MAGNET: {magnetTime:F1}s</color>");
        if (doubleTime > 0)
            _sb.AppendLine($"<color=green>2x SCORE: {doubleTime:F1}s</color>");
        if (speedTime > 0)
            _sb.AppendLine($"<color=yellow>SPEED: {speedTime:F1}s</color>");
        if (hasShield)
            _sb.AppendLine($"<color=white>SHIELD: ACTIVE</color>");
        
        _powerUpTimerText.text = _sb.ToString();
    }
}
