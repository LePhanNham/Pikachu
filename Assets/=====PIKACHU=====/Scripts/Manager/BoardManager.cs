using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random; // Import DOTween

public class BoardManager : Singleton<BoardManager>
{
    [Header("Settings")]
    public int rows;
    public int cols;
    public float offsetX, offsetY;
    [SerializeField] private float quickMatchThreshold = 3f;

    [Header("Assets")]
    public GameObject[] iconSprite;

    // State
    public NodeData[,] board;
    public GameObject[,] tiles;
    private NodeData firstSelect;
    private float lastMatchTime;
    private int comboCount;

    private void Start()
    {
        if (PathFinder.Instance != null) PathFinder.Instance.Init(rows, cols);
        
        // Cấu hình mặc định cho DOTween để an toàn
        DOTween.SetTweensCapacity(500, 50);
    }

    #region Init & Reset (Giữ nguyên logic cũ)
    public void InitializeGameBoard() { InitGrid(); CreateBoard(); }
    public void ResetGameBoard() { ClearBoard(); InitializeGameBoard(); firstSelect = null; comboCount = 0; }
    
    public void ClearBoard()
    {
        if (tiles == null) return;
        foreach (var tile in tiles) if (tile != null) Destroy(tile);
        board = null; tiles = null;
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
                if (r == 0 || r == rows - 1 || c == 0 || c == cols - 1) node.state = NodeState.Empty;
                else { node.state = NodeState.Normal; node.node = pool[index++]; }
                board[r, c] = node;
            }
        }
    }

    private void CreateBoard()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (board[r, c].state == NodeState.Empty) continue;
                Vector3 pos = new Vector3(c * offsetX, -r * offsetY, 0);
                
                GameObject tileObj = Instantiate(iconSprite[(int)board[r, c].node], pos, Quaternion.identity, transform);
                
                tileObj.transform.localScale = new Vector3(1.2f,1.2f,1.2f);
                tileObj.GetComponent<SpriteRenderer>().color = Color.white;

                Node nodeComp = tileObj.GetComponent<Node>();
                if (nodeComp) { nodeComp.Init(board[r, c]); nodeComp.node.tileObject = tileObj; }
                tiles[r, c] = tileObj;
            }
        }
    }
    
    private void ShuffleList<T>(List<T> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
    private List<NodeType> GenerateNodePool() {
        int total = (rows - 2) * (cols - 2);
        List<NodeType> pool = new List<NodeType>();
        for (int i = 0; i < total / 2; i++) {
            NodeType type = (NodeType)(i % iconSprite.Length);
            pool.Add(type); pool.Add(type);
        }
        return pool;
    }
    #endregion

    #region Gameplay Logic

    public void SelectTile(int row, int col)
    {
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
        SetTileColor(node, Color.cyan);

        node.tileObject.transform.DOKill(); 
        node.tileObject.transform.DOScale(1f, 0.3f).SetLoops(-1, LoopType.Yoyo);

        SoundManager.Instance?.Click();
    }

    private void HandleSecondSelection(NodeData second)
    {
        second.state = NodeState.Selected;
        
        second.tileObject.transform.DOKill();
        second.tileObject.transform.DOScale(1f, 0.1f).SetLoops(2, LoopType.Yoyo);

        if (PathFinder.Instance.CanConnect(board, firstSelect, second))
        {
            firstSelect.tileObject.transform.DOKill();
            firstSelect.tileObject.transform.localScale = new Vector3(1.2f,1.2f,1.2f); 
            
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
        int baseScore = 100 + (turns * 50) + (comboCount * 25);
        bool isQuick = (Time.time - lastMatchTime) < quickMatchThreshold;
        
        comboCount++;
        GameManager.Instance.AddScore(baseScore, isQuick, false);
        lastMatchTime = Time.time;
        SoundManager.Instance?.Match();
        StartCoroutine(RoutineMatchSequence(nodeA, nodeB));
    }

    private void ProcessMiss(NodeData nodeA, NodeData nodeB)
    {
        SoundManager.Instance?.NoMove();
        comboCount = 0; 

        Transform tA = nodeA.tileObject.transform;
        Transform tB = nodeB.tileObject.transform;

        tA.DOKill(); tB.DOKill();
        
        tA.DOShakePosition(0.4f, 0.15f, 20, 90, false, true);
        tB.DOShakePosition(0.4f, 0.15f, 20, 90, false, true);

        var srA = nodeA.tileObject.GetComponent<SpriteRenderer>();
        var srB = nodeB.tileObject.GetComponent<SpriteRenderer>();
        
        srA.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);
        srB.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);

        DOVirtual.DelayedCall(0.4f, () => 
        {
            ResetTileVisuals(nodeA);
            ResetTileVisuals(nodeB);
            firstSelect = null;
        });
    }

    private IEnumerator RoutineMatchSequence(NodeData nodeA, NodeData nodeB)
    {
        var path = PathFinder.Instance.GetPath(board, nodeA, nodeB);
        PathFinder.Instance.DrawDebugPath(path);
        
        yield return new WaitForSeconds(0.1f); 

        Sequence seq = DOTween.Sequence();
        
        Transform tA = nodeA.tileObject.transform;
        Transform tB = nodeB.tileObject.transform;

        seq.Join(tA.DORotate(new Vector3(0, 0, 360), 0.3f, RotateMode.FastBeyond360));
        seq.Join(tA.DOScale(0f, 0.3f).SetEase(Ease.InBack));

        seq.Join(tB.DORotate(new Vector3(0, 0, -360), 0.3f, RotateMode.FastBeyond360)); 
        seq.Join(tB.DOScale(0f, 0.3f).SetEase(Ease.InBack));

        yield return seq.WaitForCompletion();

        PathFinder.Instance.ClearDebugPath();
        HideTile(nodeA);
        HideTile(nodeB);
        firstSelect = null;

        if (IsBoardCleared())
        {
            GameManager.Instance.Victory();
        }
        else if (!HasAnyConnectablePair())
        {
            yield return StartCoroutine(ShuffleBoardWithEffect());
        }
    }

    private IEnumerator ShuffleBoardWithEffect()
    {
        List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        foreach(var t in tiles) {
            if(t != null && t.activeSelf) renderers.Add(t.GetComponent<SpriteRenderer>());
        }

        foreach(var sr in renderers) sr.DOFade(0f, 0.4f);
        yield return new WaitForSeconds(0.4f);

        ShuffleBoard();
        comboCount = 0;

        foreach(var sr in renderers) sr.DOFade(1f, 0.4f);
    }
    
    private void DeselectAll()
    {
        if (firstSelect != null)
        {
            ResetTileVisuals(firstSelect);
            firstSelect = null;
        }
    }

    private void ResetTileVisuals(NodeData data)
    {
        if (data.tileObject == null) return;
        
        Transform t = data.tileObject.transform;
        SpriteRenderer sr = data.tileObject.GetComponent<SpriteRenderer>();

        t.DOKill(); 
        t.localScale = new Vector3(1.2f,1.2f,1.2f);
        t.rotation = Quaternion.identity;
        t.localPosition = GetWorldPosition(data.posX, data.posY);
        
        sr.DOKill();
        sr.color = Color.white;
        sr.DOFade(1f, 0f); 

        data.state = NodeState.Normal;
    }

    private void HideTile(NodeData data)
    {
        ResetTileVisuals(data); 
        data.tileObject.SetActive(false);
        data.state = NodeState.Empty;
    }

    public void ShuffleBoard()
    {
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
        ShuffleList(activeTypes);
        for (int i = 0; i < activeNodes.Count; i++) {
            activeNodes[i].node = activeTypes[i];
            activeNodes[i].tileObject.GetComponent<SpriteRenderer>().sprite = iconSprite[(int)activeTypes[i]].GetComponent<SpriteRenderer>().sprite;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing&&IsBoardCleared())
        {
            GameManager.Instance.Victory();
        }
    }

    private void SetTileColor(NodeData d, Color c) { 
        if (d?.tileObject) d.tileObject.GetComponent<SpriteRenderer>().color = c; 
    }
    public void AutoSelectBestPair()
    {
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
                            a.tileObject.transform.DOKill(); b.tileObject.transform.DOKill();
                            a.tileObject.GetComponent<SpriteRenderer>().DOColor(Color.cyan, 0.3f).SetLoops(4, LoopType.Yoyo).OnComplete(()=>ResetTileVisuals(a));
                            b.tileObject.GetComponent<SpriteRenderer>().DOColor(Color.cyan, 0.3f).SetLoops(4, LoopType.Yoyo).OnComplete(()=>ResetTileVisuals(b));
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
        foreach (var node in board) if (node.state == NodeState.Normal) return false;
        return true;
    }
    
    public Vector3 GetWorldPosition(int row, int col) {
        return new Vector3(col * offsetX, -row * offsetY, 0);
    }
    #endregion
}