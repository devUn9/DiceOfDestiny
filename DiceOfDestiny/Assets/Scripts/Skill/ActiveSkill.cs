using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditorInternal.VersionControl.ListControl;

public class ActiveSkill : MonoBehaviour
{
    [SerializeField] private GameObject knightSkillEffect;
    [SerializeField] private GameObject demonSkillEffect;
    [SerializeField] private GameObject painterSkillEffect;
    [SerializeField] private GameObject fanaticSkillEffect;
    [SerializeField] private GameObject priestSkillEffect;
    [SerializeField] private GameObject woodCutterSkillEffect;
    [SerializeField] private GameObject wizardSkillEffect;

    private PainterActiveSkillUI painterActiveSkillUI;
    private MoveSkillUI moveSkillUI;
    private PieceSelectUI pieceSelectUI;
    private int fanaticPoint = 0;
    private bool isPieceSelected = false;

    private void Awake()
    {
        painterActiveSkillUI = GetComponentInParent<PainterActiveSkillUI>();
        moveSkillUI = GetComponentInParent<MoveSkillUI>();
        pieceSelectUI = GetComponentInParent<PieceSelectUI>();
    }

    // 기사 스킬: 앞으로 이동
    public IEnumerator KnightMoveForward(PieceController pieceController, Vector2Int moveDirection)
    {

        GameManager.Instance.IsLockCursor = true;

        yield return new WaitForSeconds(SkillManager.Instance.blinkTime + 0.1f);

        if (moveDirection != Vector2Int.up && moveDirection != Vector2Int.down &&
            moveDirection != Vector2Int.right && moveDirection != Vector2Int.left)
        {
            Debug.LogWarning($"Invalid move direction: {moveDirection}");
            yield break;
        }

        Vector2Int gridPos = pieceController.gridPosition;
        //gridPos += moveDirection;
        Vector2Int newPos = gridPos + moveDirection;

        pieceController.gridPosition = newPos;

        if (newPos.y == BoardManager.Instance.boardSizeY - 1)
        {
            if (!MissionManager.Instance.CanGoFinishLine())
            {
                ToastManager.Instance.ShowToast("추가 미션을 완료해야 도착점에 갈 수 있습니다.", transform, 0f);
                yield break;
            }
        }

        BoardManager.Instance.Board[gridPos.x, gridPos.y].TileColor = pieceController.lastTileColor;
        pieceController.lastTileColor = BoardManager.Instance.Board[newPos.x, newPos.y].TileColor;
        BoardManager.Instance.Board[newPos.x, newPos.y].TileColor = pieceController.GetPiece().faces[2].color;

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



        bool hasObstacle = BoardManager.Instance.IsEmptyTile(newPos);

        if (!hasObstacle)
        {
            Obstacle obstacle = BoardManager.Instance.ReturnObstacleByPosition(newPos);

            if (BoardManager.Instance.Board[newPos.x, newPos.y].Obstacle == ObstacleType.Pawn)
            {
                ObstacleManager.Instance.RemovePawnToList(obstacle.gameObject);
            }
            else if (BoardManager.Instance.Board[newPos.x, newPos.y].Obstacle == ObstacleType.Knight)
            {
                ObstacleManager.Instance.RemoveKnightToList(obstacle.gameObject);
            }
            else if (BoardManager.Instance.Board[newPos.x, newPos.y].Obstacle == ObstacleType.House)
            {
                ObstacleManager.Instance.RemoveHouseToList(obstacle.gameObject);
            }

            BoardManager.Instance.RemoveObstacleAtPosition(newPos);
        }

        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.TrySkill(newPos, pieceController);
        }
        else
        {
            Debug.LogError("SkillManager.Instance is null!");
        }

        if (skillEffect != null)
        {
            Destroy(skillEffect, 0.5f);
        }

        yield return new WaitForSeconds(0.4f);

        BoardSelectManager.Instance.PieceHighlightTiles(newPos);

        GameManager.Instance.IsLockCursor = false;

        // 도착점 체크
        MissionManager.Instance.CheckStageClearAfterMove(newPos);
        // 모든 미션완료 상태 체크
        MissionManager.Instance.IsAllMissionCompleted(pieceController);
    }

    // 악마 스킬: 독초 심기
    public IEnumerator Plant(PieceController pieceController)
    {

        GameManager.Instance.IsLockCursor = true;

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


        GameManager.Instance.IsLockCursor = false;

    }

    // 화가 스킬: 색칠하기
    public IEnumerator Paint(PieceController pieceController)
    {

        GameManager.Instance.IsLockCursor = true;

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

            GameManager.Instance.IsLockCursor = false;
        }
        else
        {
            Debug.LogWarning("PainterActiveSkillUI is not assigned!");
        }


    }

    // 광신도 스킬 : 사제를 광신도로 바꾼다. (3스택 시 히든 스킬 발동) 
    public IEnumerator ConvertToFanatic(PieceController piece)
    {

        GameManager.Instance.IsLockCursor = true;

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
                        fanaticPoint++;
                        if (fanaticPoint == 3)
                        {
                            SkillManager.Instance.FanaticMeteor();
                        }
                    }
                }
            }
        }

        if (!converted)
        {
            ToastManager.Instance.ShowToast("주변에 사제가 없어 아무 일도 일어나지 않았습니다.", piece.transform, 1.5f);
        }
        else
        {

            //ToastManager.Instance.ShowToast("성공", piece.transform);
        }


        GameManager.Instance.IsLockCursor = false;
    }

    // 사제 스킬
    public IEnumerator HealAP()
    {

        GameManager.Instance.IsLockCursor = true;

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


        GameManager.Instance.IsLockCursor = false;

    }

    // 도적 스킬 : 이동 UI 띄우기
    public IEnumerator FastMove(PieceController piece)
    {

        GameManager.Instance.IsLockCursor = true;

        yield return new WaitForSeconds(SkillManager.Instance.blinkTime + 0.1f);
        moveSkillUI.Initialize(piece); // 추가
        yield return moveSkillUI.WaitForArrowClick();
    }

    // 도적이랑 아기가 사용. 앞으로 이동
    public IEnumerator MoveForward(PieceController pieceController, Vector2Int moveDirection)
    {
        if (moveDirection != Vector2Int.up && moveDirection != Vector2Int.down &&
           moveDirection != Vector2Int.right && moveDirection != Vector2Int.left)
        {
            Debug.LogWarning($"Invalid move direction: {moveDirection}");
            yield break;
        }

        Vector2Int gridPos = pieceController.gridPosition;
        gridPos += moveDirection;
        pieceController.gridPosition = gridPos;

        if (gridPos.y == BoardManager.Instance.boardSizeY - 1)
        {
            if (!MissionManager.Instance.CanGoFinishLine())
            {
                ToastManager.Instance.ShowToast("추가 미션을 완료해야 도착점에 갈 수 있습니다.", transform, 0f);
                yield break;
            }
        }

        // 스프라이트 이동
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


        // 보드 
        Vector2Int PiecePosition = PieceManager.Instance.currentPiece.gridPosition;
        Vector2Int lastPosition = PieceManager.Instance.currentPiece.gridPosition - moveDirection;


        BoardManager.Instance.Board[lastPosition.x, lastPosition.y].SetPiece(null);
        BoardManager.Instance.Board[PiecePosition.x, PiecePosition.y].SetPiece(pieceController);

        // 기존 SetPiece 코드 위에 타일 색상 처리 추가
        BoardManager.Instance.Board[lastPosition.x, lastPosition.y].TileColor = pieceController.lastTileColor;
        pieceController.lastTileColor = BoardManager.Instance.Board[PiecePosition.x, PiecePosition.y].TileColor;
        BoardManager.Instance.Board[PiecePosition.x, PiecePosition.y].TileColor = pieceController.GetPiece().faces[2].color;


        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.TrySkill(gridPos, pieceController);
        }
        else
        {
            Debug.LogError("SkillManager.Instance is null!");
        }

        // 도착점 체크
        MissionManager.Instance.CheckStageClearAfterMove(gridPos);
        // 모든 미션완료 상태 체크
        MissionManager.Instance.IsAllMissionCompleted(pieceController);

        BoardSelectManager.Instance.PieceHighlightTiles(gridPos);

        ObstacleManager.Instance.UpdateObstacleStep();


        GameManager.Instance.IsLockCursor = false;
    }

    // 아기 스킬 : 다른 기물 이동
    public IEnumerator HelpBaby(PieceController pieceController)
    {

        GameManager.Instance.IsLockCursor = true;

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
            ToastManager.Instance.ShowToast("아기쪽으로 이동 가능한 기물이 없습니다.", pieceController.transform, 1.5f);

            GameManager.Instance.IsLockCursor = false;
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
        pieceSelectUI.CreateButtonsForMoveSkill();


        // 화살표 클릭 대기
        yield return moveSkillUI.WaitForArrowClick();

        // 기물 선택 UI 종료
        pieceSelectUI.ClearButtons();



        // 하이라이트 타일 제거 및 현재 기물 위치 하이라이트
        BoardSelectManager.Instance.DestroyPieceHighlightTile();
        PieceManager.Instance.currentPiece = pieceController;
        BoardSelectManager.Instance.PieceHighlightTiles(pieceController.gridPosition);


        GameManager.Instance.IsLockCursor = false;
    }

    // 나무꾼 스킬 : 나무 방벽 설치
    public IEnumerator CreateWoodBox()
    {


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

        if (woodCutterSkillEffect != null)
        {
            GameObject effect = Instantiate(
                woodCutterSkillEffect,
                effectPosition,
                Quaternion.identity,
                BoardManager.Instance.boardTransform
            );
            Destroy(effect, 0.5f);
        }
        else
        {
            Debug.LogWarning("나무꾼 스킬 할당 안됨");
        }

        yield return new WaitForSeconds(0.5f);
        BoardManager.Instance.CreateObstacle(selectPos, ObstacleType.WoodBox);
        SkillManager.Instance.IsSelectingProgress = false;

        GameManager.Instance.IsLockCursor = false;

    }

    // 마법사 스킬 : 본인을 제외한 기물간 위치 변경
    public IEnumerator SwapPieces(PieceController pieceController)
    {

        GameManager.Instance.IsLockCursor = true;

        yield return new WaitForSeconds(SkillManager.Instance.blinkTime + 0.1f);

        // 기물들 중 이동 가능한 타일이 있는지 확인
        bool hasOtherPiece = false;
        foreach (PieceController targetPiece in PieceManager.Instance.Pieces)
        {
            if (targetPiece == null || targetPiece == PieceManager.Instance.currentPiece) // null 기물과 본인을 제외한
                continue;
            hasOtherPiece = true;
            break; // 기물이 하나라도 있으면 루프 종료
        }

        // 이동 가능한 기물이 없으면 코루틴 종료
        if (!hasOtherPiece)
        {
            Debug.Log("마법사 스킬 썼지만 발동 가능한 기물이 없네");
            ToastManager.Instance.ShowToast("마법사쪽으로 위치변환 가능한 기물이 없습니다.", pieceController.transform, 1.5f);

            GameManager.Instance.IsLockCursor = false;
            yield break;
        }

        // 기존 하이라이트 타일 제거
        BoardSelectManager.Instance.DestroyPieceHighlightTile();

        // 모든 유효한 기물 위치에 하이라이트 타일 생성 (본인 포함)
        foreach (PieceController p in PieceManager.Instance.Pieces)
        {
            if (p == null)
                continue;
            // 하이라이트 타일 생성
            BoardSelectManager.Instance.PieceHighLightTilesMulty(p.gridPosition);
        }

        // 기물 선택 UI 생성
        pieceSelectUI.CreateButtonsForSwapSkill();

        // 두 기물 선택 대기
        yield return StartCoroutine(pieceSelectUI.WaitForArrowClick(pieceController));

        // 위치 교환 후 이펙트 생성
        if (pieceSelectUI.firstSelectedPiece != null && pieceSelectUI.isPieceSelected)
        {
            PieceController secondSelectedPiece = PieceManager.Instance.currentPiece;
            if (secondSelectedPiece != null)
            {
                // 첫 번째 기물 위치에 이펙트 생성
                if (wizardSkillEffect != null)
                {
                    GameObject effect1 = Instantiate(
                        wizardSkillEffect,
                        pieceSelectUI.firstSelectedPiece.transform.position,
                        Quaternion.identity,
                        BoardManager.Instance.boardTransform
                    );
                    Destroy(effect1, 1f);
                }

                // 두 번째 기물 위치에 이펙트 생성
                if (wizardSkillEffect != null)
                {
                    GameObject effect2 = Instantiate(
                        wizardSkillEffect,
                        secondSelectedPiece.transform.position,
                        Quaternion.identity,
                        BoardManager.Instance.boardTransform
                    );
                    Destroy(effect2, 1f);
                }

                // 위치 교환
                Vector2Int tempPosition = pieceSelectUI.firstSelectedPiece.gridPosition;
                pieceSelectUI.firstSelectedPiece.gridPosition = secondSelectedPiece.gridPosition;
                secondSelectedPiece.gridPosition = tempPosition;

                // 렌더링 위치 교환
                Vector3 tempWorldPosition = pieceSelectUI.firstSelectedPiece.transform.position;
                pieceSelectUI.firstSelectedPiece.transform.position = secondSelectedPiece.transform.position;
                secondSelectedPiece.transform.position = tempWorldPosition;

                // 타일에 Piece 설정
                BoardManager.Instance.Board[pieceSelectUI.firstSelectedPiece.gridPosition.x, pieceSelectUI.firstSelectedPiece.gridPosition.y].SetPiece(pieceSelectUI.firstSelectedPiece);
                BoardManager.Instance.Board[secondSelectedPiece.gridPosition.x, secondSelectedPiece.gridPosition.y].SetPiece(secondSelectedPiece);

                // 타일의 색상값 교환
                TileColor tempColor = BoardManager.Instance.Board[pieceSelectUI.firstSelectedPiece.gridPosition.x, pieceSelectUI.firstSelectedPiece.gridPosition.y].TileColor;
                BoardManager.Instance.Board[pieceSelectUI.firstSelectedPiece.gridPosition.x, pieceSelectUI.firstSelectedPiece.gridPosition.y].TileColor =
                   BoardManager.Instance.Board[secondSelectedPiece.gridPosition.x, secondSelectedPiece.gridPosition.y].TileColor;
                BoardManager.Instance.Board[secondSelectedPiece.gridPosition.x, secondSelectedPiece.gridPosition.y].TileColor = tempColor;


                // 타일의 렌더링 색상 교환
                Color tempColor2 = BoardManager.Instance.Board[pieceSelectUI.firstSelectedPiece.gridPosition.x, pieceSelectUI.firstSelectedPiece.gridPosition.y].GetComponent<SpriteRenderer>().color;
                BoardManager.Instance.Board[pieceSelectUI.firstSelectedPiece.gridPosition.x, pieceSelectUI.firstSelectedPiece.gridPosition.y].SetTileColor(
                    BoardManager.Instance.Board[secondSelectedPiece.gridPosition.x, secondSelectedPiece.gridPosition.y].GetComponent<SpriteRenderer>().color);
                BoardManager.Instance.Board[secondSelectedPiece.gridPosition.x, secondSelectedPiece.gridPosition.y].SetTileColor(tempColor2);

                // lastTileColor 갱신
                //pieceSelectUI.firstSelectedPiece.lastTileColor =
                //    BoardManager.Instance.Board[pieceSelectUI.firstSelectedPiece.gridPosition.x, pieceSelectUI.firstSelectedPiece.gridPosition.y].TileColor;
                //secondSelectedPiece.lastTileColor =
                //    BoardManager.Instance.Board[secondSelectedPiece.gridPosition.x, secondSelectedPiece.gridPosition.y].TileColor;

                //Debug.Log($"Swapped {pieceSelectUI.firstSelectedPiece.name} and {secondSelectedPiece.name}");

                if (SkillManager.Instance != null)
                {
                    SkillManager.Instance.TrySkill(secondSelectedPiece.gridPosition, secondSelectedPiece);
                    SkillManager.Instance.TrySkill(pieceSelectUI.firstSelectedPiece.gridPosition, pieceSelectUI.firstSelectedPiece);
                }
                else
                {
                    Debug.LogError("SkillManager.Instance is null!");
                }
            }
            else
            {
                Debug.Log("유효하지 않은 기물 선택");
                ToastManager.Instance.ShowToast("유효하지 않은 기물 선택입니다.", pieceController.transform, 1.5f);
            }
        }

        // UI 버튼 제거
        pieceSelectUI.ClearButtons();



        // 하이라이트 타일 제거 및 현재 기물 위치 하이라이트
        BoardSelectManager.Instance.DestroyPieceHighlightTile();
        PieceManager.Instance.currentPiece = pieceController;
        BoardSelectManager.Instance.PieceHighlightTiles(pieceController.gridPosition);


        GameManager.Instance.IsLockCursor = false;
    }

    // 광전사 스킬 : 1턴간 자신 기절
    public IEnumerator SelfStun(PieceController pieceController)
    {
        yield return new WaitForSeconds(SkillManager.Instance.blinkTime + 0.1f);

        var stunTurns = 1;
        pieceController.statusEffectController.SetStatus(PieceStatus.Stun, stunTurns);

        ToastManager.Instance.ShowToast($"자신을 {stunTurns}턴간 기절시킵니다.", pieceController.transform, 1.5f);

        GameManager.Instance.IsLockCursor = false;
    }

}