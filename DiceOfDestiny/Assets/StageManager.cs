using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    ReadyToRoll,
    PlayerAction,
}

public sealed class StageManager : Singletone<StageManager>
{
    [Header("Stage Settings")]
    [SerializeField] private int stageIndex = 0;
    [SerializeField] private StageData[] stageProfiles = new StageData[5];

    public StageData currentStage { get; private set; }

    public GameState GameState { get; private set; } = GameState.ReadyToRoll;
    public int CurrentTurn { get; private set; } = 1;
    public int DiceValue { get; private set; }
    [Header("Next Stage Info")]
    [SerializeField] private GameObject NextStageUI;
    [SerializeField] private GameObject mainCanvasGroup;

    [Header("Stage 5 Mission")]
    private int findGrayGrassCount = 0;
    public bool isFindGrayGrass { get; private set; } = false;

    [Header("Stage 6 Mission")]
    private List<GameObject> pawnList = new List<GameObject>();
    public int pawnMoveIndex { get; private set; } = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            StageClear();
        }
    }

    public void StartStage()
    {
        currentStage = stageProfiles[stageIndex];

        UIManager.Instance.SetStageName(currentStage.StageName);

        ObstacleManager.Instance.RemoveAllObstacle();
        BoardManager.Instance.SetBoard(currentStage);

        GameState = GameState.ReadyToRoll;
        CurrentTurn = 1;
        DiceValue = 0;
        // 배너 표시
        ShowBanner(currentStage.stageNumber, currentStage.StageName);
    }

    public void ShowBanner(int number, string title)
    {
        GameObject stagebanner = Instantiate(bannerPrefab, transform);
        stagebanner.GetComponent<StageBannerController>().Show(number, title);
    }

    public void RollDice()
    {
        if (GameState != GameState.ReadyToRoll) return;
        bool started = DiceRollManager.Instance.TryRoll((value) =>
        {
            DiceValue = value;
            ActionPointManager.Instance.AddAP(value);
            Debug.Log($"주사위를 굴려서 {value}가 나왔습니다.");
            GameState = GameState.PlayerAction;
        });
    }

    public void EndTurn()
    {
        if (GameState == GameState.ReadyToRoll)
        {
            ToastManager.Instance.ShowToast("먼저 주사위를 굴리세요.", transform);
            return;
        }
        PieceManager.Instance.DecreaseDebuffAllPieces(); // 모든 말의 디버프 감소
        CurrentTurn++;
        ResetTurn();
    }

    private void ResetTurn()
    {
        ActionPointManager.Instance.SetZero();
        GameState = GameState.ReadyToRoll;
    }

    public void IsAllMissionCompleted()
    {
        if (currentStage.missions.TrueForAll(m => m.IsMissionCompleted()))
        {
            Debug.Log("복합 미션 완료!");
            StageClear();
        }
    }

    public void StageClear()
    {
        // 인게임 보드판에 있는 피스들 인벤토리로 돌아가게 하기
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

    public void AddGrayGrassMission()
    {
        findGrayGrassCount++;

        if (findGrayGrassCount >= 3)
            isFindGrayGrass = true;
    }

    public void AddPawnToList(GameObject pawn)
    {
        if (pawn != null && !pawnList.Contains(pawn))
        {
            pawnList.Add(pawn);
        }
    }

    public void RemovePawn(GameObject pawn)
    {
        pawnList.Remove(pawn);
    }

    public int GetPawnListIndex(GameObject pawn)
    {
        return pawnList.IndexOf(pawn);
    }

    public void InOrderToMovePawn()
    {
        if (pawnMoveIndex >= 6)
        {
            pawnMoveIndex = 0;
            return;
        }
        
        pawnMoveIndex++;
    }
}



    