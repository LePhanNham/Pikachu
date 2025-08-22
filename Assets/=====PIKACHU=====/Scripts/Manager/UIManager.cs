using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    Dictionary<System.Type, UICanvas> canvasActives = new Dictionary<System.Type, UICanvas>();
    Dictionary<System.Type, UICanvas> canvasPrefabs = new Dictionary<System.Type, UICanvas>();
    [SerializeField] private Transform parent;
    private void Awake()
    {
        // load UI Prefab tu Resources
        UICanvas[] prefabs = Resources.LoadAll<UICanvas>("UI/");
        for (int i = 0; i < prefabs.Length; i++)
        {
            canvasPrefabs.Add(prefabs[i].GetType(), prefabs[i]);
        }
    }

    // mo canvas
    public T OpenUI<T>() where T : UICanvas
    {
        Debug.Log($"UIManager.OpenUI<{typeof(T).Name}> called");
        T canvas =  GetUI<T>();
        if (canvas != null)
        {
            Debug.Log($"Canvas {typeof(T).Name} retrieved, calling Setup() and Open()");
            canvas.Setup();
            canvas.Open();
            Debug.Log($"Canvas {typeof(T).Name} opened successfully");
        }
        else
        {
            Debug.LogError($"Failed to get canvas {typeof(T).Name}");
        }
        return canvas;
    }
    // dong canvas sau t (s)
    public void CloseUI<T>(float time) where T : UICanvas
    {
        if (IsUIOpened<T>())
        {
            canvasActives[typeof(T)].Close(time);
        }
    }
    
    // dong canvas truc tiep
    public void CloseUIDirectly<T>() where T : UICanvas
    {
        if (IsUIOpened<T>())
        {
            canvasActives[typeof(T)].Close(0f);
        }
    }
    
    // kiem tra canvas da duoc tao hay chua
    public bool IsUILoaded<T>() where T : UICanvas
    {
        return canvasActives.ContainsKey(typeof(T)) && canvasActives[typeof(T)] != null;
    }
    
    // kiem tra canvas duoc active hay chua
    public bool IsUIOpened<T>() where T : UICanvas
    {
        return IsUILoaded<T>() && canvasActives[typeof(T)].gameObject.activeSelf;
    }
    
    // lay active canvas
    public T GetUI<T>() where T : UICanvas
    {
        Debug.Log($"GetUI<{typeof(T).Name}> called");
        if (!IsUILoaded<T>())
        {
            Debug.Log($"Canvas {typeof(T).Name} not loaded, creating new instance");
            T prefab = GetUIPrefab<T>();
            if (prefab != null)
            {
                T canvas = Instantiate(prefab, parent);
                canvasActives[typeof(T)] = canvas;
                Debug.Log($"Canvas {typeof(T).Name} instantiated successfully");
            }
            else
            {
                Debug.LogError($"Prefab for {typeof(T).Name} not found!");
                return null;
            }
        }
        else
        {
            Debug.Log($"Canvas {typeof(T).Name} already loaded");
        }
        return canvasActives[typeof(T)] as T;
    }
    private T GetUIPrefab<T>() where T : UICanvas
    {
        Debug.Log($"GetUIPrefab<{typeof(T).Name}> called");
        if (canvasPrefabs.ContainsKey(typeof(T)))
        {
            var prefab = canvasPrefabs[typeof(T)] as T;
            Debug.Log($"Prefab {typeof(T).Name} found: {prefab != null}");
            return prefab;
        }
        else
        {
            Debug.LogError($"Prefab {typeof(T).Name} not found in canvasPrefabs!");
            Debug.Log($"Available prefabs: {string.Join(", ", canvasPrefabs.Keys)}");
            return null;
        }
    }
    // dong tat ca
    public void CloseAll()
    {
        foreach (var canvas in canvasActives)
        {
            if (canvas.Value != null && canvas.Value.gameObject.activeSelf)
            {
                canvas.Value.Close(0);
            }
        }
    }
    
    // kiểm tra xem có UI nào đang mở không
    public bool HasAnyUIOpen()
    {
        foreach (var canvas in canvasActives)
        {
            if (canvas.Value != null && canvas.Value.gameObject.activeSelf)
            {
                // Bỏ qua CanvasGamePlay vì đó là UI chính của gameplay
                if (canvas.Value.GetType() != typeof(CanvasGamePlay))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
