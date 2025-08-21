using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasMainMenu : UICanvas
{
    public void PlayButton()
    {
        Close(0);
        
        // Bắt đầu game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        
        // Mở UI gameplay
        UIManager.Instance.OpenUI<CanvasGamePlay>();
    }

    public void SettingButton()
    {
        UIManager.Instance.OpenUI<CanvasSettings>();
    }
}
