using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MissionManager : Singletone<MissionManager>
{
    [Header("FinishLine Mission")]
    public bool isFinishLine { get; private set; } = false; // 도착 지점인지 여부

    [Header("Stage 5 Mission")]
    private int findGrayGrassCount = 0;
    public bool isFindGrayGrass { get; private set; } = false;

    [Header("Stage 6 Mission")]
    private int alivePawnCount;

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
}