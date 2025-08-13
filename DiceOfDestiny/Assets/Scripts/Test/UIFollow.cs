using UnityEngine;

public class UIFollow : MonoBehaviour
{
    // 피스 프리팹에 붙이는 스크립트

    [SerializeField] private Transform target; // 따라갈 2D 오브젝트의 Transform
    [SerializeField] private GameObject uiElement; // 따라갈 UI 요소
    [SerializeField] private Vector2 offset; // UI 위치 오프셋 (화면 좌표 기준)
    
    private PieceController pieceController; // 이 스크립트가 부착된 피스
    private CameraController cameraController;
    private Camera mainCamera;
    private Canvas canvas;
    private Vector3 baseUIScale;


    private void Awake()
    {
        mainCamera = Camera.main;

        canvas = uiElement.GetComponentInParent<Canvas>();
        pieceController = GetComponentInParent<PieceController>();
        cameraController = mainCamera.GetComponent<CameraController>();
    }
    void Start()
    {
        EventManager.Instance.AddListener("ToggleUIElement", _ => ToggleUIElement());
        EventManager.Instance.AddListener("OnUIElement", _ => OnUIElement());
        

        // UI 요소의 기본 스케일 저장
        baseUIScale = uiElement.transform.localScale;
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
        uiElement.transform.localScale = baseUIScale * scaleFactor;
    }

    public void ToggleUIElement()
    {
        if (PieceManager.Instance.currentPiece != pieceController)
        {
            if (uiElement != null && uiElement.activeSelf)
            {
                uiElement.SetActive(false);
            }
            return;
        }

        EventManager.Instance.TriggerEvent("OnArrowExit");
        uiElement.gameObject.SetActive(!uiElement.gameObject.activeSelf);
    }

    public void OnUIElement()
    {
        if (PieceManager.Instance.currentPiece != pieceController)
        {
            return;
        }
        uiElement.SetActive(true);
    }

    public bool IsUIActive()
    {
        return uiElement.activeSelf;
    }
}