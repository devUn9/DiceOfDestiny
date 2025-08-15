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
    public GameState GameState { get; private set; }
    public int CurrentTurn { get; private set; } = 1;
    public int DiceValue { get; private set; }

    [Header("Next Stage Info")]
    [SerializeField] private GameObject NextStageUI;
    [SerializeField] private GameObject mainCanvasGroup;

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

        UIManager.Instance.ShowBanner(currentStage.stageNumber, currentStage.StageName);
        BoardManager.Instance.CreateBorderAndBG();
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
            UIManager.Instance.UpdateActionPointUI();
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
        UIManager.Instance.UpdateActionPointUI();
    }

    private void ResetTurn()
    {
        ActionPointManager.Instance.SetZero();
        GameState = GameState.ReadyToRoll;
    }

    public void StageClear(PieceController clearPiece = null)
    {
        var toRemove = new List<PieceController>();

        // 인게임 보드판에 있는 피스들 인벤토리로 돌아가게 하기
        foreach (var piece in PieceManager.Instance.Pieces)
        {
            if (piece != clearPiece)
            {
                toRemove.Add(piece);
            }
        }

        foreach (var piece in toRemove)
        {
            for (int i = 0; i < 3; i++)
            {
                if (PieceManager.Instance.pieceDatas[i] == null)
                {
                    PieceManager.Instance.pieceDatas[i] = piece.GetPiece();
                }
            }
            Destroy(piece.gameObject);
            PieceManager.Instance.Pieces.Remove(piece);
        }

        EventManager.Instance.TriggerEvent("Refresh");
        ToastManager.Instance.ClearAllToasts();

        BoardManager.Instance.DestroyBorder();


        clearPiece?.MoveClearPiece();

        // 현재 선택 피스 null
        PieceManager.Instance.SetCurrentPiece(null);

        // 피스 선택 테두리 제거
        BoardSelectManager.Instance.DestroyPieceHighlightTile();

        ShiftToNextStage();
    }

    public void ShiftToNextStage()
    {
        UIManager.Instance.HideUI();
        stageIndex++;

        BoardManager.Instance.ShiftBoard();
    }

    public void SetNewStage()
    {
        UIManager.Instance.ShowUI();

        StartStage();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        NextStageUI.SetActive(false);
        mainCanvasGroup.SetActive(true);

        // 기물 인벤토리 초기화

        // 기물 인벤토리 UI 새로고침
        EventManager.Instance.TriggerEvent("Refresh");
    }

    public void ResetCurrentStage()
    {
        stageIndex = 0;
    }
}   