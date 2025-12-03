using System.Collections.Generic;
using UnityEngine;

public class PathFinder : Singleton<PathFinder>
{
    private readonly int[] dx = { 0, 0, 1, -1 };
    private readonly int[] dy = { 1, -1, 0, 0 };
    private const int MAX_TURNS = 2;

    [Header("Debug")]
    public LineRenderer lineRenderer;

    private int rows, cols;
    private bool[,,] visited; 
    private (int x, int y, int dir)[,,] parents; 

    public void Init(int r, int c)
    {
        rows = r;
        cols = c;
        visited = new bool[rows, cols, 4];
        parents = new (int, int, int)[rows, cols, 4];
    }

    public bool CanConnect(NodeData[,] board, NodeData start, NodeData end)
    {
        if (!IsValidPair(start, end)) return false;
        return RunBFS(board, start, end, out _);
    }


    public List<Vector3> GetPath(NodeData[,] board, NodeData start, NodeData end)
    {
        if (!IsValidPair(start, end)) return new List<Vector3>();

        if (RunBFS(board, start, end, out var finalState))
        {
            return ReconstructPath(start, end, finalState);
        }
        return new List<Vector3>();
    }


    public int GetPathTurns(NodeData[,] board, NodeData start, NodeData end)
    {
        if (!IsValidPair(start, end)) return -1;
        
        if (RunBFS(board, start, end, out var finalState))
        {
            return finalState.turns;
        }
        return -1;
    }

    private bool IsValidPair(NodeData start, NodeData end)
    {
        if (start == null || end == null) return false;
        if (start.node != end.node) return false;
        if (start.posX == end.posX && start.posY == end.posY) return false;
        return true;
    }

    private struct SearchNode
    {
        public int x, y, dir, turns;
    }


    private bool RunBFS(NodeData[,] board, NodeData start, NodeData end, out SearchNode finalState)
    {
        finalState = default;
        
        System.Array.Clear(visited, 0, visited.Length);

        Queue<SearchNode> q = new Queue<SearchNode>();

        for (int d = 0; d < 4; d++)
        {
            int nx = start.posX + dx[d];
            int ny = start.posY + dy[d];

            if (IsWalkable(board, nx, ny, end))
            {
                q.Enqueue(new SearchNode { x = nx, y = ny, dir = d, turns = 0 });
                visited[nx, ny, d] = true;
                parents[nx, ny, d] = (start.posX, start.posY, -1); 
            }
        }

        while (q.Count > 0)
        {
            var cur = q.Dequeue();

            if (cur.x == end.posX && cur.y == end.posY)
            {
                finalState = cur;
                return true;
            }

            for (int d = 0; d < 4; d++)
            {
                int nx = cur.x + dx[d];
                int ny = cur.y + dy[d];
                
                int newTurns = cur.turns + (d == cur.dir ? 0 : 1);

                if (newTurns > MAX_TURNS) continue;

                if (IsInside(nx, ny) && !visited[nx, ny, d])
                {
                    if (IsWalkable(board, nx, ny, end))
                    {
                        visited[nx, ny, d] = true;
                        q.Enqueue(new SearchNode { x = nx, y = ny, dir = d, turns = newTurns });
                        parents[nx, ny, d] = (cur.x, cur.y, cur.dir);
                    }
                }
            }
        }

        return false;
    }

    private bool IsInside(int x, int y)
    {
        return x >= 0 && y >= 0 && x < rows && y < cols;
    }

    private bool IsWalkable(NodeData[,] board, int x, int y, NodeData target)
    {
        if (!IsInside(x, y)) return false;
        return board[x, y].state == NodeState.Empty || (x == target.posX && y == target.posY);
    }

    private List<Vector3> ReconstructPath(NodeData start, NodeData end, SearchNode finalNode)
    {
        List<Vector3> path = new List<Vector3>();

        path.Add(BoardManager.Instance.GetWorldPosition(end.posX, end.posY));

        int cx = finalNode.x;
        int cy = finalNode.y;
        int cd = finalNode.dir;

        while (true)
        {
            var p = parents[cx, cy, cd];
            if (p.dir == -1) break; 

            path.Add(BoardManager.Instance.GetWorldPosition(p.x, p.y));
            cx = p.x;
            cy = p.y;
            cd = p.dir;
        }

        path.Add(BoardManager.Instance.GetWorldPosition(start.posX, start.posY));
        
        return path;
    }
    public void DrawDebugPath(List<Vector3> path)
    {
        if (lineRenderer == null || path.Count == 0) return;
        
        lineRenderer.positionCount = path.Count;
        lineRenderer.SetPositions(path.ToArray());
        lineRenderer.enabled = true;
    }

    public void ClearDebugPath()
    {
        if (lineRenderer != null) lineRenderer.enabled = false;
    }
}