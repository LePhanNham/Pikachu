using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameManager : Singleton<GameManager>
{
    public GameState currentState { get; private set; }
    
    // Runtime Data
    public int currentLevel { get; private set; }
    public float timeLeft { get; private set; }
    public float totalTime { get; private set; } // Cần biến này để tính % slider
    
    private int _currentHint;
    private int _currentShuffle;
    public int GetCurrentHint() => _currentHint;
    public int GetCurrentShuffle() => _currentShuffle;
    public bool isPaused { get; private set; }

    [Header("Level Settings")]
    public List<LevelData> levelDatas;

    public Action<GameState> OnGameStateChanged;
    public Action<float, float> OnTimeChanged; 
    public Action<int> OnLevelChanged;
    public Action<int> OnHintChanged;   
    public Action<int> OnShuffleChanged; 

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        currentState = GameState.MainMenu;
        OnGameStateChanged?.Invoke(currentState);
        SoundManager.Instance?.PlayMusic(SoundManager.Instance.bgmMenu);
    }

    public void StartGame()
    {
        int savedLevel = LoadSaveDataManager.Instance.GetSavedLevel();
        StartLevel(savedLevel);
        currentState = GameState.Playing;
        OnGameStateChanged?.Invoke(currentState);
        SoundManager.Instance?.PlayMusic(SoundManager.Instance.bgmGameplay);
    }

    public void StartLevel(int level)
    {
        currentLevel = level;
        int dataIndex = Mathf.Clamp(currentLevel - 1, 0, levelDatas.Count - 1);
        LevelData data = levelDatas[dataIndex];

        _currentHint = LoadSaveDataManager.Instance.GetSavedHint(data.Suggestions);
        _currentShuffle = LoadSaveDataManager.Instance.GetSavedShuffle(data.Changes);
        
        totalTime = data.timeLimit;
        timeLeft = totalTime;

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ResetGameBoard();
        }

        isPaused = false;
        Time.timeScale = 1f;

        OnLevelChanged?.Invoke(currentLevel);
        OnHintChanged?.Invoke(_currentHint);
        OnShuffleChanged?.Invoke(_currentShuffle);
        OnTimeChanged?.Invoke(timeLeft, totalTime);
    }

    private void Update()
    {
        if (currentState != GameState.Playing || isPaused) return;
        timeLeft -= Time.deltaTime;
        OnTimeChanged?.Invoke(timeLeft, totalTime);
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            GameOver();
        }
    }


    public void UseHint()
    {
        if (CanUseHint())
        {
            _currentHint--;
            BoardManager.Instance?.AutoSelectBestPair();
            LoadSaveDataManager.Instance.SaveHint(_currentHint);
            OnHintChanged?.Invoke(_currentHint);
            
            SoundManager.Instance?.Click();
        }
    }

    public void UseShuffle()
    {
        if (CanUseShuffle())
        {
            _currentShuffle--;
            
            BoardManager.Instance?.ShuffleBoard();
            
            // Lưu Data và Update UI
            LoadSaveDataManager.Instance.SaveShuffle(_currentShuffle);
            OnShuffleChanged?.Invoke(_currentShuffle);
            
            SoundManager.Instance?.Click();
        }
    }

    public bool CanUseHint() => _currentHint > 0 && currentState == GameState.Playing;
    public bool CanUseShuffle() => _currentShuffle > 0 && currentState == GameState.Playing;

    public void PauseGame()
    {
        isPaused = true;
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        OnGameStateChanged?.Invoke(currentState);
    }

    public void ResumeGame()
    {
        isPaused = false;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        OnGameStateChanged?.Invoke(currentState);
    }
    public bool isPlaying { get; set; }
    public void GameOver()
    {
        isPlaying = false;
        currentState = GameState.GameOver;
        UIManager.Instance.OpenUI<CanvasDefeat>();
        OnGameStateChanged?.Invoke(currentState);
    }


    public void RetryLevel()
    {
        UIManager.Instance.CloseAll();
        
        UIManager.Instance.OpenUI<CanvasGamePlay>();
        StartLevel(currentLevel); 
    }

    public void LoadHome()
    {
        currentState = GameState.MainMenu;
        isPaused = false;
        Time.timeScale = 1f;
        
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.ClearBoard();
        }
        
        UIManager.Instance.CloseAll();
        UIManager.Instance.OpenUI<CanvasMainMenu>(); 
        OnGameStateChanged?.Invoke(currentState);
        
        SoundManager.Instance?.PlayMusic(SoundManager.Instance.bgmMenu);
    }

    public void LoadNextLevel()
    {
        int nextLevelIndex = currentLevel + 1;

        if (nextLevelIndex <= levelDatas.Count)
        {
            UIManager.Instance.CloseAll();
            UIManager.Instance.OpenUI<CanvasGamePlay>();
            StartLevel(nextLevelIndex);
        }
        else
        {
            Debug.Log("Game Completed! No more levels.");
            LoadHome();
        }
    }
    public void Victory()
    {
        currentState = GameState.Victory;
        int nextLevel = currentLevel + 1;
        LoadSaveDataManager.Instance.SaveLevel(nextLevel);
        UIManager.Instance.OpenUI<CanvasVictory>();
        OnGameStateChanged?.Invoke(currentState);
    }
    
    public float GetTimeProgress()
    {
        return totalTime > 0 ? timeLeft / totalTime : 0f;
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
