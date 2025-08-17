using UnityEngine;

public class PoisonousherbBehaviour : Obstacle, IObstacleBehaviour
{
    public void DoLogic()
    {
        Tile currentTile = BoardManager.Instance.GetTile(obstaclePosition);

        if (currentTile.GetPiece() != null)
        {
            if (currentTile.GetPiece().GetTopFace().classData.className == "Priest")
            {
                Debug.Log("저주를 무시합니다.");
                ToastManager.Instance.ShowToast("제게 저주는 통하지 않습니다.", currentTile.GetPiece().transform, 1f);
                BoardManager.Instance.RemoveObstacle(this);
                SkillManager.Instance.PriestPassive();
                return;
            }

            var point = 1;
            PieceController currentPiece = currentTile.GetPiece();
            if (currentPiece.GetTopFace().classData.className == "Demon")
            {
                ActionPointManager.Instance.AddAP(point);
                Debug.Log($"악마가 독초를 밟아 행동력 +{point}");
                ToastManager.Instance.ShowToast($"독초를 밟아 {point} 행동력을 얻었습니다.", currentPiece.transform, 1f);
                RuleEvents.TriggerRule("PoisonousHerb_Passive_vsDemon");

                BoardManager.Instance.RemoveObstacle(this);
                return;
            }

            ActionPointManager.Instance.RemoveAP(point);
            Debug.Log($"독초를 밟아 행동력 -{point}");
            ToastManager.Instance.ShowToast($"독초를 밟아 {point} 행동력을 잃었습니다.", currentPiece.transform, 1f);
            RuleEvents.TriggerRule("PoisonousHerb_Passive");

            if (currentTile.GetPiece().GetTopFace().classData.className == "Baby")
            {
                Debug.Log("아기는 너무 작아서 밟아도 독초가 안 없어집니다.");
                ToastManager.Instance.ShowToast("아기는 너무 작아서 밟아도 독초가 없어지지 않습니다.", currentTile.GetPiece().transform, 1.5f);
                return;
            }

            BoardManager.Instance.RemoveObstacle(this);
        }
    }
}
