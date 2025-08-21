using UnityEngine;
using DG.Tweening;

public class ZombieBehaviour : Obstacle, IObstacleBehaviour
{
    private ObstacleType lastObstacleType = ObstacleType.None;

    [SerializeField] private bool isStun = false;
    [SerializeField] private int stunTime = 0;

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

        if (isStun)
        {
            stunTime--;

            animator.SetTrigger("Stun");

            if (stunTime <= 0)
            {
                isStun = false;
            }
            return;
        }

        if (nextTile == null)
        {
            AnimateObstacleHalfBack(nextStep);
            nextStep = oppositeStep;

            isStun = true;
            stunTime = 2;

            return;
        }

        if (nextTile.GetPiece() == null)
        {
            // 좀비가 밟을수 있는거 : 풀 독초 슬라임똥 상자 물웅덩이
            if (nextTile.Obstacle == ObstacleType.None || nextTile.Obstacle == ObstacleType.Grass
                || nextTile.Obstacle == ObstacleType.PoisonousHerb || nextTile.Obstacle == ObstacleType.SlimeDdong
                || nextTile.Obstacle == ObstacleType.Chest || nextTile.Obstacle == ObstacleType.Puddle)
            {
                // 이동
                BoardManager.Instance.MoveObstacle(this, nextPosition, ref lastObstacleType);
                AnimateObstacleMove(nextStep);

                RuleEvents.TriggerRule("Zombie_Move");
            }
            else
            {
                AnimateObstacleHalfBack(nextStep);
                nextStep = oppositeStep;

                isStun = true;
                stunTime = 2;

                RuleEvents.TriggerRule("Zombie_Move_MeetObstacle");
            }
        }
        else
        {
            ZombieAttackProcess(oppositeStep, nextTile);
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

    private void AnimateObstacleMove(NextStep nextStep)
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

    private void AnimateObstacleHalfBack(NextStep nextStep)
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

            seq.AppendCallback(() => { animator.SetTrigger("Stun"); });

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

    private void AnimateZombieNyamNyam(NextStep nextStep)
    {
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

        seq.AppendInterval(1f);
        seq.AppendCallback(() => { animator.SetTrigger("Eat"); });

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

    private void ZombieAttackProcess(NextStep _oppositeStep, Tile _nextTile)
    {
        if (_nextTile.GetPiece().GetTopFace().classData.IsCombatClass || _nextTile.GetPiece().statusEffectController.IsStatusActive(PieceStatus.Stun))
        {
            AnimateObstacleHalfBack(nextStep);
            nextStep = _oppositeStep;

            isStun = true;
            stunTime = 2;
            
            ToastManager.Instance.ShowToast("어림도 없지! <color=red>(팅!)</color>", _nextTile.GetPiece().transform, 1f);

            RuleEvents.TriggerRule("Zombie_VsCombat_RecoilStun");
        }
        else
        {
            AnimateZombieNyamNyam(nextStep);
            nextStep = _oppositeStep;

            if (_nextTile.GetPiece().GetTopFace().classData.className == "Priest")
            {
                SkillManager.Instance.PriestPassive();
                ToastManager.Instance.ShowToast("감히 더러운 <color=grey>언데드</color> 따위가!", _nextTile.GetPiece().transform, 1f);
                return;
            }

            var stunTurns = 2;
            _nextTile.GetPiece().statusEffectController.SetStatus(PieceStatus.Stun, stunTurns);
            ToastManager.Instance.ShowToast($"좀비한테 물렸습니다! {stunTurns}턴간 기절합니다.", _nextTile.GetPiece().transform, 1f);

            RuleEvents.TriggerRule("Zombie_VsNonCombat_BiteStun");
        }
    }
}
