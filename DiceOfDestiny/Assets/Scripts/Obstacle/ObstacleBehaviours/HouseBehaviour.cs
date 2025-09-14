using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class HouseBehaviour : Obstacle, IObstacleBehaviour
{
    enum SponObstacleDir
    {
        UpLeft,
        Up,
        UpRight,
        Right,
        Left,
        DownLeft,
        Down,
        DownRight
    }

    enum SpawnTable // 0 ~ 9 좀비 10 ~ 19 슬라임 20 ~ 23 폰 24 나이트
    {
        Zombie = 9,
        Slime = 19,
        Pawn = 23,
        Knight
    }

    private List<SponObstacleDir> randObstacleDir = new();
    [SerializeField] public int life = 5;
    [SerializeField] private int sponTurn = 0;

    public void DoLogic()
    {
        // 행동 단위
    }

    public void DoLogicTurn()
    {
        // 턴 단위
        if (sponTurn < 1)
        {
            sponTurn++;
            return;
        }
        else
        {
            sponTurn = 0;
        }

        randObstacleDir.Clear();

        // 8 방향 중에 소환가능한 방향을 리스트에 추가
        foreach (SponObstacleDir dir in Enum.GetValues(typeof(SponObstacleDir)))
        {
            if (CanObstacleDirection(dir))
                randObstacleDir.Add(dir);
        }

        if (randObstacleDir.Count > 0)
        {
            int randIndex = Random.Range(0, randObstacleDir.Count); // 0 ~ Count-1
            SponObstacle(randObstacleDir[randIndex]);
        }
    }

    private Vector2Int GetDirection(SponObstacleDir dir)
    {
        return dir switch
        {
            SponObstacleDir.UpLeft => new Vector2Int(-1, 1),
            SponObstacleDir.Up => new Vector2Int(0, 1),
            SponObstacleDir.UpRight => new Vector2Int(1, 1),
            SponObstacleDir.Right => new Vector2Int(1, 0),
            SponObstacleDir.Left => new Vector2Int(-1, 0),
            SponObstacleDir.DownLeft => new Vector2Int(-1, -1),
            SponObstacleDir.Down => new Vector2Int(0, -1),
            SponObstacleDir.DownRight => new Vector2Int(1, -1),
            _ => Vector2Int.zero
        };
    }

    private bool CanObstacleDirection(SponObstacleDir dir)
    {
        Vector2Int direction = GetDirection(dir);

        Vector2Int nextPosition = obstaclePosition + direction;
        Tile nextTile = BoardManager.Instance.GetTile(nextPosition);

        // 보드 안밖인지, 시작점 도착점 소환 불가
        if (nextTile == null || !BoardManager.Instance.IsMovementArea(nextPosition))
            return false;

        // 기물이 있거나 장애물이 있으면 소환 불가
        if (nextTile.GetPiece() != null || nextTile.Obstacle != ObstacleType.None)
            return false;

        return true;
    }

    private void SponObstacle(SponObstacleDir dir)
    {
        Vector2Int direction = GetDirection(dir);

        Vector2Int nextPosition = obstaclePosition + direction;

        int randNum = Random.Range(0, 24); // 0 ~ 9 좀비 10 ~ 19 슬라임 20 ~ 23 폰 24 나이트

        ObstacleType obstacleType;

        if (randNum <= (int)SpawnTable.Zombie)
            obstacleType = ObstacleType.Zombie;
        else if (randNum <= (int)SpawnTable.Slime)
            obstacleType = ObstacleType.Slime;
        else if (randNum <= (int)SpawnTable.Pawn)
        {
            obstacleType = ObstacleType.Pawn;
            GameObject pawn = BoardManager.Instance.CreateObstacle(nextPosition, obstacleType);
            ObstacleManager.Instance.AddPawnToList(pawn);

            return;
        }
        else
        {
            obstacleType = ObstacleType.Knight;
            GameObject knight = BoardManager.Instance.CreateObstacle(nextPosition, obstacleType);
            ObstacleManager.Instance.AddKnightToList(knight);

            return;
        }


        BoardManager.Instance.CreateObstacle(nextPosition, obstacleType);
    }

    public void TakeDamage(int damage)
    {
        life -= damage;
        
        if (life <= 0)
        {
            ObstacleManager.Instance.DestroyHouse(obstaclePosition);
        }
    }
}
