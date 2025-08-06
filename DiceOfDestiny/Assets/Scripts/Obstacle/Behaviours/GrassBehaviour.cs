using UnityEngine;

public class GrassBehaviour : Obstacle, IObstacleBehaviour
{
    public void DoLogic()
    {
        Tile currentTile = BoardManager.Instance.GetTile(this.obstaclePosition);
        var piece = currentTile.GetPiece();
        if (piece == null) return;

        // 이미 질병 상태면 아무것도 하지 않음
        if (piece.statusEffectController.IsStatusActive(StatusType.Disease)) return;

        string className = piece.GetTopFace().classData.className;

        if (className == "Baby")
        {
            Debug.Log("아기는 풀을 밟지 못 합니다.");
            ToastManager.Instance.ShowToast("아기는 풀을 밟지 못 합니다.", piece.transform, 1f);
            return;
        }

        int rand = Random.Range(0, 10);

        if (className == "Knight")
        {
            if (rand < 2)
            {
                Debug.Log("기사가 20%의 확률로 질병에 걸렸습니다.");
                ToastManager.Instance.ShowToast("기사가 20%의 확률로 질병에 걸렸습니다.", piece.transform, 1f);
                piece.statusEffectController.SetStatus(StatusType.Disease, 2);
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
            Debug.Log("10%의 확률로 질병에 걸렸습니다.");
            ToastManager.Instance.ShowToast("10%의 확률로 질병에 걸렸습니다.", piece.transform, 1f);
            if (className == "Baby")
            {
                StartCoroutine(GoHand(piece));
            }
            piece.statusEffectController.SetStatus(StatusType.Disease, 2);
        }
        else
        {
            Debug.Log("90%의 확률로 질병을 극복했습니다.");
            ToastManager.Instance.ShowToast("90%의 확률로 질병을 극복했습니다.", piece.transform, 1f);
        }


        BoardManager.Instance.RemoveObstacle(this);
    }

}
