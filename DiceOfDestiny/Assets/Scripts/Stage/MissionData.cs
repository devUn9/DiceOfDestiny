using UnityEngine;

public enum MissionType
{
    ReachFinishLine,
    KillAllMonsters,
    FindGrayGrass,
    KillPawn,
    KillKnight,
    DestroyHouse
}

[CreateAssetMenu(menuName = "Mission/MissionData")]
public class MissionData : ScriptableObject
{
    public MissionType missionType;

    public bool IsCompleted()
    {
        switch (missionType)
        {
            case MissionType.ReachFinishLine:
                return MissionManager.Instance.isFinishLine;
            case MissionType.KillAllMonsters:
                return MissionManager.Instance.HasMovingEnemyObstacles();
            case MissionType.FindGrayGrass:
                return MissionManager.Instance.isFindGrayGrass;
            case MissionType.KillPawn:
                return MissionManager.Instance.isKillTwoPawn;
            case MissionType.KillKnight:
                return MissionManager.Instance.isKillTwoKnight;
            case MissionType.DestroyHouse:
                return MissionManager.Instance.isDestroyHouse;
            default:
                return false;
        }
    }
}