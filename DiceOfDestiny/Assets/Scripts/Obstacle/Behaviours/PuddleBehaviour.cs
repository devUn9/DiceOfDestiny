using UnityEngine;

public class PuddleBehaviour : Obstacle, IObstacleBehaviour
{
    public void DoLogic()
    {
        Tile currentTile = BoardManager.Instance.GetTile(obstaclePosition);
        var piece = currentTile.GetPiece();

        if (piece != null)
        {
            if (piece.statusEffectController.IsStatusActive(StatusType.Disease))
                return;

            string className = piece.GetTopFace().classData.className;

            if (className == "Knight")
            {
                Debug.Log("기사가 확률 100%로 질병에 걸렸습니다.");
                ToastManager.Instance.ShowToast("기사가 확률 100%로 질병에 걸렸습니다.", piece.transform, 1f);

                piece.statusEffectController.SetStatus(StatusType.Disease, 2);
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
                Debug.Log("확률 50%로 질병에 걸렸습니다.");
                ToastManager.Instance.ShowToast("확률 50%로 질병에 걸렸습니다.", piece.transform, 1f);

                if (className == "Baby")
                {
                    Debug.Log("패로 돌아가는 코루틴 시작");
                    
                    StartCoroutine(GoHand(piece));
                }

                // 질병 디버프 걸리는 함수 실행
                piece.statusEffectController.SetStatus(StatusType.Disease, 2);
            }
        }
    }
}
