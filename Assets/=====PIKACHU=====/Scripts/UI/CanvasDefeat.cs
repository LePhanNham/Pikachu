using UnityEngine;
using UnityEngine.UI;

public class CanvasDefeat : UICanvas
{
    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button homeButton;

    public override void Setup()
    {
        base.Setup();
        
        // Gán sự kiện code-based để an toàn và dễ quản lý
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(OnHomeClicked);
        }
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