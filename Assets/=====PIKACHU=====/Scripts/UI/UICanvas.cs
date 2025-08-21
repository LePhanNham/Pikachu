using System.Collections;
using UnityEngine;

public class UICanvas : MonoBehaviour
{
    [SerializeField] bool isDestroyOnClose = false;
    private void Awake()
    {
        // xu ly tai tho
        RectTransform rect = GetComponent<RectTransform>();
        float ratio = (float)Screen.width / (float)Screen.height;
        if (ratio > 2.1f)
        {
            Vector2 leftBottom = rect.offsetMin;
            Vector2 rightTop = rect.offsetMax;
            
            leftBottom.y = 0;
            rightTop.y = -100;
            
            rect.offsetMin = leftBottom;
            rect.offsetMax = rightTop;
        }
    }

    // goi truoc khi canvas duoc active
    public virtual void Setup()
    {
        
    }
    // goi sau khi active
    public virtual void Open()
    {
        gameObject.SetActive(true);
    }
    // tat canvas sau t (s)
    public virtual void Close(float time)
    {
        if (time > 0f)
        {
            StartCoroutine(CloseAfterDelay(time));
            return;
        }
        DoClose();
    }

    private void DoClose()
    {
        if (isDestroyOnClose)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator CloseAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        DoClose();
    }
    
}
