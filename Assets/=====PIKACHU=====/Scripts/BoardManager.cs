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



    private void Start()
    {
        InitGrid();
        CreateBoard();
        firstSelect = null;
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

                }
                tiles[row, col] = tile;
            }
        }
    }

    public void SelectTile(int row, int col)
    {
        NodeData node = board[row, col];
        if (node.state != NodeState.Normal) return;

        if (firstSelect.state != NodeState.Selected)
        {
            firstSelect = node;
            firstSelect.state = NodeState.Selected;
            firstSelect.tileObject.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        else
        {
            // Nếu có thể nối
            node.tileObject.GetComponent<SpriteRenderer>().color = Color.yellow;
            if (PathFinder.Instance.CanConnect(board, rows, cols, firstSelect, node))
            {
                tiles[firstSelect.posX, firstSelect.posY].gameObject.SetActive(false);
                board[firstSelect.posX, firstSelect.posY].state = NodeState.Empty;
                tiles[row, col].gameObject.SetActive(false);
                board[row, col].state = NodeState.Empty;
                firstSelect.state = NodeState.Empty;
                node.state = NodeState.Empty;
            }
            else
            {
                // reset màu
                firstSelect.tileObject.GetComponent<SpriteRenderer>().color = Color.white;
                node.tileObject.GetComponent<SpriteRenderer>().color = Color.white;
                firstSelect.state = NodeState.Normal;
                node.state = NodeState.Normal;
                board[row, col].state = NodeState.Normal;
            }

            firstSelect = null;
        }
    }

}



