using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;
using Random = UnityEngine.Random;

public class KnightBehaviour : Obstacle, IObstacleBehaviour
{
    enum KnightNextStep
    {
        UpRight,
        UpLeft,
        RightUp,
        RightDown,
        DownRight,
        DownLeft,
        LeftUp,
        LeftDown
    }
    [SerializeField] private float duration = 0.4f;

    private List<KnightNextStep> randNextStep = new();
    private bool playerTriggered;
    private bool isMoving = false;

    public void DoLogic()
    {
        // 애니메이션 중엔 재호출 금지
        if (isMoving || DOTween.IsTweening(transform)) return;

        randNextStep.Clear();
        playerTriggered = false;

        // 8 방향 중에 이동 가능한 방향을 리스트에 추가
        foreach (KnightNextStep step in Enum.GetValues(typeof(KnightNextStep)))
        {
            if (CanNextStep(step))
                randNextStep.Add(step);

            if (playerTriggered)
            {
                MoveKnight(step);
                return;
            }
        }

        if (randNextStep.Count > 0)
        {
            int randIndex = Random.Range(0, randNextStep.Count); // 0 ~ Count-1
            MoveKnight(randNextStep[randIndex]);
        }
    }

    private Vector2Int GetDirection(KnightNextStep step)
    {
        return step switch
        {
            KnightNextStep.UpRight => new Vector2Int(1, 2),
            KnightNextStep.UpLeft => new Vector2Int(-1, 2),
            KnightNextStep.RightUp => new Vector2Int(2, 1),
            KnightNextStep.RightDown => new Vector2Int(2, -1),
            KnightNextStep.DownRight => new Vector2Int(1, -2),
            KnightNextStep.DownLeft => new Vector2Int(-1, -2),
            KnightNextStep.LeftUp => new Vector2Int(-2, 1),
            KnightNextStep.LeftDown => new Vector2Int(-2, -1),
            _ => Vector2Int.zero
        };
    }

    private bool CanNextStep(KnightNextStep nextStep)
    {
        Vector2Int direction = GetDirection(nextStep);

        Vector2Int nextPosition = obstaclePosition + direction;
        Tile nextTile = BoardManager.Instance.GetTile(nextPosition);

        // 보드 안밖인지
        if (nextTile == null)
            return false;

        // 보드 시작점 도착점 이동 막기
        if (!BoardManager.Instance.IsMovementArea(nextPosition))
            return false;

        // 기물 포착 !
        if (nextTile.GetPiece() != null)
        {
            playerTriggered = true;
            return false;
        }

        // 룩 제외 모든 장애물 부술 수 있음
        if (nextTile.Obstacle == ObstacleType.Rook || nextTile.Obstacle == ObstacleType.Knight ||
            nextTile.Obstacle == ObstacleType.House || nextTile.Obstacle == ObstacleType.Pawn)
            return false;

        return true;
    }

    private void MoveKnight(KnightNextStep nextStep)
    {
        Vector2Int direction = GetDirection(nextStep);

        Vector2Int nextPosition = obstaclePosition + direction;
        Tile nextTile = BoardManager.Instance.GetTile(nextPosition);

        if (nextTile.Obstacle != ObstacleType.None && nextTile.Obstacle != ObstacleType.Knight &&
            nextTile.Obstacle != ObstacleType.House && nextTile.Obstacle != ObstacleType.Pawn)
        {
            // 장애물 부수기
            BoardManager.Instance.RemoveObstacleAtPosition(nextPosition);
            Debug.Log("장애물 부수기 !");
        }
        if (nextTile.GetPiece() != null)
        {
            // 기물 보내기
            StartCoroutine(GoHand(nextTile.GetPiece()));
        }

        isMoving = true;

        BoardManager.Instance.MoveObstacle(this, nextPosition);
        AnimateObstacleMove(nextStep);
    }

    private void AnimateObstacleMove(KnightNextStep nextStep)
    {
        Vector2Int direction = GetDirection(nextStep);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(direction.x, direction.y, 0);

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMove(targetPos, duration).SetEase(Ease.InOutSine));
        seq.OnComplete(() =>
        {
            transform.position = new Vector3(
                BoardManager.Instance.boardTransform.position.x + obstaclePosition.x,
                BoardManager.Instance.boardTransform.position.y + obstaclePosition.y,
                0
            );

            BoardManager.Instance.Board[obstaclePosition.x, obstaclePosition.y].TileColor = TileColor.Gray;
            BoardManager.Instance.Board[obstaclePosition.x, obstaclePosition.y].SetTileColor(Color.gray);

            isMoving = false;
        });
    }

    void OnDestroy()
    {
        ObstacleManager.Instance.RemoveKnightToList(gameObject);

        GameObject pawn = BoardManager.Instance.CreateObstacle(obstaclePosition, ObstacleType.Pawn);
        ObstacleManager.Instance.AddPawnToList(pawn);

        UIManager.Instance.UpdateMissionUI();
    }
}