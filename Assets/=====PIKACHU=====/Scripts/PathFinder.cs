using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PathFinder : Singleton<PathFinder>
{

    public int[] dx = { 0, 0, 1, -1 };
    public int[] dy = { 1, -1, 0, 0 };
    public bool CanConnect(NodeData[,] board, int rows, int cols, NodeData start, NodeData end)
    {
        if (start.node != end.node || (start.posX == end.posX && start.posY == end.posY)) return false;
        return BFS(board, rows, cols, start, end);

    }

    public List<Vector3> lastPath = new List<Vector3>();
    public LineRenderer lineRenderer;
    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.positionCount = 0;
        }
    }

    public bool BFS(NodeData[,] board, int maxX, int maxY, NodeData start, NodeData end)
    {
        lastPath.Clear();

        Queue<PathNode> q = new Queue<PathNode>();
        Dictionary<(int,int,int), (int,int,int)> parent = new(); // lưu cha để reconstruct đường đi
        bool[,,] visited = new bool[maxX, maxY, 4];

        for (int d = 0; d < 4; d++)
        {
            int nX = start.posX + dx[d];
            int nY = start.posY + dy[d];

            if (CheckInSide(nX, nY, maxX, maxY) &&
                (board[nX, nY].state == NodeState.Empty || (nX == end.posX && nY == end.posY)))
            {
                q.Enqueue(new PathNode(nX, nY, d, 0));
                visited[nX, nY, d] = true;
                parent[(nX,nY,d)] = (start.posX, start.posY, -1);
            }
        }

        while (q.Count > 0)
        {
            PathNode cur = q.Dequeue();

            if (cur.x == end.posX && cur.y == end.posY && cur.turns <= 2)
            {
                ReconstructPath(parent, cur, start, end);
                return true;
            }

            for (int d = 0; d < 4; d++)
            {
                int nX = cur.x + dx[d];
                int nY = cur.y + dy[d];
                int turns = cur.turns + (d == cur.dir ? 0 : 1);

                if (turns > 2) continue;

                if (CheckInSide(nX, nY, maxX, maxY) && !visited[nX, nY, d])
                {
                    if (board[nX, nY].state == NodeState.Empty || (nX == end.posX && nY == end.posY))
                    {
                        visited[nX, nY, d] = true;
                        q.Enqueue(new PathNode(nX, nY, d, turns));
                        parent[(nX,nY,d)] = (cur.x, cur.y, cur.dir);
                    }
                }
            }
        }

        return false;
    }

    private void ReconstructPath(Dictionary<(int,int,int),(int,int,int)> parent, PathNode endNode, NodeData start, NodeData end)
    {
        float offsetX = BoardManager.Instance.offsetX;
        float offsetY = BoardManager.Instance.offsetY;
        List<Vector3> path = new List<Vector3>();
        var key = (endNode.x, endNode.y, endNode.dir);

        while (parent.ContainsKey(key))
        {
            var p = parent[key];
            path.Add(new Vector3(
                key.Item2 * offsetX,
                -key.Item1 * offsetY,
                0));
            key = p;
        }
        path.Add(new Vector3(
            start.posY * offsetX,
            -start.posX * offsetY,
            0));
        path.Reverse();
        lastPath = path;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = lastPath.Count;
            lineRenderer.SetPositions(lastPath.ToArray());
            HideLineAfterDelay(.5f); // Tắt line sau 3 giây
        }
    }

    public void HideLineAfterDelay(float delay)
    {
        StartCoroutine(HideLineCoroutine(delay));
    }

    private IEnumerator HideLineCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }



    // public bool BFS(NodeData[,] board, int rows, int cols, NodeData start, NodeData end)
    // {
    //     Queue<PathNode> q = new Queue<PathNode>();
    //     bool[,,] visited = new bool[rows, cols, 4];
    //     for (int d = 0; d < 4; d++)
    //     {
    //         int nX = start.posX + dx[d];
    //         int nY = start.posY + dy[d];
    //         if (CheckInSide(nX, nY, rows, cols) && (board[nX, nY].state == NodeState.Empty || (nX == end.posX && nY == end.posY)))
    //         {
    //             q.Enqueue(new PathNode(nX, nY, d, 0));
    //             visited[nX, nY, d] = true;
    //         }
    //     }

    //     while (q.Count > 0)
    //     {
    //         PathNode cur = q.Dequeue();
    //         Debug.Log(cur);
    //         if (cur.x == end.posX && cur.y == end.posY && cur.turns <= 2)
    //         {
    //             return true;
    //         }

    //         for (int d = 0; d < 4; d++)
    //         {
    //             int nX = cur.x + dx[d];
    //             int nY = cur.y + dy[d];
    //             int turns = cur.turns + (d == cur.dir ? 0 : 1);

    //             if (turns > 2) continue;

    //             if (CheckInSide(nX, nY, rows, cols) && !visited[nX, nY, d])
    //             {
    //                 if (board[nX, nY].state == NodeState.Empty || (nX == end.posX && nY == end.posY))
    //                 {
    //                     visited[nX, nY, d] = true;
    //                     q.Enqueue(new PathNode(nX, nY, d, turns));
    //                 }
    //             }
    //         }
    //     }
    //     return false;
    // }


    public bool CheckInSide(int x, int y, int rows, int cols)
    {
        return x >= 0 && y >= 0 && x < rows && y < cols;
    }

}



public class PathNode
{
    public int x, y;
    public int dir;
    public int turns;

    public PathNode(int x, int y, int dir, int turn)
    {
        this.x = x;
        this.y = y;
        this.dir = dir;
        this.turns = turn;
    }
}