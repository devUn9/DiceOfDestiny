using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionUI : MonoBehaviour
{
    private enum MissionImageType
    {
        Zombie,
        Slime,
        GrayGrass,
        Pawn,
        Knight,
        House
    }

    [SerializeField] private GameObject MissionImageGroup;
    [SerializeField] private GameObject MissionImagePrefab;
    [SerializeField] private TextMeshProUGUI ProgressText;

    [SerializeField] private Sprite[] sprites = new Sprite[4];

    private void Start()
    {
        UIManager.Instance.InitMissionUI();
    }

    public void Init()
    {
        ResetMissionImage();

        StageManager.Instance.currentStage.missions.ForEach(mission =>
        {
            // 3스테이지
            if (mission.missionType is MissionType.KillAllMonsters)
            {
                CreateMissionImage(sprites[(int)MissionImageType.Zombie]);
                CreateMissionImage(sprites[(int)MissionImageType.Slime]);
            }
            // 5스테이지
            else if (mission.missionType is MissionType.FindGrayGrass)
            {
                CreateMissionImage(sprites[(int)MissionImageType.GrayGrass]);
            }
            // 6스테이지
            else if (mission.missionType is MissionType.KillPawn)
            {
                CreateMissionImage(sprites[(int)MissionImageType.Pawn]);
            }
            // 9스테이지
            else if (mission.missionType is MissionType.KillKnight)
            {
                CreateMissionImage(sprites[(int)MissionImageType.Knight]);
            }
            // 10스테이지
            else if (mission.missionType is MissionType.DestroyHouse)
            {
                CreateMissionImage(sprites[(int)MissionImageType.House]);
            }
            else
            {
                CreateMissionImage();
            }
        });
    }

    private void CreateMissionImage(Sprite _sprite = null)
    {
        if (_sprite == null)
            return;

        var missionImage = Instantiate(MissionImagePrefab, MissionImageGroup.transform);
        missionImage.GetComponent<Image>().sprite = _sprite;
    }

    private void ResetMissionImage()
    {
        foreach (Transform child in MissionImageGroup.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateProgressText()
    {
        StageManager.Instance.currentStage.missions.ForEach(mission =>
        {
            // 3스테이지
            if (mission.missionType is MissionType.KillAllMonsters)
            {
                ProgressText.text = $"{MissionManager.Instance.killEnemyCount} / 8";
            }
            // 5스테이지
            else if (mission.missionType is MissionType.FindGrayGrass)
            {
                ProgressText.text = $"{MissionManager.Instance.findGrayGrassCount} / 3";
            }
            // 6스테이지
            else if (mission.missionType is MissionType.KillPawn)
            {
                ProgressText.text = $"{MissionManager.Instance.alivePawnCount} / 2";
            }
            // 9스테이지
            else if (mission.missionType is MissionType.KillKnight)
            {
                ProgressText.text = $"{MissionManager.Instance.aliveKnightCount} / 2";
            }
            // 10스테이지
            else if (mission.missionType is MissionType.DestroyHouse)
            {
                ProgressText.text = $"{MissionManager.Instance.aliveHouseCount} / 2";
            }
            else
            {
                ProgressText.text = " ";
            }
        });
    }
}
