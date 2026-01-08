using UnityEngine;
using UnityEngine.UI;

public class CanvasVictory : UICanvas
{
    [Header("Buttons")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button homeButton;

    // Có thể thêm Text score hoặc số sao ở đây nếu muốn hiển thị
    // [SerializeField] private TextMeshProUGUI scoreText;

    public override void Setup()
    {
        base.Setup();
        
        // 1. Gán sự kiện cho nút Next
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }

        // 2. Gán sự kiện cho nút Replay (Chơi lại màn vừa thắng để cày sao chẳng hạn)
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        // 3. Gán sự kiện cho nút Home
        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(OnHomeClicked);
        }

        // Hiển thị thông tin (nếu có)
        // ShowVictoryInfo();
    }

    private void OnNextLevelClicked()
    {
        GameManager.Instance.LoadNextLevel();
    }

    private void OnRetryClicked()
    {
        GameManager.Instance.RetryLevel();
    }

    private void OnHomeClicked()
    {
        GameManager.Instance.LoadHome();
    }
}