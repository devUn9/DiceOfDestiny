using System;
using System.Collections;
using UnityEngine;

public sealed class StageManager : Singletone<StageManager>
{
    public static event Action<StageData> StageLoaded;

    [Header("Stage Settings")]
    [SerializeField] private int stageIndex = 0;
    [SerializeField] private StageData[] stageProfiles = null!;

    [Header("UI References")]
    [SerializeField] private StageBannerManager bannerManager = null!;

    public StageData currentStage { get; private set; } = null!;
    public StageInfo currentStageInfo => currentStage != null ? ScriptableObject.CreateInstance<StageInfo>() : null;


    private void Awake()
    {
        // StageData 유효성 검사
        if (stageProfiles == null || stageProfiles.Length == 0)
        {
            Debug.LogError("[StageManager] StageProfiles 배열이 비어 있습니다.");
        }

        // bannerManager가 비어 있으면 Canvas에서 자동 검색
        if (bannerManager == null)
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                bannerManager = canvas.GetComponentInChildren<StageBannerManager>(true);
            }

            if (bannerManager == null)
            {
                Debug.LogWarning("[StageManager] StageBannerManager를 찾지 못했습니다. 배너를 띄우지 않습니다.");
            }
        }
    }

    private void Start()
    {
        stageIndex = Mathf.Clamp(stageIndex, 0, stageProfiles.Length - 1);
        currentStage = stageProfiles[stageIndex];

        StageLoaded?.Invoke(currentStage);

        // 배너 호출 (null 체크)
        if (bannerManager != null)
        {
            bannerManager.ShowBanner(currentStage.stageNumber,
                                     currentStage.stageTitle);
        }

        StartCoroutine(DeferredStartStage());
    }

    private IEnumerator DeferredStartStage()
    {
        yield return null;      // 1 frame wait
        StartStage();
    }

    public void StartStage()
    {
        ObstacleManager.Instance.RemoveAllObstacle();
        BoardManager.Instance.SetBoard(currentStage);
    }

    // 다음 스테이지 불러오기
    public bool TryLoadNextStage()
    {
        if (stageIndex + 1 >= stageProfiles.Length) return false;

        stageIndex++;
        currentStage = stageProfiles[stageIndex];

        StageLoaded?.Invoke(currentStage);

        StartCoroutine(DeferredStartStage());
        return true;
    }
}