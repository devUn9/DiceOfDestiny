using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToastManager : Singletone<ToastManager>
{
    [Header("Prefabs & Canvas")]
    [SerializeField] private GameObject toastPrefab;
    [SerializeField] private Canvas uiCanvas;

    [Header("Toast Settings")]
    [SerializeField] private float toastDuration = 2f; 
    [SerializeField] private float worldYOffset = 0f;
    [SerializeField] private float pixelYOffset = 15f;

    private readonly List<RectTransform> activeToasts = new List<RectTransform>();
    private readonly List<GameObject> toastInstances = new List<GameObject>(); // 토스트 인스턴스 추적

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    /// <summary>
    /// 기물 위에 토스트를 표시합니다.
    /// </summary>
    /// <param name="message">토스트에 표시할 텍스트</param>
    /// <param name="targetPiece">토스트를 띄울 대상 기물의 Transform</param>
    /// <param name="delay">딜레이(초). 0이면 즉시 표시</param>

    public void Initialize()
    {
        if (uiCanvas == null)
        {
            uiCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        }

        // Screen-Space–Camera 모드라면 worldCamera 할당
        if (uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay && uiCanvas.worldCamera == null)
            uiCanvas.worldCamera = Camera.main;
    }


    public void ShowToast(string message, Transform targetPiece, float delay = 0f)
    {
        StartCoroutine(ShowToastRoutine(message, targetPiece, delay));
    }

    private IEnumerator ShowToastRoutine(string message, Transform targetPiece, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        GameObject toastInstance = Instantiate(toastPrefab, uiCanvas.transform);
        RectTransform toastRect = toastInstance.GetComponent<RectTransform>();
        toastRect.pivot = new Vector2(0.5f, 0f);
        toastInstance.GetComponentInChildren<TextMeshProUGUI>().text = message;

        toastInstance.SetActive(true);

        activeToasts.Add(toastRect);
        toastInstances.Add(toastInstance);
        int orderIndex = activeToasts.Count - 1;

        float elapsed = 0f;
        while (elapsed < toastDuration)
        {
            if (targetPiece == null) break;

            Vector3 worldPos = targetPiece.position + Vector3.up * worldYOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();

            Camera cam = uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                         ? null
                         : uiCanvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, cam, out Vector2 localPoint);

            float yStackOffset = ((pixelYOffset + 20) * orderIndex) / uiCanvas.scaleFactor;
            toastRect.anchoredPosition = new Vector2(localPoint.x, localPoint.y + yStackOffset);

            var textComponent = toastInstance.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = message;

            elapsed += Time.unscaledDeltaTime;
            yield return null;               // 다음 프레임
        }
        activeToasts.Remove(toastRect);
        toastInstances.Remove(toastInstance);

        Destroy(toastInstance);
    }

    public void ClearAllToasts()
    {
        StopAllCoroutines();
        // 모든 토스트 인스턴스 파괴
        foreach (var toast in toastInstances)
        {
            if (toast != null)
                Destroy(toast);
        }

        // 리스트 초기화
        activeToasts.Clear();
        toastInstances.Clear();
    }
}
