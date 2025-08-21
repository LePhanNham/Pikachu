using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInput : MonoBehaviour
{
    void Update()
    {
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
                    BoardManager.Instance.SelectTile(node.node.posX, node.node.posY);
                }
            }
        }
    }
}
