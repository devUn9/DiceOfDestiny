using UnityEngine;

public enum MissionType
{
    FindGrayGrass,
    KillPawn,
    KillKnight,
    DestroyHouse,
    FirstMovePiece,
    KillAllMonsters,
    ReachFinishLine,
}

[CreateAssetMenu(menuName = "Mission/MissionData")]
public class MissionData : ScriptableObject
{
    public MissionType missionType;

    public bool IsCompleted()
    {
        switch (missionType)
        {
            case MissionType.FindGrayGrass:
                return MissionManager.Instance.isFindGrayGrass;
            case MissionType.FirstMovePiece:
                return MissionManager.Instance.isFirstMovePiece;
            case MissionType.KillAllMonsters:
                return MissionManager.Instance.HasMovingEnemyObstacles();
            case MissionType.KillPawn:
                return MissionManager.Instance.isKillTwoPawn;
            case MissionType.KillKnight:
                return MissionManager.Instance.isKillTwoKnight;
            case MissionType.DestroyHouse:
                return MissionManager.Instance.isDestroyHouse;
            case MissionType.ReachFinishLine:
                return MissionManager.Instance.isFinishLine;
            default:
                return false;
        }
    }
}