using UnityEngine;
using UnityEngine.UI;

public class PuddleBehaviour : Obstacle, IObstacleBehaviour
{
    public void DoLogic()
    {
        Tile currentTile = BoardManager.Instance.GetTile(obstaclePosition);
        var piece = currentTile.GetPiece();
        if (piece == null) return;

        RuleEvents.TriggerRule("Puddle_Passive");

        if (piece.statusEffectController.IsStatusActive(PieceStatus.Disease))
            return;

        string className = piece.GetTopFace().classData.className;

        if (currentTile.GetPiece().GetTopFace().classData.className == "Priest")
        {
            Debug.Log("저주를 무시합니다.");
            ToastManager.Instance.ShowToast("제게 저주는 통하지 않습니다.", currentTile.GetPiece().transform, 1f);
            SkillManager.Instance.PriestPassive();
            return;
        }

        if (className == "Knight")
        {
            Debug.Log("기사가 100%의 확률로 질병에 걸렸습니다.");
            ToastManager.Instance.ShowToast("기사가 100%의 확률로 질병에 걸렸습니다.", piece.transform, 1f);
            RuleEvents.TriggerRule("Knight_Passive_DiseaseX2");

            piece.statusEffectController.SetStatus(PieceStatus.Disease, 2);
            return;
        }

        if (className == "Baby")
        {
            Debug.Log("아기가 물에 빠져서 패로 돌아갑니다.");
            ToastManager.Instance.ShowToast($"응애! <color=blue>(퐁당)</color>", piece.transform, 0f);
            StartCoroutine(GoHand(piece));
            return;
        }

        int rand = Random.Range(0, 2);
        if (rand == 0)
        {
            Debug.Log("50%의 확률로 질병을 극복했습니다.");
            ToastManager.Instance.ShowToast("50%의 확률로 질병을 극복했습니다.", piece.transform, 1f);
        }
        else
        {
            Debug.Log("50%의 확률로 질병에 걸렸습니다.");
            ToastManager.Instance.ShowToast("50%의 확률로 질병에 걸렸습니다.", piece.transform, 1f);

            // 질병 디버프 걸리는 함수 실행
            piece.statusEffectController.SetStatus(PieceStatus.Disease, 2);
        }

    }
}
