using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadSaveDataManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI LevelText;
    [SerializeField] private TextMeshProUGUI NumOfHint;
    [SerializeField] private TextMeshProUGUI NumOfChanges;
    void OnEnable()
    {

        if (!GameManager.Instance.isContinue)
        {
            PlayerPrefs.SetInt("Level", 0);
            PlayerPrefs.SetInt("Hint", GameManager.Instance.levelDatas[0].Suggestions);
            PlayerPrefs.SetInt("Changes", GameManager.Instance.levelDatas[0].Changes);
        }
        GetText();
    }
    public void LevelUp()
    {
        PlaySound.Instance.PlayClickSound();
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
        Restart();
    }
    public void Hint()
    {
        //if (int.Parse(NumOfHint.text) == 0) return;
        PlayerPrefs.SetInt("Hint", PlayerPrefs.GetInt("Hint") - 1);
        NumOfHint.text = PlayerPrefs.GetInt("Hint").ToString();
    }
    public void Restart()
    {
        int level = PlayerPrefs.GetInt("Level");
        PlayerPrefs.SetInt("Hint", (GameManager.Instance.levelDatas[(level >= 8) ? 7 : (level - 1)].Suggestions));
        PlayerPrefs.SetInt("Changes", (GameManager.Instance.levelDatas[(level >= 8) ? 7 : (level - 1)].Changes));
        PlayerPrefs.SetInt("Time", (GameManager.Instance.levelDatas[(level >= 8) ? 7 : (level - 1)].timeLimit));
        GetText();
    }
    public void Lose()
    {
        int level = PlayerPrefs.GetInt("Level");
        PlayerPrefs.SetInt("Time", GameManager.Instance.levelDatas[(level <= 8 && level >= 1) ? level - 1 : 8].timeLimit);
        PlayerPrefs.SetInt("Hint", (GameManager.Instance.levelDatas[(level >= 8) ? 7 : (level - 1)].Suggestions));
        PlayerPrefs.SetInt("Changes", (GameManager.Instance.levelDatas[(level >= 8) ? 7 : (level - 1)].Changes));
    }
    public void Changes()
    {
        //if (int.Parse(NumOfChanges.text) == 0) return;
        PlayerPrefs.SetInt("Changes", PlayerPrefs.GetInt("Changes") - 1);
        //Debug.LogError(PlayerPrefs.GetInt("Changes"));
        NumOfChanges.text = PlayerPrefs.GetInt("Changes").ToString();
    }
    void GetText()
    {
        NumOfHint.text = PlayerPrefs.GetInt("Hint").ToString();
        NumOfChanges.text = PlayerPrefs.GetInt("Changes").ToString();
        LevelText.text = "LEVEL : " + PlayerPrefs.GetInt("Level").ToString();
    }
    public void GetIAP(bool isChange)
    {
        int current = (isChange) ? PlayerPrefs.GetInt("Changes") : PlayerPrefs.GetInt("Hint");
        current += 1;
        PlayerPrefs.SetInt((isChange) ? "Changes" : "Hint", current);
        if (isChange) {
            NumOfChanges.text = current.ToString(); 
        } else {
            NumOfHint.text = current.ToString(); 
        }
    }
}