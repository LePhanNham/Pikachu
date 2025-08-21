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
            // Open settings as pause menu
            var settings = UIManager.Instance.OpenUI<CanvasSettings>();
            settings.SetState(this);
        }
        else if (GameManager.Instance.CurrentState == GameState.Paused)
        {
            UIManager.Instance.CloseUIDirectly<CanvasSettings>();
            GameManager.Instance.ResumeGame();
        }
    }

    private void OnHintClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.CanUseHint())
        {
            GameManager.Instance.UseHint();
            BoardManager.Instance.AutoSelectBestPair();
        }
    }

    private void OnShuffleClicked()
    {
        BoardManager.Instance.ShuffleBoard();
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
