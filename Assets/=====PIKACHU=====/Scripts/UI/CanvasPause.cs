using UnityEngine;
using UnityEngine.UI;

public class CanvasPause : UICanvas
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button mainMenuButton;

    public override void Setup()
    {
        base.Setup();
        
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinue);
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
            GameManager.Instance.ResumeGame();
        }
        else
        {
            Debug.LogError("GameManager.Instance is null!");
        }
        UIManager.Instance.CloseUIDirectly<CanvasPause>();
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


