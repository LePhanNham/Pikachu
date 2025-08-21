using UnityEngine;
using UnityEngine.UI;

public class CanvasVictory : UICanvas
{


    public override void Setup()
    {
        base.Setup();
        // ShowVictoryInfo();
    }


    // ================= BUTTON =================
    public void NextLevelButton()
    {
        UIManager.Instance.CloseAll();
        GameManager.Instance.LoadNextLevel();
    }

    public void RetryButton()
    {
        UIManager.Instance.CloseAll();
        GameManager.Instance.RestartLevel();
    }

    public void HomeButton()
    {
        UIManager.Instance.CloseAll();
        GameManager.Instance.LoadHome();
    }
    
}
