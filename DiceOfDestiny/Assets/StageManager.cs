using System;
using System.Collections;
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

    [SerializeField] private GameObject bannerPrefab;

    public GameState GameState { get; private set; } = GameState.ReadyToRoll;
    public int CurrentTurn { get; private set; } = 1;
    public int DiceValue { get; private set; }

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
    }
}