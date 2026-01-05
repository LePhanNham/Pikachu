using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class GameManager : Singleton<GameManager>
{
    public GameState currentState { get; private set; }
    public int currentLevel { get; private set; } = 1;
    public float timeLeft { get; private set; }
    public bool isPlaying { get; private set; }
    public bool isPaused { get; private set; }
    public bool isContinue;

    [Header("Level Settings")]
    [SerializeField] public List<LevelData> levelDatas;
    public int totalTime;
    private int _hint;
    private int _changes;

    public Action<GameState> OnGameStateChanged;
    public Action<float> OnTimeChanged;
    public Action<int> OnLevelChanged;
    public Action OnHintChanged;
    public Action OnShuffleChanged;

    private void OnEnable()
    {
        OnHintChanged += UseHint;
    }

    private void OnDisable()
    {
        OnHintChanged -= UseHint;
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        currentState = GameState.MainMenu;
        currentLevel = 0;
        OnGameStateChanged?.Invoke(currentState);
        SoundManager.Instance.PlayMusic(SoundManager.Instance.bgmMenu);
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        currentLevel = 1;
        isPlaying = true;
        isPaused = false;
        StartLevel(currentLevel);
        OnGameStateChanged?.Invoke(currentState);
        SoundManager.Instance.PlayMusic(SoundManager.Instance.bgmGameplay);
    }

    public void StartLevel(int level)
    {
        currentLevel = level;
        _hint = levelDatas[level].Suggestions;
        _changes = levelDatas[level].Changes;
        timeLeft = levelDatas[currentLevel].timeLimit;
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
        
        OnLevelChanged?.Invoke(currentLevel);
        OnTimeChanged?.Invoke(timeLeft);
    }

    private void Update()
    {
        if (currentState != GameState.Playing || isPaused) return;

        timeLeft -= Time.deltaTime;
        OnTimeChanged?.Invoke(timeLeft);
        
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            GameOver();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            BoardManager.Instance.ClearBoard();
        }
    }
    public void UseHint()
    {
        if (_hint>0)
        {
            _hint--;
        }
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        
        isPaused = true;
        currentState = GameState.Paused;
        OnGameStateChanged?.Invoke(currentState);
        
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        
        isPaused = false;
        currentState = GameState.Playing;
        OnGameStateChanged?.Invoke(currentState);
        
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        if (currentState != GameState.Playing) return;
        isPlaying = false;
        currentState = GameState.GameOver;
        OnGameStateChanged?.Invoke(currentState);
        Time.timeScale = 1f;
        UIManager.Instance.OpenUI<CanvasDefeat>();
    }

    public void Victory()
    {
        if (currentState != GameState.Playing) return;
        
        isPlaying = false;
        currentState = GameState.Victory;
        OnGameStateChanged?.Invoke(currentState);
        
        
        if (currentLevel >= levelDatas.Count)
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
        
        if (currentLevel >= levelDatas.Count)
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
        currentState = GameState.GameComplete;
        OnGameStateChanged?.Invoke(currentState);
        UIManager.Instance.OpenUI<CanvasVictory>();
    }

    public void RestartLevel()
    {
        isPlaying = true;
        isPaused = false;
        Time.timeScale = 1f;
        
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ClearBoard();
        }

        
        StartLevel(currentLevel);
        UIManager.Instance.CloseAll();
        UIManager.Instance.OpenUI<CanvasGamePlay>();
    }

    public void LoadNextLevel()
    {
        if (currentLevel < levelDatas.Count)
        {
            StartLevel(currentLevel + 1);
            UIManager.Instance.CloseUIDirectly<CanvasVictory>();
        }
        else
        {
            LoadHome();
        }
    }

    public void LoadHome()
    {
        currentState = GameState.MainMenu;
        OnGameStateChanged?.Invoke(currentState);
        
        isPlaying = false;
        isPaused = false;
        Time.timeScale = 1f;
        
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ClearBoard();
        }
        
        UIManager.Instance.CloseAll();
        UIManager.Instance.OpenUI<CanvasMainMenu>();
        
        SoundManager.Instance.PlayMusic(SoundManager.Instance.bgmMenu);
    }

    public float GetTimeProgress()
    {
        return totalTime > 0 ? timeLeft / totalTime : 0f;
    }

    public bool CanUseHint()
    {
        return _hint>0 && currentState == GameState.Playing;
    }
    public bool CanUseShuffle()
    {
        return _changes>0 && currentState == GameState.Playing;
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
