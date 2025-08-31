using System.Collections;
using UnityEngine;
using System.Collections.Generic;

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
    private bool blocked = false;

    public void DoLogic()
    {
        StartCoroutine(CheckPiece());
    }

    private IEnumerator CheckPiece()
    {
        yield return new WaitForSeconds(1.0f);

        foreach (var piece in PieceManager.Instance.Pieces)
        {
            // 같은 가로 열일 때
            if (obstaclePosition.y == piece.gridPosition.y)
            {
                if (obstaclePosition.x < piece.gridPosition.x)
                {
                    CreateArrow(RookArrowDirection.Right, Vector2Int.right, piece);
                }
                else if (obstaclePosition.x > piece.gridPosition.x)
                {
                    CreateArrow(RookArrowDirection.Left, Vector2Int.left, piece);
                }
            }
            // 같은 세로 열일 때
            else if (obstaclePosition.x == piece.gridPosition.x)
            {
                if (obstaclePosition.y < piece.gridPosition.y)
                {
                    CreateArrow(RookArrowDirection.Up, Vector2Int.up, piece);
                }
                else if (obstaclePosition.y > piece.gridPosition.y)
                {
                    CreateArrow(RookArrowDirection.Down, Vector2Int.down, piece);
                }
            }
        }
    }

    private void CreateArrow(RookArrowDirection arrowDir, Vector2Int dir, PieceController piece)
    {
        GameObject Arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        Arrow.transform.rotation = Quaternion.Euler(0, 0, (float)arrowDir);
        Arrow.GetComponent<RookArrow>().Init(dir);

        int pivotX = piece.gridPosition.x;
        int pivotY = piece.gridPosition.y;
        blocked = false;

        // 풀 속 아기는 면역
        if (piece.GetTopFace().classData.className == "Baby" &&
            BoardManager.Instance.Board[pivotX, pivotY].Obstacle == ObstacleType.Grass)
        {
            return;
        }

        // 좌우 확인
        if (dir == Vector2Int.right)
        {
            for (int x = pivotX - 1; x >= obstaclePosition.x + 1; x--)
            {
                if (CheckBlocked(x, pivotY))
                    break;
            }
        }
        else if (dir == Vector2Int.left)
        {
            for (int x = pivotX - 1; x <= obstaclePosition.x - 1; x++)
            {
                if (CheckBlocked(x, pivotY))
                    break;
            }
        }
        else if (dir == Vector2Int.up)
        {
            for (int y = pivotY - 1; y >= obstaclePosition.y + 1; y--)
            {
                if (CheckBlocked(pivotX, y))
                    break;
            }
        }
        else if (dir == Vector2Int.down)
        {
            for (int y = pivotY + 1; y <= obstaclePosition.y - 1; y++)
            {
                if (CheckBlocked(pivotX, y))
                    break;
            }
        }

        if (!blocked)
            HitPiece(piece);
    }

    private bool CheckBlocked(int x, int y)
    {
        if (BoardManager.Instance.Board[x, y].Obstacle == ObstacleType.Tree ||
            BoardManager.Instance.Board[x, y].Obstacle == ObstacleType.Rock ||
            BoardManager.Instance.Board[x, y].GetPiece() != null)
        {
            blocked = true;
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