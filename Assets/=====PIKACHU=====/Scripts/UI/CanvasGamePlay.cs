using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasGamePlay : UICanvas
{
    [Header("HUD Elements")]
    [SerializeField] private Image timerFill;
    [SerializeField] private TextMeshProUGUI levelText; // Renamed from 'text' for clarity

    [Header("Interactions")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button shuffleButton;

    public override void Setup()
    {
        base.Setup();
        
        // Use helper to bind buttons (cleaner Setup)
        BindButton(pauseButton, OnPauseClicked);
        BindButton(settingsButton, OnSettingsClicked);
        BindButton(hintButton, OnHintClicked);
        BindButton(shuffleButton, OnShuffleClicked);

        // Initial UI State
        RefreshLevelDisplay();
    }

    private void Update()
    {
        // Only update timer if game is actively playing
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            UpdateTimerVisuals();
        }
    }

    #region Visual Updates

    private void UpdateTimerVisuals()
    {
        if (timerFill != null)
        {
            timerFill.fillAmount = GameManager.Instance.GetTimeProgress();
            
            if (timerFill.fillAmount < 0.2f) 
                timerFill.color = Color.red;
            else 
                timerFill.color = Color.white; 
        }
    }

    private void RefreshLevelDisplay()
    {
        if (levelText != null && GameManager.Instance != null)
        {
            // levelText.text = $"Level {GameManager.Instance.CurrentLevel}";
        }
    }

    #endregion

    #region Button Actions

    private void OnPauseClicked()
    {
        if (GameManager.Instance == null) return;

        // Determine action based on state
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.PauseGame();
            UIManager.Instance.OpenUI<CanvasPause>();
        }
        else if (GameManager.Instance.CurrentState == GameState.Paused)
        {
            // Edge case: If user somehow clicks this while paused (usually blocked by Pause Canvas)
            UIManager.Instance.CloseUIDirectly<CanvasPause>();
            GameManager.Instance.ResumeGame();
        }
    }

    private void OnSettingsClicked()
    {
        // Open Settings and pass 'this' so Settings knows to come back here
        var settingsCanvas = UIManager.Instance.OpenUI<CanvasSettings>();
        if (settingsCanvas != null)
        {
            settingsCanvas.ConfigureBackNavigation(this);
        }
    }

    private void OnHintClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.CanUseHint())
        {
            GameManager.Instance.UseHint();
            BoardManager.Instance.AutoSelectBestPair();
            SoundManager.Instance?.Click();
        }
        else
        {
            Debug.Log("Cannot use hint");
        }
    }

    private void OnShuffleClicked()
    {
        // In a real game, you might want to check GameManager.CanUseShuffle() here first
        BoardManager.Instance.ShuffleBoard();
        SoundManager.Instance?.Click();
    }

    #endregion

    #region Helpers

    private void BindButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }

    #endregion
}