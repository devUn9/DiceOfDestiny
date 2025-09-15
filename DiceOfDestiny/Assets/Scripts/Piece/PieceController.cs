using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceController : MonoBehaviour
{
    [SerializeField] private Piece piece;
    [SerializeField] private Vector2Int lastMoveDirection = Vector2Int.zero;
    [SerializeField] public Vector2Int gridPosition;

    [SerializeField] private SpriteRenderer classRenderer;
    [SerializeField] public SpriteRenderer colorRenderer;
    [SerializeField] private SpriteRenderer animationRenderer;

    public bool isMoving { get; private set; } = false; // 이동 중인지 여부
    public bool canControl = true; // 기물 조작 가능 여부
    private bool animPlaying = false; // 애니메이션 재생 중인지 여부
    public bool isOutStartingLine = false; // 시작 지점에서 벗어났는지 여부
    public TileColor lastTileColor = TileColor.None; // 마지막 타일 색상

    public PieceStatusEffectController statusEffectController;
    public UIFollow uiFollow;
    private Animator animator;

    // 전개도 데이터 (십자형: 0:바닥, 1:앞, 2:위, 3:뒤, 4:왼쪽, 5:오른쪽)
    private readonly int[] upTransition = new int[] { 1, 2, 3, 0, 4, 5 }; // 위로 이동
    private readonly int[] downTransition = new int[] { 3, 0, 1, 2, 4, 5 }; // 아래로 이동
    private readonly int[] leftTransition = new int[] { 4, 1, 5, 3, 2, 0 }; // 왼쪽으로 이동
    private readonly int[] rightTransition = new int[] { 5, 1, 4, 3, 0, 2 }; // 오른쪽으로 이동

    private float duration = 0.8f; // 이동 애니메이션 시간

    void Start()
    {
        statusEffectController = GetComponent<PieceStatusEffectController>();
        animator = GetComponentInChildren<Animator>();
        uiFollow = GetComponent<UIFollow>();
    }

    void Update()
    {
        //if (!canControl) return;
        TestInput();
    }


    // 상, 하, 좌, 우 버튼 클릭 시 호출될 public 메서드
    public void MoveUp()
    {
        if (this != PieceManager.Instance.GetCurrentPiece() || isMoving) return;
        MoveToDirection(Vector2Int.up);
    }

    public void MoveDown()
    {
        if (this != PieceManager.Instance.GetCurrentPiece() || isMoving) return;
        MoveToDirection(Vector2Int.down);
    }

    public void MoveLeft()
    {
        if (this != PieceManager.Instance.GetCurrentPiece() || isMoving) return;
        MoveToDirection(Vector2Int.left);
    }

    public void MoveRight()
    {
        if (this != PieceManager.Instance.GetCurrentPiece() || isMoving) return;
        MoveToDirection(Vector2Int.right);
    }


    public void TestInput() // 이벤트로 넘기거나 할 필요가 있을듯................................하바ㅏㅏㅏㅏㅏㅏ니ㅏㄷ..............밑에관련메소드잇음................................
    {
        if (this != PieceManager.Instance.GetCurrentPiece())
            return;

        Vector2Int moveDirection = Vector2Int.zero;
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                moveDirection = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                moveDirection = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                moveDirection = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                moveDirection = Vector2Int.right;
        }
        MoveToDirection(moveDirection);

    }

    public void MoveToDirection(Vector2Int moveDirection)
    {
        // Null checks for required singletons
        if (PieceManager.Instance == null || BoardManager.Instance == null || MissionManager.Instance == null || SkillManager.Instance == null)
        {
            Debug.LogWarning("MoveToDirection: Required singleton is null.");
            return;
        }
        //if (!canControl) return;
        if (moveDirection == Vector2Int.zero) return;
        if (moveDirection != Vector2Int.zero)
        {
            Vector2Int newPosition = gridPosition + moveDirection;

            // 이동 확정 시
            // 행동력이 0이면 행동 불가
            if (!ActionPointManager.Instance.CanUse())
            {
                ToastManager.Instance.ShowToast("행동력이 부족합니다.", transform, 0f);
                return;
            }

            // 이동하는 곳이 보드 밖이면 return
            if (!BoardManager.Instance.IsInsideBoard(newPosition))
            {
                return;
            }

            if (newPosition.y == BoardManager.Instance.boardSizeY - 1)
            {
                if (!MissionManager.Instance.CanGoFinishLine())
                {
                    ToastManager.Instance.ShowToast("추가 미션을 완료해야 도착점에 갈 수 있습니다.", transform, 0f);
                    return;
                }
            }

            if (statusEffectController.IsStatusActive(PieceStatus.Stun))
            {
                int stunTurn = statusEffectController.GetRemainingTurn(PieceStatus.Stun);
                Debug.Log("Piece is stunned!");
                ToastManager.Instance.ShowToast(message: $"기물이 기절했습니다! {stunTurn}턴간 이동할 수 없습니다.", transform, 0f);
                return;
            }

            if (statusEffectController.IsStatusActive(PieceStatus.Disease) && ActionPointManager.Instance.GetAP() < 2)
            {
                int DiseaseTurn = statusEffectController.GetRemainingTurn(PieceStatus.Disease);
                Debug.Log("Piece is diseased!");
                ToastManager.Instance.ShowToast(message: $"기물이 질병에 걸렸습니다! {DiseaseTurn}턴간 행동이 제한됩니다.", transform, 0f);
                return;
            }

            // 시작지점으로 다시 돌아가려고 하면
            if (isOutStartingLine && newPosition.y == 0)
            {
                ToastManager.Instance.ShowToast(message: "시작 지점으로 돌아갈 수 없습니다!", transform, 0f);
                RotateHalfBack(moveDirection);
                return;
            }

            // 이동하는 곳에 장애물이 있으면
            Debug.Log("Obstacle Name : " + BoardManager.Instance.Board[newPosition.x, newPosition.y].Obstacle);
            if (BoardManager.Instance.Board[newPosition.x, newPosition.y].Obstacle != ObstacleType.None ||
                BoardManager.Instance.Board[newPosition.x, newPosition.y].GetPiece() != null)
            {
                // 밟을 수 없다면
                if (!BoardManager.Instance.Board[newPosition.x, newPosition.y].isWalkable)
                {
                    RotateHalfBack(moveDirection); // 튕김 애니메이션
                    return;
                }
            }

            // 다음 윗면의 인덱스 계산
            int nextFaceIndex = -1;
            if (moveDirection == Vector2Int.up)
                nextFaceIndex = upTransition[2];
            else if (moveDirection == Vector2Int.down)
                nextFaceIndex = downTransition[2];
            else if (moveDirection == Vector2Int.left)
                nextFaceIndex = leftTransition[2];
            else if (moveDirection == Vector2Int.right)
                nextFaceIndex = rightTransition[2];

            // 다음 윗면이 악마인 경우 사제와의 제약 조건 확인
            if (nextFaceIndex >= 0 && piece.faces[nextFaceIndex].classData.className == "Demon")
            {
                foreach (PieceController targetPiece in PieceManager.Instance.Pieces)
                {
                    if (targetPiece == null || targetPiece == this) continue;

                    Face targetFace = targetPiece.GetTopFace();
                    if (targetFace.classData.className == "Priest")
                    {
                        List<Vector2Int> surroundList = BoardManager.Instance.GetTilePositions(DirectionType.Eight, targetPiece.gridPosition);
                        if (surroundList.Contains(newPosition))
                        {
                            Debug.Log("Cannot move to a position near a Priest due to Demon face!");
                            ToastManager.Instance.ShowToast("악마와 사제는 공존할 수 없습니다!", transform, 0f);
                            RotateHalfBack(moveDirection);
                            return;
                        }
                    }
                }
            }

            // 다음 윗면이 사제인 경우 악마와의 제약 조건 확인
            if (nextFaceIndex >= 0 && piece.faces[nextFaceIndex].classData.className == "Priest")
            {
                foreach (PieceController targetPiece in PieceManager.Instance.Pieces)
                {
                    if (targetPiece == null || targetPiece == this) continue; // 널이거나 본인 제외

                    Face targetFace = targetPiece.GetTopFace();
                    if (targetFace.classData.className == "Demon")
                    {
                        List<Vector2Int> surroundList = BoardManager.Instance.GetTilePositions(DirectionType.Eight, targetPiece.gridPosition);
                        if (surroundList.Contains(newPosition))
                        {
                            Debug.Log("Cannot move to a position near a Priest due to Demon face!");
                            ToastManager.Instance.ShowToast("악마와 사제는 공존할 수 없습니다!", transform, 0f);
                            RotateHalfBack(moveDirection);
                            return;
                        }
                    }
                }
            }

            if (newPosition.x >= 0 && newPosition.x < BoardManager.Instance.boardSize &&
                newPosition.y >= 0 && newPosition.y < BoardManager.Instance.boardSizeY)
            {
                if (PieceManager.Instance == null)
                {
                    Debug.LogError("PieceManager.Instance is null!");
                    return;
                }

                if (piece == null)
                {
                    Debug.LogError("Piece is null!");
                    return;
                }

                ActionPointManager.Instance.RemoveAP(); // 행동력 감소

                if (statusEffectController.IsStatusActive(PieceStatus.Disease))
                {
                    ActionPointManager.Instance.RemoveAP(); // 질병 상태라면 추가로 행동력 감소
                }

                // 이전 타일에 Piece 값을 null로 바꾸고, 다음 타일에 Piece 값을 적용 
                BoardManager.Instance.Board[gridPosition.x, gridPosition.y].SetPiece(null);
                BoardManager.Instance.Board[newPosition.x, newPosition.y].SetPiece(this);

                //// 현재 타일에 색 적용
                //BoardManager.Instance.Board[gridPosition.x, gridPosition.y].TileColor = lastTileColor;
                //lastTileColor = BoardManager.Instance.Board[newPosition.x, newPosition.y].TileColor;
                //BoardManager.Instance.Board[newPosition.x, newPosition.y].TileColor = piece.faces[2].color;

                BoardManager.Instance.Board[gridPosition.x, gridPosition.y].TileColor = lastTileColor; // 기존 타일이 가지고 있던 색
                // 이전 타일 색상 저장
                lastTileColor = BoardManager.Instance.Board[newPosition.x, newPosition.y].TileColor;

                // 다음 윗면의 인덱스 계산
                int nextFaceIndex2 = -1;
                if (moveDirection == Vector2Int.up)
                    nextFaceIndex2 = upTransition[2];
                else if (moveDirection == Vector2Int.down)
                    nextFaceIndex2 = downTransition[2];
                else if (moveDirection == Vector2Int.left)
                    nextFaceIndex2 = leftTransition[2];
                else if (moveDirection == Vector2Int.right)
                    nextFaceIndex2 = rightTransition[2];

                // 다음 윗면의 색상을 보드 타일에 할당
                if (nextFaceIndex2 != -1) // 유효한 인덱스인지 확인
                {
                    BoardManager.Instance.Board[newPosition.x, newPosition.y].TileColor = piece.faces[nextFaceIndex2].color;
                }
                else
                {
                    Debug.LogError("유효하지 않은 이동 방향입니다.");
                }



                // 마지막 이동 방향 저장
                lastMoveDirection = moveDirection;

                // 실제 이동
                RotateToTopFace(moveDirection);
                UpdateTopFace(moveDirection); // 윗면 업데이트

                // 도착점 체크
                MissionManager.Instance.CheckStageClearAfterMove(newPosition);

                // 모든 미션완료 상태 체크
                StartCoroutine(MissionManager.Instance.IsAllMissionCompleted(this));

                // 모든 장애물 기믹 동작
                ObstacleManager.Instance.UpdateObstacleStep();

                // 스킬 발동
                StartCoroutine(SkillCoroutine());



                FindAnyObjectByType<TutorialS1Director>()?.OnTileMoved();
            }
            //if (SkillManager.Instance != null)
            //{
            //    if (gridPosition.y != 0 && gridPosition.y != BoardManager.Instance.boardSizeY - 1)
            //    {
            //        SkillManager.Instance.TrySkill(gridPosition, this);
            //        MissionManager.Instance.CheckPassiveSkillUse();
            //    }
            //}
            else
            {
                Debug.LogWarning($"Invalid move to position: {newPosition}");
            }
        }
    }

    IEnumerator SkillCoroutine()
    {
        yield return new WaitForSeconds(duration); // 스킬 발동 후 잠시 대기
        // 스킬 발동
        if (SkillManager.Instance != null)
        {
            // y값이 0이나 14가 아니면
            if (PieceManager.Instance.currentPiece.gridPosition.y != 0 && PieceManager.Instance.currentPiece.gridPosition.y != 14)
            {
                SkillManager.Instance.TrySkill(gridPosition, this);
                //SkillManager.Instance.TryActiveSkill(gridPosition, this);

                // 미션 발동
                MissionManager.Instance.CheckPassiveSkillUse();
            }
        }
        else
        {
            Debug.LogError("SkillManager.Instance is null!");
        }
    }

    // 기물 눌렀을 때 호출, BoardSelectManager에 저장
    private void OnMouseUp()
    {
        if (GameManager.Instance.IsLockCursor)
            return; // 커서 잠금 상태면 무시

        
        // UI 위 클릭이면 무시
        if (IsPointerOnLayer("BlockUI"))
        {
            return;
        }

        // 움직이거나 컨트롤 불가능하면 무시
        if (isMoving || SkillManager.Instance.IsSelectingProgress || !canControl)
        {
            return;
        }

        Vector2Int position = new Vector2Int(
        Mathf.RoundToInt(transform.position.x - BoardManager.Instance.boardTransform.position.x),
        Mathf.RoundToInt(transform.position.y - BoardManager.Instance.boardTransform.position.y));

        if (piece != null)
        {
            Debug.Log($"Piece clicked: {piece}, at position: {position}");
            PieceManager.Instance.currentPiece = this;
            BoardSelectManager.Instance.PieceHighlightTiles(position);
            EventManager.Instance.TriggerEvent("ToggleUIElement");
        }

        // 클릭 시 애니메이션 트리거 실행
        if (animator != null && !animPlaying && uiFollow != null && uiFollow.IsUIActive())
        {
            string animationName = "";
            if (piece.faces == null || piece.faces.Length < 3 || piece.faces[2].classData == null)
            {
                Debug.LogWarning("piece.faces[2] or its classData is null. Animation cannot be played.");
                return;
            }
            switch (piece.faces[2].classData.className)
            {
                case "Knight":
                    animationName = "Knight_Idle";
                    break;
                case "Priest":
                    animationName = "Priest_Idle";
                    break;
                case "Demon":
                    animationName = "Demon_Idle";
                    break;
                case "Thief":
                    animationName = "Thief_Idle";
                    break;
                case "Baby":
                    animationName = "Baby_Idle";
                    break;
                case "Painter":
                    animationName = "Painter_Idle";
                    break;
                case "Fanatic":
                    animationName = "Fanatic_Idle";
                    break;
                case "Logger":
                    animationName = "Logger_Idle";
                    break;
                case "Wizard":
                    animationName = "Wizard_Idle";
                    break;
                case "Berserker":
                    animationName = "Berserker_Idle";
                    break;

                default:
                    Debug.LogWarning($"Unknown class: {piece.faces[2].classData.className}");
                    break;
            }
            if (classRenderer == null || animationRenderer == null)
            {
                Debug.LogWarning("classRenderer or animationRenderer is null.");
                return;
            }
            classRenderer.gameObject.SetActive(false);
            animationRenderer.gameObject.SetActive(true);
            animator.enabled = true; // Animator 활성화
            animator.Play(animationName, 0, 0f); // 애니메이션 재생
            // currentPiece일 경우 루프 애니메이션 시작
            if (PieceManager.Instance.currentPiece == this)
            {
                StartCoroutine(LoopAnimation(animationName));
            }
            else
            {
                animator.Play(animationName, 0, 0f); // 한 번만 재생
                StartCoroutine(EndAnimation(animationName));
            }
        }
    }

    private IEnumerator LoopAnimation(string animationName)
    {
        animPlaying = true; // 애니메이션 재생 중 상태 설정

        float animationLength = GetAnimationLength(animationName);

        while (PieceManager.Instance.currentPiece == this && animPlaying)
        {
            animator.Play(animationName, 0, 0f); // 애니메이션 재생
            yield return new WaitForSeconds(animationLength); // 애니메이션 길이만큼 대기
        }

        // 루프 종료 후 애니메이션 비활성화
        StartCoroutine(EndAnimation(animationName));
    }

    private IEnumerator EndAnimation(string animationName)
    {
        float animationLength = GetAnimationLength(animationName);
        yield return new WaitForSeconds(animationLength);

        if (animator != null)
        {
            animator.enabled = false; // 애니메이션 종료 후 Animator 비활성화
        }

        classRenderer.gameObject.SetActive(true);
        animationRenderer.gameObject.SetActive(false);
        animPlaying = false;
    }

    // 애니메이션 클립의 길이를 가져오는 헬퍼 메서드
    private float GetAnimationLength(string animationName)
    {
        if (animator == null) return 1f; // 기본값
        var controller = animator.runtimeAnimatorController;
        foreach (var clip in controller.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        return 1f; // 기본값
    }

    private bool IsPointerOnLayer(string layerName)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        int targetLayer = LayerMask.NameToLayer(layerName);

        foreach (var result in results)
        {
            if (result.gameObject.layer == targetLayer)
                return true; // 해당 레이어 위에 있음
        }

        return false; // 해당 레이어 위에 없음
    }

    public Face GetTopFace()
    {
        return piece.faces[2];
    }

    public void UpdateTopFace(Vector2Int direction)
    {
        Face[] newFaces = new Face[6];

        // 이동 방향에 따라 faces 배열 재배치
        if (direction == Vector2Int.up)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = piece.faces[upTransition[i]];

        }
        else if (direction == Vector2Int.down)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = piece.faces[downTransition[i]];

        }
        else if (direction == Vector2Int.left)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = piece.faces[leftTransition[i]];

        }
        else if (direction == Vector2Int.right)
        {
            for (int i = 0; i < 6; i++)
                newFaces[i] = piece.faces[rightTransition[i]];
        }
        else
        {
            Debug.LogWarning($"Invalid move direction: {direction}");
            return;
        }

        piece.faces = newFaces;
    }

    public void RotateToTopFace(Vector2Int moveDirection)
    {
        classRenderer.gameObject.SetActive(true);
        animationRenderer.gameObject.SetActive(false);
        animPlaying = false;

        StartCoroutine(RotateToTopFaceCoroutine(moveDirection));
    }

    public IEnumerator RotateToTopFaceCoroutine(Vector2Int moveDirection)
    {
        GameManager.Instance.IsLockCursor = true;
        isMoving = true; // 이동 중 입력받지 아니함. 

        int nextfaceIndex = 3; // 다음 윗면 인덱스

        if (moveDirection == Vector2Int.up)
            nextfaceIndex = 3;
        else if (moveDirection == Vector2Int.down)
            nextfaceIndex = 1;
        else if (moveDirection == Vector2Int.right)
            nextfaceIndex = 4;
        else if (moveDirection == Vector2Int.left)
            nextfaceIndex = 5;
        else
        {
            Debug.LogWarning($"Invalid move direction for rotation: {moveDirection}");

            GameManager.Instance.IsLockCursor = false;
            isMoving = false;
            yield break;
        }

        bool vertical = moveDirection == Vector2Int.up || moveDirection == Vector2Int.down;
        Vector3 moveVec = new Vector3(moveDirection.x, moveDirection.y, 0);

        Transform classTransform = classRenderer.transform;
        Transform colorTransform = colorRenderer.transform;

        GameObject newClassObj = Instantiate(classRenderer.gameObject, transform);
        GameObject newColorObj = Instantiate(colorRenderer.gameObject, transform);

        newClassObj.name = "NewClassRenderer";
        newColorObj.name = "NewColorRenderer";

        SpriteRenderer newClassRenderer = newClassObj.GetComponent<SpriteRenderer>();
        SpriteRenderer newColorRenderer = newColorObj.GetComponent<SpriteRenderer>();

        newClassRenderer.sprite = piece.faces[nextfaceIndex].classData.sprite;
        newColorRenderer.color = BoardManager.Instance.tileColors[(int)piece.faces[nextfaceIndex].color];

        // 위치 초기화
        classTransform.localPosition = moveVec * 0.5f;
        colorTransform.localPosition = moveVec * 0.5f;
        newClassObj.transform.localPosition = -moveVec * 0.5f;
        newColorObj.transform.localPosition = -moveVec * 0.5f;

        // 스케일 초기화
        classTransform.localScale = Vector3.one;
        colorTransform.localScale = Vector3.one;
        newClassObj.transform.localScale = Vector3.zero;
        newColorObj.transform.localScale = Vector3.zero;

        duration = 0.8f;
        float time = 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + moveVec;

        float inflateAmount = 0.4f; // 부풀어 오르는 양

        while (time < duration)
        {
            float t = time / duration;
            float ease = Mathf.SmoothStep(0f, 1f, t);

            float inflate = Mathf.Sin(ease * Mathf.PI) * inflateAmount;
            float totalScale = 1f + inflate;

            float scaleOld = (1f - ease) * totalScale;
            float scaleNew = ease * totalScale;

            // 렌더러 위치 (접점 기준 위치)
            Vector3 offsetOld = moveVec * (scaleOld * 0.5f);
            Vector3 offsetNew = -moveVec * (scaleNew * 0.5f);

            classTransform.localPosition = offsetOld;
            colorTransform.localPosition = offsetOld;
            newClassObj.transform.localPosition = offsetNew;
            newColorObj.transform.localPosition = offsetNew;

            Vector3 scaleVecOld;
            Vector3 scaleVecNew;

            // 렌더러 스케일 
            if (vertical)
            {
                scaleVecOld = new Vector3(1f, scaleOld, 1f);
                scaleVecNew = new Vector3(1f, scaleNew, 1f);
            }
            else
            {
                scaleVecOld = new Vector3(scaleOld, 1f, 1f);
                scaleVecNew = new Vector3(scaleNew, 1f, 1f);
            }

            classTransform.localScale = scaleVecOld;
            colorTransform.localScale = scaleVecOld;

            newClassObj.transform.localScale = scaleVecNew;
            newColorObj.transform.localScale = scaleVecNew;

            // 정확히 접점 기준으로 1만큼 이동하도록 보정
            float arc = Mathf.Sin(ease * Mathf.PI) * 0.15f;
            float contactOffset = (scaleOld - scaleNew) * 0.5f;

            transform.position = Vector3.Lerp(startPos, endPos, ease) + Vector3.up * arc - moveVec * contactOffset;

            time += Time.deltaTime;
            yield return null;
        }

        // 마무리.
        gridPosition += moveDirection;
        transform.position = endPos;

        Destroy(classRenderer.gameObject);
        Destroy(colorRenderer.gameObject);

        classRenderer = newClassRenderer;
        colorRenderer = newColorRenderer;

        classRenderer.transform.localPosition = Vector3.zero;
        colorRenderer.transform.localPosition = Vector3.zero;
        classRenderer.transform.localScale = Vector3.one;
        colorRenderer.transform.localScale = Vector3.one;

        BoardSelectManager.Instance.PieceHighlightTiles(gridPosition);

        GameManager.Instance.IsLockCursor = false;
        isMoving = false;


        CheckOutStartingLine();

        // 스킬 발동
        //if (SkillManager.Instance != null)
        //{
        //    // y값이 0이나 14가 아니면
        //    if (PieceManager.Instance.currentPiece.gridPosition.y != 0 && PieceManager.Instance.currentPiece.gridPosition.y != 14)
        //    {
        //        SkillManager.Instance.TrySkill(gridPosition, this);
        //        //SkillManager.Instance.TryActiveSkill(gridPosition, this);
        //        MissionManager.Instance.CheckPassiveSkillUse();
        //    }
        //}
        //else
        //{
        //    Debug.LogError("SkillManager.Instance is null!");
        //}
    }

    public void RotateHalfBack(Vector2Int moveDirection)
    {
        if (animator != null)
        {
            animator.enabled = false; // 애니메이션 종료 후 Animator 비활성화
        }

        classRenderer.gameObject.SetActive(true);
        animationRenderer.gameObject.SetActive(false);
        animPlaying = false;

        StartCoroutine(RotateHalfBackCoroutine(moveDirection));
    }

    IEnumerator RotateHalfBackCoroutine(Vector2Int moveDirection)
    {
        GameManager.Instance.IsLockCursor = true;
        isMoving = true;

        float overshoot = 0.3f; // 진행 정도

        int nextfaceIndex = 3;
        if (moveDirection == Vector2Int.up) nextfaceIndex = 3; // 위로 이동
        if (moveDirection == Vector2Int.down) nextfaceIndex = 1;
        if (moveDirection == Vector2Int.right) nextfaceIndex = 4;
        if (moveDirection == Vector2Int.left) nextfaceIndex = 5;

        bool vertical = moveDirection == Vector2Int.up || moveDirection == Vector2Int.down;
        Vector3 moveVec = new Vector3(moveDirection.x, moveDirection.y, 0);

        Transform classTransform = classRenderer.transform;
        Transform colorTransform = colorRenderer.transform;

        GameObject newClassObj = Instantiate(classRenderer.gameObject, transform);
        GameObject newColorObj = Instantiate(colorRenderer.gameObject, transform);

        newClassObj.name = "NewClassRenderer";
        newColorObj.name = "NewColorRenderer";

        SpriteRenderer newClassRenderer = newClassObj.GetComponent<SpriteRenderer>();
        SpriteRenderer newColorRenderer = newColorObj.GetComponent<SpriteRenderer>();

        newClassRenderer.sprite = piece.faces[nextfaceIndex].classData.sprite;
        newColorRenderer.color = BoardManager.Instance.tileColors[(int)piece.faces[nextfaceIndex].color];

        // 초기 위치
        classTransform.localPosition = moveVec * 0.5f;
        colorTransform.localPosition = moveVec * 0.5f;
        newClassObj.transform.localPosition = -moveVec * 0.5f;
        newColorObj.transform.localPosition = -moveVec * 0.5f;

        classTransform.localScale = Vector3.one;
        colorTransform.localScale = Vector3.one;
        newClassObj.transform.localScale = Vector3.zero;
        newColorObj.transform.localScale = Vector3.zero;

        float duration = 0.4f; // 튕겨나감 + 복귀 전체 시간
        float time = 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + moveVec * overshoot;

        float inflateAmount = 0.3f;

        while (time < duration)
        {
            float t = time / duration;

            // 빠르게 튕기고, 천천히 복귀하는 커브
            float ease = t < 0.3f
                ? Mathf.SmoothStep(0f, 1f, t / 0.3f)                    // 빠르게 전진 (0~0.3초)
                : Mathf.SmoothStep(1f, 0f, (t - 0.3f) / (0.7f));         // 천천히 복귀 (0.3~1)

            float inflate = Mathf.Sin(ease * Mathf.PI) * inflateAmount;
            float totalScale = 1f + inflateAmount * Mathf.Sin(Mathf.PI * ease);
            float scaleOld = (1f - ease) * totalScale;
            float scaleNew = ease * totalScale;

            Vector3 offsetOld = moveVec * (scaleOld * 0.5f);
            Vector3 offsetNew = -moveVec * (scaleNew * 0.5f);

            classTransform.localPosition = offsetOld;
            colorTransform.localPosition = offsetOld;
            newClassObj.transform.localPosition = offsetNew;
            newColorObj.transform.localPosition = offsetNew;

            Vector3 scaleVecOld = vertical ? new Vector3(1f, scaleOld, 1f) : new Vector3(scaleOld, 1f, 1f);
            Vector3 scaleVecNew = vertical ? new Vector3(1f, scaleNew, 1f) : new Vector3(scaleNew, 1f, 1f);

            classTransform.localScale = scaleVecOld;
            colorTransform.localScale = scaleVecOld;
            newClassObj.transform.localScale = scaleVecNew;
            newColorObj.transform.localScale = scaleVecNew;

            float arc = Mathf.Sin(ease * Mathf.PI) * 0.15f;
            float contactOffset = (scaleOld - scaleNew) * 0.5f;

            transform.position = Vector3.Lerp(startPos, endPos, ease)
                                 + Vector3.up * arc
                                 - moveVec * contactOffset;

            time += Time.deltaTime;
            yield return null;
        }

        // 정리
        classTransform.localPosition = Vector3.zero;
        colorTransform.localPosition = Vector3.zero;
        classTransform.localScale = Vector3.one;
        colorTransform.localScale = Vector3.one;
        transform.position = startPos;

        Destroy(newClassObj);
        Destroy(newColorObj);

        GameManager.Instance.IsLockCursor = false;
        isMoving = false;
    }

    public Vector2Int GetLastMoveDirection()
    {
        return lastMoveDirection;
    }

    public Face GetFace(int index)
    {
        if (index >= 0 && index < 6)
            return piece.faces[index];
        Debug.LogError($"Invalid face index: {index}");
        return default;
    }

    public Piece GetPiece()
    {
        return piece;
    }

    public void SetPiece(Piece newPiece)
    {
        piece = newPiece;
    }

    public void SetTopFace()
    {
        if (classRenderer == null || colorRenderer == null || piece.faces == null || piece.faces.Length < 3 || piece.faces[2].classData == null)
        {
            Debug.LogWarning("SetTopFace: classRenderer, colorRenderer, or piece.faces[2].classData is null.");
            return;
        }
        classRenderer.sprite = piece.faces[2].classData.sprite;
        colorRenderer.color = BoardManager.Instance.tileColors[(int)piece.faces[2].color];
    }

    private bool isInGame;
    public bool IsinGame => isInGame;


    public void Init(Piece piece)
    {
        gridPosition = new Vector2Int(0, 0);
        SetPiece(piece);
    }

    public void SetInGame(bool value)
    {
        isInGame = value;
    }

    public Vector2Int MovePiece(Directions dir)
    {
        switch (dir)
        {
            case Directions.Up:
                return Vector2Int.up;
            case Directions.Down:
                return Vector2Int.down;
            case Directions.Left:
                return Vector2Int.left;
            case Directions.Right:
                return Vector2Int.right;
            default:
                return Vector2Int.zero;
        }

    }

    // 기물 직업 변경 메소드, 면 인덱스, 변경할 클래스 이름을 인자로 받음
    public void ChangeClass(int faceIndex, string newClassName)
    {
        if (faceIndex < 0 || faceIndex >= piece.faces.Length)
        {
            Debug.LogError($"Invalid face index: {faceIndex}");
            return;
        }

        // 새로운 클래스 데이터 찾기 (ClassData는 ScriptableObject로 가정)
        ClassData newClassData = Resources.Load<ClassData>($"Class/Class/{newClassName}");
        if (newClassData == null)
        {
            Debug.LogError($"ClassData for {newClassName} not found!");
            return;
        }

        // 해당 면의 클래스 데이터 변경 (runtime only)
        piece.faces[faceIndex].classData = newClassData;

        // 윗면(인덱스 2)이라면 렌더러 업데이트
        if (faceIndex == 2)
        {
            classRenderer.sprite = newClassData.sprite;
        }
    }

    public void SetFaceColor(int faceIndex, TileColor color)
    {
        if (faceIndex < 0 || faceIndex >= piece.faces.Length)
        {
            Debug.LogError($"Invalid face index: {faceIndex}");
            return;
        }
        piece.faces[faceIndex].color = color;
        if (faceIndex == 2)
        {
            colorRenderer.color = BoardManager.Instance.tileColors[(int)color];
        }
    }

    public void MoveClearPiece()
    {
        StartCoroutine(MoveClearPieceCoroutine());
    }

    IEnumerator MoveClearPieceCoroutine()
    {
        // 기물 확대
        float duration = BoardManager.Instance.boardSize * 0.1f;
        float time = 0;


        while (time < duration)
        {
            this.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 2f, time / duration);

            time += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // 사이즈 확정 보정
        this.transform.localScale = Vector3.one * 2f;

        // 기물 이동
        Vector3 targetPosition = transform.position + new Vector3(0, -BoardManager.Instance.boardSize - 1f, 0f);
        Vector3 startPositon = transform.position;
        float moveDuration = 2f;

        float moveTime = 0f;

        while (moveTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPositon, targetPosition, moveTime / moveDuration);
            moveTime += Time.deltaTime;
            yield return null;
        }

        // 위치 확정 보정
        transform.position = targetPosition;

        time = 0;
        while (time < duration)
        {
            this.transform.localScale = Vector3.Lerp(Vector3.one * 2f, Vector3.one, time / duration);

            time += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // 사이즈 확정 보정
        this.transform.localScale = Vector3.one;

        // 기물 위치 변경
        gridPosition = new Vector2Int(gridPosition.x, 0); // 기물 위치 초기화
        Debug.Log($"Piece moved to clear position: {gridPosition}");
    }

    public void CheckOutStartingLine()
    {
        if (this.gridPosition.y == 1)
            isOutStartingLine = true;
    }
}