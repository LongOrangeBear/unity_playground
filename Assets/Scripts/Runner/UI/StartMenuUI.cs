using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Start menu UI.
/// Updated to use TextMeshPro and animations.
/// </summary>
public class StartMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _instructionsText;
    [SerializeField] private Button _startButton;
    
    private void Awake()
    {
        // Default to shown - this is the start menu
        if (_panel != null)
            _panel.SetActive(true);
    }
    
    private void Start()
    {
        if (_startButton != null)
            _startButton.onClick.AddListener(OnStartClicked);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
            // Sync with current state in case GameManager started before us
            OnGameStateChanged(GameManager.Instance.CurrentState);
        }
        
        // Start Animations
        if (_panel != null && _panel.activeSelf)
        {
            AnimateIntro();
        }
    }
    
    private void AnimateIntro()
    {
        // Pulse the start button continuously
        if (_startButton != null)
        {
            // Simple approach: Start a coroutine that loops pulse
            StartCoroutine(PulseStartButtonLoop());
        }
        
        // Slide in title
        if (_titleText != null)
        {
            StartCoroutine(UIAnimator.ScaleIn(_titleText.transform, 1.0f));
        }
    }
    
    private System.Collections.IEnumerator PulseStartButtonLoop()
    {
        while (_startButton != null && _startButton.gameObject.activeInHierarchy)
        {
            yield return UIAnimator.Pulse(_startButton.transform, 1.5f, 1.1f);
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        
        if (_startButton != null)
            _startButton.onClick.RemoveListener(OnStartClicked);
    }
    
    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (_panel == null) return;
        
        if (state == GameManager.GameState.Menu)
        {
            _panel.SetActive(true);
            AnimateIntro();
        }
        else
        {
            _panel.SetActive(false);
        }
    }
    
    private void OnStartClicked()
    {
        GameManager.Instance?.StartGame();
    }
}
