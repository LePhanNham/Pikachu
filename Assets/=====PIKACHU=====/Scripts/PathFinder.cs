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

 
    public List<Vector3> GetPath(NodeData[,] board, int rows, int cols, NodeData start, NodeData end)
    {
        if (start.node != end.node || (start.posX == end.posX && start.posY == end.posY)) return new List<Vector3>();
        
        lastPath.Clear();
        if (BFSWithPath(board, rows, cols, start, end))
        {
            return new List<Vector3>(lastPath);
        }
        return new List<Vector3>();
    }


    public int GetPathTurns(NodeData[,] board, int rows, int cols, NodeData start, NodeData end)
    {
        if (start.node != end.node || (start.posX == end.posX && start.posY == end.posY)) return -1;
        
        return BFSWithTurns(board, rows, cols, start, end);
    }

    public List<Vector3> lastPath = new List<Vector3>();
    public LineRenderer lineRenderer;

    public bool BFS(NodeData[,] board, int maxX, int maxY, NodeData start, NodeData end)
    {
        lastPath.Clear();


        Queue<PathNode> q = new Queue<PathNode>();
        Dictionary<(int,int,int), (int,int,int)> parent = new();
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



    public bool CheckInSide(int x, int y, int rows, int cols)
    {
        return x >= 0 && y >= 0 && x < rows && y < cols;
    }

    /// <summary>
    /// BFS với đường đi
    /// </summary>
    public bool BFSWithPath(NodeData[,] board, int maxX, int maxY, NodeData start, NodeData end)
    {
        lastPath.Clear();

        Queue<PathNode> q = new Queue<PathNode>();
        Dictionary<(int,int,int), (int,int,int)> parent = new();
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
                // Reconstruct path
                ReconstructPath(parent, start, end, cur);
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


    public int BFSWithTurns(NodeData[,] board, int maxX, int maxY, NodeData start, NodeData end)
    {
        Queue<PathNode> q = new Queue<PathNode>();
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
            }
        }

        while (q.Count > 0)
        {
            PathNode cur = q.Dequeue();

            if (cur.x == end.posX && cur.y == end.posY && cur.turns <= 2)
            {
                return cur.turns;
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
                    }
                }
            }
        }

        return -1;
    }


    private void ReconstructPath(Dictionary<(int,int,int), (int,int,int)> parent, NodeData start, NodeData end, PathNode finalNode)
    {
        lastPath.Clear();
        
        System.Func<int,int,Vector3> GridToWorld = (r, c) =>
        {
            float x = c * BoardManager.Instance.offsetX;
            float y = -r * BoardManager.Instance.offsetY;
            return new Vector3(x, y, 0);
        };

        // Thêm điểm cuối (center of cell)
        lastPath.Add(GridToWorld(end.posX, end.posY));

        // Tái tạo ngược từ điểm cuối về điểm đầu
        (int x, int y, int dir) current = (finalNode.x, finalNode.y, finalNode.dir);
        
        while (parent.ContainsKey(current))
        {
            var prev = parent[current];
            if (prev.Item3 == -1) break; // Đã đến điểm đầu
            
            lastPath.Insert(0, GridToWorld(prev.Item1, prev.Item2));
            current = prev;
        }
        
        // Thêm điểm đầu
        lastPath.Insert(0, GridToWorld(start.posX, start.posY));
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

/// <summary>
/// BFS với đường đi
/// </summary>
// (đã di chuyển các hàm BFSWithPath, BFSWithTurns, ReconstructPath vào trong lớp PathFinder)