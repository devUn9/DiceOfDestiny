using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(ScrollRect))]
public sealed class ScrollSnap : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    [Header("필수 컴포넌트")]
    [SerializeField] private ScrollRect scrollRect;

    [Header("설정")]
    private int itemCount;
    [SerializeField, Min(1f)] private float snapSpeed = 10f;        // 스냅 애니메이션 속도
    [SerializeField, Min(0f)] private float snapEpsilon = 0.001f;  // 정착 허용 오차(정규화 좌표)
    [SerializeField, Min(0.1f)] private float maxSnapTime = 0.6f;   // 무한 대기 방지
    [SerializeField] private bool disableInertiaDuringSnap = true;   // 스냅 동안 관성 OFF
    [SerializeField] private bool zeroElasticityDuringSnap = true;   // 스냅 동안 탄성 0으로

    // private bool isSnapping = false;
    private Coroutine snapCoroutine;
    private RectTransform viewport;
    private RectTransform content;

    private void Awake()
    {
        if (!scrollRect) scrollRect = GetComponent<ScrollRect>();
        viewport = scrollRect.viewport;
        content = scrollRect.content;
    }

    private void Update()
    {
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        itemCount = scrollRect.content.childCount;
        // 스크롤 위치 (0~1)
        float scrollPos = scrollRect.horizontalNormalizedPosition;

        // 중심에 가까운 인덱스 계산
        float centerIndex = scrollPos * (itemCount - 1);

        for (int i = 0; i < scrollRect.content.childCount; i++)
        {
            Transform item = scrollRect.content.GetChild(i);
            CanvasGroup cg = item.GetComponent<CanvasGroup>();
            if (cg == null) continue;

            float distance = Mathf.Abs(i - centerIndex);

            float alpha = Mathf.Lerp(1f, 0.5f, distance / 2f);
            cg.alpha = alpha;

            float scale = Mathf.Lerp(1f, 0.8f, distance / 2f);
            item.localScale = Vector3.one * scale;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 시 스냅 코루틴 중단
        if (snapCoroutine != null)
        {
            StopCoroutine(snapCoroutine);
            snapCoroutine = null;
        }
        scrollRect.inertia = true;
    }

    public void OnEndDrag(PointerEventData eventData) => TryStartSnap();

    private void TryStartSnap()
    {
        if (!scrollRect || !content || !viewport) return;
        if (content.childCount == 0) return;

        if (snapCoroutine != null) StopCoroutine(snapCoroutine);
        snapCoroutine = StartCoroutine(SnapToClosest());
    }

    IEnumerator SnapToClosest()
    {
        // 현재 설정 백업
        bool prevInertia = scrollRect.inertia;
        float prevElasticity = scrollRect.elasticity;

        // 스냅 모드 진입: 충돌 요인 제거
        if (disableInertiaDuringSnap) scrollRect.inertia = false;
        if (zeroElasticityDuringSnap) scrollRect.elasticity = 0f;

        // 관성/탄성 잔여힘 제거
        scrollRect.StopMovement();
        scrollRect.velocity = Vector2.zero;

        // 목표 인덱스/위치 계산 (균일 간격 가정)
        int count = Mathf.Max(1, scrollRect.content.childCount);
        float current = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition);
        int targetIndex = Mathf.RoundToInt(current * (count - 1));
        targetIndex = Mathf.Clamp(targetIndex, 0, count - 1);
        float target = (count == 1) ? 0f : (float)targetIndex / (count - 1);

        // SmoothDamp로 안정 수렴
        float vel = 0f;
        float elapsed = 0f;
        float smoothTime = 1f / Mathf.Max(1f, snapSpeed);

        while (elapsed < maxSnapTime)
        {
            float next = Mathf.SmoothDamp(
                scrollRect.horizontalNormalizedPosition,
                target,
                ref vel,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);

            // Dead-zone: 목표 근처에서는 즉시 고정
            if (Mathf.Abs(next - target) <= snapEpsilon) next = target;

            scrollRect.horizontalNormalizedPosition = next;

            if (next == target) break;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 최종 고정 및 힘 제거
        scrollRect.horizontalNormalizedPosition = target;
        scrollRect.velocity = Vector2.zero;

        // 설정 복구 (Movement Type은 내내 Elastic 유지)
        scrollRect.inertia = prevInertia;
        scrollRect.elasticity = prevElasticity;

        snapCoroutine = null;
        Debug.Log($"Snap 완료! 현재 아이템 인덱스: {targetIndex}");
    }
}
