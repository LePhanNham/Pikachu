using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasGamePlay : UICanvas
{
    [Header("HUD Elements")]
    [SerializeField] private Slider timerFill;
    [SerializeField] private TextMeshProUGUI levelText; 
    [SerializeField] private TextMeshProUGUI numOfHintText;
    [SerializeField] private TextMeshProUGUI numOfShuffleText;

    [Header("Interactions")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button shuffleButton;

    public override void Setup()
    {
        base.Setup();
        
        // Setup Buttons
        pauseButton.onClick.RemoveAllListeners();
        pauseButton.onClick.AddListener(OnPauseClicked);

        settingsButton.onClick.RemoveAllListeners();
        settingsButton.onClick.AddListener(OnSettingsClicked);

        hintButton.onClick.RemoveAllListeners();
        hintButton.onClick.AddListener(OnHintClicked);

        shuffleButton.onClick.RemoveAllListeners();
        shuffleButton.onClick.AddListener(OnShuffleClicked);
    }

    // Đăng ký nhận sự kiện khi Canvas hiện lên
    private void OnEnable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnTimeChanged += UpdateTimer;
        GameManager.Instance.OnLevelChanged += UpdateLevelDisplay;
        GameManager.Instance.OnHintChanged += UpdateHintDisplay;
        GameManager.Instance.OnShuffleChanged += UpdateShuffleDisplay;

        // Force update lần đầu để tránh UI bị trống
        UpdateLevelDisplay(GameManager.Instance.currentLevel);
        UpdateHintDisplay(GameManager.Instance.GetCurrentHint());
        UpdateShuffleDisplay(GameManager.Instance.GetCurrentShuffle());
    }

    // Hủy đăng ký khi Canvas ẩn đi để tránh lỗi null
    private void OnDisable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnTimeChanged -= UpdateTimer;
        GameManager.Instance.OnLevelChanged -= UpdateLevelDisplay;
        GameManager.Instance.OnHintChanged -= UpdateHintDisplay;
        GameManager.Instance.OnShuffleChanged -= UpdateShuffleDisplay;
    }

    // --- VISUAL UPDATES ---

    private void UpdateTimer(float currentTime, float totalTime)
    {
        if (timerFill != null && totalTime > 0)
        {
            timerFill.value = currentTime / totalTime;
        }
    }

    private void UpdateLevelDisplay(int level)
    {
        if (levelText != null) levelText.text = "LEVEL : " + level;
    }

    private void UpdateHintDisplay(int amount)
    {
        if (numOfHintText != null) numOfHintText.text = amount.ToString();
        // Có thể làm mờ nút Hint nếu amount == 0
        hintButton.interactable = amount > 0;
    }

    private void UpdateShuffleDisplay(int amount)
    {
        if (numOfShuffleText != null) numOfShuffleText.text = amount.ToString();
        shuffleButton.interactable = amount > 0;
    }

    // --- BUTTON ACTIONS ---

    private void OnPauseClicked()
    {
        GameManager.Instance.PauseGame();
        UIManager.Instance.OpenUI<CanvasPause>();
    }

    private void OnSettingsClicked()
    {
        var settings = UIManager.Instance.OpenUI<CanvasSettings>();
        settings.ConfigureBackNavigation(this); 
    }

    private void OnHintClicked()
    {
        // UI chỉ gọi lệnh, Logic để GameManager lo
        GameManager.Instance.UseHint();
    }

    private void OnShuffleClicked()
    {
        GameManager.Instance.UseShuffle();
    }
}