using System;
using UnityEngine;
using UnityEngine.UI;

public class CanvasSettings : UICanvas
{
    [Header("Audio Controls")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle musicToggle;
    [Space(5)]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle sfxToggle;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    private Action onBackCallback;

    public override void Setup()
    {
        base.Setup();
        InitializeAudioEvents();
        InitializeNavigationEvents();
        RefreshUIValues();
    }


    public void ConfigureBackNavigation(UICanvas previousCanvas)
    {
        // Define behavior based on who opened us
        if (previousCanvas is CanvasMainMenu)
        {
            onBackCallback = () => UIManager.Instance.OpenUI<CanvasMainMenu>();
        }
        else if (previousCanvas is CanvasPause)
        {
            onBackCallback = () => UIManager.Instance.OpenUI<CanvasPause>();
        }
        else
        {
            // Default fallback (e.g., if opened from Gameplay without pause)
            onBackCallback = () => 
            {
                if (GameManager.Instance.CurrentState == GameState.Paused)
                    UIManager.Instance.OpenUI<CanvasPause>();
                else
                    Close(0f);
            };
        }
    }

    // Maintained for backward compatibility with your existing SetState calls
    public void SetState(UICanvas canvas) => ConfigureBackNavigation(canvas);

    #region Initialization & UI Updates

    private void InitializeAudioEvents()
    {
        BindSlider(musicSlider, SetMusicVolume);
        BindSlider(sfxSlider, SetSfxVolume);
        BindToggle(musicToggle, SetMusicMute);
        BindToggle(sfxToggle, SetSfxMute);
    }

    private void InitializeNavigationEvents()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    private void RefreshUIValues()
    {
        if (SoundManager.Instance == null) return;

        UpdateSlider(musicSlider, SoundManager.Instance.musicVolume);
        UpdateSlider(sfxSlider, SoundManager.Instance.sfxVolume);
        UpdateToggle(musicToggle, !SoundManager.Instance.muteMusic);
        UpdateToggle(sfxToggle, !SoundManager.Instance.muteSfx);
    }

    #endregion

    #region Event Handlers

    private void SetMusicVolume(float value)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.musicVolume = value;
        SoundManager.Instance.ApplyVolumes();
    }

    private void SetSfxVolume(float value)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.sfxVolume = value;
        SoundManager.Instance.ApplyVolumes();
    }

    private void SetMusicMute(bool isOn)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.muteMusic = !isOn; 
        SoundManager.Instance.ApplyVolumes();
    }

    private void SetSfxMute(bool isOn)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.muteSfx = !isOn;
        SoundManager.Instance.ApplyVolumes();
    }

    private void OnBackClicked()
    {
        UIManager.Instance.CloseUIDirectly<CanvasSettings>();
        onBackCallback?.Invoke();
    }

    #endregion

    #region Helper Methods

    private void BindSlider(Slider slider, UnityEngine.Events.UnityAction<float> action)
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(action);
        }
    }

    private void BindToggle(Toggle toggle, UnityEngine.Events.UnityAction<bool> action)
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(action);
        }
    }

    private void UpdateSlider(Slider slider, float value)
    {
        if (slider != null) slider.value = value;
    }

    private void UpdateToggle(Toggle toggle, bool isOn)
    {
        if (toggle != null) toggle.isOn = isOn;
    }

    #endregion
    public void BackButton() => OnBackClicked();
}