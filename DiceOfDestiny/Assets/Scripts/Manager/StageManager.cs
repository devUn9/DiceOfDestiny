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
    public StageData[] stageProfiles = new StageData[10];
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
        MissionManager.Instance.ResetMission();

        GameState = GameState.ReadyToRoll;
        CurrentTurn = 1;
        DiceValue = 0;

        // 미션이 두개 이상일 때 미션 배너 생성
        if (currentStage.missions.Count >= 2)
        {
            UIManager.Instance.InitMissionUI();
            UIManager.Instance.UpdateMissionUI();

            UIManager.Instance.ShowMissionUI();
        }
        else
        {
            UIManager.Instance.HideMissionUI();
        }

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
            //Debug.Log($"주사위를 굴려서 {value}가 나왔습니다.");
            GameState = GameState.PlayerAction;
            UIManager.Instance.UpdateActionPointUI();
        });
    }

    public void EndTurn()
    {
        if (GameState == GameState.ReadyToRoll)
        {
            ToastManager.Instance.ShowToast("먼저 주사위를 굴리세요.", transform, 0f);
            return;
        }
        PieceManager.Instance.DecreaseDebuffAllPieces(); // 모든 말의 디버프 감소
        CurrentTurn++;

        if (CheckMissionFailed()) return; // 현재 턴이 최대 턴을 초과했는지 확인

        ResetTurn();
        UIManager.Instance.UpdateActionPointUI();

        CheckStormStage();
        CheckHouseStage();
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

        if (clearPiece != null)
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
            UIManager.Instance.ShowStageFailedUI();
            return true;
        }

        return false;
    }

    public void CheckStormStage()
    {
        if (currentStage.stageNumber == 8)
        {
            StormDirection randStormDir = (StormDirection)Random.Range(0, 4);
            Debug.Log($"이번 바람은 {randStormDir} 에서 옵니다.");

            switch (randStormDir)
            {
                case StormDirection.Left:
                    StartCoroutine(ApplyStorm(Vector2Int.left, new Vector3(-20, 0, 0), Vector2Int.right));
                    break;
                case StormDirection.Right:
                    StartCoroutine(ApplyStorm(Vector2Int.right, new Vector3(20, 0, 0), Vector2Int.left));
                    break;
                case StormDirection.Up:
                    StartCoroutine(ApplyStorm(Vector2Int.up, new Vector3(0, 15, 0), Vector2Int.down));
                    break;
                case StormDirection.None:
                    Debug.Log($"이번 턴에는 바람이 안 붑니다.");
                    break;
            }
        }
    }

    private IEnumerator ApplyStorm(Vector2Int searchDir, Vector3 effectPos, Vector2Int pushDir)
    {
        yield return new WaitForSeconds(1f);

        foreach (var piece in PieceManager.Instance.Pieces)
        {
            int pivotX = piece.gridPosition.x;
            int pivotY = piece.gridPosition.y;
            bool blocked = false;

            // 좌우 확인
            if (searchDir.x != 0)
            {
                for (int x = pivotX - 1; x <= pivotX + 1; x += 2)
                {
                    if (x < 0 || x >= BoardManager.Instance.boardSize)
                        continue;

                    if (BoardManager.Instance.Board[x, pivotY].Obstacle == ObstacleType.Tree ||
                        BoardManager.Instance.Board[x, pivotY].Obstacle == ObstacleType.Rock ||
                        BoardManager.Instance.Board[x, pivotY].GetPiece() != null)
                    {
                        blocked = true;
                        break;
                    }
                }
            }
            // 상하 확인
            else if (searchDir.y != 0)
            {
                for (int y = pivotY - 1; y <= pivotY + 1; y += 2)
                {
                    if (y < 1 || y >= BoardManager.Instance.boardSize + 1)
                        continue;

                    if (BoardManager.Instance.Board[pivotX, y].Obstacle == ObstacleType.Tree ||
                        BoardManager.Instance.Board[pivotX, y].Obstacle == ObstacleType.Rock ||
                        BoardManager.Instance.Board[pivotX, y].GetPiece() != null)
                    {
                        blocked = true;
                        break;
                    }
                }
            }

            EffectManager.Instance.PlayEffect("WindEffect", effectPos, pushDir);

            if (!blocked)
                StormMove(piece, pushDir);
        }
    }


    private void StormMove(PieceController piece, Vector2Int stormDir)
    {
        // 다음 좌표와 타일
        Vector2Int nextPosition = piece.gridPosition + stormDir;

        if (nextPosition.x < 0 || nextPosition.x >= BoardManager.Instance.boardSize
            || nextPosition.y < 1 || nextPosition.y >= BoardManager.Instance.boardSize + 1)
            return;

        Tile nextTile = BoardManager.Instance.Board[nextPosition.x, nextPosition.y];

        var stunTurns = 1;

        // 뒤에 밀리지 않을만한 장애물이 있는 경우
        if (nextTile.Obstacle == ObstacleType.Zombie || nextTile.Obstacle == ObstacleType.Slime)
        {
            // 밀리지 않고 기절만
            if (piece.statusEffectController.IsStatusActive(PieceStatus.Stun))
                return;
            piece.statusEffectController.SetStatus(PieceStatus.Stun, stunTurns);
            ToastManager.Instance.ShowToast($"바람에 맞았습니다. {stunTurns}턴간 기절합니다.", piece.transform, 1f);
            return;
        }

        // 그 외에는 밀리고 기절 //

        // 이전 타일에 Piece 값을 null로 바꾸고, 다음 타일에 Piece 값을 적용 
        BoardManager.Instance.Board[piece.gridPosition.x, piece.gridPosition.y].SetPiece(null);
        nextTile.SetPiece(piece);

        // 현재 타일에 색 적용
        BoardManager.Instance.Board[piece.gridPosition.x, piece.gridPosition.y].TileColor = piece.lastTileColor;
        piece.lastTileColor = nextTile.TileColor;
        nextTile.TileColor = piece.GetPiece().faces[2].color;

        piece.RotateToTopFace(stormDir);
        piece.UpdateTopFace(stormDir);

        if (piece.statusEffectController.IsStatusActive(PieceStatus.Stun))
            return;
        piece.statusEffectController.SetStatus(PieceStatus.Stun, stunTurns);
        ToastManager.Instance.ShowToast($"바람에 맞았습니다. {stunTurns}턴간 기절합니다.", piece.transform, 1f);
    }

    private void CheckHouseStage()
    {
        if (currentStage.stageNumber == 10)
        {
            ObstacleManager.Instance.HouseListToLogicTurn();
        }
    }
    public void ClearStageState()
    {
        stageIndex = 0;
        currentStage = null;
        GameState = default;
        CurrentTurn = 0;
        DiceValue = 0;
        stageProfiles = null; // 스테이지 데이터도 초기화
        // TODO: 초기화해야 할 추가 필드가 있으면 여기에 추가
    }
}