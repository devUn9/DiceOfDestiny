using System.Collections.Generic;
using UnityEngine;

public enum TileColor
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple,
    Gray,
    None
}
public enum DirectionType
{
    Four,      // 상하좌우 4방향
    Eight,     // 8방향 (상하좌우 + 대각선)
    Diagonal,  // 대각선만
    ForwardThree // 굴러온 방향 기준 전방 3칸
}

public class BoardManager : Singletone<BoardManager>
{
    [Header("Board Size Settings")]
    [SerializeField] public int boardSize = 13;
    public int boardSizeY;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] public Transform boardTransform;
    public Tile[,] Board { get; set; }

    [Header("Tile Colors Settings")]
    [SerializeField]
    public Color[] tileColors = new Color[] {
        new Color(1f, 0f, 0f), // 빨강
        new Color(0f, 1f, 0f), // 초록
        new Color(0f, 0f, 1f), // 파랑
        new Color(1f, 1f, 0f), // 노랑
        new Color(1f, 0f, 1f), // 보라
        new Color(0.9f, 0.9f, 0.9f) // 회색
    };

    List<int> colorIndices = new List<int>();
    List<ObstacleType> obstacleIndices = new List<ObstacleType>();


    void Start()
    {
        boardSizeY = boardSize + 2;

        GenerateBoard();
    }

    private void Update()
    {

    }

    // 보드 경계 체크 함수, 보드 안쪽을 리턴
    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < boardSize && position.y >= 0 && position.y < boardSize;
    }

    private void GenerateBoard()
    {
        Board = new Tile[boardSize, boardSizeY];

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                GameObject tileObject = Instantiate(tilePrefab, new Vector3(boardTransform.position.x + x, boardTransform.position.y + y, 0), Quaternion.identity, boardTransform);
                tileObject.name = $"Tile_{x}_{y}";
                Tile tile = tileObject.GetComponent<Tile>();
                tile.TileColor = TileColor.None;
                tile.Obstacle = ObstacleType.None;
                Board[x, y] = tile;
            }
        }
    }

    public void SetBoard(StageData profile)
    {
        // 가중치에 맞게 색을 설정하는 부분
        colorIndices = new List<int>();
        for (int color = 0; color < tileColors.Length; color++)
        {
            for (int i = 0; i < profile.minimumColorEnsure; i++)
                colorIndices.Add(color);
        }

        int[] colorWeights = new int[]
        {
        profile.redWeight * profile.weightPower,
        profile.greenWeight * profile.weightPower,
        profile.blueWeight * profile.weightPower,
        profile.yellowWeight * profile.weightPower,
        profile.purpleWeight * profile.weightPower,
        profile.orangeWeight * profile.weightPower
        };

        int[] cumulativeWeights = new int[colorWeights.Length];
        cumulativeWeights[0] = colorWeights[0];

        for (int i = 1; i < colorWeights.Length; i++)
            cumulativeWeights[i] = cumulativeWeights[i - 1] + colorWeights[i];

        int total = cumulativeWeights[cumulativeWeights.Length - 1];


        for (int i = 0; i < (boardSize * boardSize) - (tileColors.Length * profile.minimumColorEnsure); i++)
        {
            int rand = Random.Range(0, total);
            for (int j = 0; j < cumulativeWeights.Length; j++)
            {
                if (rand < cumulativeWeights[j])
                {
                    colorIndices.Add(j);
                    break;
                }
            }
        }

        for (int i = colorIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (colorIndices[i], colorIndices[j]) = (colorIndices[j], colorIndices[i]);
        }

        // 색칠하는 부분
        int idx = 0;
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 1; y < boardSize+1; y++)
            {
                Board[x, y].SetTileColor(tileColors[colorIndices[idx]]);
                switch (colorIndices[idx])
                {
                    case 0: // 빨강
                        Board[x, y].TileColor = TileColor.Red;
                        break;
                    case 1: // 초록
                        Board[x, y].TileColor = TileColor.Green;
                        break;
                    case 2: // 파랑
                        Board[x, y].TileColor = TileColor.Blue;
                        break;
                    case 3: // 노랑
                        Board[x, y].TileColor = TileColor.Yellow;
                        break;
                    case 4: // 보라
                        Board[x, y].TileColor = TileColor.Purple;
                        break;
                    case 5: // 회색
                        Board[x, y].TileColor = TileColor.Gray;
                        break;
                }
                idx++;
            }
        }


        // 장애물 배치 부분
        obstacleIndices = new List<ObstacleType>();
        int tileCount = boardSize * boardSize;
        int obstacleCount = Mathf.RoundToInt(tileCount * profile.obstacleDensity);
        for (int i = 0; i < tileCount - obstacleCount; i++) // 장애물이 없는 타일 추가
        {
            obstacleIndices.Add(ObstacleType.None);
        }

        List<ObstacleType> availableObstacleWeight = new List<ObstacleType>();
        for (int i = 0; i < profile.availableObstacle.Count; i++) // 장애물 타입을 인덱스에 추가
        {
            for (int j = 0; j < profile.availableObstacle[i].weight * 10; j++)
            {
                availableObstacleWeight.Add(profile.availableObstacle[i].type);
            }
        }

        for (int i = 0; i < obstacleCount; i++) // 장애물이 있는 타일
        {
            int randIndex = Random.Range(0, availableObstacleWeight.Count);
            obstacleIndices.Add(availableObstacleWeight[randIndex]);
        }

        for (int i = obstacleIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (obstacleIndices[i], obstacleIndices[j]) = (obstacleIndices[j], obstacleIndices[i]);
        }

        idx = 0;
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 1; y < boardSize+1; y++)
            {
                Board[x, y].Obstacle = obstacleIndices[idx];
                // 장애물 생성
                if (obstacleIndices[idx] != ObstacleType.None)
                {
                    GameObject obstacle = Instantiate(ObstacleManager.Instance.obstaclePrefabs[obstacleIndices[idx]],
                        new Vector3(boardTransform.position.x + x, boardTransform.position.y + y, 0), Quaternion.identity, boardTransform);
                    obstacle.GetComponent<Obstacle>().obstaclePosition = new Vector2Int(x, y);
                    ObstacleManager.Instance.SetObstacle(obstacle);

                    Board[x, y].isWalkable = obstacle.GetComponent<Obstacle>().isWalkable;
                }
                idx++;
            }
        }
    }



    public Obstacle ReturnObstacleByPosition(Vector2Int position)
    {
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSizeY)
        {
            Debug.LogError("Position out of bounds");
            return null;
        }
        Tile tile = Board[position.x, position.y];
        if (tile.Obstacle == ObstacleType.None)
        {
            return null;
        }
        foreach (GameObject obstacle in ObstacleManager.Instance.currentObstacles)
        {
            if (obstacle.GetComponent<Obstacle>().obstaclePosition == position)
            {
                return obstacle.GetComponent<Obstacle>();
            }
        }
        return null;
    }

    public bool IsEmptyTile(Vector2Int position)
    {
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSizeY)
        {
            // Debug.LogError("Position out of bounds");
            return false;
        }
        return Board[position.x, position.y].Obstacle == ObstacleType.None;
    }

    public void MoveObstacle(Obstacle obstacle, Vector2Int nextPos)
    {
        Vector2Int currentPos = obstacle.obstaclePosition;
        Board[currentPos.x, currentPos.y].Obstacle = ObstacleType.None; // 현재 타일의 장애물 제거
        Board[nextPos.x, nextPos.y].Obstacle = obstacle.obstacleType; // 다음 타일에 장애물 설정
        obstacle.obstaclePosition = nextPos; // 장애물의 위치 업데이트
        // obstacle.transform.position = new Vector3(boardTransform.position.x + nextPos.x, boardTransform.position.y + nextPos.y, 0); // 장애물 위치 이동
    }

    // 주변 8칸 중 윗면과 같은 색이 몇개인지 카운팅하는 함수
    public int CountMatchingColors(Vector2Int position, TileColor targetColor)
    {
        int matchCount = 0;
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(-1, -1), // 좌상
            new Vector2Int(-1, 0),  // 좌
            new Vector2Int(-1, 1),  // 좌하
            new Vector2Int(0, -1),  // 상
            new Vector2Int(0, 1),   // 하
            new Vector2Int(1, -1),  // 우상
            new Vector2Int(1, 0),   // 우
            new Vector2Int(1, 1)    // 우하
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = position + dir;
            if (checkPos.x >= 0 && checkPos.x < boardSize &&
                checkPos.y >= 0 && checkPos.y < boardSizeY)
            {
                if (Board[checkPos.x, checkPos.y] != null &&
                    Board[checkPos.x, checkPos.y].TileColor == targetColor)
                {
                    // 체크된게 어느 타일인지 보여주면 굿.
                    matchCount++;
                }
            }
        }

        return matchCount;
    }

    // 주변 8칸 중 윗면과 같은 색의 위치를 가져오는 함수
    public List<Vector2Int> GetMatchingColorTiles(Vector2Int position, TileColor targetColor)
    {
        List<Vector2Int> matchingTiles = new List<Vector2Int>();
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(-1, -1), // 좌상
            new Vector2Int(-1, 0),  // 좌
            new Vector2Int(-1, 1),  // 좌하
            new Vector2Int(0, -1),  // 상
            new Vector2Int(0, 1),   // 하
            new Vector2Int(1, -1),  // 우상
            new Vector2Int(1, 0),   // 우
            new Vector2Int(1, 1)    // 우하
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = position + dir;
            if (checkPos.x >= 0 && checkPos.x < boardSize &&
                checkPos.y >= 0 && checkPos.y < boardSizeY)
            {
                if (Board[checkPos.x, checkPos.y] != null &&
                    Board[checkPos.x, checkPos.y].TileColor == targetColor)
                {
                    matchingTiles.Add(checkPos);
                }
            }
        }

        return matchingTiles;
    }

    // 주변 8칸 중 특정 색과 일치하는 칸만 재배정하는 함수
    public void ReassignMatchingColorTiles(Vector2Int position, TileColor targetColor)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(-1, -1), // 좌상
        new Vector2Int(-1, 0),  // 좌
        new Vector2Int(-1, 1),  // 좌하
        new Vector2Int(0, -1),  // 상
        new Vector2Int(0, 1),   // 하
        new Vector2Int(1, -1),  // 우상
        new Vector2Int(1, 0),   // 우
        new Vector2Int(1, 1)    // 우하
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = position + dir;
            if (checkPos.x >= 0 && checkPos.x < boardSize &&
                checkPos.y >= 0 && checkPos.y < boardSizeY &&
                Board[checkPos.x, checkPos.y] != null &&
                Board[checkPos.x, checkPos.y].TileColor == targetColor)
            {
                // 각 타일마다 독립적으로 무작위 색상 인덱스 선택 (None 제외)
                int randomColorIndex = Random.Range(0, tileColors.Length); // tileColors.Length는 6 (Red, Green, Blue, Yellow, Purple, Gray)

                // 타일 색상 설정
                Board[checkPos.x, checkPos.y].SetTileColor(tileColors[randomColorIndex]);
                // TileColor 열거형 값 설정
                switch (randomColorIndex)
                {
                    case 0: Board[checkPos.x, checkPos.y].TileColor = TileColor.Red; break;
                    case 1: Board[checkPos.x, checkPos.y].TileColor = TileColor.Green; break;
                    case 2: Board[checkPos.x, checkPos.y].TileColor = TileColor.Blue; break;
                    case 3: Board[checkPos.x, checkPos.y].TileColor = TileColor.Yellow; break;
                    case 4: Board[checkPos.x, checkPos.y].TileColor = TileColor.Purple; break;
                    case 5: Board[checkPos.x, checkPos.y].TileColor = TileColor.Gray; break;
                }
            }
        }
    }

    // 주변 8칸 색상 재배정하는 함수
    public void ReassignSurroundingColors(Vector2Int position)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(-1, -1), // 좌상
            new Vector2Int(-1, 0),  // 좌
            new Vector2Int(-1, 1),  // 좌하
            new Vector2Int(0, -1),  // 상
            new Vector2Int(0, 1),   // 하
            new Vector2Int(1, -1),  // 우상
            new Vector2Int(1, 0),   // 우
            new Vector2Int(1, 1)    // 우하
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = position + dir;
            if (checkPos.x >= 0 && checkPos.x < boardSize &&
                checkPos.y >= 0 && checkPos.y < boardSizeY &&
                Board[checkPos.x, checkPos.y] != null)
            {
                // 무작위 색상 인덱스 선택
                int randomColorIndex = Random.Range(0, tileColors.Length);
                // 타일 색상 설정
                Board[checkPos.x, checkPos.y].SetTileColor(tileColors[randomColorIndex]);
                // TileColor 열거형 값 설정
                switch (randomColorIndex)
                {
                    case 0: Board[checkPos.x, checkPos.y].TileColor = TileColor.Red; break;
                    case 1: Board[checkPos.x, checkPos.y].TileColor = TileColor.Green; break;
                    case 2: Board[checkPos.x, checkPos.y].TileColor = TileColor.Blue; break;
                    case 3: Board[checkPos.x, checkPos.y].TileColor = TileColor.Yellow; break;
                    case 4: Board[checkPos.x, checkPos.y].TileColor = TileColor.Purple; break;
                    case 5: Board[checkPos.x, checkPos.y].TileColor = TileColor.Gray; break;
                }
            }
        }
    }

    /// <summary>
    /// DirectionTyped에 따라 4방향, 8방향, 대각선, 굴러온 방향 기준 전방 3칸을 반환한다.
    /// 인자는 타입, 좌표값을 받는다.
    /// </summary>
    /// <param name="directionType"></param>
    /// <param name="gridPosition"></param>
    /// <returns></returns>
    // 좌표값을 입력받아 좌표값 리스트를 반환하는 함수

    

    public List<Vector2Int> GetTilePositions(DirectionType directionType, Vector2Int gridPosition)
    {
        Vector2Int currentPieceLastDirection = PieceManager.Instance.currentPiece.GetLastMoveDirection();
        List<Vector2Int> positions = new List<Vector2Int>();

        // 4방향 (상, 하, 좌, 우)
        Vector2Int[] fourDirections = new Vector2Int[]
        {
        new Vector2Int(0, 1),   // 상
        new Vector2Int(0, -1),  // 하
        new Vector2Int(-1, 0),  // 좌
        new Vector2Int(1, 0)    // 우
        };

        // 대각선 (좌상, 우상, 좌하, 우하)
        Vector2Int[] diagonalDirections = new Vector2Int[]
        {
        new Vector2Int(-1, 1),  // 좌상
        new Vector2Int(1, 1),   // 우상
        new Vector2Int(-1, -1), // 좌하
        new Vector2Int(1, -1)   // 우하
        };

        // 8방향 (4방향 + 대각선)
        Vector2Int[] eightDirections = new Vector2Int[]
        {
        new Vector2Int(0, 1),   // 상
        new Vector2Int(0, -1),  // 하
        new Vector2Int(-1, 0),  // 좌
        new Vector2Int(1, 0),   // 우
        new Vector2Int(-1, 1),  // 좌상
        new Vector2Int(1, 1),   // 우상
        new Vector2Int(-1, -1), // 좌하
        new Vector2Int(1, -1)   // 우하
        };

        switch (directionType)
        {
            case DirectionType.Four:
                foreach (var dir in fourDirections)
                {
                    Vector2Int newPos = gridPosition + dir;
                    if (IsValidPosition(newPos))
                        positions.Add(newPos);
                }
                break;

            case DirectionType.Eight:
                foreach (var dir in eightDirections)
                {
                    Vector2Int newPos = gridPosition + dir;
                    if (IsValidPosition(newPos))
                        positions.Add(newPos);
                }
                break;

            case DirectionType.Diagonal:
                foreach (var dir in diagonalDirections)
                {
                    Vector2Int newPos = gridPosition + dir;
                    if (IsValidPosition(newPos))
                        positions.Add(newPos);
                }
                break;

            case DirectionType.ForwardThree:
                if (currentPieceLastDirection != Vector2Int.zero) // 유효한 이동 방향인지 확인
                {
                    // 전방 1칸
                    Vector2Int forward = currentPieceLastDirection;
                    Vector2Int forwardPos = gridPosition + forward;
                    if (IsValidPosition(forwardPos))
                        positions.Add(forwardPos);

                    // 전방 대각선 1칸 (좌우 대각선)
                    Vector2Int leftDiagonal = Vector2Int.zero;
                    Vector2Int rightDiagonal = Vector2Int.zero;

                    // 마지막 이동 방향에 따라 대각선 방향 설정
                    if (currentPieceLastDirection == new Vector2Int(0, 1)) // 상
                    {
                        leftDiagonal = new Vector2Int(-1, 1); // 좌상
                        rightDiagonal = new Vector2Int(1, 1); // 우상
                    }
                    else if (currentPieceLastDirection == new Vector2Int(0, -1)) // 하
                    {
                        leftDiagonal = new Vector2Int(-1, -1); // 좌하
                        rightDiagonal = new Vector2Int(1, -1); // 우하
                    }
                    else if (currentPieceLastDirection == new Vector2Int(-1, 0)) // 좌
                    {
                        leftDiagonal = new Vector2Int(-1, 1); // 좌상
                        rightDiagonal = new Vector2Int(-1, -1); // 좌하
                    }
                    else if (currentPieceLastDirection == new Vector2Int(1, 0)) // 우
                    {
                        leftDiagonal = new Vector2Int(1, 1); // 우상
                        rightDiagonal = new Vector2Int(1, -1); // 우하
                    }

                    // 대각선 타일 추가
                    Vector2Int leftPos = gridPosition + leftDiagonal;
                    Vector2Int rightPos = gridPosition + rightDiagonal;
                    if (IsValidPosition(leftPos))
                        positions.Add(leftPos);
                    if (IsValidPosition(rightPos))
                        positions.Add(rightPos);
                }
                else
                {
                    Debug.LogWarning("Current piece has no valid last move direction.");
                }
                break;
        }

        return positions;
    }


    public void RemoveObstacleAtPosition(Vector2Int position)
    {
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSizeY)
        {
            Debug.LogError("Position out of bounds");
            return;
        }

        Tile tile = Board[position.x, position.y];
        if (tile.Obstacle != ObstacleType.None)
        {
            Obstacle obstacle = ReturnObstacleByPosition(position);
            if (obstacle != null)
            {
                ObstacleManager.Instance.RemoveSingleObstacle(obstacle.gameObject);
                tile.Obstacle = ObstacleType.None;
            }
        }
    }

    public void RemoveObstacle(Obstacle obstacle)
    {
        Vector2Int position = obstacle.obstaclePosition;

        Tile tile = Board[position.x, position.y];
        if (tile.Obstacle != ObstacleType.None)
        {
            if (obstacle != null)
            {
                ObstacleManager.Instance.RemoveSingleObstacle(obstacle.gameObject);
                tile.Obstacle = ObstacleType.None;
            }
        }
    }

    public void CreateObstacle(Vector2Int position, ObstacleType obstacleType)
    {
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSizeY)
        {
            Debug.LogError("Position out of bounds");
            return;
        }

        Tile tile = Board[position.x, position.y];
        if (tile.Obstacle != ObstacleType.None)
        {
            Debug.LogWarning($"Obstacle already exists at position ({position.x}, {position.y})");
            return;
        }

        // 장애물 타입 설정
        tile.Obstacle = obstacleType;

        // 장애물 프리팹 생성
        if (ObstacleManager.Instance.obstaclePrefabs.ContainsKey(obstacleType))
        {
            GameObject obstacle = Instantiate(
                ObstacleManager.Instance.obstaclePrefabs[obstacleType],
                new Vector3(boardTransform.position.x + position.x, boardTransform.position.y + position.y, 0),
                Quaternion.identity,
                boardTransform
            );
            obstacle.name = $"Obstacle_{obstacleType}_{position.x}_{position.y}";
            Obstacle obstacleComponent = obstacle.GetComponent<Obstacle>();
            obstacleComponent.obstaclePosition = position;
            ObstacleManager.Instance.SetObstacle(obstacle);

            // 타일의 isWalkable 속성 업데이트
            tile.isWalkable = obstacleComponent.isWalkable;
        }
        else
        {
            Debug.LogError($"No prefab found for ObstacleType: {obstacleType}");
            tile.Obstacle = ObstacleType.None; // 프리팹이 없으면 장애물 설정 취소
        }
    }

    // 상하좌우 칸에 장애물 확인
    public bool HasObstacleCardinal(Vector2Int pos)
    {
        Vector2Int[] dirs = new Vector2Int[]
        {
        new Vector2Int(-1, 0),  // 좌
        new Vector2Int(0, -1),  // 상
        new Vector2Int(0, 1),   // 하
        new Vector2Int(1, 0)    // 우
        };

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int checkPos = pos + dir;
            // 경계 조건을 먼저 확인
            if (checkPos.x >= 0 && checkPos.x < boardSize &&
                checkPos.y >= 0 && checkPos.y < boardSizeY)
            {
                // 좀비나 고블린인지 확인
                if (Board[checkPos.x, checkPos.y]?.Obstacle is ObstacleType.Slime or ObstacleType.Zombie)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // 대각선 칸에 장애물 확인
    public bool HasObstacleDiagonal(Vector2Int pos)
    {
        Vector2Int[] dirs = new Vector2Int[]
        {
        new Vector2Int(-1, -1), // 좌상
        new Vector2Int(-1, 1),  // 좌하
        new Vector2Int(1, -1),  // 우상
        new Vector2Int(1, 1)    // 우하
        };

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int checkPos = pos + dir;
            if (checkPos.x >= 0 && checkPos.x < boardSize &&
                checkPos.y >= 0 && checkPos.y < boardSizeY &&
                Board[checkPos.x, checkPos.y] != null &&
                Board[checkPos.x, checkPos.y].Obstacle != ObstacleType.None)
            {
                return true;
            }
        }
        return false;
    }

    // 전방 3칸(직진 및 대각선)에 장애물 확인
    public bool HasObstacleForward(Vector2Int pos, Vector2Int dir)
    {
        Vector2Int forward = -dir;
        Vector2Int[] tiles = new Vector2Int[]
        {
        pos + forward, // 직진
        pos + forward + new Vector2Int(-forward.y, forward.x), // 좌 대각선
        pos + forward + new Vector2Int(forward.y, -forward.x)  // 우 대각선
        };

        foreach (Vector2Int checkPos in tiles)
        {
            if (checkPos.x >= 0 && checkPos.x < boardSize &&
                checkPos.y >= 0 && checkPos.y < boardSizeY &&
                Board[checkPos.x, checkPos.y] != null &&
                Board[checkPos.x, checkPos.y].Obstacle != ObstacleType.None)
            {
                return true;
            }
        }
        return false;
    }

    public void SetTileColor(Vector2Int position, TileColor targetColor)
    {
        // 위치 유효성 검사
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSizeY)
        {
            Debug.LogError($"Position out of bounds: {position}");
            return;
        }

        // 타일 가져오기
        Tile tile = Board[position.x, position.y];
        if (tile == null)
        {
            Debug.LogError($"No tile found at position: {position}");
            return;
        }

        // 타일 색상 설정
        tile.TileColor = targetColor;
        switch (targetColor)
        {
            case TileColor.Red:
                tile.SetTileColor(tileColors[0]);
                break;
            case TileColor.Green:
                tile.SetTileColor(tileColors[1]);
                break;
            case TileColor.Blue:
                tile.SetTileColor(tileColors[2]);
                break;
            case TileColor.Yellow:
                tile.SetTileColor(tileColors[3]);
                break;
            case TileColor.Purple:
                tile.SetTileColor(tileColors[4]);
                break;
            case TileColor.Gray:
                tile.SetTileColor(tileColors[5]);
                break;
            case TileColor.None:
                tile.SetTileColor(Color.white); // None일 경우 기본 색상 (예: 흰색)
                break;
            default:
                Debug.LogWarning($"Unknown TileColor: {targetColor}");
                break;
        }
    }


    public Tile GetTile(Vector2Int position)
    {
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSizeY)
        {
            return null;
        }
        else
        {
            return Board[position.x, position.y];
        }
    }

    public Color GetColor(TileColor tileColor)
    {
        if ((int)tileColor < 0 || (int)tileColor >= tileColors.Length)
        {
            Debug.LogError($"Invalid TileColor: {tileColor}");
            return Color.white; // 기본 색상 반환
        }
        else
        {
            return tileColors[(int)tileColor];
        }
    }
}
