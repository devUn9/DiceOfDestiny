using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MissionManager : Singletone<MissionManager>
{
    [Header("Tutorial Mission")]
    public bool isTutorialMissionCompleted { get; set; } = false; // 튜토리얼 미션 완료 여부
    public bool isFirstMovePiece { get; set; } = false; // 첫 번째 말 이동 여부
    public bool isFirstActiveSkillUse { get; set; } = false; // 첫 번째 액티브 스킬 사용 여부
    public bool isFirstPassiveSkillUse { get; set; } = false; // 첫 번째 패시브 스킬 사용 여부

    [Header("FinishLine Mission")]
    public bool isFinishLine { get; private set; } = false; // 도착 지점인지 여부

    [Header("Stage 3 Mission")]
    public int killEnemyCount { get; private set; } = 0;

    [Header("Stage 5 Mission")]
    public int findGrayGrassCount { get; private set; } = 0;
    public bool isFindGrayGrass { get; private set; } = false;

    [Header("Stage 6 Mission")]
    public int alivePawnCount { get; private set; } = 0;
    public bool isKillTwoPawn { get; private set; } = false;

    [Header("Stage 9 Mission")]
    public int aliveKnightCount { get; private set; } = 0;
    public bool isKillTwoKnight { get; private set; } = false;

    [Header("Stage 10 Mission")]
    public int aliveHouseCount { get; private set; } = 0;
    public bool isDestroyHouse { get; private set; } = false;

    public void Start()
    {
        ResetMission();
    }

    public void ResetMission()
    {
        killEnemyCount = 0;
        findGrayGrassCount = 0;
        alivePawnCount = 0;
        aliveKnightCount = 0;
        aliveHouseCount = 0;

        isFinishLine = false;
        isFindGrayGrass = false;
        isKillTwoPawn = false;
        isKillTwoKnight = false;
        isDestroyHouse = false;
    }

    public IEnumerator IsAllMissionCompleted(PieceController clearPiece)
    {
        while (PieceManager.Instance.currentPiece.isMoving)
            yield return null;

        UpdateKillEnemyCount();
        UIManager.Instance.UpdateMissionUI();

        if (StageManager.Instance.currentStage.missions.TrueForAll(m => m.IsCompleted()))
        {
            Debug.Log("복합 미션 완료!");
            StageManager.Instance.StageClear(clearPiece);
        }
    }

    // 도착 지점이면 true
    public void CheckStageClearAfterMove(Vector2Int newPosition)
    {
        // 도착 지점이라면
        if (newPosition.y == BoardManager.Instance.boardSizeY - 1)
        {
            isFinishLine = true;
        }
        else
        {
            isFinishLine = false;
        }
    }

    // 다른 미션이 없거나 다른 미션을 완료하여 도착점에 갈 수 있는지 없는지
    public bool CanGoFinishLine()
    {
        // 미션이 하나만 있다면 무조건 도착점에 갈 수 있음
        if (StageManager.Instance.currentStage.missions.Count == 1)
        {
            return true;
        }

        // 두 번째 미션이 완료되었다면 도착점에 갈 수 있음
        if (StageManager.Instance.currentStage.missions[1].IsCompleted())
        {
            return true;
        }

        return false;
    }

    private int UpdateKillEnemyCount(int count = 0)
    {
        killEnemyCount = 8;

        for (int x = 0; x < BoardManager.Instance.boardSize; x++)
        {
            for (int y = 1; y < BoardManager.Instance.boardSize + 1; y++)
            {
                if (BoardManager.Instance.Board[x, y].Obstacle == ObstacleType.Slime || BoardManager.Instance.Board[x, y].Obstacle == ObstacleType.Zombie)
                {
                    ++count;
                }
            }
        }

        killEnemyCount -= count;
        return count;
    }

    //적을 모두 처치했는지 확인
    public bool HasMovingEnemyObstacles()
    {
        int count = 0;

        count = UpdateKillEnemyCount(count);

        return count <= 0;
    }

    // 회색 풀 밟기 미션에 대해 카운팅
    public void AddGrayGrassMission()
    {
        findGrayGrassCount++;

        if (findGrayGrassCount >= 3)
            isFindGrayGrass = true;
    }

    public void AlivePawnCountCheck()
    {
        alivePawnCount++;

        if (alivePawnCount >= 2)
        {
            isKillTwoPawn = true;
        }
    }

    public void AliveKnightCountCheck()
    {
        aliveKnightCount++;

        if (aliveKnightCount >= 2)
        {
            isKillTwoKnight = true;
        }
    }

    public void AliveHouseCountCheck()
    {
        aliveHouseCount++;

        if (aliveHouseCount >= 2)
        {
            isDestroyHouse = true;
        }
    }
    public void CheckMoveMission()
    {
        if (!isFirstMovePiece)
            isFirstMovePiece = true;
    }

    public void CheckPassiveSkillUse()
    {
        if (!isFirstPassiveSkillUse)
            isFirstPassiveSkillUse = true;
    }

    public void CheckActiveSkillUse()
    {
        if (!isFirstActiveSkillUse)
            isFirstActiveSkillUse = true;
    }
}