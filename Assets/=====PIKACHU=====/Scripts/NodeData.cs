using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class NodeData
{
    public float X, Y;
    public int posX;
    public int posY;
    public NodeType node;
    public NodeState state;
    public GameObject tileObject;

}



public enum NodeType
{
    Ani_0 = 0,
    Ani_1 = 1,
    Ani_2 = 2,
    Ani_3 = 3,
    Ani_4 = 4,
    Ani_5 = 5,
    Ani_6 = 6,
    Ani_7 = 7,
    Ani_8 = 8,
    Ani_9 = 9,
    Ani_10 = 10,
    Ani_11 = 11,
    Ani_12 = 12,
    Ani_13 = 13,
    Ani_14 = 14,
    Ani_15 = 15,
    Ani_16 = 16,
    Ani_17 = 17,
    Ani_18 = 18,
    Ani_19 = 19,
    Ani_20 = 20,
    Ani_21 = 21,
    Ani_22 = 22,
    Ani_23 = 23,
    Ani_24 = 24,
    Ani_25 = 25,
    Ani_26 = 26,
    Ani_27 = 27,
    Ani_28 = 28,
    Ani_29 = 29,
    Ani_30 = 30,
    Ani_31 = 31,
    Ani_32 = 32,
    Ani_33 = 33,
    Ani_34 = 34,
    Ani_35 = 35,
}



public enum NodeState
{
    Empty,
    Normal,
    Selected
}