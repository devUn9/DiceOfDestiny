using System.Collections;
using UnityEngine;

public enum RookArrowDirection
{
    Right = 0,
    Up = 90,
    Left = 180,
    Down = 270
}

public class RookBehaviour : Obstacle, IObstacleBehaviour
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] private int arrowStunTurns = 1;
    [SerializeField] private int cooldown = 2;
    private int currentcooldown = 0;

    public void DoLogic()
    {
        if (currentcooldown >= cooldown)
        {
            StartCoroutine(CheckPiece());
            currentcooldown = 0;
        }
        else
            ++currentcooldown;
    }

    private IEnumerator CheckPiece()
    {
        yield return new WaitForSeconds(1.0f);

        foreach (var piece in PieceManager.Instance.Pieces)
        {
            Vector2Int diff = piece.gridPosition - obstaclePosition;

            // 같은 가로줄
            if (diff.y == 0)
            {
                var dir = diff.x > 0 ? Vector2Int.right : Vector2Int.left;
                var arrowDir = diff.x > 0 ? RookArrowDirection.Right : RookArrowDirection.Left;
                CreateArrow(arrowDir, dir, piece);
            }
            // 같은 세로줄
            else if (diff.x == 0)
            {
                var dir = diff.y > 0 ? Vector2Int.up : Vector2Int.down;
                var arrowDir = diff.y > 0 ? RookArrowDirection.Up : RookArrowDirection.Down;
                CreateArrow(arrowDir, dir, piece);
            }
        }
    }

    private void CreateArrow(RookArrowDirection arrowDir, Vector2Int dir, PieceController piece)
    {
        GameObject Arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        Arrow.transform.rotation = Quaternion.Euler(0, 0, (float)arrowDir);
        Arrow.GetComponent<RookArrow>().Init(dir);

        Vector2Int pos = piece.gridPosition;
        Vector2Int step = dir; // (1,0), (-1,0), (0,1), (0,-1)

        int x = pos.x;
        int y = pos.y;

        // 이미 기절이면 면역
        if (piece.statusEffectController.IsStatusActive(PieceStatus.Stun))
            return;

        // 풀 속 아기는 면역
        if (piece.GetTopFace().classData.className == "Baby" &&
            BoardManager.Instance.Board[x, y].Obstacle == ObstacleType.Grass)
        {
            return;
        }

        while (true)
        {
            x -= step.x;
            y -= step.y;

            if (x == obstaclePosition.x && y == obstaclePosition.y)
                break;

            if (CheckBlocked(x, y))
                return; // 중간에 막힘 → 함수 종료
        }

        // 막히지 않았다면 피스 타격
        HitPiece(piece);
    }

    private bool CheckBlocked(int x, int y)
    {
        if (BoardManager.Instance.Board[x, y].Obstacle == ObstacleType.Tree ||
            BoardManager.Instance.Board[x, y].Obstacle == ObstacleType.Rock ||
            BoardManager.Instance.Board[x, y].GetPiece() != null)
        {
            return true;
        }
        return false;
    }

    private void HitPiece(PieceController piece)
    {
        piece.statusEffectController.SetStatus(PieceStatus.Stun, arrowStunTurns);
        ToastManager.Instance.ShowToast($"화살에 맞았습니다.! {arrowStunTurns}턴간 기절합니다.", piece.transform, 1f);
    }
}