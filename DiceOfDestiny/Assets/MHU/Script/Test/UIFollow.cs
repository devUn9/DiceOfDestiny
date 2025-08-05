using UnityEngine;

public class UIFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // 따라갈 2D 오브젝트의 Transform
    [SerializeField] private RectTransform uiElement; // 따라갈 UI 요소의 RectTransform
    [SerializeField] private Vector2 offset; // UI 위치 오프셋 (화면 좌표 기준)

    private Camera mainCamera;
    private Canvas canvas;

    void Start()
    {
        // 메인 카메라와 캔버스 참조 가져오기
        mainCamera = Camera.main;
        canvas = uiElement.GetComponentInParent<Canvas>();
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
        uiElement.position = adjustedPos;
    }
}