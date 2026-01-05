using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasGamePlay : UICanvas
{
    [Header("HUD Elements")]
    [SerializeField] private Slider timerFill;
    [SerializeField] private TextMeshProUGUI levelText; 

    [Header("Interactions")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button shuffleButton;

    public override void Setup()
    {
        base.Setup();
        
        BindButton(pauseButton, OnPauseClicked);
        BindButton(settingsButton, OnSettingsClicked);
        BindButton(hintButton, OnHintClicked);
        BindButton(shuffleButton, OnShuffleClicked);
        RefreshLevelDisplay();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.Playing)
        {
            UpdateTimerVisuals();
        }
    }

    #region Visual Updates

    private void UpdateTimerVisuals()
    {
        if (timerFill != null)
        {
            timerFill.value = GameManager.Instance.GetTimeProgress();
        }
    }

    private void RefreshLevelDisplay()
    {
        if (levelText != null && GameManager.Instance != null)
        {
        }
    }

    #endregion

    #region Button Actions

    private void OnPauseClicked()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.currentState == GameState.Playing)
        {
            GameManager.Instance.PauseGame();
            UIManager.Instance.OpenUI<CanvasPause>();
        }
        else if (GameManager.Instance.currentState == GameState.Paused)
        {
            UIManager.Instance.CloseUIDirectly<CanvasPause>();
            GameManager.Instance.ResumeGame();
        }
    }

    private void OnSettingsClicked()
    {
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
            GameManager.Instance.OnHintChanged?.Invoke();
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
        if (GameManager.Instance != null && GameManager.Instance.CanUseShuffle())
        GameManager.Instance.OnShuffleChanged?.Invoke();
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