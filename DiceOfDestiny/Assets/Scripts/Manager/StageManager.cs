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
    [SerializeField] private StageData[] stageProfiles = new StageData[6];
    public StageData currentStage { get; private set; }
    public GameState GameState { get; private set; }
    public int CurrentTurn { get; private set; } = 1;
    public int DiceValue { get; private set; }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            UIManager.Instance.UpdateMissionUI();

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
        
        UIManager.Instance.InitMissionUI();
        UIManager.Instance.UpdateMissionUI();

        UIManager.Instance.UpdateActionPointUI();
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
            ToastManager.Instance.ShowToast("먼저 주사위를 굴리세요.", transform, 1f);
            return;
        }
        PieceManager.Instance.DecreaseDebuffAllPieces(); // 모든 말의 디버프 감소
        CurrentTurn++;

        if (CheckMissionFailed()) return; // 현재 턴이 최대 턴을 초과했는지 확인

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
        // 현재 선택 피스 null
        PieceManager.Instance.SetCurrentPiece(null);

        // 피스 선택 테두리 제거
        BoardSelectManager.Instance.DestroyPieceHighlightTile();

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
            for (int i = 0; i < 4; i++)
            {
                if (PieceManager.Instance.pieceDatas[i] == null)
                {
                    PieceManager.Instance.pieceDatas[i] = piece.GetPiece();
                }
            }
            Destroy(piece.gameObject);
            PieceManager.Instance.Pieces.Remove(piece);
        }

        if(clearPiece != null)
        {
            clearPiece.isOutStartingLine = false;
        }


        EventManager.Instance.TriggerEvent("Refresh");

        ToastManager.Instance.ClearAllToasts();

        
        clearPiece?.MoveClearPiece();

        ShiftToNextStage();
    }

    public void ShiftToNextStage()
    {
        
        stageIndex++;

        BoardManager.Instance.ShiftBoard();
    }

    public void SetNewStage()
    {
        UIManager.Instance.ShowUI();

        StartStage();
    }

    public void ResetCurrentStage()
    {
        stageIndex = 0;
    }
    
    public int GetCurrentStage()
    {
        return stageIndex;
    }

    public bool CheckMissionFailed()
    {
        // 최대 턴 넘으면 실패
        if (CurrentTurn > currentStage.maxTurn)
        {
            PieceManager.Instance.ResetPieces();
            UIManager.Instance.ShowStageFailedUI();
            return true;
        }

        return false;
    }
}   