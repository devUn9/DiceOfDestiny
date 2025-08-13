using UnityEditor.SceneManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "Missions/FindGrayGrass")]
public class FindGrayGrassSO : MissionSO
{
    public override bool IsMissionCompleted()
    {
        return StageManager.Instance.isFindGrayGrass;
    }
}