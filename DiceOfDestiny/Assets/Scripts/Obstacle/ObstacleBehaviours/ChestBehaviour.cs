using UnityEngine;

public class ChestBehaviour : Obstacle, IObstacleBehaviour
{
    public void DoLogic()
    {
        Tile currentTile = BoardManager.Instance.GetTile(obstaclePosition);
        var piece = currentTile.GetPiece();

        if (piece != null)
        {
            string className = piece.GetTopFace().classData.className;

            if (className == "Thief")
            {
                Debug.Log("도둑이 상자를 엽니다.");

                RuleEvents.TriggerRule("Thief_Passive_OpenBox");

                SkillManager.Instance.ThiefPassive();
                
                string item = InventoryManager.Instance.GetRandomItem();

                ToastManager.Instance.ShowToast($"{item}을(를) 얻었습니다.", this.transform, 1.5f);

                BoardManager.Instance.RemoveObstacle(this);

                return;
            }

            Debug.Log("상자는 도둑만 열 수 있습니다.");
            ToastManager.Instance.ShowToast("상자는 도둑만 열 수 있습니다.", this.transform, 1f);
        }
    }
}
