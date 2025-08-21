using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : Singleton<BoardManager>
{

    [Header("Data Manager")]
    public GameObject[] iconSprite;
    public int rows;
    public int cols;
    public float offsetX, offsetY;

    public NodeData[,] board;
    public GameObject[,] tiles;

    public List<NodeType> nodePool;

    // public List<NodeData> nodeData;


    [Header("In Game")]

    public NodeData firstSelect;



    private void Awake()
    {
        // Không spawn board ngay, chỉ khởi tạo các biến
        firstSelect = null;
    }

    /// <summary>
    /// Khởi tạo game board khi bắt đầu game
    /// </summary>
    public void InitializeGameBoard()
    {
        InitGrid();
        CreateBoard();
        Debug.Log($"🎮 Game board đã được khởi tạo: {rows}x{cols}");
    }

    /// <summary>
    /// Reset game board cho level mới
    /// </summary>
    public void ResetGameBoard()
    {
        // Xóa board cũ
        ClearBoard();
        
        // Tạo board mới
        InitGrid();
        CreateBoard();
        
        // Reset game state
        firstSelect = null;
        
        Debug.Log($"🔄 Board đã được reset cho level mới");
    }

    /// <summary>
    /// Xóa board cũ
    /// </summary>
    private void ClearBoard()
    {
        // Xóa tất cả tiles
        if (tiles != null)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (tiles[row, col] != null)
                    {
                        DestroyImmediate(tiles[row, col]);
                        tiles[row, col] = null;
                    }
                }
            }
        }
        
        // Clear arrays
        board = null;
        tiles = null;
    }

    public Vector3 GetWorldPosition(int row, int col)
    {
        return new Vector3(
            col * offsetX + offsetX * 0.5f,   // X: theo cột, +0.5 để lấy tâm
            -row * offsetY - offsetY * 0.5f,  // Y: theo hàng, +0.5 để lấy tâm
            0
        );
    }


    private List<NodeType> GenerateNodePool()
    {
        List<NodeType> pool = new List<NodeType>();
        int repeat = 8;

        for (int i = 0; i < iconSprite.Length; i++)
        {
            for (int j = 0; j < repeat; j++)
            {
                pool.Add((NodeType)i);
            }
        }

        return pool;
    }

    // Fisher-Yates shuffle
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }

    // Khởi tạo Grid
    private void InitGrid()
    {
        board = new NodeData[rows, cols];
        tiles = new GameObject[rows, cols];

        nodePool = GenerateNodePool();
        Shuffle(nodePool);

        int index = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                NodeData node = new NodeData
                {
                    posX = row,
                    posY = col
                };

                if (row == 0 || row == rows - 1 || col == 0 || col == cols - 1)
                {
                    node.state = NodeState.Empty;
                    node.node = 0;
                }
                else
                {
                    node.state = NodeState.Normal;
                    node.node = nodePool[index];
                    index++;
                }
                // nodeData.Add(node);
                board[row, col] = node;
            }
        }
    }

    // Vẽ Board
    private void CreateBoard()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (board[row, col].state == NodeState.Empty) continue;

                Vector3 pos = new Vector3(col * offsetX, -row * offsetY, 0);

                GameObject tile = Instantiate(iconSprite[(int)board[row, col].node], pos, Quaternion.identity, transform);
                tile.GetComponent<SpriteRenderer>().color = Color.white;
                Node nodeComp = tile.GetComponent<Node>();
                if (nodeComp != null)
                {
                    nodeComp.Init(board[row, col]);
                    nodeComp.node.tileObject = tile;
                    nodeComp.node.X = pos.x;
                    nodeComp.node.Y = pos.y;

                }
                tiles[row, col] = tile;
            }
        }
    }

    public void SelectTile(int row, int col)
    {
        NodeData node = board[row, col];
        if (node == null || node.state != NodeState.Normal) return;

        // Nếu chưa chọn gì
        if (firstSelect == null)
        {
            firstSelect = node;
            firstSelect.state = NodeState.Selected;
            firstSelect.tileObject.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        else
        {
            // Nếu chọn node thứ 2
            node.state = NodeState.Selected;
            node.tileObject.GetComponent<SpriteRenderer>().color = Color.yellow;

            if (PathFinder.Instance.CanConnect(board, rows, cols, firstSelect, node))
            {
                // Tính điểm dựa trên độ khó của đường đi
                int pathTurns = PathFinder.Instance.GetPathTurns(board, rows, cols, firstSelect, node);
                int baseScore = CalculateMatchScore(pathTurns);

                // Kiểm tra quick match
                bool isQuickMatch = Time.time - lastMatchTime < 3f;
                GameManager.Instance.AddScore(baseScore, isQuickMatch, false);

                // Vẽ đường 1s rồi xóa
                StartCoroutine(ShowPathAndRemove(firstSelect, node));

                // Cập nhật thời gian match cuối
                lastMatchTime = Time.time;

                // Các kiểm tra (thắng/hết nước) sẽ được gọi sau khi xóa trong coroutine
            }
            else
            {
                // reset màu nếu không nối được
                firstSelect.tileObject.GetComponent<SpriteRenderer>().color = Color.white;
                node.tileObject.GetComponent<SpriteRenderer>().color = Color.white;

                firstSelect.state = NodeState.Normal;
                node.state = NodeState.Normal;

                firstSelect = null;
            }
        }
    }


    public bool HasAnyConnectablePair()
    {
        for (int row1 = 0; row1 < rows; row1++)
        {
            for (int col1 = 0; col1 < cols; col1++)
            {
                NodeData nodeA = board[row1, col1];
                if (nodeA == null || nodeA.state != NodeState.Normal) continue;

                for (int row2 = row1; row2 < rows; row2++)
                {
                    int startCol = (row2 == row1) ? col1 + 1 : 0;
                    for (int col2 = startCol; col2 < cols; col2++)
                    {
                        NodeData nodeB = board[row2, col2];
                        if (nodeB == null || nodeB.state != NodeState.Normal) continue;
                        if (nodeA.node != nodeB.node) continue;

                        if (PathFinder.Instance.CanConnect(board, rows, cols, nodeA, nodeB))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void ShuffleBoard()
    {
        List<NodeType> remainTypes = new List<NodeType>();
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (board[row, col].state == NodeState.Normal)
                {
                    remainTypes.Add(board[row, col].node);
                }
            }
        }

        Shuffle(remainTypes);

        int idx = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (board[row, col].state == NodeState.Normal)
                {
                    board[row, col].node = remainTypes[idx];
                    // Đổi sprite cho tile
                    tiles[row, col].GetComponent<SpriteRenderer>().sprite = iconSprite[(int)remainTypes[idx]].GetComponent<SpriteRenderer>().sprite;
                    idx++;
                }
            }
        }
    }

    // ================= SCORING SYSTEM =================
    private float lastMatchTime = 0f;
    private int comboCount = 0;
    
    /// <summary>
    /// Tính điểm dựa trên độ khó của đường đi
    /// </summary>
    private int CalculateMatchScore(int pathTurns)
    {
        int baseScore = 100;
        
        // Bonus cho đường đi khó (nhiều lần rẽ)
        switch (pathTurns)
        {
            case 0: // Đường thẳng
                baseScore = 100;
                break;
            case 1: // 1 lần rẽ
                baseScore = 150;
                break;
            case 2: // 2 lần rẽ
                baseScore = 200;
                break;
        }
        
        // Bonus cho combo
        comboCount++;
        if (comboCount > 1)
        {
            baseScore += (comboCount - 1) * 25; // +25 điểm cho mỗi combo
        }
        
        return baseScore;
    }
    
    /// <summary>
    /// Reset combo khi không có match
    /// </summary>
    private void ResetCombo()
    {
        comboCount = 0;
    }

    // ================= EFFECTS & ANIMATIONS =================
    
    /// <summary>
    /// Xóa tiles với hiệu ứng
    /// </summary>
    private IEnumerator RemoveTilesWithEffect(NodeData nodeA, NodeData nodeB)
    {
        // Hiệu ứng flash trước khi xóa
        yield return StartCoroutine(FlashTiles(nodeA, nodeB, Color.white, Color.yellow, 3));
        
        // Xóa tiles
        tiles[nodeA.posX, nodeA.posY].SetActive(false);
        board[nodeA.posX, nodeA.posY].state = NodeState.Empty;

        tiles[nodeB.posX, nodeB.posY].SetActive(false);
        board[nodeB.posX, nodeB.posY].state = NodeState.Empty;

        firstSelect = null;
        nodeA.state = NodeState.Empty;
        nodeB.state = NodeState.Empty;
    }

    private IEnumerator ShowPathAndRemove(NodeData nodeA, NodeData nodeB)
    {
        // Lấy đường đi từ PathFinder và vẽ tạm 1s bằng LineRenderer
        if (PathFinder.Instance != null)
        {
            var path = PathFinder.Instance.GetPath(board, rows, cols, nodeA, nodeB);
            if (path != null && path.Count > 0)
            {
                if (PathFinder.Instance.lineRenderer == null)
                {
                    var go = new GameObject("TempPath");
                    PathFinder.Instance.lineRenderer = go.AddComponent<LineRenderer>();
                    PathFinder.Instance.lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    PathFinder.Instance.lineRenderer.startWidth = 0.05f;
                    PathFinder.Instance.lineRenderer.endWidth = 0.05f;
                    PathFinder.Instance.lineRenderer.startColor = Color.green;
                    PathFinder.Instance.lineRenderer.endColor = Color.green;
                    PathFinder.Instance.lineRenderer.sortingOrder = 20;
                }

                PathFinder.Instance.lineRenderer.positionCount = path.Count;
                PathFinder.Instance.lineRenderer.SetPositions(path.ToArray());
                PathFinder.Instance.lineRenderer.enabled = true;

                yield return new WaitForSeconds(1f);

                PathFinder.Instance.lineRenderer.enabled = false;
            }
        }

        // Xóa tiles thật sự
        tiles[nodeA.posX, nodeA.posY].SetActive(false);
        board[nodeA.posX, nodeA.posY].state = NodeState.Empty;
        tiles[nodeB.posX, nodeB.posY].SetActive(false);
        board[nodeB.posX, nodeB.posY].state = NodeState.Empty;

        firstSelect = null;
        nodeA.state = NodeState.Empty;
        nodeB.state = NodeState.Empty;

        // Kiểm tra thắng
        if (IsBoardCleared())
        {
            GameManager.Instance.Victory();
            yield break;
        }

        // Kiểm tra nước đi
        if (!HasAnyConnectablePair())
        {
            yield return StartCoroutine(ShuffleBoardWithEffect());
        }
    }
    
    /// <summary>
    /// Hiệu ứng flash cho tiles
    /// </summary>
    private IEnumerator FlashTiles(NodeData nodeA, NodeData nodeB, Color color1, Color color2, int flashCount)
    {
        SpriteRenderer rendererA = nodeA.tileObject.GetComponent<SpriteRenderer>();
        SpriteRenderer rendererB = nodeB.tileObject.GetComponent<SpriteRenderer>();
        
        for (int i = 0; i < flashCount; i++)
        {
            rendererA.color = color1;
            rendererB.color = color1;
            yield return new WaitForSeconds(0.1f);
            
            rendererA.color = color2;
            rendererB.color = color2;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    /// <summary>
    /// Shuffle board với hiệu ứng
    /// </summary>
    private IEnumerator ShuffleBoardWithEffect()
    {
        // Hiệu ứng fade out
        yield return StartCoroutine(FadeOutAllTiles());
        
        // Shuffle
        ShuffleBoard();
        
        // Hiệu ứng fade in
        yield return StartCoroutine(FadeInAllTiles());
        
        // Reset combo
        ResetCombo();
    }
    
    /// <summary>
    /// Fade out tất cả tiles
    /// </summary>
    private IEnumerator FadeOutAllTiles()
    {
        SpriteRenderer[] renderers = new SpriteRenderer[tiles.GetLength(0) * tiles.GetLength(1)];
        int index = 0;
        
        // Collect all renderers
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (tiles[row, col] != null && tiles[row, col].activeSelf)
                {
                    renderers[index] = tiles[row, col].GetComponent<SpriteRenderer>();
                    index++;
                }
            }
        }
        
        // Fade out
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            
            for (int i = 0; i < index; i++)
            {
                if (renderers[i] != null)
                {
                    Color color = renderers[i].color;
                    color.a = alpha;
                    renderers[i].color = color;
                }
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Fade in tất cả tiles
    /// </summary>
    private IEnumerator FadeInAllTiles()
    {
        SpriteRenderer[] renderers = new SpriteRenderer[tiles.GetLength(0) * tiles.GetLength(1)];
        int index = 0;
        
        // Collect all renderers
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (tiles[row, col] != null && tiles[row, col].activeSelf)
                {
                    renderers[index] = tiles[row, col].GetComponent<SpriteRenderer>();
                    index++;
                }
            }
        }
        
        // Fade in
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            
            for (int i = 0; i < index; i++)
            {
                if (renderers[i] != null)
                {
                    Color color = renderers[i].color;
                    color.a = alpha;
                    renderers[i].color = color;
                }
            }
            
            yield return null;
        }
        
        // Reset alpha về 1
        for (int i = 0; i < index; i++)
        {
            if (renderers[i] != null)
            {
                Color color = renderers[i].color;
                color.a = 1f;
                renderers[i].color = color;
            }
        }
    }

    // ================= SIMPLE HINT =================
    public void AutoSelectBestPair()
    {
        for (int row1 = 0; row1 < rows; row1++)
        {
            for (int col1 = 0; col1 < cols; col1++)
            {
                NodeData nodeA = board[row1, col1];
                if (nodeA == null || nodeA.state != NodeState.Normal) continue;

                for (int row2 = row1; row2 < rows; row2++)
                {
                    int startCol = (row2 == row1) ? col1 + 1 : 0;
                    for (int col2 = startCol; col2 < cols; col2++)
                    {
                        NodeData nodeB = board[row2, col2];
                        if (nodeB == null || nodeB.state != NodeState.Normal) continue;
                        if (nodeA.node != nodeB.node) continue;

                        if (PathFinder.Instance.CanConnect(board, rows, cols, nodeA, nodeB))
                        {
                            if (nodeA.tileObject != null) nodeA.tileObject.GetComponent<SpriteRenderer>().color = Color.cyan;
                            if (nodeB.tileObject != null) nodeB.tileObject.GetComponent<SpriteRenderer>().color = Color.cyan;
                            Debug.Log($"Hint: ({nodeA.posX},{nodeA.posY}) <-> ({nodeB.posX},{nodeB.posY})");
                            return;
                        }
                    }
                }
            }
        }

        Debug.Log("Không có gợi ý khả dụng");
    }

    // ================= BOARD STATE CHECK =================
    private bool IsBoardCleared()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (board[row, col].state == NodeState.Normal)
                {
                    return false;
                }
            }
        }
        return true;
    }
}

