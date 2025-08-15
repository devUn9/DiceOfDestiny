using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveSkill : MonoBehaviour
{
    [SerializeField] private GameObject knightSkillEffect;
    [SerializeField] private GameObject demonSkillEffect;
    [SerializeField] private GameObject painterSkillEffect;
    [SerializeField] private GameObject fanaticSkillEffect;
    [SerializeField] private GameObject priestSkillEffect;

    private PainterActiveSkillUI painterActiveSkillUI;
    private MoveSkillUI moveSkillUI;
    private PieceSelectUI pieceSelectUI;

    private void Awake()
    {
        painterActiveSkillUI = GetComponentInParent<PainterActiveSkillUI>();
        moveSkillUI = GetComponentInParent<MoveSkillUI>();
        pieceSelectUI = GetComponentInParent<PieceSelectUI>();
    }

    // 기사 스킬: 앞으로 이동
    public IEnumerator KnightMoveForward(PieceController pieceController, Vector2Int moveDirection)
    {
        PieceManager.Instance.SetCurrentPieceControl(false);

        yield return new WaitForSeconds(SkillManager.Instance.blinkTime + 0.1f);

        if (moveDirection != Vector2Int.up && moveDirection != Vector2Int.down &&
            moveDirection != Vector2Int.right && moveDirection != Vector2Int.left)
        {
            Debug.LogWarning($"Invalid move direction: {moveDirection}");
            yield break;
        }

        Vector3 moveVec = new Vector3(moveDirection.x, moveDirection.y, 0);
        float moveDuration = 0.4f;
        float time = 0f;

        Vector3 startPos = pieceController.transform.position;
        Vector3 endPos = startPos + moveVec;

        GameObject skillEffect = null;
        if (knightSkillEffect != null)
        {
            skillEffect = Instantiate(knightSkillEffect, startPos, Quaternion.identity);
            skillEffect.transform.SetParent(pieceController.transform);

            if (moveDirection == Vector2Int.left)
            {
                skillEffect.transform.localScale = new Vector3(1f, 1f, 1f);
                skillEffect.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (moveDirection == Vector2Int.right)
            {
                skillEffect.transform.localScale = new Vector3(-1f, 1f, 1f);
                skillEffect.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (moveDirection == Vector2Int.up)
            {
                skillEffect.transform.localScale = new Vector3(1f, 1f, 1f);
                skillEffect.transform.localRotation = Quaternion.Euler(0f, 0f, -120f);
            }
            else if (moveDirection == Vector2Int.down)
            {
                skillEffect.transform.localScale = new Vector3(1f, 1f, 1f);
                skillEffect.transform.localRotation = Quaternion.Euler(0f, 0f, 60f);
            }

            SoundManager.Instance.Play("Knight_Skill");
        }
        else
        {
            Debug.LogWarning("Skill effect prefab is not assigned!");
        }

        while (time < moveDuration)
        {
            float t = time / moveDuration;
            float ease = Mathf.SmoothStep(0f, 1f, t);
            pieceController.transform.position = Vector3.Lerp(startPos, endPos, ease);
            time += Time.deltaTime;
            yield return null;
        }

        pieceController.transform.position = endPos;
        Vector2Int gridPos = pieceController.gridPosition;
        gridPos += moveDirection;
        pieceController.gridPosition = gridPos;

        bool hasObstacle = BoardManager.Instance.IsEmptyTile(gridPos);

        if (!hasObstacle)
        {
            if (BoardManager.Instance.Board[gridPos.x, gridPos.y].Obstacle == ObstacleType.Pawn)
            {
                Obstacle pawn = BoardManager.Instance.ReturnObstacleByPosition(gridPos);
                ObstacleManager.Instance.RemovePawnToList(pawn.gameObject);
            }

            BoardManager.Instance.RemoveObstacleAtPosition(gridPos);
        }

        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.TrySkill(gridPos, pieceController);
        }
        else
        {
            Debug.LogError("SkillManager.Instance is null!");
        }

        if (skillEffect != null)
        {
            Destroy(skillEffect, 0.5f);
        }

        BoardSelectManager.Instance.PieceHighlightTiles(gridPos);
        PieceManager.Instance.SetCurrentPieceControl(true);
    }

    // 악마 스킬: 독초 심기
    public IEnumerator Plant(PieceController pieceController)
    {
        PieceManager.Instance.SetCurrentPieceControl(false);
        
       yield return new WaitForSeconds(SkillManager.Instance.blinkTime + 0.1f);

        BoardSelectManager.Instance.HighlightTiles();
        yield return BoardSelectManager.Instance.WaitForTileClick();

        SkillManager.Instance.IsSelectingProgress = true;
        Vector2Int selectPos = BoardSelectManager.Instance.lastClickedPosition;

        Vector3 effectPosition = new Vector3(
                   selectPos.x - 6f,
                   selectPos.y - 6f,
                   0f
               );

        if (demonSkillEffect != null)
        {
            GameObject effect = Instantiate(
                demonSkillEffect,
                effectPosition,
                Quaternion.identity,
                BoardManager.Instance.boardTransform
            );
            Destroy(effect, 0.5f);
        }
        else
        {
            Debug.LogWarning("DemonSkillEffect is not assigned!");
        }

        yield return new WaitForSeconds(0.5f);
        BoardManager.Instance.CreateObstacle(selectPos, ObstacleType.PoisonousHerb);
        SkillManager.Instance.IsSelectingProgress = false;

        PieceManager.Instance.SetCurrentPieceControl(true);
        
    }

    // 화가 스킬: 색칠하기
    public IEnumerator Paint(PieceController pieceController)
    {
        PieceManager.Instance.SetCurrentPieceControl(false);
        yield return new WaitForSeconds(SkillManager.Instance.blinkTime + 0.1f);

        BoardSelectManager.Instance.AllHighlightTiles();
        yield return BoardSelectManager.Instance.WaitForTileClick();

        SkillManager.Instance.IsSelectingProgress = true;
        Vector2Int gridPos = BoardSelectManager.Instance.lastClickedPosition;

        if (painterActiveSkillUI != null)
        {
            painterActiveSkillUI.OnDisable();
            painterActiveSkillUI.ShowPalette();
            while (painterActiveSkillUI.SelectedColor == TileColor.None)
            {
                yield return null;
            }

            TileColor selectedColor = painterActiveSkillUI.SelectedColor;

            if (painterSkillEffect != null)
            {
                Vector2Int selectPos = BoardSelectManager.Instance.lastClickedPosition;
                Vector3 effectPosition = new Vector3(
                    selectPos.x - 6f,
                    selectPos.y - 6.5f,
                    0f
                );

                ObstacleManager.Instance.DeathPawn(selectPos);

                GameObject effect = Instantiate(
                    painterSkillEffect,
                    effectPosition,
                    Quaternion.identity
                );
                Destroy(effect, 1f);
            }
            else
            {
                Debug.LogWarning("PainterSkillEffect is not assigned!");
            }

            yield return new WaitForSeconds(0.5f);
            BoardManager.Instance.SetTileColor(gridPos, selectedColor);
            SkillManager.Instance.IsSelectingProgress = false;
            PieceManager.Instance.SetCurrentPieceControl(true);
        }
        else
        {
            Debug.LogWarning("PainterActiveSkillUI is not assigned!");
        }


    }

    public IEnumerator ConvertToFanatic(PieceController piece)
    {
        yield return new WaitForSeconds(SkillManager.Instance.blinkTime + 0.1f);

        List<Vector2Int> surroundList = BoardManager.Instance.GetTilePositions(DirectionType.Eight, piece.gridPosition);

        bool converted = false;
        foreach (PieceController targetPiece in PieceManager.Instance.Pieces)
        {
            if (targetPiece == null || targetPiece == piece) continue;

            if (surroundList.Contains(targetPiece.gridPosition))
            {
                for (int i = 0; i < 6; i++)
                {
                    Face face = targetPiece.GetFace(i);
                    if (face.classData.className == "Priest")
                    {

                        targetPiece.ChangeClass(i, "Fanatic");
                        Debug.Log($"Converted Priest to Fanatic on face {i} at position {targetPiece.gridPosition}");
                        converted = true;

                        if (fanaticSkillEffect != null)
                        {
                            GameObject effect = Instantiate(
                                fanaticSkillEffect,
                                new Vector3(
                                    BoardManager.Instance.boardTransform.position.x + targetPiece.gridPosition.x,
                                    BoardManager.Instance.boardTransform.position.y + targetPiece.gridPosition.y,
                                    -1),
                                Quaternion.identity,
                                BoardManager.Instance.boardTransform
                            );
                            Destroy(effect, 0.5f);
                        }
                    }
                }
            }
        }

        if (!converted)
        {
            ToastManager.Instance.ShowToast("주변에 사제가 없어 아무 일도 일어나지 않았습니다.", piece.transform);
        }
        else
        {

            //ToastManager.Instance.ShowToast("성공", piece.transform);
        }
    }

    // 사제 스킬
    public IEnumerator HealAP()
    {
        PieceManager.Instance.SetCurrentPieceControl(false);

        yield return new WaitForSeconds(SkillManager.Instance.blinkTime + 0.1f);

        if (priestSkillEffect != null)
        {
            GameObject effect = Instantiate(
                priestSkillEffect,
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

        PieceManager.Instance.SetCurrentPieceControl(true);
    }

    // 도적 스킬 : 이동 UI 띄우기
    public IEnumerator FastMove(PieceController piece)
    {
        PieceManager.Instance.SetCurrentPieceControl(false);
        yield return new WaitForSeconds(SkillManager.Instance.blinkTime + 0.1f);
        moveSkillUI.Initialize(piece); // 추가
        yield return moveSkillUI.WaitForArrowClick();
    }

    // 도적 스킬 : 앞으로 이동
    public IEnumerator MoveForward(PieceController pieceController, Vector2Int moveDirection)
    {
        if (moveDirection != Vector2Int.up && moveDirection != Vector2Int.down &&
           moveDirection != Vector2Int.right && moveDirection != Vector2Int.left)
        {
            Debug.LogWarning($"Invalid move direction: {moveDirection}");
            yield break;
        }

        Vector3 moveVec = new Vector3(moveDirection.x, moveDirection.y, 0);
        float moveDuration = 0.4f;
        float time = 0f;

        Vector3 startPos = pieceController.transform.position;
        Vector3 endPos = startPos + moveVec;

        while (time < moveDuration)
        {
            float t = time / moveDuration;
            float ease = Mathf.SmoothStep(0f, 1f, t);
            pieceController.transform.position = Vector3.Lerp(startPos, endPos, ease);
            time += Time.deltaTime;
            yield return null;
        }

        pieceController.transform.position = endPos;
        Vector2Int gridPos = pieceController.gridPosition;
        gridPos += moveDirection;
        pieceController.gridPosition = gridPos;

        Vector2Int PiecePosition = PieceManager.Instance.currentPiece.gridPosition;
        Vector2Int lastPosition = PieceManager.Instance.currentPiece.gridPosition - moveDirection;
        BoardManager.Instance.Board[lastPosition.x, lastPosition.y].SetPiece(null);
        //Vector2Int newPosition2 = PieceManager.Instance.currentPiece.gridPosition + moveDirection;
        BoardManager.Instance.Board[PiecePosition.x, PiecePosition.y].SetPiece(pieceController);


        //bool hasObstacle = BoardManager.Instance.IsEmptyTile(gridPos);

        //if (!hasObstacle)
        //{
        //    BoardManager.Instance.RemoveObstacleAtPosition(gridPos);
        //}

        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.TrySkill(gridPos, pieceController);
        }
        else
        {
            Debug.LogError("SkillManager.Instance is null!");
        }


        BoardSelectManager.Instance.PieceHighlightTiles(gridPos);

        ObstacleManager.Instance.UpdateObstacleStep();

        PieceManager.Instance.SetCurrentPieceControl(true);
    }

    // 아기 스킬 : 다른 기물 이동
    public IEnumerator HelpBaby(PieceController pieceController)
    {
        PieceManager.Instance.SetCurrentPieceControl(false);

        yield return new WaitForSeconds(SkillManager.Instance.blinkTime + 0.1f);

        // 본인을 제외한 기물들 중 이동 가능한 타일이 있는지 확인
        bool hasMovablePiece = false;
        foreach (PieceController targetPiece in PieceManager.Instance.Pieces)
        {
            if (targetPiece == null || targetPiece == pieceController) // 본인 또는 null 기물 제외
                continue;

            // 기물의 상하좌우 또는 대각선 타일 확인
            List<Vector2Int> movableTiles = BoardManager.Instance.GetTilePositions(DirectionType.Diagonal, targetPiece.gridPosition);
            bool canMove = false;

            // 이동 가능한 타일이 있는지 확인
            foreach (Vector2Int tile in movableTiles)
            {
                if (BoardManager.Instance.IsEmptyTile(tile))
                {
                    canMove = true;
                    break; // 빈 타일이 있으면 더 이상 확인할 필요 없음
                }
            }

            if (canMove)
            {
                hasMovablePiece = true;
                break; // 이동 가능한 기물이 하나라도 있으면 루프 종료
            }
        }

        // 이동 가능한 기물이 없으면 코루틴 종료
        if (!hasMovablePiece)
        {
            Debug.Log("아기 스킬 썼지만 발동 가능한 기물이 없네");
            PieceManager.Instance.SetCurrentPieceControl(true);
            yield break;
        }

        // 기존 하이라이트 타일 제거
        BoardSelectManager.Instance.DestroyPieceHighlightTile();

        // 본인을 제외한 기물 위치에 하이라이트 타일 생성
        foreach (PieceController piece in PieceManager.Instance.Pieces)
        {
            if (piece == null || piece == PieceManager.Instance.currentPiece)
                continue;

            // 하이라이트 타일 생성
            BoardSelectManager.Instance.PieceHighLightTilesMulty(piece.gridPosition);
        }

        // 기물 선택 UI 생성
        pieceSelectUI.CreateButtonsForPieces();


        // 화살표 클릭 대기
        yield return moveSkillUI.WaitForArrowClick();

        // 기물 선택 UI 종료
        pieceSelectUI.ClearButtons();

        // 하이라이트 타일 제거 및 현재 기물 위치 하이라이트
        BoardSelectManager.Instance.DestroyPieceHighlightTile();
        PieceManager.Instance.currentPiece = pieceController;
        BoardSelectManager.Instance.PieceHighlightTiles(pieceController.gridPosition);

        PieceManager.Instance.SetCurrentPieceControl(true);
    }
}