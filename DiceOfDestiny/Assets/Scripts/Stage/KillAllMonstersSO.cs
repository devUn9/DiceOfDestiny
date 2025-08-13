using UnityEngine;

[CreateAssetMenu(menuName = "Missions/KillAllMonsters")]
public class KillAllMonstersSO : MissionSO
{
    public override bool IsMissionCompleted()
    {
        return !BoardManager.Instance.HasMovingEnemyObstacles();
    }
}