using UnityEngine;

public sealed class StageBannerManager : MonoBehaviour
{
    [SerializeField] private StageBannerController bannerPrefab = null!;
    [SerializeField] private Canvas targetCanvas;

    private StageBannerController instance;

    /// <summary>
    /// 스테이지 시작 배너 호출
    /// </summary> 

    private void Awake()
    {
        if (bannerPrefab == null)
        {
            Debug.LogError("[StageBannerManager] bannerPrefab 미할당");
        }
        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInParent<Canvas>(true);
            if (targetCanvas == null)
                Debug.LogWarning("[StageBannerManager] 상위 Canvas를 찾지 못했습니다.");
        }
    }

    public void ShowBanner(int number, string title)
    {
        if (bannerPrefab == null) return;

        if (instance == null)
        {
            var parent = (targetCanvas != null) ? targetCanvas.transform : transform;
            instance = Instantiate(bannerPrefab, parent);
        }
        instance.transform.SetAsLastSibling();
        instance.gameObject.SetActive(true);
        instance.Show(number, title);
    }
}
