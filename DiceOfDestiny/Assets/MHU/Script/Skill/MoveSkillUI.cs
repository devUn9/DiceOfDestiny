using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoveSkillUI : MonoBehaviour
{
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private GameObject selecting; // 버튼들을 포함한 부모 오브젝트
    [SerializeField] private Vector2 offset = new Vector2(0, 50); // UI 오프셋 (화면 좌표 기준)

    private Vector2Int selectedDirection = Vector2Int.zero;
    private bool isDirectionSelected = false;
    private Camera mainCamera;
    private Canvas canvas;
    private PieceSelectUI pieceSelectUI;

    void Start()
    {
        mainCamera = Camera.main;
        canvas = selecting.GetComponentInParent<Canvas>();
        pieceSelectUI = GetComponent<PieceSelectUI>();
    }

    void LateUpdate()
    {
        if (selecting == null || PieceManager.Instance.currentPiece == null) return;

        // currentPiece의 월드 좌표를 화면 좌표로 변환
        Vector3 screenPos = mainCamera.WorldToScreenPoint(PieceManager.Instance.currentPiece.transform.position);

        // 캔버스 스케일 고려
        Vector2 canvasScale = canvas.GetComponent<RectTransform>().localScale;
        Vector2 adjustedPos = new Vector2(screenPos.x / canvasScale.x, screenPos.y / canvasScale.y);

        // 오프셋 적용
        adjustedPos += offset;

        // UI 위치 업데이트
        selecting.transform.position = adjustedPos;
    }

    public void Initialize(PieceController piece)
    {
        if (selecting == null)
        {
            Debug.LogError("uiParent is not assigned in MoveSkillUI!");
            return;
        }

        // 초기화: 모든 버튼 비활성화
        if (upButton != null) upButton.gameObject.SetActive(false);
        if (downButton != null) downButton.gameObject.SetActive(false);
        if (leftButton != null) leftButton.gameObject.SetActive(false);
        if (rightButton != null) rightButton.gameObject.SetActive(false);

        // 장애물 확인 후 버튼 활성화
        Vector2Int currentPos = piece.gridPosition;

        // 비어있거나 장애물이 이동 가능한 경우에만 버튼 활성화
        // 상
        if (BoardManager.Instance.IsEmptyTile(currentPos + Vector2Int.up) ||
            BoardManager.Instance.ReturnObstacleByPosition(currentPos + Vector2Int.up).isWalkable && upButton != null)
        {
            upButton.gameObject.SetActive(true);
            upButton.onClick.RemoveAllListeners();
            upButton.onClick.AddListener(() => SelectDirection(Vector2Int.up));
        }

        // 하
        if (BoardManager.Instance.IsEmptyTile(currentPos + Vector2Int.down) ||
            BoardManager.Instance.ReturnObstacleByPosition(currentPos + Vector2Int.down).isWalkable && upButton != null)
        {
            downButton.gameObject.SetActive(true);
            downButton.onClick.RemoveAllListeners();
            downButton.onClick.AddListener(() => SelectDirection(Vector2Int.down));
        }

        // 좌
        if (BoardManager.Instance.IsEmptyTile(currentPos + Vector2Int.left) ||
            BoardManager.Instance.ReturnObstacleByPosition(currentPos + Vector2Int.left).isWalkable && upButton != null)
        {
            leftButton.gameObject.SetActive(true);
            leftButton.onClick.RemoveAllListeners();
            leftButton.onClick.AddListener(() => SelectDirection(Vector2Int.left));
        }

        // 우
        if (BoardManager.Instance.IsEmptyTile(currentPos + Vector2Int.right) ||
            BoardManager.Instance.ReturnObstacleByPosition(currentPos + Vector2Int.right).isWalkable && upButton != null)
        {
            rightButton.gameObject.SetActive(true);
            rightButton.onClick.RemoveAllListeners();
            rightButton.onClick.AddListener(() => SelectDirection(Vector2Int.right));
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
        if (upButton != null) upButton.gameObject.SetActive(false);
        if (downButton != null) downButton.gameObject.SetActive(false);
        if (leftButton != null) leftButton.gameObject.SetActive(false);
        if (rightButton != null) rightButton.gameObject.SetActive(false);

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