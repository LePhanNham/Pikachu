using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasSettings : UICanvas
{
    [Header("Sound Controls")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle musicToggle; // On = unmuted
    [SerializeField] private Toggle sfxToggle;   // On = unmuted

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    private bool openedOverMainMenu = false;
    private bool openedFromPause = false;

    public override void Setup()
    {
        base.Setup();

        // Wire up events
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }
        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveAllListeners();
            musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        }
        if (sfxToggle != null)
        {
            sfxToggle.onValueChanged.RemoveAllListeners();
            sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }

        SyncUIFromSound();
    }

    // Called by whoever opens settings to inform context
    public void SetState(UICanvas canvas)
    {
        openedOverMainMenu = canvas is CanvasMainMenu;
        // Track if opened from pause menu
        openedFromPause = canvas is CanvasPause;
    }

    private void SyncUIFromSound()
    {
        if (SoundManager.Instance == null) return;
        var sm = SoundManager.Instance;
        if (musicSlider != null) musicSlider.value = sm.musicVolume;
        if (sfxSlider != null) sfxSlider.value = sm.sfxVolume;
        if (musicToggle != null) musicToggle.isOn = !sm.muteMusic;
        if (sfxToggle != null) sfxToggle.isOn = !sm.muteSfx;
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.musicVolume = value;
        SoundManager.Instance.ApplyVolumes();
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.sfxVolume = value;
        SoundManager.Instance.ApplyVolumes();
    }

    private void OnMusicToggleChanged(bool isOn)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.muteMusic = !isOn;
        SoundManager.Instance.ApplyVolumes();
    }

    private void OnSfxToggleChanged(bool isOn)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.muteSfx = !isOn;
        SoundManager.Instance.ApplyVolumes();
    }

    private void OnBackClicked()
    {
        UIManager.Instance.CloseUIDirectly<CanvasSettings>();

        // If opened over main menu, ensure main menu is visible
        if (openedOverMainMenu)
        {
            UIManager.Instance.OpenUI<CanvasMainMenu>();
            return;
        }

        // If opened from pause menu, go back to pause menu
        if (openedFromPause)
        {
            UIManager.Instance.OpenUI<CanvasPause>();
            return;
        }

        // If game is paused but not from pause menu, go back to pause menu
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
        {
            UIManager.Instance.OpenUI<CanvasPause>();
        }
        // If from gameplay, just close settings (game continues)
    }

    // Backward compatibility for UI bindings
    public void BackButton() => OnBackClicked();
    public void NextButton() => Close(0);
    public void MainMenuButton() => OnBackClicked();
}
