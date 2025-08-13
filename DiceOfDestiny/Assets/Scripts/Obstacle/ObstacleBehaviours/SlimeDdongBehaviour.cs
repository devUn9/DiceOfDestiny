using UnityEngine;

public class SlimeDdongBehaviour : Obstacle, IObstacleBehaviour
{
    [SerializeField] private Sprite[] smallDdong;

    [SerializeField] private int remainingTurn = 2;

    private TileColor lastTileColor;

    private void Start()
    {
        lastTileColor = BoardManager.Instance.Board[obstaclePosition.x, obstaclePosition.y].TileColor;
        BoardManager.Instance.Board[obstaclePosition.x, obstaclePosition.y].TileColor = TileColor.None;
    }

    public void DoLogic()
    {
        remainingTurn--;

        if (remainingTurn < 0)
        {
            BoardManager.Instance.RemoveObstacle(this);
            BoardManager.Instance.Board[obstaclePosition.x, obstaclePosition.y].TileColor = lastTileColor;
            return;
        }

        this.spriteRenderer.sprite = smallDdong[remainingTurn];

        Tile currentTile = BoardManager.Instance.GetTile(this.obstaclePosition);
        var piece = currentTile.GetPiece();
        if (piece == null) return;

        RuleEvents.TriggerRule("Slime_Move_Ddong");
    }
}