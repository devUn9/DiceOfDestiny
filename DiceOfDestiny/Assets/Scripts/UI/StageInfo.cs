using UnityEngine;

[CreateAssetMenu(fileName = "StageInfo", menuName = "Game/Stage Info")]
public sealed class StageInfo : ScriptableObject
{
    [field: SerializeField] public int StageNumber { get; private set; }
    [field: SerializeField] public string StageName { get; private set; } = string.Empty;
}
