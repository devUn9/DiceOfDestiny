using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PassiveSkill : MonoBehaviour
{
    [SerializeField] private GameObject knightPassiveEffect;
    [SerializeField] private GameObject demonPassiveEffect;
    [SerializeField] private GameObject fanaticPassiveEffect;
    [SerializeField] private GameObject priestPassiveEffect;
    [SerializeField] private GameObject thiefPassiveEffect;
    [SerializeField] private GameObject loggerPassiveEffect;
    [SerializeField] private GameObject berserkerPassiveEffect;


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
                (obstacle.obstacleType == ObstacleType.Slime || obstacle.obstacleType == ObstacleType.Zombie ||
                    obstacle.obstacleType == ObstacleType.Pawn || obstacle.obstacleType == ObstacleType.Knight ||
                    obstacle.obstacleType == ObstacleType.House))
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

            ToastManager.Instance.ShowToast("기사 패시브 발동! 주변 4방향을 공격합니다.", pieceController.transform, 0f);

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
                    (targetObstacle.obstacleType == ObstacleType.Slime || targetObstacle.obstacleType == ObstacleType.Zombie ||
                     targetObstacle.obstacleType == ObstacleType.Knight))
                {
                    BoardManager.Instance.RemoveObstacleAtPosition(targetPos);

                    RuleEvents.TriggerRule("Knight_Active_ObstacleMove");
                }
                else if(targetObstacle != null && targetObstacle.obstacleType == ObstacleType.Pawn)
                {
                    ObstacleManager.Instance.HitPawn(targetPos);
                }
                else if (targetObstacle != null && targetObstacle.obstacleType == ObstacleType.House)
                {
                    ObstacleManager.Instance.HitHouse(targetPos);
                }
            }

            GameManager.Instance.IsLockCursor = true;

            // 이펙트 지속 시간 대기
            yield return new WaitForSeconds(0.5f);

            GameManager.Instance.IsLockCursor = false;

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
                (obstacle.obstacleType == ObstacleType.Slime || obstacle.obstacleType == ObstacleType.Zombie ||
                    obstacle.obstacleType == ObstacleType.Pawn || obstacle.obstacleType == ObstacleType.Knight ||
                    obstacle.obstacleType == ObstacleType.House))
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
                Vector3 effectPos = pos + new Vector2(-6f, -6f); // 타일 중앙 위치

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
                    (targetObstacle.obstacleType == ObstacleType.Slime || targetObstacle.obstacleType == ObstacleType.Zombie ||
                        targetObstacle.obstacleType == ObstacleType.Knight))
                {
                    BoardManager.Instance.RemoveObstacleAtPosition(pos);
                    Debug.Log($"장애물 제거됨: ({pos.x}, {pos.y})");

                    RuleEvents.TriggerRule("Demon_Active_ObstacleMove");
                }
                else if (targetObstacle != null && targetObstacle.obstacleType == ObstacleType.Pawn)
                {
                    ObstacleManager.Instance.HitPawn(pos);
                }
                else if (targetObstacle != null && targetObstacle.obstacleType == ObstacleType.House)
                {
                    ObstacleManager.Instance.HitHouse(pos);
                }
            }

            GameManager.Instance.IsLockCursor = true;

            // 이펙트 지속 시간 대기
            yield return new WaitForSeconds(0.5f);

            GameManager.Instance.IsLockCursor = false;

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
            if (obstacle != null &&
                (obstacle.obstacleType == ObstacleType.Slime || obstacle.obstacleType == ObstacleType.Zombie ||
                    obstacle.obstacleType == ObstacleType.Pawn || obstacle.obstacleType == ObstacleType.Knight ||
                    obstacle.obstacleType == ObstacleType.House))
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
                Vector3 effectPos = pos + new Vector2(-6, -6); // 타일 중앙 위치

                Vector2Int dir = pos - pieceController.gridPosition;

                Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
                GameObject skillEffect = Instantiate(
                    fanaticPassiveEffect,
                    effectPos,
                    rotation
                );
                skillEffects.Add(skillEffect);

                var targetObstacle = BoardManager.Instance.ReturnObstacleByPosition(pos);
                if (targetObstacle != null &&
                   (targetObstacle.obstacleType == ObstacleType.Slime || targetObstacle.obstacleType == ObstacleType.Zombie ||
                    targetObstacle.obstacleType == ObstacleType.Knight))
                {
                    BoardManager.Instance.RemoveObstacleAtPosition(pos);
                    Debug.Log($"장애물 제거됨: ({pos.x}, {pos.y})");
                    
                    RuleEvents.TriggerRule("Fanatic_Active_ObstacleMove");
                }
                else if (targetObstacle != null && targetObstacle.obstacleType == ObstacleType.Pawn)
                {
                    ObstacleManager.Instance.HitPawn(pos);
                }
                else if (targetObstacle != null && targetObstacle.obstacleType == ObstacleType.House)
                {
                    ObstacleManager.Instance.HitHouse(pos);
                }
            }

            GameManager.Instance.IsLockCursor = true;

            // 이펙트 지속 시간 대기
            yield return new WaitForSeconds(0.5f);

            GameManager.Instance.IsLockCursor = false;

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

    // 광신도가 사제를 광신도로 3번 바꾸면 전체 타일에 메테오 공격하는 히든 스킬
    public IEnumerator DoFanaticMeteor()
    {
        GameManager.Instance.IsLockCursor = true;

        yield return new WaitForSeconds(0.5f);

        // 전체 타일 위치 가져오기
        List<Vector2Int> allTiles = BoardManager.Instance.GetCircleTilePositions();

        if (allTiles.Count == 0)
        {
            Debug.LogWarning("보드에 타일이 없습니다.");
            yield break;
        }

        List<GameObject> skillEffects = new List<GameObject>();

        // 모든 타일에 이펙트 생성
        foreach (var pos in allTiles)
        {
            // 그리드 위치를 월드 위치로 변환
            Vector3 effectPos = pos + new Vector2(-6, -6); // 타일 중앙 위치

            // 이펙트 생성 (회전은 기본값으로 설정)
            GameObject skillEffect = Instantiate(
                fanaticPassiveEffect,
                effectPos,
                Quaternion.Euler(0f, 0f, 0f)
            );
            skillEffects.Add(skillEffect);

            // 타겟 장애물 확인
            var targetObstacle = BoardManager.Instance.ReturnObstacleByPosition(pos);
            if (targetObstacle != null)
            {
                if (targetObstacle.obstacleType == ObstacleType.Slime ||
                    targetObstacle.obstacleType == ObstacleType.Zombie ||
                    targetObstacle.obstacleType == ObstacleType.Knight)
                {
                    BoardManager.Instance.RemoveObstacleAtPosition(pos);
                    Debug.Log($"장애물 제거됨: ({pos.x}, {pos.y})");

                    RuleEvents.TriggerRule("Fanatic_Passive2_ObstacleMove");
                }
                else if (targetObstacle.obstacleType == ObstacleType.Pawn)
                {
                    ObstacleManager.Instance.HitPawn(pos);
                    Debug.Log($"폰 데미지 입힘: ({pos.x}, {pos.y})");
                }
                else if (targetObstacle.obstacleType == ObstacleType.House)
                {
                    ObstacleManager.Instance.HitHouse(pos);
                }
            }
        }

        // 이펙트 지속 시간
        yield return new WaitForSeconds(0.5f);

        // 이펙트 제거
        foreach (var skillEffect in skillEffects)
        {
            if (skillEffect != null)
            {
                Destroy(skillEffect);
            }
        }

        GameManager.Instance.IsLockCursor = false;
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

    // 도둑 패시브 스킬 : 보물 훔치기
    public IEnumerator Steal()
    {
        SkillManager.Instance.DelayTime = 2f;
        yield return new WaitForSeconds(0.8f); // 기물이 굴러가는 시간

        ToastManager.Instance.ShowToast("도둑이 보물을 훔칩니다.", PieceManager.Instance.currentPiece.transform, 0f);

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

    // 나무꾼 패시브 스킬 : 전방 1칸에 나무 또는 나무상자가 있으면 제거
    public IEnumerator CutDownTree(PieceController pieceController)
    {
        // 전방 한칸 타일 위치 가져오기
        Vector2Int forwardPos = BoardManager.Instance.GetTilePositions(DirectionType.ForwardOne, pieceController.gridPosition)[0];
        var targetObstacle = BoardManager.Instance.ReturnObstacleByPosition(forwardPos);

        // 나무 또는 나무상자가 있는 경우
        if (targetObstacle != null &&
            targetObstacle.obstacleType == ObstacleType.Tree)
        {
            // 마지막 이동 방향 가져오기
            Vector2Int lastMoveDirection = PieceManager.Instance.currentPiece.GetLastMoveDirection();

            // 이동 방향에 따라 회전 각도 설정
            float rotationAngle = 0f;
            if (lastMoveDirection == Vector2Int.left)
                rotationAngle = 90f;
            else if (lastMoveDirection == Vector2Int.down)
                rotationAngle = 180f;
            else if (lastMoveDirection == Vector2Int.right)
                rotationAngle = 270f;
            else if (lastMoveDirection == Vector2Int.up)
                rotationAngle = 0f;

            // 이펙트 생성 (회전 적용)
            Vector3 effectPos = forwardPos + new Vector2(-6f, -6f);
            GameObject skillEffect = Instantiate(loggerPassiveEffect, effectPos, Quaternion.Euler(0f, 0f, rotationAngle));

            // 나무 또는 나무상자 제거
            if (targetObstacle.obstacleType == ObstacleType.Tree)
            {
                BoardManager.Instance.RemoveObstacleAtPosition(forwardPos);
                Debug.Log($"나무 제거됨: ({forwardPos.x}, {forwardPos.y})");
                RuleEvents.TriggerRule("Demon_Active_ObstacleMove");
            }

            // 이펙트 지속 시간 대기 후 제거

            GameManager.Instance.IsLockCursor = true;

            // 이펙트 지속 시간 대기
            yield return new WaitForSeconds(0.5f);

            GameManager.Instance.IsLockCursor = false;

            if (skillEffect != null)
            {
                Destroy(skillEffect);
            }
        }
        else
        {
            yield return null;
        }
    }

    // 광전사 패시브 : 주변 8칸의 적을 찾아서 랜덤 공격 (돌진)
    public IEnumerator BerserkerAttack(PieceController pieceController)
    {
        if (pieceController == null || berserkerPassiveEffect == null)
        {
            Debug.LogWarning("PieceController or BerserkerPassiveEffect is null.");
            yield break;
        }

        // 주변 8방향 타일 위치 가져오기
        List<Vector2Int> searchList = BoardManager.Instance.GetTilePositions(DirectionType.Eight, pieceController.gridPosition);

        // 공격 가능한 타겟 목록
        List<(Vector2Int position, Obstacle obstacle)> targets = new List<(Vector2Int, Obstacle)>();
        for (int i = 0; i < searchList.Count; i++)
        {
            var obstacle = BoardManager.Instance.ReturnObstacleByPosition(searchList[i]);
            if (obstacle != null &&
                (obstacle.obstacleType == ObstacleType.Slime ||
                 obstacle.obstacleType == ObstacleType.Zombie ||
                 obstacle.obstacleType == ObstacleType.Pawn ||
                 obstacle.obstacleType == ObstacleType.Knight ||
                 obstacle.obstacleType == ObstacleType.House))
            {
                targets.Add((searchList[i], obstacle));
            }
        }

        // 타겟이 있는지 확인
        bool hasTarget = targets.Count > 0;
        if (!hasTarget)
        {
            yield return null;
            yield break;
        }

        // 타겟이 있으면 랜덤으로 하나 선택
        int randomIndex = Random.Range(0, targets.Count);
        var selectedTarget = targets[randomIndex];
        Vector2Int targetPos = selectedTarget.position;
        var targetObstacle = selectedTarget.obstacle;

        // 방향과 회전 각도 설정
        Vector2Int direction = targetPos - pieceController.gridPosition;
        float rotationZ = 0f;
        if (direction == new Vector2Int(0, 1)) rotationZ = 180f;    // 상
        else if (direction == new Vector2Int(0, -1)) rotationZ = 0f; // 하
        else if (direction == new Vector2Int(-1, 0)) rotationZ = 270f;  // 좌
        else if (direction == new Vector2Int(1, 0)) rotationZ = 90f;  // 우
        else if (direction == new Vector2Int(-1, 1)) rotationZ = 225f;  // 좌상
        else if (direction == new Vector2Int(1, 1)) rotationZ = 135f;  // 우상
        else if (direction == new Vector2Int(-1, -1)) rotationZ = 315f; // 좌하
        else if (direction == new Vector2Int(1, -1)) rotationZ = 45f; // 우하

        // 토스트 메시지 표시
        ToastManager.Instance.ShowToast("광전사 패시브 발동! 랜덤 타겟을 공격합니다.", pieceController.transform, 0f);

        // 이펙트 생성 (방향 벡터를 더한 위치에)
        Vector3 startPos = pieceController.transform.position;
        Vector3 effectPos = startPos + new Vector3(direction.x, direction.y, 0);
        Quaternion rotation = Quaternion.Euler(0f, 0f, rotationZ);
        GameObject skillEffect = Instantiate(
            berserkerPassiveEffect,
            effectPos,
            rotation,
            pieceController.transform
        );

        // 스케일 조정 (좌우 방향에 따라 반전)
        if (direction == new Vector2Int(-1, 0) || direction == new Vector2Int(-1, 1) || direction == new Vector2Int(-1, -1))
        {
            skillEffect.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (direction == new Vector2Int(1, 0) || direction == new Vector2Int(1, 1) || direction == new Vector2Int(1, -1))
        {
            skillEffect.transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        // 타겟 처리
        bool shouldRush = true;
        if (targetObstacle.obstacleType == ObstacleType.Slime ||
            targetObstacle.obstacleType == ObstacleType.Zombie)
        {
            BoardManager.Instance.RemoveObstacleAtPosition(targetPos);
            RuleEvents.TriggerRule("Berserker_Active_ObstacleMove");
        }
        else if (targetObstacle.obstacleType == ObstacleType.Knight)
        {
            shouldRush = false;
            BoardManager.Instance.RemoveObstacleAtPosition(targetPos);
            RuleEvents.TriggerRule("Berserker_Active_ObstacleMove");
        }
        else if (targetObstacle.obstacleType == ObstacleType.Pawn)
        {
            if (targetObstacle.GetComponent<PawnBehaviour>().life == 1)
            {
                ObstacleManager.Instance.HitPawn(targetPos);
                BoardManager.Instance.RemoveObstacleAtPosition(targetPos);
                RuleEvents.TriggerRule("Berserker_Active_ObstacleMove");
            }
            else
            {
                ObstacleManager.Instance.HitPawn(targetPos);
                shouldRush = false;
            }
        }
        else if (targetObstacle.obstacleType == ObstacleType.House)
        {
            if (targetObstacle.GetComponent<HouseBehaviour>().life == 1)
            {
                ObstacleManager.Instance.HitHouse(targetPos);
                BoardManager.Instance.RemoveObstacleAtPosition(targetPos);
                RuleEvents.TriggerRule("Berserker_Active_ObstacleMove");
            }
            else
            {
                ObstacleManager.Instance.HitHouse(targetPos);
                shouldRush = false;
            }
        }

        // 돌진 로직 (shouldRush가 true일 때만)
        if (shouldRush)
        {
            // 보드에서 현재 위치의 피스 제거
            BoardManager.Instance.Board[pieceController.gridPosition.x, pieceController.gridPosition.y].SetPiece(null);

            // 새로운 위치 계산 및 보드 경계 체크
            Vector2Int newPosition = pieceController.gridPosition + direction;
            if (!BoardManager.Instance.IsInsideBoard(newPosition))
            {
                if (skillEffect != null)
                {
                    Destroy(skillEffect);
                }
                GameManager.Instance.IsLockCursor = false;
                yield break;
            }

            // 피스 위치 업데이트
            pieceController.gridPosition = newPosition;
            BoardManager.Instance.Board[newPosition.x, newPosition.y].SetPiece(pieceController);

            // 부드러운 이동
            Vector3 moveVec = new Vector3(direction.x, direction.y, 0);
            Vector3 endPos = startPos + moveVec;
            float moveDuration = 0.4f;
            float time = 0f;

            GameManager.Instance.IsLockCursor = true;

            while (time < moveDuration)
            {
                float t = time / moveDuration;
                float ease = Mathf.SmoothStep(0f, 1f, t);
                pieceController.transform.position = Vector3.Lerp(startPos, endPos, ease);
                time += Time.deltaTime;
                yield return null;
            }

            pieceController.transform.position = endPos;
        }

        // 이펙트 지속 시간 대기
        yield return new WaitForSeconds(0.5f);

        // 이펙트 제거
        if (skillEffect != null)
        {
            Destroy(skillEffect);
        }

        GameManager.Instance.IsLockCursor = false;

        // 타일 하이라이트 및 컨트롤 복구
        BoardSelectManager.Instance.PieceHighlightTiles(pieceController.gridPosition);
        

      
    }
}