using UnityEngine;
using System.Collections.Generic;

public class CursorManager : Singletone<CursorManager>
{
    [Header("Refs")]
    [SerializeField] private GameObject cursorPrefab;

    [Header("Order")]
    [SerializeField] private int defaultSortOrder = 32760;

    private GameObject cursorInstance;
    private Canvas cursorCanvas;
    private RectTransform cursorRectTransform;

    // 싱글톤 인스턴스가 파괴되었는지 여부
    private static bool s_IsQuitting;

    // 외부 Overlay 캔버스 정렬용
    private static readonly HashSet<Canvas> activeCanvases = new();

    public static void RegisterCanvas(Canvas canvas)
    {
        if (s_IsQuitting || canvas == null) return;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return;
        
        activeCanvases.Add(canvas);
        UpdateCursorCanvasOrder();
    }

    public static void UnregisterCanvas(Canvas canvas)
    {
        if (s_IsQuitting || canvas == null) return;
        activeCanvases.Remove(canvas);
        UpdateCursorCanvasOrder();
    }

    private static void UpdateCursorCanvasOrder()
    {
        if (s_IsQuitting) return;

        var mgr = FindAnyObjectByType<CursorManager>(FindObjectsInactive.Include);
        if (mgr == null || mgr.cursorCanvas == null) return;

        int highestOrder = mgr.defaultSortOrder;
        foreach (var canvas in activeCanvases)
        {
            if (canvas != null && canvas != Instance.cursorCanvas)
            {
                highestOrder = Mathf.Max(highestOrder, canvas.sortingOrder + 1);
            }
        }
        Instance.cursorCanvas.sortingOrder = highestOrder;

    }

    private void Start()
    {
        s_IsQuitting = false;

        CreateCursorCanvas();
        CreateCursor();

        UpdateCursorCanvasOrder();
    }

    private void Update()
    {
        if (cursorRectTransform == null || cursorCanvas == null) return;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorRectTransform.parent as RectTransform,
            Input.mousePosition,
            cursorCanvas.worldCamera,
            out pos
        );
        cursorRectTransform.anchoredPosition = pos;
    }
    private void OnDisable()
    {
        // 씬 언로드/플레이모드 종료 중 정적 경로 차단
        s_IsQuitting = true;
    }

    protected override void OnDestroy()
    {
        // 종료 플래그를 최우선 세워 재생성 루트를 원천 차단
        s_IsQuitting = true;

        // 커서 오브젝트 정리
        if (cursorInstance != null)
        {
            Destroy(cursorInstance);
            cursorInstance = null;
        }

        // 캔버스 정리
        if (cursorCanvas != null)
        {
            UnregisterCanvas(cursorCanvas);
            Destroy(cursorCanvas.gameObject);
            cursorCanvas = null;
        }

        activeCanvases.Clear();

        base.OnDestroy();
    }

    private void CreateCursorCanvas()
    {
        if (cursorCanvas != null) return;

        var found = GameObject.Find("CursorCanvas");
        if (found != null)
        {
            cursorCanvas = found.GetComponent<Canvas>();
            return;
        }

        GameObject canvasObj = new GameObject("CursorCanvas");
        cursorCanvas = canvasObj.AddComponent<Canvas>();
        cursorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        cursorCanvas.sortingOrder = defaultSortOrder;

        canvasObj.transform.SetParent(transform, false);
    }

    private void CreateCursor()
    {
        if (cursorInstance != null) return; // 중복 생성 방지

        if (cursorPrefab == null)
        {
            Debug.LogError("[CursorUIManager] cursorPrefab이 할당되어 있지 않습니다. Project(Assets) 내 프리팹을 드래그하세요.");
            return;
        }
        if (cursorCanvas == null)
        {
            Debug.LogError("[CursorUIManager] cursorCanvas가 생성되어 있지 않습니다.");
            return;
        }

        cursorInstance = Instantiate(cursorPrefab, cursorCanvas.transform);
        cursorRectTransform = cursorInstance.GetComponent<RectTransform>();
        Cursor.visible = false;
    }
}
