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

    private List<SponObstacleDir> randObstacleDir = new();
    [SerializeField] private int life = 5;
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

        int randNum = Random.Range(0, 11); // 0 ~ 4 좀비 5 ~ 9 슬라임 10 폰

        ObstacleType obstacleType;

        if (randNum <= 4)
            obstacleType = ObstacleType.Zombie;
        else if (randNum <= 9)
            obstacleType = ObstacleType.Slime;
        else
        {
            obstacleType = ObstacleType.Pawn;
            GameObject pawn = BoardManager.Instance.CreateObstacle(nextPosition, obstacleType);
            ObstacleManager.Instance.AddPawnToList(pawn);

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
