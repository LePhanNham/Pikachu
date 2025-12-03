using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public int CurrentScore { get; private set; }
    public float TimeLeft { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsPaused { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private float baseTime = 20f; // 20 giây để test
    [SerializeField] private float timeReductionPerLevel = 5f; // Giảm 5s mỗi level
    [SerializeField] private int scoreTargetPerLevel = 1000; // Điểm cần đạt để qua level
    [SerializeField] private int maxLevel = 10;

    [Header("Scoring")]
    [SerializeField] private int baseScorePerMatch = 100;
    [SerializeField] private int bonusScoreForQuickMatch = 50;
    [SerializeField] private int bonusScoreForNoHint = 25;

    private float totalTime;
    private int currentLevelScore;
    private bool usedHintThisLevel;

    // Events
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<int> OnScoreChanged;
    public System.Action<float> OnTimeChanged;
    public System.Action<int> OnLevelChanged;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        CurrentState = GameState.MainMenu;
        CurrentLevel = 1;
        CurrentScore = 0;
        OnGameStateChanged?.Invoke(CurrentState);
        SoundManager.Instance.PlayMusic(SoundManager.Instance.bgmMenu);
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        CurrentLevel = 1;
        CurrentScore = 0;
        IsPlaying = true;
        IsPaused = false;
        
        StartLevel(CurrentLevel);
        OnGameStateChanged?.Invoke(CurrentState);
        SoundManager.Instance.PlayMusic(SoundManager.Instance.bgmGameplay);
    }

    public void StartLevel(int level)
    {
        if (baseTime <= 0)
        {
            baseTime = 20f;
        }
        if (timeReductionPerLevel < 0)
        {
            timeReductionPerLevel = 5f;
        }
        
        CurrentLevel = level;
        currentLevelScore = 0;
        usedHintThisLevel = false;
        
        float calculatedTime = Mathf.Max(baseTime - (level - 1) * timeReductionPerLevel, 120f);
        
        totalTime = calculatedTime;
        TimeLeft = totalTime;
        

        if (BoardManager.Instance != null)
        {
            if (level == 1)
            {
                BoardManager.Instance.InitializeGameBoard();
            }
            else
            {
                BoardManager.Instance.ResetGameBoard();
            }
        }
        
        OnLevelChanged?.Invoke(CurrentLevel);
        OnTimeChanged?.Invoke(TimeLeft);
        
        Debug.Log($"🚀 Bắt đầu Level {level} - Thời gian: {TimeLeft:F0}s");
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing || IsPaused) return;

        TimeLeft -= Time.deltaTime;
        OnTimeChanged?.Invoke(TimeLeft);
        
        if (TimeLeft <= 0)
        {
            TimeLeft = 0;
            GameOver();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            BoardManager.Instance.ClearBoard();
        }
    }
    public void AddScore(int value, bool isQuickMatch = false, bool usedHint = false)
    {
        int bonus = 0;
        
        if (isQuickMatch) bonus += bonusScoreForQuickMatch;
        if (!usedHint && !usedHintThisLevel) bonus += bonusScoreForNoHint;
        
        int totalScore = value + bonus;
        CurrentScore += totalScore;
        currentLevelScore += totalScore;
        
        OnScoreChanged?.Invoke(CurrentScore);
        
        string bonusText = bonus > 0 ? $" (+{bonus} bonus)" : "";
        
    }

    public void UseHint()
    {
        usedHintThisLevel = true;
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        
        IsPaused = true;
        CurrentState = GameState.Paused;
        OnGameStateChanged?.Invoke(CurrentState);
        
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        
        IsPaused = false;
        CurrentState = GameState.Playing;
        OnGameStateChanged?.Invoke(CurrentState);
        
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        if (CurrentState != GameState.Playing) return;
        
        IsPlaying = false;
        CurrentState = GameState.GameOver;
        OnGameStateChanged?.Invoke(CurrentState);
        
        Time.timeScale = 1f;
        UIManager.Instance.OpenUI<CanvasDefeat>();
    }

    public void Victory()
    {
        if (CurrentState != GameState.Playing) return;
        
        IsPlaying = false;
        CurrentState = GameState.Victory;
        OnGameStateChanged?.Invoke(CurrentState);
        
        
        if (CurrentLevel >= maxLevel)
        {
            GameComplete();
        }
        else
        {
            UIManager.Instance.OpenUI<CanvasVictory>();
        }
    }

    private void LevelComplete()
    {
        
        if (CurrentLevel >= maxLevel)
        {
            GameComplete();
        }
        else
        {
            StartCoroutine(NextLevelAfterDelay(2f));
        }
    }

    private IEnumerator NextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadNextLevel();
    }

    private void GameComplete()
    {
        CurrentState = GameState.GameComplete;
        OnGameStateChanged?.Invoke(CurrentState);
        Debug.Log("🏆 Chúc mừng! Bạn đã hoàn thành tất cả levels!");
        UIManager.Instance.OpenUI<CanvasVictory>();
    }

    public void RestartLevel()
    {
        IsPlaying = true;
        IsPaused = false;
        Time.timeScale = 1f;
        
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ClearBoard();
        }

        
        StartLevel(CurrentLevel);
        UIManager.Instance.CloseAll();
        UIManager.Instance.OpenUI<CanvasGamePlay>();
    }

    public void LoadNextLevel()
    {
        if (CurrentLevel < maxLevel)
        {
            StartLevel(CurrentLevel + 1);
            UIManager.Instance.CloseUIDirectly<CanvasVictory>();
        }
        else
        {
            LoadHome();
        }
    }

    public void LoadHome()
    {
        CurrentState = GameState.MainMenu;
        OnGameStateChanged?.Invoke(CurrentState);
        
        IsPlaying = false;
        IsPaused = false;
        Time.timeScale = 1f;
        
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ClearBoard();
        }
        
        UIManager.Instance.CloseAll();
        UIManager.Instance.OpenUI<CanvasMainMenu>();
        
        Debug.Log("🏠 Về Main Menu");
        SoundManager.Instance.PlayMusic(SoundManager.Instance.bgmMenu);
    }

    public float GetTimeProgress()
    {
        return totalTime > 0 ? TimeLeft / totalTime : 0f;
    }

    public int GetScoreProgress()
    {
        return scoreTargetPerLevel > 0 ? Mathf.Min(currentLevelScore * 100 / scoreTargetPerLevel, 100) : 0;
    }

    public bool CanUseHint()
    {
        return !usedHintThisLevel && CurrentState == GameState.Playing;
    }
}

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Victory,
    GameComplete
}
