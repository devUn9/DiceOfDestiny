using UnityEngine;

[CreateAssetMenu(menuName = "Missions/ReachFinishLine")]
public class ReachFinishLineSO : MissionSO
{
    public override bool IsMissionCompleted()
    {
        return PieceManager.Instance.currentPiece.isFinishLine;
    }
}