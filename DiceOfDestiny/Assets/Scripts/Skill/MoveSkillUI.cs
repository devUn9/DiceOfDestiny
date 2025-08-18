using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoveSkillUI : MonoBehaviour
{
    [SerializeField] private GameObject upButton; // Button -> Gameroger
    [SerializeField] private GameObject downButton;
    [SerializeField] private GameObject leftButton;
    [SerializeField] private GameObject rightButton;
    [SerializeField] private GameObject selecting; // 버튼들을 포함한 부모 오브젝트

    private Vector2Int selectedDirection = Vector2Int.zero;
    private bool isDirectionSelected = false;
    private Camera mainCamera;
    private Canvas canvas;

    private CameraController cameraController;
    private Vector3 baseUIScale;

    void Awake()
    {
        mainCamera = Camera.main;
        canvas = selecting.GetComponentInParent<Canvas>();
        cameraController = mainCamera.GetComponent<CameraController>();
    }

    private void Start()
    {
        // UI 요소의 기본 스케일 저장
        baseUIScale = selecting.transform.localScale;
    }

    void LateUpdate()
    {
        if (selecting == null || PieceManager.Instance.currentPiece == null) return;

        // currentPiece의 월드 좌표를 화면 좌표로 변환
        Vector3 screenPos = mainCamera.WorldToScreenPoint(PieceManager.Instance.currentPiece.transform.position);

        // 캔버스 스케일 고려
        Vector2 canvasScale = canvas.GetComponent<RectTransform>().localScale;
        Vector2 adjustedPos = new Vector2(screenPos.x / canvasScale.x, screenPos.y / canvasScale.y);

        // UI 위치 업데이트
        selecting.transform.position = adjustedPos;

        // UI 스케일 업데이트
        UpdateUIScale();
    }
    private void UpdateUIScale()
    {
        if (cameraController == null) return;

        // 카메라의 현재 orthographicSize를 기준으로 스케일 계산
        float baseZoom = cameraController.GetZoomLevels()[0]; // 기본 줌 레벨 (예: 7f)
        float currentZoom = mainCamera.orthographicSize; // 현재 줌 레벨
        float scaleFactor = baseZoom / currentZoom; // 기본 줌 대비 스케일 비율

        // UI 요소의 스케일 조정
        selecting.transform.localScale = baseUIScale * scaleFactor;
    }

    public void Initialize(PieceController piece)
    {
        if (selecting == null)
        {
            Debug.LogError("selecting is not assigned in MoveSkillUI!");
            return;
        }

        // 초기화: 모든 버튼 비활성화
        if (upButton != null) upButton.SetActive(false);
        if (downButton != null) downButton.SetActive(false);
        if (leftButton != null) leftButton.SetActive(false);
        if (rightButton != null) rightButton.SetActive(false);

        // 장애물 및 보드 범위 확인 후 버튼 활성화
        Vector2Int currentPos = piece.gridPosition;

        // 상
        Vector2Int upPos = currentPos + Vector2Int.up;
        if (BoardManager.Instance.IsInsideBoard(upPos) &&
            (BoardManager.Instance.IsEmptyTile(upPos) ||
            BoardManager.Instance.ReturnObstacleByPosition(upPos).isWalkable) &&
            BoardManager.Instance.Board[upPos.x, upPos.y].GetPiece() == null)
        {
            upButton.SetActive(true);
            RegisterButtonEvents(upButton, () => SelectDirection(Vector2Int.up));           
        }

        // 하
        Vector2Int downPos = currentPos + Vector2Int.down;
        if (BoardManager.Instance.IsInsideBoard(downPos) &&
            (BoardManager.Instance.IsEmptyTile(downPos) ||
             BoardManager.Instance.ReturnObstacleByPosition(downPos).isWalkable) &&
             BoardManager.Instance.Board[downPos.x, downPos.y].GetPiece() == null &&
             BoardManager.Instance.IsMovementArea(downPos))
        {
            downButton.SetActive(true);
            RegisterButtonEvents(downButton, () => SelectDirection(Vector2Int.down));
        }

        // 좌
        Vector2Int leftPos = currentPos + Vector2Int.left;
        if (BoardManager.Instance.IsInsideBoard(leftPos) &&
            (BoardManager.Instance.IsEmptyTile(leftPos) ||
             BoardManager.Instance.ReturnObstacleByPosition(leftPos).isWalkable) &&
            BoardManager.Instance.Board[leftPos.x, leftPos.y].GetPiece() == null)
        {
            leftButton.SetActive(true);
            RegisterButtonEvents(leftButton, () => SelectDirection(Vector2Int.left));
        }

        // 우
        Vector2Int rightPos = currentPos + Vector2Int.right;
        if (BoardManager.Instance.IsInsideBoard(rightPos) &&
            (BoardManager.Instance.IsEmptyTile(rightPos) ||
             BoardManager.Instance.ReturnObstacleByPosition(rightPos).isWalkable) &&
            BoardManager.Instance.Board[rightPos.x, rightPos.y].GetPiece() == null)
        {
            rightButton.SetActive(true);
            RegisterButtonEvents(rightButton, () => SelectDirection(Vector2Int.right));
        }
    }

    // GameObject와 그 하위 오브젝트의 Button 컴포넌트에 클릭 이벤트 등록
    private void RegisterButtonEvents(GameObject buttonObject, UnityEngine.Events.UnityAction action)
    {
        // 버튼 오브젝트 자체
        Button mainButton = buttonObject.GetComponent<Button>();
        if (mainButton != null)
        {
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(action);
        }
        else
        {
            Debug.LogWarning($"No Button component found on {buttonObject.name}");
        }

        // 하위 오브젝트의 버튼들
        Button[] childButtons = buttonObject.GetComponentsInChildren<Button>();
        foreach (Button childButton in childButtons)
        {
            // 동일한 오브젝트의 버튼은 제외
            if (childButton != mainButton)
            {
                childButton.onClick.RemoveAllListeners();
                childButton.onClick.AddListener(action);
            }
        }
    }

    private void SelectDirection(Vector2Int direction)
    {
        selectedDirection = direction;
        isDirectionSelected = true;
    }

    public IEnumerator WaitForArrowClick()
    {
        isDirectionSelected = false;
        selectedDirection = Vector2Int.zero;

        while (!isDirectionSelected)
        {
            yield return null;
        }

        // UI 비활성화
        if (upButton != null) upButton.SetActive(false);
        if (downButton != null) downButton.SetActive(false);
        if (leftButton != null) leftButton.SetActive(false);
        if (rightButton != null) rightButton.SetActive(false);

        // 선택된 방향으로 이동
        if (selectedDirection != Vector2Int.zero)
        {
            PieceController piece = PieceManager.Instance.currentPiece;
            if (piece != null)
            {
                yield return StartCoroutine(GetComponent<ActiveSkill>().MoveForward(piece, selectedDirection));
            }
        }
    }
}