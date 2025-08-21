using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSettings : UICanvas
{
    [SerializeField] private GameObject[] buttons;

    [SerializeField] private GameObject backButtonGameplay;     // shown in Gameplay
    [SerializeField] private GameObject continueButtonGameplay; // shown in Gameplay
    [SerializeField] private GameObject backButtonMainMenu;     // shown in Main Menu

    public void NextButton()
    {
        Close(0);
    }

    public void SetState(UICanvas canvas)
    {
        if (buttons != null)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null) buttons[i].SetActive(false);
            }
        }

        if (backButtonGameplay != null) backButtonGameplay.SetActive(false);
        if (continueButtonGameplay != null) continueButtonGameplay.SetActive(false);
        if (backButtonMainMenu != null) backButtonMainMenu.SetActive(false);

        if (canvas is CanvasMainMenu)
        {
            if (buttons != null && buttons.Length > 2 && buttons[2] != null)
                buttons[2].SetActive(true);
            else if (backButtonMainMenu != null)
                backButtonMainMenu.SetActive(true);
        }
        else if (canvas is CanvasGamePlay)
        {
            bool shownAny = false;
            if (buttons != null && buttons.Length > 1)
            {
                if (buttons[0] != null) { buttons[0].SetActive(true); shownAny = true; }
                if (buttons[1] != null) { buttons[1].SetActive(true); shownAny = true; }
            }

            if (!shownAny)
            {
                if (backButtonGameplay != null) backButtonGameplay.SetActive(true);
                if (continueButtonGameplay != null) continueButtonGameplay.SetActive(true);
            }
        }
    }
    public void MainMenuButton()
    {
        UIManager.Instance.CloseUIDirectly<CanvasSettings>();
        UIManager.Instance.OpenUI<CanvasMainMenu>();
    }
}
