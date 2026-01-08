using UnityEngine;

public class LoadSaveDataManager : Singleton<LoadSaveDataManager>
{
    public int GetSavedLevel()
    {
        return PlayerPrefs.GetInt(GameConstants.KEY_LEVEL, 1);
    }

    public void SaveLevel(int level)
    {
        PlayerPrefs.SetInt(GameConstants.KEY_LEVEL, level);
        PlayerPrefs.Save();
    }

    public int GetSavedHint(int defaultVal = 3)
    {
        return PlayerPrefs.GetInt(GameConstants.KEY_HINT, defaultVal);
    }

    public void SaveHint(int amount)
    {
        PlayerPrefs.SetInt(GameConstants.KEY_HINT, amount);
        PlayerPrefs.Save();
    }

    public int GetSavedShuffle(int defaultVal = 3)
    {
        return PlayerPrefs.GetInt(GameConstants.KEY_SHUFFLE, defaultVal);
    }

    public void SaveShuffle(int amount)
    {
        PlayerPrefs.SetInt(GameConstants.KEY_SHUFFLE, amount);
        PlayerPrefs.Save();
    }
}