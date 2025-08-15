using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MissionManager : Singletone<MissionManager>
{
    [Header("FinishLine Mission")]
    public bool isFinishLine { get; private set; } = false; // 도착 지점인지 여부

    [Header("Stage 5 Mission")]
    [SerializeField] private int findGrayGrassCount = 0;
    public bool isFindGrayGrass { get; private set; } = false;

    [Header("Stage 6 Mission")]
    [SerializeField] private int alivePawnCount = 0;
    public bool isKillTwoPawn { get; private set; } = false;

    public void IsAllMissionCompleted()
    {
        if (StageManager.Instance.currentStage.missions.TrueForAll(m => m.IsCompleted()))
        {
            Debug.Log("복합 미션 완료!");
            StartCoroutine(StageManager.Instance.StageClear());
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

    // 적을 모두 처치했는지 확인
    public bool HasMovingEnemyObstacles()
    {
        int count = 0;

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

        if(alivePawnCount >= 2)
        {
            isKillTwoPawn = true;
        }
    }
}