using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PawnBehaviour : Obstacle, IObstacleBehaviour
{
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private GameObject attackEffectPrefab;

    private void Start()
    {
        BoardManager.Instance.Board[obstaclePosition.x, obstaclePosition.y].TileColor = TileColor.Gray;
        BoardManager.Instance.Board[obstaclePosition.x, obstaclePosition.y].SetTileColor(Color.gray);
    }

    public void DoLogic()
    {
        if (ObstacleManager.Instance.GetPawnListIndex(gameObject) != ObstacleManager.Instance.pawnMoveIndex)
        {
            return;
        }
        ObstacleManager.Instance.InOrderToMovePawn();

        // 이동 관련 변수
        Vector2Int direction = Vector2Int.down;
        nextStep = NextStep.Down;
        Vector2Int nextPosition = obstaclePosition + direction;
        Tile nextTile = BoardManager.Instance.GetTile(nextPosition);

        // 공격 관련 변수
        Vector2Int leftDownDirection = new Vector2Int(-1, -1);
        Vector2Int rightDownDirection = new Vector2Int(1, -1);
        Vector2Int leftDownAttackPos = obstaclePosition + leftDownDirection;
        Vector2Int rightDownAttackPos = obstaclePosition + rightDownDirection;
        Tile leftDownTile = BoardManager.Instance.GetTile(leftDownAttackPos);
        Tile rightDownTile = BoardManager.Instance.GetTile(rightDownAttackPos);

        // 참조하려는 좌표값이 보드 밖이면 return // 근데 폰이 시작점에 도달하면 끝나는 로직을 추가할 때, 아래 코드 제거 예정
        if (!BoardManager.Instance.IsInsideBoard(nextPosition))
            return;

        Debug.Log("왼쪽 아래 타일 좌표: " + leftDownAttackPos + ", 오른쪽 아래 타일 좌표: " + rightDownAttackPos);

        if (BoardManager.Instance.IsInsideBoard(leftDownAttackPos) && leftDownTile.GetPiece() != null && leftDownTile.Obstacle == ObstacleType.None)
        {
            StartCoroutine(PlayAttackEffect(true, leftDownAttackPos, leftDownDirection));
            StartCoroutine(GoHand(leftDownTile.GetPiece()));

            return;
        }
        else if (BoardManager.Instance.IsInsideBoard(rightDownAttackPos) && rightDownTile.GetPiece() != null && rightDownTile.Obstacle == ObstacleType.None)
        {
            StartCoroutine(PlayAttackEffect(false, rightDownAttackPos, rightDownDirection));
            StartCoroutine(GoHand(rightDownTile.GetPiece()));

            return;
        }

        // 다음 타일이 장애물도 없고 피스도 없으면 이동
        if (nextTile.Obstacle == ObstacleType.None && nextTile.GetPiece() == null)
        {
            BoardManager.Instance.MoveObstacle(this, nextPosition);
            AnimateObstacleMove(direction);
        }
    }

    private void AnimateObstacleMove(Vector2Int direction)
    {
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
        });

        BoardManager.Instance.Board[obstaclePosition.x, obstaclePosition.y].TileColor = TileColor.Gray;
        BoardManager.Instance.Board[obstaclePosition.x, obstaclePosition.y].SetTileColor(Color.gray);
    }

    private IEnumerator PlayAttackEffect(bool isLeft, Vector2Int attackPos, Vector2Int dir)
    {
        float effectDelay = 0.5f;

        animator.SetTrigger("Attack");

        if (attackEffectPrefab != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, transform.position, Quaternion.identity);

            if (isLeft)
                effect.GetComponent<SpriteRenderer>().flipX = true;
            else
                effect.GetComponent<SpriteRenderer>().flipX = false;

            Destroy(effect, effectDelay); // 효과는 1초 후에 제거

            yield return new WaitForSeconds(effectDelay);

            BoardManager.Instance.MoveObstacle(this, attackPos);
            AnimateObstacleMove(dir);
        }
    }
}
