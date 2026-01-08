using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class BoardManager : Singleton<BoardManager>
{
    [Header("Grid Settings")]
    public int rows;
    public int cols;
    public float offsetX, offsetY;
    [SerializeField] private float quickMatchThreshold = 3f;

    [Header("Visual Settings")]
    [SerializeField] private float tileScale = 1.2f;
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private Color selectColor = Color.cyan;
    [SerializeField] private Color errorColor = Color.red;

    [Header("Assets")]
    public GameObject[] iconSprite;

    // Core Data
    public NodeData[,] board;
    public GameObject[,] tiles;
    
    // State Data
    private NodeData firstSelect;
    private float lastMatchTime;
    private bool isProcessing; // Chặn input khi đang chạy animation

    private void Start()
    {
        if (PathFinder.Instance != null) PathFinder.Instance.Init(rows, cols);
        DOTween.SetTweensCapacity(1000, 50); // Tăng capacity để an toàn
    }


    #region Init & Reset
    public void InitializeGameBoard() { InitGrid(); CreateBoard(); }

    public void ResetGameBoard() 
    { 
        ClearBoard(); 
        InitializeGameBoard(); 
        firstSelect = null; 
        isProcessing = false;
    }
    
    public void ClearBoard()
    {
        if (tiles == null || board == null) return;
        
        // Kill toàn bộ tween đang chạy để tránh lỗi
        DOTween.KillAll();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (board[r, c] != null) board[r, c].state = NodeState.Empty;
                if (tiles[r, c] != null) Destroy(tiles[r, c]); // Destroy thay vì SetActive để clean sạch scene cũ
            }
        }
        firstSelect = null;
    }

    private void InitGrid()
    {
        board = new NodeData[rows, cols];
        tiles = new GameObject[rows, cols];
        List<NodeType> pool = GenerateNodePool();
        ShuffleList(pool);
        int index = 0;
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                NodeData node = new NodeData { posX = r, posY = c };
                if (r == 0 || r == rows - 1 || c == 0 || c == cols - 1) 
                {
                    node.state = NodeState.Empty;
                }
                else 
                { 
                    node.state = NodeState.Normal; 
                    node.node = pool[index++]; 
                }
                board[r, c] = node;
            }
        }
    }

    private void CreateBoard()
    {
        // Clear children cũ nếu có
        foreach (Transform child in transform) Destroy(child.gameObject);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (board[r, c].state == NodeState.Empty) continue;
                
                Vector3 pos = GetWorldPosition(r, c);
                GameObject tileObj = Instantiate(iconSprite[(int)board[r, c].node], pos, Quaternion.identity, transform);
                
                tileObj.transform.localScale = Vector3.one * tileScale;

                // Setup Component
                Node nodeComp = tileObj.GetComponent<Node>();
                if (nodeComp) 
                { 
                    nodeComp.Init(board[r, c]); 
                    nodeComp.node.tileObject = tileObj; 
                }
                tiles[r, c] = tileObj;
            }
        }
    }

    private List<NodeType> GenerateNodePool() {
        int total = (rows - 2) * (cols - 2);
        List<NodeType> pool = new List<NodeType>();
        // Tạo các cặp icon
        for (int i = 0; i < total / 2; i++) {
            NodeType type = (NodeType)(i % iconSprite.Length);
            pool.Add(type); pool.Add(type);
        }
        return pool;
    }
    
    private void ShuffleList<T>(List<T> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
    #endregion

    #region Gameplay Logic

    public void SelectTile(int row, int col)
    {
        // Chặn input nếu đang xử lý match hoặc game over
        if (isProcessing || GameManager.Instance.currentState != GameState.Playing) return;
        
        if (row < 0 || row >= rows || col < 0 || col >= cols) return;
        
        NodeData current = board[row, col];
        if (current.state != NodeState.Normal) return;

        if (firstSelect == null)
        {
            HandleFirstSelection(current);
        }
        else if (firstSelect == current)
        {
            DeselectAll();
        }
        else
        {
            HandleSecondSelection(current);
        }
    }

    private void HandleFirstSelection(NodeData node)
    {
        firstSelect = node;
        node.state = NodeState.Selected;
        AnimateSelection(node);
        SoundManager.Instance?.Click();
    }

    private void HandleSecondSelection(NodeData second)
    {
        // Tạm khóa input để xử lý logic check
        isProcessing = true;
        second.state = NodeState.Selected;
        
        // Animation click nhẹ cho tile thứ 2
        AnimateClickFeedback(second);

        if (firstSelect.node == second.node && PathFinder.Instance.CanConnect(board, firstSelect, second))
        {
            ProcessMatch(firstSelect, second);
        }
        else
        {
            ProcessMiss(firstSelect, second);
        }
    }

    private void ProcessMatch(NodeData nodeA, NodeData nodeB)
    {
        int turns = PathFinder.Instance.GetPathTurns(board, nodeA, nodeB);
        // Combo calculation
        
        lastMatchTime = Time.time;
        SoundManager.Instance?.Match();
        
        StartCoroutine(RoutineMatchSequence(nodeA, nodeB));
    }

    private void ProcessMiss(NodeData nodeA, NodeData nodeB)
    {
        SoundManager.Instance?.NoMove();

        // Rung lắc báo sai
        AnimateMiss(nodeA, nodeB, () => {
            ResetTileVisuals(nodeA);
            ResetTileVisuals(nodeB);
            firstSelect = null;
            isProcessing = false; // Mở khóa input
        });
    }

    private IEnumerator RoutineMatchSequence(NodeData nodeA, NodeData nodeB)
    {
        // 1. Draw Path
        var path = PathFinder.Instance.GetPath(board, nodeA, nodeB);
        PathFinder.Instance.DrawDebugPath(path);
        
        yield return new WaitForSeconds(0.15f); 

        // 2. Animate Disappear
        yield return StartCoroutine(AnimateMatchDisappear(nodeA, nodeB));

        // 3. Logic Cleanup
        PathFinder.Instance.ClearDebugPath();
        HideTile(nodeA);
        HideTile(nodeB);
        firstSelect = null;

        // 4. Check Win Condition (Chuyển logic check từ Update về đây)
        CheckGameStatus();
    }

    private void CheckGameStatus()
    {
        if (IsBoardCleared())
        {
            isProcessing = false;
            GameManager.Instance.Victory();
        }
        else if (!HasAnyConnectablePair())
        {
            // Auto shuffle nếu hết nước đi
            StartCoroutine(ShuffleBoardWithEffect());
        }
        else
        {
            isProcessing = false; // Mở khóa input để chơi tiếp
        }
    }

    private IEnumerator ShuffleBoardWithEffect()
    {
        isProcessing = true; // Khóa input khi shuffle

        List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        foreach(var t in tiles) {
            if(t != null && t.activeSelf) renderers.Add(t.GetComponent<SpriteRenderer>());
        }

        // Fade out
        foreach(var sr in renderers) sr.DOFade(0f, 0.4f);
        yield return new WaitForSeconds(0.4f);

        // Logic Shuffle
        ShuffleBoard();
        // Fade in
        foreach(var sr in renderers) sr.DOFade(1f, 0.4f);
        yield return new WaitForSeconds(0.4f);
        
        isProcessing = false; // Mở lại input
    }
    
    #endregion

    #region Visual Helpers (Animation)

    private void AnimateSelection(NodeData node)
    {
        if (node.tileObject == null) return;
        Transform t = node.tileObject.transform;
        SpriteRenderer sr = node.tileObject.GetComponent<SpriteRenderer>();

        t.DOKill();
        sr.DOKill();

        sr.color = selectColor;
        t.DOScale(1f, animDuration).SetLoops(-1, LoopType.Yoyo);
    }

    private void AnimateClickFeedback(NodeData node)
    {
        if (node.tileObject == null) return;
        node.tileObject.transform.DOScale(1f, 0.1f).SetLoops(2, LoopType.Yoyo);
    }

    private void AnimateMiss(NodeData a, NodeData b, Action onComplete)
    {
        Transform tA = a.tileObject.transform;
        Transform tB = b.tileObject.transform;
        SpriteRenderer srA = a.tileObject.GetComponent<SpriteRenderer>();
        SpriteRenderer srB = b.tileObject.GetComponent<SpriteRenderer>();

        tA.DOKill(); tB.DOKill();
        
        tA.DOShakePosition(0.4f, 0.15f, 20, 90, false, true);
        tB.DOShakePosition(0.4f, 0.15f, 20, 90, false, true);
        
        srA.DOColor(errorColor, 0.1f).SetLoops(2, LoopType.Yoyo);
        srB.DOColor(errorColor, 0.1f).SetLoops(2, LoopType.Yoyo)
            .OnComplete(() => onComplete?.Invoke());
    }

    private IEnumerator AnimateMatchDisappear(NodeData a, NodeData b)
    {
        Sequence seq = DOTween.Sequence();
        
        Transform tA = a.tileObject.transform;
        Transform tB = b.tileObject.transform;

        seq.Join(tA.DORotate(new Vector3(0, 0, 360), animDuration, RotateMode.FastBeyond360));
        seq.Join(tA.DOScale(0f, animDuration).SetEase(Ease.InBack));

        seq.Join(tB.DORotate(new Vector3(0, 0, -360), animDuration, RotateMode.FastBeyond360)); 
        seq.Join(tB.DOScale(0f, animDuration).SetEase(Ease.InBack));

        yield return seq.WaitForCompletion();
    }

    private void ResetTileVisuals(NodeData data)
    {
        if (data == null || data.tileObject == null) return;
        
        Transform t = data.tileObject.transform;
        SpriteRenderer sr = data.tileObject.GetComponent<SpriteRenderer>();

        t.DOKill(); 
        sr.DOKill();

        t.localScale = Vector3.one * tileScale;
        t.rotation = Quaternion.identity;
        t.localPosition = GetWorldPosition(data.posX, data.posY);
        
        sr.color = Color.white;
        sr.DOFade(1f, 0f); 

        data.state = NodeState.Normal;
    }

    private void DeselectAll()
    {
        if (firstSelect != null)
        {
            ResetTileVisuals(firstSelect);
            firstSelect = null;
        }
    }

    private void HideTile(NodeData data)
    {
        ResetTileVisuals(data); 
        data.tileObject.SetActive(false);
        data.state = NodeState.Empty;
    }
    #endregion

    #region Utilities

    public void ShuffleBoard()
    {
        // Lấy danh sách các node còn sống
        List<NodeType> activeTypes = new List<NodeType>();
        List<NodeData> activeNodes = new List<NodeData>();

        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                if (board[r, c].state == NodeState.Normal) {
                    activeNodes.Add(board[r, c]);
                    activeTypes.Add(board[r, c].node);
                }
            }
        }

        // Shuffle loại node
        ShuffleList(activeTypes);

        // Gán lại
        for (int i = 0; i < activeNodes.Count; i++) {
            activeNodes[i].node = activeTypes[i];
            activeNodes[i].tileObject.GetComponent<SpriteRenderer>().sprite = iconSprite[(int)activeTypes[i]].GetComponent<SpriteRenderer>().sprite;
            ResetTileVisuals(activeNodes[i]); // Reset visual phòng trường hợp node đang bị select
        }
        
        // Reset state
        firstSelect = null;
    }

    public void AutoSelectBestPair()
    {
         if (isProcessing) return; // Không hint khi đang chạy effect

         // Logic tìm cặp như cũ
         for (int r1 = 0; r1 < rows; r1++) {
            for (int c1 = 0; c1 < cols; c1++) {
                NodeData a = board[r1, c1];
                if (a.state != NodeState.Normal) continue;
                for (int r2 = r1; r2 < rows; r2++) {
                    int startCol = (r2 == r1) ? c1 + 1 : 0;
                    for (int c2 = startCol; c2 < cols; c2++) {
                        NodeData b = board[r2, c2];
                        if (b.state != NodeState.Normal || a.node != b.node) continue;

                        if (PathFinder.Instance.CanConnect(board, a, b)) {
                            // Highlight Effect
                            Sequence hintSeq = DOTween.Sequence();
                            hintSeq.Append(a.tileObject.GetComponent<SpriteRenderer>().DOColor(selectColor, 0.3f).SetLoops(4, LoopType.Yoyo));
                            hintSeq.Join(b.tileObject.GetComponent<SpriteRenderer>().DOColor(selectColor, 0.3f).SetLoops(4, LoopType.Yoyo));
                            hintSeq.OnComplete(() => {
                                ResetTileVisuals(a);
                                ResetTileVisuals(b);
                            });
                            return;
                        }
                    }
                }
            }
         }
    }
    
    public bool HasAnyConnectablePair() {
         for (int r1 = 0; r1 < rows; r1++) {
            for (int c1 = 0; c1 < cols; c1++) {
                NodeData a = board[r1, c1];
                if (a.state != NodeState.Normal) continue;
                for (int r2 = r1; r2 < rows; r2++) {
                    int startCol = (r2 == r1) ? c1 + 1 : 0;
                    for (int c2 = startCol; c2 < cols; c2++) {
                        NodeData b = board[r2, c2];
                        if (b.state != NodeState.Normal || a.node != b.node) continue;
                        if (PathFinder.Instance.CanConnect(board, a, b)) return true;
                    }
                }
            }
         }
         return false;
    }
    
    private bool IsBoardCleared() {
        foreach (var node in board) if (node != null && node.state == NodeState.Normal) return false;
        return true;
    }
    
    public Vector3 GetWorldPosition(int row, int col) {
        return new Vector3(col * offsetX, -row * offsetY, 0);
    }
    #endregion
}