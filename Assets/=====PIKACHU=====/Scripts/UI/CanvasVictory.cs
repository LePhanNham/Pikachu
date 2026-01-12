using UnityEngine;
using UnityEngine.UI;

public class CanvasVictory : UICanvas
{
    [Header("Buttons")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button homeButton;
    public override void Setup()
    {
        base.Setup();
        
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }

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