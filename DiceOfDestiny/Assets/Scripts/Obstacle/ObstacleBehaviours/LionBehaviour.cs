using UnityEngine;

public class LionBehaviour : Obstacle, IObstacleBehaviour
{
    [SerializeField] private bool isAttackReady = false;
    [SerializeField] private int standByAttack = 0;

    public void DoLogic()
    {
        isAttackReady = false;

        DetectPlayer(obstaclePosition);

        if (isAttackReady)
        {
            standByAttack++;
        }
        else
        {
            standByAttack = 0;
        }
    }

    private void Attack(PieceController pieceController)
    {
        StartCoroutine(GoHand(pieceController));

        // 공격 대기 초기화
        standByAttack = 0;

        // 기물 선택 타일 제거
        BoardSelectManager.Instance.DestroyPieceHighlightTile();
    }

    private void DetectPlayer(Vector2Int position)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(-1, -1), // 좌상
            new Vector2Int(-1, 0),  // 좌
            new Vector2Int(-1, 1),  // 좌하
            new Vector2Int(0, -1),  // 상
            new Vector2Int(0, 1),   // 하
            new Vector2Int(1, -1),  // 우상
            new Vector2Int(1, 0),   // 우
            new Vector2Int(1, 1),    // 우하
            new Vector2Int(0, 0)    // 중앙
        };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int detection = position + directions[i];

            // 감지하려는 위치가 밖이면 continue
            if (!IsInsideBoard(detection))
            {
                continue;
            }
            if (BoardManager.Instance.Board[detection.x, detection.y].GetPiece() != null)
            {
                if (standByAttack >= 2)
                {
                    // 공격
                    Attack(BoardManager.Instance.Board[detection.x, detection.y].GetPiece());
                }

                isAttackReady = true;
                return;
            }
        }
    }
    public bool IsInsideBoard(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < BoardManager.Instance.Board.GetLength(0)
            && pos.y >= 0 && pos.y < BoardManager.Instance.Board.GetLength(1);
    }
}
