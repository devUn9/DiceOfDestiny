using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PieceController : MonoBehaviour
{
    [SerializeField] private Piece piece; // 현재 기물
    [SerializeField] private Vector2Int lastMoveDirection = Vector2Int.zero; // 마지막 방향
    [SerializeField] public Vector2Int gridPosition;


    // 전개도 데이터 (십자형: 0:바닥, 1:앞, 2:위, 3:뒤, 4:왼쪽, 5:오른쪽)
    private readonly int[] upTransition = new int[] { 1, 2, 3, 0, 4, 5 }; // 위로 이동
    private readonly int[] downTransition = new int[] { 3, 0, 1, 2, 4, 5 }; // 아래로 이동
    private readonly int[] leftTransition = new int[] { 4, 1, 5, 3, 2, 0 }; // 왼쪽으로 이동
    private readonly int[] rightTransition = new int[] { 5, 1, 4, 3, 0, 2 }; // 오른쪽으로 이동

    [SerializeField] private SpriteRenderer classRenderer;
    [SerializeField] public SpriteRenderer colorRenderer;


    private bool isMoving = false; // 이동 중인지 여부
    [SerializeField] public bool isFinishLine = false; // 도착 지점인지 여부

    public StatusEffectController statusEffectController;

    void Start()
    {
        statusEffectController = GetComponent<StatusEffectController>();
    }

    void Update()
    {
        TestInput();
    }


    // 상, 하, 좌, 우 버튼 클릭 시 호출될 public 메서드
    public void MoveUp()
    {
        if (this != PieceManager.Instance.GetCurrentPiece() || isMoving) return;
        MoveToDirection(Vector2Int.up);
        Debug.Log("호출함");
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

            else if (Input.GetKeyDown(KeyCode.O)) // 스테이지 시작
            {
                PieceFaceManager.Instance.SavePieceFaceData(0);
                ToastManager.Instance.ShowToast("0번 피스 저장 !", PieceManager.Instance.currentPiece.transform);
            }
            else if (Input.GetKeyDown(KeyCode.P)) // 스테이지 끝
            {
                PieceFaceManager.Instance.RestorePieceFaceData(0);
                ToastManager.Instance.ShowToast("0번 피스 복구 !", PieceManager.Instance.currentPiece.transform);
            }
        }
        MoveToDirection(moveDirection);

    }

    public void MoveToDirection(Vector2Int moveDirection)
    {
        if (moveDirection != Vector2Int.zero)
        {
            Vector2Int newPosition = gridPosition + moveDirection;

            // 이동 확정 시
            // 행동력이 0이면 행동 불가
            if (!GameManager.Instance.actionPointManager.TryUseAP())
            {
                ToastManager.Instance.ShowToast("행동력이 부족합니다.", transform);
                return;
            }

            // 이동하는 곳이 보드 밖이면 return
            if (!BoardManager.Instance.IsInsideBoard(newPosition))
            {
                return;
            }

            if (statusEffectController.IsStatusActive(StatusType.Stun))
            {
                int stunTurn = statusEffectController.GetRemainingTurn(StatusType.Stun);
                Debug.Log("Piece is stunned!");
                ToastManager.Instance.ShowToast(message: $"기물이 기절했습니다! {stunTurn}턴간 이동할 수 없습니다.", transform);
                return;
            }

            if (statusEffectController.IsStatusActive(StatusType.Disease) && GameManager.Instance.actionPointManager.CurrentAP < 2)
            {
                int DiseaseTurn = statusEffectController.GetRemainingTurn(StatusType.Disease);
                Debug.Log("Piece is diseased!");
                ToastManager.Instance.ShowToast(message: $"기물이 질병에 걸렸습니다! {DiseaseTurn}턴간 행동이 제한됩니다.", transform);
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
                            ToastManager.Instance.ShowToast(message: "악마와 사제는 공존할 수 없습니다!", transform);
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
                            ToastManager.Instance.ShowToast(message: "악마와 사제는 공존할 수 없습니다!", transform);
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

                GameManager.Instance.actionPointManager.PieceAction();

                if (statusEffectController.IsStatusActive(StatusType.Disease))
                {
                    GameManager.Instance.actionPointManager.PieceAction();
                }


                // 이전 타일에 Piece 값을 null로 바꾸고, 다음 타일에 Piece 값을 적용 
                BoardManager.Instance.Board[gridPosition.x, gridPosition.y].SetPiece(null);
                BoardManager.Instance.Board[newPosition.x, newPosition.y].SetPiece(this);



                // 마지막 이동 방향 저장
                lastMoveDirection = moveDirection;

                // 실제 이동
                RotateToTopFace(moveDirection);
                UpdateTopFace(moveDirection); // 윗면 업데이트

                // newPosition이 도착지점이면 isFinishLine true
                StartCoroutine(CheckStageClearAfterMove(newPosition));

                ObstacleManager.Instance.UpdateObstacleStep();
            }
            else
            {
                Debug.LogWarning($"Invalid move to position: {newPosition}");
            }
        }
    }

    // 기물 눌렀을 때 호출, BoardSelectManager에 저장
    private void OnMouseUp()
    {
        if (SkillManager.Instance.IsSelectingProgress)
            return; // 스킬 진행 중이면 클릭 무시

        Vector2Int position = new Vector2Int(
        Mathf.RoundToInt(transform.position.x - BoardManager.Instance.boardTransform.position.x),
        Mathf.RoundToInt(transform.position.y - BoardManager.Instance.boardTransform.position.y));

        if (piece != null)
        {
            BoardSelectManager.Instance.PieceHighlightTiles(position);
            EventManager.Instance.TriggerEvent("ToggleUIElement");
        }
        //BoardSelectManager.Instance.SetClickedTilePosition(position);
        BoardSelectManager.Instance.ClearAllEffects();
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
        StartCoroutine(RotateToTopFaceCoroutine(moveDirection));
    }

    IEnumerator RotateToTopFaceCoroutine(Vector2Int moveDirection)
    {
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

        float duration = 0.8f;
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

        isMoving = false;

        // 스킬 발동
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.TrySkill(gridPosition, this);
            //SkillManager.Instance.TryActiveSkill(gridPosition, this);
        }
        else
        {
            Debug.LogError("SkillManager.Instance is null!");
        }
    }

    public void RotateHalfBack(Vector2Int moveDirection)
    {
        StartCoroutine(RotateHalfBackCoroutine(moveDirection));
    }

    IEnumerator RotateHalfBackCoroutine(Vector2Int moveDirection)
    {
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
            float totalScale = 1f + inflate;

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

        isMoving = false;
    }

    private IEnumerator CheckStageClearAfterMove(Vector2Int newPosition)
    {
        // 이동 애니메이션이 끝날 때까지 대기
        while (isMoving)
            yield return null;

        // 도착 지점이라면
        if (newPosition.y == BoardManager.Instance.boardSizeY - 1)
        {
            //BoardManager.Instance.Board[newPosition.x, newPosition.y].SetPiece(null);

            isFinishLine = true;

            // 모든 미션 클리어 시 스테이지 클리어 함수 호출
            StageManager.Instance.IsAllMissionCompleted();
        }
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
        classRenderer.sprite = piece.faces[2].classData.sprite;
        colorRenderer.color = BoardManager.Instance.tileColors[(int)piece.faces[2].color];
    }


    //public Vector2Int GetGridPosition()
    //{
    //    return gridPosition;
    //}

    //public Vector2Int SetGridPosition(Vector2Int newPosition)
    //{
    //    gridPosition = newPosition;
    //}

    public Vector2Int GetLastMoveDirection()
    {
        return lastMoveDirection;
    }

    private bool isInGame;
    public bool IsinGame => IsinGame;


    public void Init(Piece piece)
    {
        gridPosition = new Vector2Int(0, 0);
        this.piece = piece;
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
        ClassData newClassData = Resources.Load<ClassData>($"Data/Class/{newClassName}");
        if (newClassData == null)
        {
            Debug.LogError($"ClassData for {newClassName} not found!");
            return;
        }

        // 해당 면의 클래스 데이터 변경
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
        // 해당 면의 색상 변경
        piece.faces[faceIndex].color = color;
        // 윗면(인덱스 2)이라면 색상 렌더러 업데이트
        if (faceIndex == 2)
        {
            colorRenderer.color = BoardManager.Instance.tileColors[(int)color];
        }
    }

}