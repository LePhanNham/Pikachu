using UnityEngine;
using UnityEngine.UI;

public class CanvasPause : UICanvas
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    public override void Setup()
    {
        base.Setup();
        
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinue);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettings);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenu);
        }
    }

    private void OnContinue()
    {
        Debug.Log("Continue button clicked!");
        if (GameManager.Instance != null)
        {
            Debug.Log($"Game state before resume: {GameManager.Instance.CurrentState}");
            GameManager.Instance.ResumeGame();
            Debug.Log($"Game state after resume: {GameManager.Instance.CurrentState}");
        }
        else
        {
            Debug.LogError("GameManager.Instance is null!");
        }
        UIManager.Instance.CloseUIDirectly<CanvasPause>();
    }

    private void OnSettings()
    {
        var settings = UIManager.Instance.OpenUI<CanvasSettings>();
        settings.SetState(this);
    }

    private void OnMainMenu()
    {
        UIManager.Instance.CloseUIDirectly<CanvasPause>();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadHome();
        }
    }
}


