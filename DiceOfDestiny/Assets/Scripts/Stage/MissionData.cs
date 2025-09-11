using UnityEngine;

public enum MissionType
{
    FindGrayGrass,
    FirstActiveSkillUse,
    FirstMovePiece,
    FirstPassiveSkillUse,
    KillAllMonsters,
    KillPawn,
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
            case MissionType.FirstActiveSkillUse:
                return MissionManager.Instance.isFirstActiveSkillUse;
            case MissionType.FirstMovePiece:
                return MissionManager.Instance.isFirstMovePiece;
            case MissionType.FirstPassiveSkillUse:
                return MissionManager.Instance.isFirstPassiveSkillUse;
            case MissionType.KillAllMonsters:
                return MissionManager.Instance.HasMovingEnemyObstacles();
            case MissionType.KillPawn:
                return MissionManager.Instance.isKillTwoPawn;
            case MissionType.ReachFinishLine:
                return MissionManager.Instance.isFinishLine;
            default:
                return false;
        }
    }
}