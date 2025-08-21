using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasGamePlay : UICanvas
{
    [Header("Optional UI")]
    [SerializeField] private Image timerFill;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button shuffleButton;
    [SerializeField] private Button settingsButton;

    public override void Setup()
    {
        base.Setup();

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(OnPauseClicked);
        }
        if (hintButton != null)
        {
            hintButton.onClick.RemoveAllListeners();
            hintButton.onClick.AddListener(OnHintClicked);
        }
        if (shuffleButton != null)
        {
            shuffleButton.onClick.RemoveAllListeners();
            shuffleButton.onClick.AddListener(OnShuffleClicked);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }
    }

    private void Update()
    {
        if (timerFill != null && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            timerFill.fillAmount = GameManager.Instance.GetTimeProgress();
        }
    }

    private void OnPauseClicked()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.PauseGame();
            UIManager.Instance.OpenUI<CanvasPause>();
        }
        else if (GameManager.Instance.CurrentState == GameState.Paused)
        {
            UIManager.Instance.CloseUIDirectly<CanvasPause>();
            GameManager.Instance.ResumeGame();
        }
    }

    private void OnSettingsClicked()
    {
        var settings = UIManager.Instance.OpenUI<CanvasSettings>();
        settings.SetState(this);
    }

    private void OnHintClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.CanUseHint())
        {
            GameManager.Instance.UseHint();
            BoardManager.Instance.AutoSelectBestPair();
            SoundManager.Instance.Click();
        }
    }

    private void OnShuffleClicked()
    {
        BoardManager.Instance.ShuffleBoard();
        SoundManager.Instance.Click();
    }

    public void PauseButton() {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Paused) {
            text.text = "Continue";
            GameManager.Instance.ResumeGame();
        } else {
            text.text = "Pause";
            GameManager.Instance.PauseGame();
        }
    }
}
