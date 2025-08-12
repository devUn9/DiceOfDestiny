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

    [Header("Next Stage Info")]
    [SerializeField] private GameObject NextStageUI;
    [SerializeField] private GameObject mainCanvasGroup;
    private Coroutine bannerRoutine;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            StageClear();
        }
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

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

    public void StageClear()
    {
        // 인게임 보드판에 있는 피스들 제거
        foreach (var piece in PieceManager.Instance.Pieces)
        {
            Destroy(piece.gameObject);
        }

        // 피스 리스트에 제거
        PieceManager.Instance.Pieces.Clear();

        // 현재 선택 피스 null
        PieceManager.Instance.SetCurrentPiece(null);

        // 피스 선택 테두리 제거
        BoardSelectManager.Instance.DestroyPieceHighlightTile();

        Time.timeScale = 0f;

        mainCanvasGroup.SetActive(false);
        NextStageUI.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        NextStageUI.SetActive(false);
        mainCanvasGroup.SetActive(true);

        // 행동력, 턴 상태 초기화
        GameManager.Instance.actionPointManager.ResetAll();

        // 기물 인벤토리 초기화

        // 기물 인벤토리 UI 새로고침
        EventManager.Instance.TriggerEvent("Refresh");

        bannerManager?.ShowBanner(currentStage.stageNumber,
                                  currentStage.stageTitle);
    }
    private void OnEnable()
    {
        StageManager.StageLoaded += UpdateCurrentStage;
    }
    private void OnDisable()
    {
        StageManager.StageLoaded -= UpdateCurrentStage;
    }

    private void UpdateCurrentStage(StageData stage)
    {
        currentStage = stage;
    }
}