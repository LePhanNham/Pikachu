using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInput : MonoBehaviour
{
    void Update()
    {
        // Chỉ cho phép input khi game đang playing
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                Node node = hit.collider.GetComponent<Node>();
                if (node != null && node.node != null && node.node.state == NodeState.Normal && node.node.tileObject != null && node.node.tileObject.activeSelf)
                {
                    Debug.Log($"Click tile at ({node.node.posX}, {node.node.posY}) - State: {node.node.state}");
                    BoardManager.Instance.SelectTile(node.node.posX, node.node.posY);
                }
                else
                {
                    if (node == null) Debug.Log("Node component is null");
                    else if (node.node == null) Debug.Log("NodeData is null");
                    else if (node.node.state != NodeState.Normal) Debug.Log($"Tile state is {node.node.state}, not Normal");
                    else if (node.node.tileObject == null) Debug.Log("TileObject is null");
                    else if (!node.node.tileObject.activeSelf) Debug.Log("TileObject is not active");
                }
            }
            else
            {
                Debug.Log("No collider hit");
            }
        }
    }
}
