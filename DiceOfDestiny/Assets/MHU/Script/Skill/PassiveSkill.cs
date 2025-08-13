using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveSkill : MonoBehaviour
{
    [SerializeField] private GameObject knightPassiveEffect;
    [SerializeField] private GameObject demonPassiveEffect;
    [SerializeField] private GameObject fanaticPassiveEffect;
    [SerializeField] private GameObject priestPassiveEffect;
    [SerializeField] private GameObject thiefPassiveEffect;


    // 기사 공격 스킬
    public IEnumerator KnightAttack(PieceController pieceController)
    {
        if (pieceController == null || knightPassiveEffect == null)
        {
            Debug.LogWarning("PieceController or knightPassiveEffect is null.");
            yield break;
        }

        //상하좌우 칸에 있는 슬라임과 좀비 장애물 제거
        List<Vector2Int> cardinalList = BoardManager.Instance.GetTilePositions(DirectionType.Four, PieceManager.Instance.GetCurrentPiece().gridPosition);

        bool hasTarget = false;
        for (int i = 0; i < cardinalList.Count; i++)
        {
            var obstacle = BoardManager.Instance.ReturnObstacleByPosition(cardinalList[i]);
            if (obstacle != null &&
                (obstacle.obstacleType == ObstacleType.Slime || obstacle.obstacleType == ObstacleType.Zombie))
            {
                hasTarget = true;
                Debug.Log($"기사가 공격 대상 찾았어: ({cardinalList[i].x}, {cardinalList[i].y})");
                break;
            }
        }
        
        if (hasTarget)
        {
            List<GameObject> skillEffects = new List<GameObject>();
            (Vector2Int direction, float rotationZ)[] directions = new[]
            {
                (new Vector2Int(0, 1), 0f),   // 상 
                (new Vector2Int(0, -1), 180f), // 하
                (new Vector2Int(-1, 0), 90f),  // 좌
                (new Vector2Int(1, 0), -90f)   // 우
            };

            ToastManager.Instance.ShowToast("기사 패시브 발동! 주변 4방향을 공격합니다.", pieceController.transform);

            foreach (var (dir, rotationZ) in directions)
            {
                Vector2Int targetPos = pieceController.gridPosition + dir;
                Vector3 effectPos = pieceController.transform.position;
                Quaternion rotation = Quaternion.Euler(0f, 0f, rotationZ);
                GameObject skillEffect = Instantiate(
                    knightPassiveEffect,
                    effectPos,
                    rotation
                );
                skillEffects.Add(skillEffect);

                var targetObstacle = BoardManager.Instance.ReturnObstacleByPosition(targetPos);
                if (targetObstacle != null &&
                    (targetObstacle.obstacleType == ObstacleType.Slime || targetObstacle.obstacleType == ObstacleType.Zombie))
                {
                    BoardManager.Instance.RemoveObstacleAtPosition(targetPos);

                    RuleEvents.TriggerRule("Knight_Active_ObstacleMove");
                }
            }

            yield return new WaitForSeconds(0.5f);
            foreach (var skillEffect in skillEffects)
            {
                if (skillEffect != null)
                {
                    Destroy(skillEffect);
                }
            }
        }
        else
        {
            yield return null;
        }
    }

    // 악마 공격 스킬
    public IEnumerator DemonAttack(PieceController pieceController)
    {
        if (pieceController == null || demonPassiveEffect == null)
        {
            Debug.LogWarning("PieceController or demonPassiveEffect is null.");
            yield break;
        }

        // 전방 3칸 타일 위치 가져오기 (전방 1칸 + 좌우 대각선 1칸)
        List<Vector2Int> forwardList = BoardManager.Instance.GetTilePositions(DirectionType.ForwardThree, pieceController.gridPosition);

        bool hasTarget = false;
        for (int i = 0; i < forwardList.Count; i++)
        {
            var obstacle = BoardManager.Instance.ReturnObstacleByPosition(forwardList[i]);
            if (obstacle != null &&
                (obstacle.obstacleType == ObstacleType.Slime || obstacle.obstacleType == ObstacleType.Zombie))
            {
                hasTarget = true;
                break;
            }
        }

        // 공격 대상이 있는 경우
        if (hasTarget)
        {
            List<GameObject> skillEffects = new List<GameObject>();
            Vector2Int lastMoveDirection = PieceManager.Instance.currentPiece.GetLastMoveDirection();

            // 전방 3칸 타일 위치에 이펙트 생성
            foreach (var pos in forwardList)
            {
                // 그리드 위치를 월드 위치로 변환 (pieceController.transform.position 사용 유지)
                Vector3 effectPos = pos + new Vector2(-5.5f, -5.5f); // 타일 중앙 위치

                // 타일의 인덱스를 기준으로 회전 각도 설정
                int index = forwardList.IndexOf(pos);
                float rotationZ = index switch
                {
                    0 => 0f,    // 전방 1칸
                    1 => -60f,  // 좌 대각선
                    _ => 60f    // 우 대각선
                };

                // 이동 방향에 따라 회전 각도 조정
                if (lastMoveDirection == new Vector2Int(0, -1)) // 하
                    rotationZ += 180f;
                else if (lastMoveDirection == new Vector2Int(-1, 0)) // 좌
                    rotationZ += 90f;
                else if (lastMoveDirection == new Vector2Int(1, 0)) // 우
                    rotationZ += -90f;

                Quaternion rotation = Quaternion.Euler(0f, 0f, rotationZ);
                GameObject skillEffect = Instantiate(
                    demonPassiveEffect,
                    effectPos,
                    rotation
                );
                skillEffects.Add(skillEffect);

                var targetObstacle = BoardManager.Instance.ReturnObstacleByPosition(pos);
                if (targetObstacle != null &&
                    (targetObstacle.obstacleType == ObstacleType.Slime || targetObstacle.obstacleType == ObstacleType.Zombie))
                {
                    BoardManager.Instance.RemoveObstacleAtPosition(pos);
                    Debug.Log($"장애물 제거됨: ({pos.x}, {pos.y})");

                    RuleEvents.TriggerRule("Demon_Active_ObstacleMove");
                }
            }

            // 이펙트 지속 시간 대기
            yield return new WaitForSeconds(0.5f);

            // 모든 이펙트 제거
            foreach (var skillEffect in skillEffects)
            {
                if (skillEffect != null)
                {
                    Destroy(skillEffect);
                }
            }
        }
        else
        {
            yield return null;
        }
    }

    //광신도 공격 스킬
    public IEnumerator FanaticAttack(PieceController pieceController)
    {
        if (pieceController == null || fanaticPassiveEffect == null)
        {
            Debug.LogWarning("PieceController 또는 fanaticPassiveEffect가 null입니다.");
            yield break;
        }

        // 대각선 타일 위치 가져오기
        List<Vector2Int> diagonalList = BoardManager.Instance.GetTilePositions(DirectionType.Diagonal, pieceController.gridPosition);

        bool hasTarget = false;
        for (int i = 0; i < diagonalList.Count; i++)
        {
            var obstacle = BoardManager.Instance.ReturnObstacleByPosition(diagonalList[i]);
            if (obstacle != null && obstacle.obstacleType == ObstacleType.Slime)
            {
                hasTarget = true;
                Debug.Log($"광신도가 공격 대상 찾음: ({diagonalList[i].x}, {diagonalList[i].y})");
                break;
            }
        }
        
        if (hasTarget)
        {
            List<GameObject> skillEffects = new List<GameObject>();
            // 대각선 방향과 회전 각도 설정
            (Vector2Int direction, float rotationZ)[] directions = new[]
            {
            (new Vector2Int(1, 1), 45f),    // 우상
            (new Vector2Int(1, -1), -45f),  // 우하
            (new Vector2Int(-1, 1), 135f),  // 좌상
            (new Vector2Int(-1, -1), -135f) // 좌하
        };

            // 대각선 타일 위치에 이펙트 생성
            foreach (var pos in diagonalList)
            {
                // 그리드 위치를 월드 위치로 변환
                Vector3 effectPos = pos + new Vector2(-5.5f, -5.5f); // 타일 중앙 위치

                Vector2Int dir = pos - pieceController.gridPosition;

                Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
                GameObject skillEffect = Instantiate(
                    fanaticPassiveEffect,
                    effectPos,
                    rotation
                );
                skillEffects.Add(skillEffect);

                var targetObstacle = BoardManager.Instance.ReturnObstacleByPosition(pos);
                if (targetObstacle != null && targetObstacle.obstacleType == ObstacleType.Slime)
                {
                    BoardManager.Instance.RemoveObstacleAtPosition(pos);
                    Debug.Log($"장애물 제거됨: ({pos.x}, {pos.y})");
                    
                    RuleEvents.TriggerRule("Fanatic_Active_ObstacleMove");
                }
            }

            yield return new WaitForSeconds(0.5f);
            foreach (var skillEffect in skillEffects)
            {
                if (skillEffect != null)
                {
                    Destroy(skillEffect);
                }
            }
        }
        else
        {
            yield return null;
        }
    }

    // 사제 패시브 스킬
    public IEnumerator Halo()
    {
        SkillManager.Instance.DelayTime = 2f;
        yield return new WaitForSeconds(0.8f); // 기물이 굴러가는 시간

        //ToastManager.Instance.ShowToast("사제가 독초의 저주를 무시합니다.", PieceManager.Instance.currentPiece.transform);

        if (priestPassiveEffect != null)
        {
            GameObject effect = Instantiate(
                priestPassiveEffect,
                new Vector3(
                    BoardManager.Instance.boardTransform.position.x + PieceManager.Instance.currentPiece.gridPosition.x,
                    BoardManager.Instance.boardTransform.position.y + PieceManager.Instance.currentPiece.gridPosition.y,
                    -1),
                Quaternion.identity,
                BoardManager.Instance.boardTransform
            );
            Destroy(effect, 0.5f);
        }
        else
        {
            Debug.LogWarning("PriestSkillEffect is not assigned!");

        }

        SkillManager.Instance.DelayTime = 0f;
        yield return new WaitForSeconds(0.5f);
    }

    public IEnumerator Steal()
    {
        SkillManager.Instance.DelayTime = 2f;
        yield return new WaitForSeconds(0.8f); // 기물이 굴러가는 시간

        ToastManager.Instance.ShowToast("도둑이 보물을 훔칩니다.", PieceManager.Instance.currentPiece.transform);

        if (thiefPassiveEffect != null)
        {
            GameObject effect = Instantiate(
                thiefPassiveEffect,
                new Vector3(
                    BoardManager.Instance.boardTransform.position.x + PieceManager.Instance.currentPiece.gridPosition.x,
                    BoardManager.Instance.boardTransform.position.y + PieceManager.Instance.currentPiece.gridPosition.y + 0.5f,
                    -1),
                Quaternion.identity,
                BoardManager.Instance.boardTransform
            );
            Destroy(effect, 1f);
        }
        else
        {
            Debug.LogWarning("thiefPassiveEffect is not assigned!");

        }

        SkillManager.Instance.DelayTime = 0f;
        yield return new WaitForSeconds(1f);
    }

}