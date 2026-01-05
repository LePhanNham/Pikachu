using UnityEngine;

public enum GravityDirection { None, Down, Up, Left, Right }

[CreateAssetMenu(fileName = "NewLevel", menuName = "Pikachu/Level Data")]
public class LevelData : ScriptableObject
{
    public int level = 1;
    public int timeLimit; 
    public GravityDirection gravity = GravityDirection.None;
    public int Changes = 5;
    public int Suggestions = 5;
}

