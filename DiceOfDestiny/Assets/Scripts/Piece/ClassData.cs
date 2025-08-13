using UnityEngine;
[CreateAssetMenu(fileName = "ClassData", menuName = "Piece/ClassData", order = 10)]
public class ClassData : ScriptableObject
{
    public string className = "Class Name";
    public Sprite sprite;

    [SerializeField]private bool isCombatClass;
    public bool IsCombatClass => isCombatClass;
}
