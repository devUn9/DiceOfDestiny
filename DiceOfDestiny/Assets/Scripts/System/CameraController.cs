using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Zoom Settings")]
    [SerializeField] private float[] zoomLevels = { 7f, 5f, 3f };
    private int currentZoomLevel = 0;

    [Header("Drag Settings")]
    [SerializeField] private float dragSpeed = 1.0f;
    private bool canDrag = false;

    private Vector3 lastMousePosition;
    private Vector2 minBounds;
    private Vector2 maxBounds;

    // 보드판 중심 (타겟) - 필요에 따라 에디터에서 설정 가능
    [SerializeField] private Vector2 boardCenter = Vector2.zero;

    public float[] GetZoomLevels()
    {
        return zoomLevels;
    }

    void Start()
    {
        mainCamera = Camera.main;
        mainCamera.orthographicSize = zoomLevels[currentZoomLevel];
        SetInitialBounds();

        // 초기 카메라 위치를 보드판 중심으로 설정
        mainCamera.transform.position = new Vector3(boardCenter.x, boardCenter.y, -10f);
    }

    void Update()
    {
        HandleZoom();
        HandleDrag();
    }

    void SetInitialBounds()
    {
        Vector3 camPos = mainCamera.transform.position;
        float camHeight = mainCamera.orthographicSize * 2f;
        float camWidth = camHeight * mainCamera.aspect;

        minBounds = new Vector2(camPos.x - camWidth / 2f, camPos.y - camHeight / 2f);
        maxBounds = new Vector2(camPos.x + camWidth / 2f, camPos.y + camHeight / 2f);
    }

    void HandleZoom()
    {
        if (GameManager.Instance.isPaused) return;
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            // 현재 카메라 위치와 타겟(보드판 중심)의 상대적 위치 계산
            Vector3 currentCamPos = mainCamera.transform.position;
            Vector2 targetPos = boardCenter;
            Vector2 relativePos = new Vector2(currentCamPos.x, currentCamPos.y) - targetPos;

            if (scroll > 0f) // 확대
            {
                if (currentZoomLevel < zoomLevels.Length - 1)
                {
                    currentZoomLevel++;
                    mainCamera.orthographicSize = zoomLevels[currentZoomLevel];
                }
            }
            else if (scroll < 0f) // 축소
            {
                if (currentZoomLevel > 0)
                {
                    currentZoomLevel--;
                    mainCamera.orthographicSize = zoomLevels[currentZoomLevel];
                }
            }

            if (currentZoomLevel == 0) // 가장 큰 줌 레벨일 경우
            {
                mainCamera.transform.position = new Vector3(boardCenter.x, boardCenter.y, -10f);
            }
            else // 다른 줌 레벨에서는 타겟을 기준으로 위치 조정
            {
                // 줌 레벨에 따라 상대적 위치를 스케일링
                float zoomFactor = zoomLevels[0] / zoomLevels[currentZoomLevel];
                Vector2 newRelativePos = relativePos * zoomFactor;
                Vector3 newCamPos = new Vector3(
                    targetPos.x + newRelativePos.x,
                    targetPos.y + newRelativePos.y,
                    -10f
                );

                mainCamera.transform.position = newCamPos;
                ClampCameraPosition();
            }

            canDrag = currentZoomLevel > 0;
        }
    }

    void HandleDrag()
    {
        if (!canDrag) return;
        if (GameManager.Instance.isPaused) return;

        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, -delta.y, 0f) * (mainCamera.orthographicSize / 1000f) * dragSpeed;
            mainCamera.transform.position += move;

            lastMousePosition = Input.mousePosition;
            ClampCameraPosition();
        }
    }

    void ClampCameraPosition()
    {
        Vector3 pos = mainCamera.transform.position;

        float camHeight = mainCamera.orthographicSize * 2f;
        float camWidth = camHeight * mainCamera.aspect;

        float paddingRatio = 0.1f;
        float zoomFactor = zoomLevels[0] / mainCamera.orthographicSize;
        float paddingX = camWidth * paddingRatio * zoomFactor;

        float minX = minBounds.x + camWidth / 2f + paddingX;
        float maxX = maxBounds.x - camWidth / 2f - paddingX;

        float minY = minBounds.y + camHeight / 2f;
        float maxY = maxBounds.y - camHeight / 2f;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        mainCamera.transform.position = pos;
    }
}