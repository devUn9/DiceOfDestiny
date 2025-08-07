using UnityEngine;

public sealed class StageBannerManager : MonoBehaviour
{
    [SerializeField] private StageBannerController bannerPrefab = null!;

    private StageBannerController instance;

    /// <summary>
    /// 스테이지 시작 배너 호출
    /// </summary> 

    public void ShowBanner(int number, string title)
    {
        instance ??= Instantiate(bannerPrefab, transform);
        instance.gameObject.SetActive(true);
        instance.Show(number, title);
    }
}
