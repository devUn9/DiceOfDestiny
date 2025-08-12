using UnityEngine;
using DG.Tweening;

public class ZombieBehaviour : Obstacle, IObstacleBehaviour
{
    public void DoLogic()
    {
        if (nextStep == NextStep.None)
        {
            nextStep = Random.Range(0, 2) == 1 ? NextStep.Left : NextStep.Right;
        }

        Vector2Int direction = GetDirection(nextStep);
        NextStep oppositeStep = GetOppositeStep(nextStep);

        Vector2Int nextPosition = obstaclePosition + direction;
        Tile nextTile = BoardManager.Instance.GetTile(nextPosition);

        if (nextTile == null)
        {
            AnimateObstacleHalfBack(nextStep);
            nextStep = oppositeStep;
            return;
        }

        if (nextTile.GetPiece() == null)
        {
            if (nextTile.Obstacle == ObstacleType.None)
            {
                BoardManager.Instance.MoveObstacle(this, nextPosition);
                AnimateObstacleMove(nextStep);

                RuleEvents.TriggerRule("Zombie_Move");
            }
            else
            {
                AnimateObstacleHalfBack(nextStep);
                nextStep = oppositeStep;

                RuleEvents.TriggerRule("Zombie_Move_MeetObstacle");
            }
        }
        else
        {
            if (nextTile.GetPiece().GetTopFace().classData.IsCombatClass || nextTile.GetPiece().statusEffectController.IsStatusActive(StatusType.Stun))
            {
                AnimateObstacleHalfBack(nextStep);
                nextStep = oppositeStep;
                ToastManager.Instance.ShowToast("어림도 없지! <color=red>(팅!)</color>", nextTile.GetPiece().transform, 1f);

                RuleEvents.TriggerRule("Zombie_VsCombat_RecoilStun");
            }
            else
            {
                AnimateZombieNyamNyam(nextStep);
                nextStep = oppositeStep;

                if (nextTile.GetPiece().GetTopFace().classData.className == "Priest")
                {
                    Debug.Log("사제는 기절을 무시합니다.");
                    ToastManager.Instance.ShowToast("감히 더러운 <color=grey>언데드</color> 따위가!", nextTile.GetPiece().transform, 1f);
                    return;
                }
                Debug.Log("Piece Stun!");
                var stunTurns = 2;
                nextTile.GetPiece().statusEffectController.SetStatus(StatusType.Stun, stunTurns);
                ToastManager.Instance.ShowToast($"좀비한테 물렸습니다! {stunTurns}턴간 기절합니다.", nextTile.GetPiece().transform, 1f);

                RuleEvents.TriggerRule("Zombie_VsNonCombat_BiteStun");
            }
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

        float duration = 0.4f;
        float jumpHeight = 0.2f;

        if (direction.x != 0) // 좌우 이동은 점프 효과
        {
            Sequence seq = DOTween.Sequence();

            // 1) X축 이동 (duration 전체)
            seq.Append(transform.DOMoveX(targetPos.x, duration).SetEase(Ease.InOutSine));

            // 2) Y축 점프 (올라갔다 내려오기) - duration 전체, Y만 움직임
            seq.Join(transform.DOMoveY(startPos.y + jumpHeight, duration / 2).SetEase(Ease.OutSine));
            seq.Append(transform.DOMoveY(startPos.y, duration / 2).SetEase(Ease.InSine));

        }
        else // 상하 이동은 자연스러운 이동
        {
            transform.DOMove(targetPos, duration).SetEase(Ease.InOutSine);
        }
    }

    public void AnimateObstacleHalfBack(NextStep nextStep)
    {
        Vector2Int direction = GetDirection(nextStep);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(direction.x, direction.y, 0);

        float duration = 0.6f;
        float jumpHeight = 0.2f;

        float ratio = 0.7f;
        Vector3 hitPos = Vector3.Lerp(startPos, targetPos, ratio);

        if (direction.x != 0) // 좌우 이동은 점프 효과
        {
            Sequence seq = DOTween.Sequence();

            // 1) X축 이동 (duration 전체)
            seq.Append(transform.DOMoveX(hitPos.x, duration / 3).SetEase(Ease.InSine));
            // 2) Y축 점프 (올라갔다 내려오기) - duration 전체, Y만 움직임
            seq.Append(transform.DOMoveY(startPos.y + jumpHeight, duration / 3).SetEase(Ease.OutSine));
            seq.Join(transform.DOMoveX(startPos.x, duration / 3 * 2).SetEase(Ease.OutSine));
            seq.Append(transform.DOMoveY(startPos.y, duration / 3).SetEase(Ease.InSine));
        }
        else
        {
            Sequence seq = DOTween.Sequence();

            seq.Append(transform.DOMoveY(hitPos.y, duration / 2).SetEase(Ease.OutSine));
            seq.Append(transform.DOMoveY(startPos.y, duration / 2).SetEase(Ease.InSine));
        }
    }

    public void AnimateZombieNyamNyam(NextStep nextStep)
    {
        Vector2Int direction = GetDirection(nextStep);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 offset;
        float angle;

        if (nextStep == NextStep.Left || nextStep == NextStep.Up)
        {
            angle = 45f;
            offset = (nextStep == NextStep.Left) ? new Vector3(-0.5f, 0.5f, 0) : new Vector3(0.5f, 0.5f, 0);
        }
        else
        {
            angle = -45f;
            offset = (nextStep == NextStep.Right) ? new Vector3(0.5f, 0.5f, 0) : new Vector3(-0.5f, 0.5f, 0);
        }

        Vector3 targetPos = startPos + offset;
        float shakeAmount = 0.05f;
        float shakeDuration = 0.1f;

        Sequence seq = DOTween.Sequence();

        // 회전 및 위치 이동
        seq.Append(transform.DORotate(new Vector3(0, 0, angle), 0.2f).SetEase(Ease.InOutSine));
        seq.Join(transform.DOMove(targetPos, 0.2f).SetEase(Ease.InOutSine));

        // 위아래 흔들기 3번
        for (int i = 0; i < 3; i++)
        {
            seq.Append(transform.DOMoveY(targetPos.y + shakeAmount, shakeDuration).SetEase(Ease.InOutSine));
            seq.Append(transform.DOMoveY(targetPos.y - shakeAmount, shakeDuration).SetEase(Ease.InOutSine));
        }

        // 원래 회전, 위치 복귀
        seq.Append(transform.DORotateQuaternion(startRot, 0.2f).SetEase(Ease.InOutSine));
        seq.Join(transform.DOMove(startPos, 0.2f).SetEase(Ease.InOutSine));
    }
}
