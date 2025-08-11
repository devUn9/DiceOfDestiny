using UnityEngine;
using DG.Tweening;

public class SlimeBehaviour : Obstacle, IObstacleBehaviour
{
    [SerializeField] private float duration = 0.4f;

    NextStep oppositeStep = NextStep.None;

    public void DoLogic()
    {
        int rand = Random.Range(0, 4); // 0~3
        switch (rand)
        {
            case 0: nextStep = NextStep.Up; break;
            case 1: nextStep = NextStep.Down; break;
            case 2: nextStep = NextStep.Left; break;
            case 3: nextStep = NextStep.Right; break;
        }

        if (nextStep == oppositeStep)
        {
            nextStep = GetOppositeStep(nextStep);
        }

        Vector2Int direction = GetDirection(nextStep);
        oppositeStep = GetOppositeStep(nextStep);

        Vector2Int nextPosition = obstaclePosition + direction;
        Tile nextTile = BoardManager.Instance.GetTile(nextPosition);

        if (nextTile == null)
        {
            animator.SetTrigger("Jump");

            if (nextStep == NextStep.Left)
                spriteRenderer.flipX = true;
            else
                spriteRenderer.flipX = false;
            return;
        }

        if (!BoardManager.Instance.IsMovementArea(nextPosition))
            return;

        if (nextTile.Obstacle == ObstacleType.None && nextTile.GetPiece() == null)
        {
            Vector2Int beforePosition = obstaclePosition;

            BoardManager.Instance.MoveObstacle(this, nextPosition);
            AnimateObstacleMove(nextStep);

            BoardManager.Instance.CreateObstacle(beforePosition, ObstacleType.SlimeDdong);

            RuleEvents.TriggerRule("Slime_Move");
        }
        else
        {
            animator.SetTrigger("Jump");
        }

        if (nextStep == NextStep.Left)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
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

    private NextStep GetOppositeStep(NextStep step)
    {
        return step switch
        {
            NextStep.Right => NextStep.Left,
            NextStep.Left => NextStep.Right,
            NextStep.Up => NextStep.Down,
            NextStep.Down => NextStep.Up,
            _ => NextStep.None
        };
    }

    public void AnimateObstacleMove(NextStep nextStep)
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
}