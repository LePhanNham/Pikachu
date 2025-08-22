using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    // Game state (not serialized)
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
        // Play menu music
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
        
        Debug.Log("🎮 Bắt đầu game!");
        SoundManager.Instance.PlayMusic(SoundManager.Instance.bgmGameplay);
    }

    public void StartLevel(int level)
    {
        Debug.Log("=== START LEVEL START ===");
        Debug.Log($"StartLevel called for level {level}");
        Debug.Log($"Before StartLevel - totalTime: {totalTime:F1}s, TimeLeft: {TimeLeft:F1}s");
        Debug.Log($"baseTime: {baseTime}, timeReductionPerLevel: {timeReductionPerLevel}");
        
        // Validate values
        if (baseTime <= 0)
        {
            Debug.LogError($"baseTime is invalid: {baseTime}, setting to 20f");
            baseTime = 20f;
        }
        if (timeReductionPerLevel < 0)
        {
            Debug.LogError($"timeReductionPerLevel is invalid: {timeReductionPerLevel}, setting to 5f");
            timeReductionPerLevel = 5f;
        }
        
        CurrentLevel = level;
        currentLevelScore = 0;
        usedHintThisLevel = false;
        
        // Tính thời gian dựa trên level
        float calculatedTime = Mathf.Max(baseTime - (level - 1) * timeReductionPerLevel, 120f);
        Debug.Log($"Calculated time: {calculatedTime:F1}s (baseTime: {baseTime} - (level-1)*{timeReductionPerLevel})");
        
        totalTime = calculatedTime;
        TimeLeft = totalTime;
        
        Debug.Log($"After StartLevel - totalTime: {totalTime:F1}s, TimeLeft: {TimeLeft:F1}s");
        Debug.Log("=== START LEVEL COMPLETED ===");
        
        // Khởi tạo game board cho level mới
        if (BoardManager.Instance != null)
        {
            if (level == 1)
            {
                // Level đầu tiên: khởi tạo board mới
                BoardManager.Instance.InitializeGameBoard();
            }
            else
            {
                // Level tiếp theo: reset board
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
    }

    // ================= SCORE SYSTEM =================
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
        Debug.Log($"💰 +{totalScore} điểm{bonusText}");
        
        // Không tự động qua level bằng điểm. Chỉ thắng khi ăn hết board (BoardManager gọi Victory)
    }

    public void UseHint()
    {
        usedHintThisLevel = true;
        Debug.Log("💡 Đã sử dụng gợi ý");
    }

    // ================= GAME STATE CONTROL =================
    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        
        IsPaused = true;
        CurrentState = GameState.Paused;
        OnGameStateChanged?.Invoke(CurrentState);
        
        Time.timeScale = 0f;
        Debug.Log("⏸️ Game tạm dừng");
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        
        IsPaused = false;
        CurrentState = GameState.Playing;
        OnGameStateChanged?.Invoke(CurrentState);
        
        Time.timeScale = 1f;
        Debug.Log("▶️ Tiếp tục game");
    }

    public void GameOver()
    {
        if (CurrentState != GameState.Playing) return;
        
        IsPlaying = false;
        CurrentState = GameState.GameOver;
        OnGameStateChanged?.Invoke(CurrentState);
        
        Time.timeScale = 1f;
        Debug.Log("⏰ Hết giờ! Game Over!");
        
        UIManager.Instance.OpenUI<CanvasDefeat>();
    }

    public void Victory()
    {
        if (CurrentState != GameState.Playing) return;
        
        IsPlaying = false;
        CurrentState = GameState.Victory;
        OnGameStateChanged?.Invoke(CurrentState);
        
        Debug.Log("🎉 Chiến thắng level!");
        
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
        Debug.Log($"🎯 Hoàn thành Level {CurrentLevel} với {currentLevelScore} điểm!");
        
        if (CurrentLevel >= maxLevel)
        {
            GameComplete();
        }
        else
        {
            // Tự động chuyển level sau 2 giây
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

    // ================= LEVEL CONTROL =================
    public void RestartLevel()
    {
        Debug.Log("=== RESTART LEVEL START ===");
        Debug.Log($"RestartLevel called for level {CurrentLevel}");
        Debug.Log($"Before reset - TimeLeft: {TimeLeft:F1}s, IsPlaying: {IsPlaying}, IsPaused: {IsPaused}");
        Debug.Log($"CurrentState: {CurrentState}");
        
        // Reset game state first
        IsPlaying = true;
        IsPaused = false;
        Time.timeScale = 1f;
        Debug.Log($"After state reset - IsPlaying: {IsPlaying}, IsPaused: {IsPaused}, TimeScale: {Time.timeScale}");
        
        // Clear board and restart level
        if (BoardManager.Instance != null)
        {
            Debug.Log("Clearing board...");
            BoardManager.Instance.ClearBoard();
            Debug.Log("Board cleared successfully");
        }
        else
        {
            Debug.LogError("BoardManager.Instance is null!");
        }
        
        Debug.Log("Calling StartLevel...");
        StartLevel(CurrentLevel);
        Debug.Log($"After StartLevel - TimeLeft: {TimeLeft:F1}s, totalTime: {totalTime:F1}s");
        
        Debug.Log($"After reset - TimeLeft: {TimeLeft:F1}s, IsPlaying: {IsPlaying}, IsPaused: {IsPaused}");
        
        // Close all UI and open gameplay
        UIManager.Instance.CloseAll();
        UIManager.Instance.OpenUI<CanvasGamePlay>();
        
        Debug.Log("=== RESTART LEVEL COMPLETED ===");
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
        
        // Reset game state
        IsPlaying = false;
        IsPaused = false;
        Time.timeScale = 1f;
        
        // Clear board if exists
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ClearBoard();
        }
        
        UIManager.Instance.CloseAll();
        UIManager.Instance.OpenUI<CanvasMainMenu>();
        
        Debug.Log("🏠 Về Main Menu");
        SoundManager.Instance.PlayMusic(SoundManager.Instance.bgmMenu);
    }

    // ================= UTILITY =================
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
