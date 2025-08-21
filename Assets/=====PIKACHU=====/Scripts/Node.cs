using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public NodeData node;

    // Hàm khởi tạo custom (gọi sau Instantiate)
    public void Init(NodeData data)
    {
        node = data;
    }


}
