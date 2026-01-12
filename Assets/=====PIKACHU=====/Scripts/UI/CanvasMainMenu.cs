using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class CanvasMainMenu : UICanvas
{
    [Header("Buttons")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnSettings;
    
    private RectTransform m_RectTransform;

    protected override void Awake()
    {
        base.Awake();
        m_RectTransform = GetComponent<RectTransform>();

        btnPlay.onClick.AddListener(OnPlayClicked);
        btnSettings.onClick.AddListener(OnSettingsClicked);
    }

    private void OnPlayClicked()
    {
        Close(0.5f); 
        if (GameManager.Instance != null) GameManager.Instance.StartGame();
        UIManager.Instance.OpenUI<CanvasGamePlay>();
    }

    private void OnSettingsClicked()
    {
        var settings = UIManager.Instance.OpenUI<CanvasSettings>();
        settings.SetState(this); 
    }

    // private void OnQuitClicked()
    // {
    //     Application.Quit();
    //     #if UNITY_EDITOR
    //     UnityEditor.EditorApplication.isPlaying = false;
    //     #endif
    // }
}