using UnityEngine;
using UnityEngine.UI;

public class CanvasDefeat : UICanvas
{


    // Gọi khi mở CanvasDefeat
    public override void Setup()
    {
        base.Setup();
    }


    public void RetryButton()
    {
        // Load lại màn chơi
        UIManager.Instance.CloseAll();
        GameManager.Instance.RestartLevel();
    }

    public void HomeButton()
    {
        // Về menu chính
        UIManager.Instance.CloseAll();
        GameManager.Instance.LoadHome();
    }
}
