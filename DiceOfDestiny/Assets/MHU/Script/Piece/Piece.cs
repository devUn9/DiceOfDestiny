using UnityEngine;

[CreateAssetMenu(fileName = "Piece", menuName = "Piece/Piece", order = 10)]
public class Piece : ScriptableObject
{
    public bool isAvailable;
    [SerializeField] public Face[] faces = new Face[6]; // 6개 면 데이터
}

[System.Serializable]
public struct Face
{
    public ClassData classData; // 클래스 데이터
    public TileColor color;
}