using UnityEngine;
using UnityEngine.EventSystems;

public class BoardInput : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) 
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<Node>(out var nodeComponent))
            {
                ValidateAndSelect(nodeComponent);
            }
        }
    }

    private void ValidateAndSelect(Node nodeComponent)
    {
        var data = nodeComponent.node;

        if (data != null && 
            data.state == NodeState.Normal && 
            data.tileObject != null && 
            data.tileObject.activeSelf)
        {
            BoardManager.Instance.SelectTile(data.posX, data.posY);
        }
    }
}