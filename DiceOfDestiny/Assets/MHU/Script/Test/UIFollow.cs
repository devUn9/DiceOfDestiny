using UnityEngine;

public class UIFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // 따라갈 2D 오브젝트의 Transform
    [SerializeField] private GameObject uiElement; // 따라갈 UI 요소
    [SerializeField] private Vector2 offset; // UI 위치 오프셋 (화면 좌표 기준)
    [SerializeField] private PieceController pieceController; // 이 스크립트가 부착된 피스 (Piece 컴포넌트 참조)
  
    private Camera mainCamera;
    private Canvas canvas;

    void Start()
    {
        EventManager.Instance.AddListener("ToggleUIElement", _ => ToggleUIElement());
        EventManager.Instance.AddListener("OnUIElement", _ => OnUIElement());

        mainCamera = Camera.main;

        canvas = uiElement.GetComponentInParent<Canvas>();
        pieceController = GetComponentInParent<PieceController>();
    }

    void LateUpdate()
    {
        if (target == null || uiElement == null) return;

        // 오브젝트의 월드 좌표를 화면 좌표로 변환
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);

        // Screen Space - Overlay에서는 캔버스 스케일 고려
        Vector2 canvasScale = canvas.GetComponent<RectTransform>().localScale;
        Vector2 adjustedPos = new Vector2(screenPos.x / canvasScale.x, screenPos.y / canvasScale.y);

        // 오프셋 적용
        adjustedPos += offset;

        // UI 위치 업데이트
        uiElement.transform.position = adjustedPos;
    }

   public void ToggleUIElement()
    {
        // 현재 조종중인 말이 아니면
        if (PieceManager.Instance.currentPiece != pieceController)
        {
            // UI가 활성화되어 있다면 (그러니까 다른 기물들을)
            if (uiElement.activeSelf)
            {
                // UI 비활성화
                uiElement.SetActive(false);
            }
          
            return;
        }

        EventManager.Instance.TriggerEvent("OnArrowExit");
        uiElement.gameObject.SetActive(!uiElement.gameObject.activeSelf);

    }

    public void OnUIElement()
    {   // 현재 조종중인 말이 아니면
        if (PieceManager.Instance.currentPiece != pieceController)
        {
            return;  
        }
        uiElement.SetActive(true);
    }
}