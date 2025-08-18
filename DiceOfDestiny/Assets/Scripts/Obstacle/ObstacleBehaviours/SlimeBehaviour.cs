using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class SlimeBehaviour : Obstacle, IObstacleBehaviour
{
    [SerializeField] private float duration = 0.4f;

    private List<NextStep> randNextStep = new List<NextStep>();

    public void DoLogic()
    {
        randNextStep.Clear();

        if (CanNextStep(NextStep.Up)) randNextStep.Add(NextStep.Up);
        if (CanNextStep(NextStep.Down)) randNextStep.Add(NextStep.Down);
        if (CanNextStep(NextStep.Left)) randNextStep.Add(NextStep.Left);
        if (CanNextStep(NextStep.Right)) randNextStep.Add(NextStep.Right);

        if (randNextStep.Count > 0)
        {
            int randIndex = Random.Range(0, randNextStep.Count); // 0 ~ Count-1
            MoveSlime(randNextStep[randIndex]);
        }
    }

    private Vector2Int GetDirection(NextStep step)
    {
        return step switch
        {
            NextStep.Right => Vector2Int.right,
            NextStep.Left => Vector2Int.left,
            NextStep.Up => Vector2Int.up,
            NextStep.Down => Vector2Int.down,
            _ => Vector2Int.zero
        };
    }

    private void AnimateObstacleMove(NextStep nextStep)
    {
        Vector2Int direction = GetDirection(nextStep);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(direction.x, direction.y, 0);

        if (direction.x != 0) // 좌우 이동은 점프 효과
        {
            Sequence seq = DOTween.Sequence();

            seq.AppendCallback(() => animator.SetTrigger("Jump"));

            // 1) X축 이동 (duration 전체)
            seq.Append(transform.DOMoveX(targetPos.x, duration).SetEase(Ease.InOutSine));
            seq.OnComplete(() =>
            {
                // DOTween 애니메이션 끝난 후 위치를 보드 기준으로 맞춤
                transform.position = new Vector3(
                    BoardManager.Instance.boardTransform.position.x + obstaclePosition.x,
                    BoardManager.Instance.boardTransform.position.y + obstaclePosition.y,
                    0
                );
            });
        }
        else // 상하 이동은 자연스러운 이동
        {
            Sequence seq = DOTween.Sequence();

            seq.AppendCallback(() => animator.SetTrigger("Jump"));

            seq.Append(transform.DOMove(targetPos, duration).SetEase(Ease.InOutSine));
            seq.OnComplete(() =>
            {
                transform.position = new Vector3(
                    BoardManager.Instance.boardTransform.position.x + obstaclePosition.x,
                    BoardManager.Instance.boardTransform.position.y + obstaclePosition.y,
                    0
                );
            });
        }
    }

    private bool CanNextStep(NextStep nextStep)
    {
        Vector2Int direction = GetDirection(nextStep);

        Vector2Int nextPosition = obstaclePosition + direction;
        Tile nextTile = BoardManager.Instance.GetTile(nextPosition);

        // 좌우 반전
        if (nextTile == null)
        {
            animator.SetTrigger("Jump");

            if (nextStep == NextStep.Left)
                spriteRenderer.flipX = true;
            else
                spriteRenderer.flipX = false;
            return false;
        }

        // 보드 안밖인지
        if (!BoardManager.Instance.IsMovementArea(nextPosition))
            return false;

        if (nextTile.Obstacle == ObstacleType.None && nextTile.GetPiece() == null)
        {
            return true;
        }

        return false;
    }

    private void MoveSlime(NextStep nextStep)
    {
        Vector2Int direction = GetDirection(nextStep);

        Vector2Int nextPosition = obstaclePosition + direction;
        Tile nextTile = BoardManager.Instance.GetTile(nextPosition);

        if (nextTile.Obstacle == ObstacleType.None && nextTile.GetPiece() == null)
        {
            Vector2Int beforePosition = obstaclePosition;

            BoardManager.Instance.MoveObstacle(this, nextPosition);
            AnimateObstacleMove(nextStep);

            BoardManager.Instance.CreateObstacle(beforePosition, ObstacleType.SlimeDdong);

            RuleEvents.TriggerRule("Slime_Move");
        }

        // 좌우 반전
        if (nextStep == NextStep.Left)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
    }
}