using UnityEngine;

public class GrassBehaviour : Obstacle, IObstacleBehaviour
{
    public void DoLogic()
    {
        Tile currentTile = BoardManager.Instance.GetTile(this.obstaclePosition);
        var piece = currentTile.GetPiece();
        if (piece == null) return;

        // 만약 미션중에 회색 풀 찾기 미션이 있다면
        StageManager.Instance.currentStage.missions.ForEach(mission =>
        {
            if (mission.missionType is MissionType.FindGrayGrass)
            {
                MissionManager.Instance.AddGrayGrassMission();
            }
        });

        RuleEvents.TriggerRule("Grass_Passive");

        // 이미 질병 상태면 아무것도 하지 않음
        if (piece.statusEffectController.IsStatusActive(PieceStatus.Disease)) return;

        string className = piece.GetTopFace().classData.className;

        if (currentTile.GetPiece().GetTopFace().classData.className == "Priest")
        {
            Debug.Log("저주를 무시합니다.");
            ToastManager.Instance.ShowToast("제게 저주는 통하지 않습니다.", currentTile.GetPiece().transform, 1f);
            BoardManager.Instance.RemoveObstacle(this);
            SkillManager.Instance.PriestPassive();
            return;
        }
        
        int rand = Random.Range(0, 10);

        if (className == "Knight")
        {
            if (rand < 2)
            {
                Debug.Log("기사가 20%의 확률로 질병에 걸렸습니다.");
                ToastManager.Instance.ShowToast("기사가 20%의 확률로 질병에 걸렸습니다.", piece.transform, 1f);
                RuleEvents.TriggerRule("Knight_Passive_DiseaseX2");

                piece.statusEffectController.SetStatus(PieceStatus.Disease, 2);
            }
            else
            {
                Debug.Log("기사가 80%의 확률로 질병을 극복했습니다.");
                ToastManager.Instance.ShowToast("기사가 80%의 확률로 질병을 극복했습니다.", piece.transform, 1f);


            }

            BoardManager.Instance.RemoveObstacle(this);
            return;
        }

        if (rand == 0)
        {
            ToastManager.Instance.ShowToast("10%의 확률로 질병에 걸렸습니다.", piece.transform, 0f);
            piece.statusEffectController.SetStatus(PieceStatus.Disease, 2);

            if (className == "Baby")
            {
                StartCoroutine(GoHand(piece));
                return;
            }
        }
        else
        {
            ToastManager.Instance.ShowToast("90%의 확률로 질병을 극복했습니다.", piece.transform, 1f);

            if (className == "Baby")
                return;
        }


        BoardManager.Instance.RemoveObstacle(this);
    }

}
