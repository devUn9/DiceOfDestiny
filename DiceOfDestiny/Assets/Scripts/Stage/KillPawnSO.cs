using UnityEngine;

[CreateAssetMenu(menuName = "Missions/KillPawnSO")]
public class KillPawnSO : MissionSO
{
    public override bool IsMissionCompleted()
    {
        return false;
    }
}
