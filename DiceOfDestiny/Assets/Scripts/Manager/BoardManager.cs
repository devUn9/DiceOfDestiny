using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileColor
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple,
    Brown,
    Gray,
    None
}
public enum DirectionType
{
    Four,      // 상하좌우 4방향
    Eight,     // 8방향 (상하좌우 + 대각선)
    Diagonal,  // 대각선만
    ForwardThree, // 굴러온 방향 기준 전방 3칸
    ForwardOne // 굴러온 방향 기준 전방 1칸
}

public class BoardManager : Singletone<BoardManager>
{
    [Header("Board Size Settings")]
    [SerializeField] public int boardSize = 13;
    public int boardSizeY;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] public GameObject boardPosParent;
    [SerializeField] public Transform boardTransform;

    [SerializeField] private SpriteRenderer borderRenderer; // 보드판
    [SerializeField] private Sprite[] borderSprites;
    [SerializeField] private SpriteRenderer backGroundRenderer; // 배경
    [SerializeField] private Sprite[] backGroundSprites;


    [SerializeField] public Transform newBoardTransform;

    public Tile[,] Board { get; set; }
    public Tile[,] TempBoard { get; set; }

    [Header("Tile Colors Settings")]
    [SerializeField]
    public Color[] tileColors = new Color[] {
        new Color(1f, 0f, 0f), // 빨강
        new Color(0f, 1f, 0f), // 초록
        new Color(0f, 0f, 1f), // 파랑
        new Color(1f, 1f, 0f), // 노랑
        new Color(1f, 0f, 1f), // 보라
        new Color(0.9f, 0.9f, 0.9f) // 갈색
    };

    List<int> colorIndices = new List<int>();
    List<ObstacleType> obstacleIndices = new List<ObstacleType>();

    public void Initialize()
    {
        boardSizeY = boardSize + 2;
        GenerateBoard();
    }

    public void CreateBorderAndBG()
    {
        GenerateBorder();
        GenerateBackGround();
    }

    // 보드 경계 체크 함수, 보드 안쪽을 리턴
    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < boardSize && position.y >= 0 && position.y < boardSizeY;
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
    public void GenerateBorder()
    {
        DestroyBorder(); // 기존 경계 제거
        SetBoarder();
        borderRenderer.gameObject.SetActive(true);
    }

    public void DestroyBorder()
    {
        borderRenderer.sprite = null; // 스프라이트 초기화
        borderRenderer.gameObject.SetActive(false);
    }

    public void SetBoarder()
    {
        int stageNum = StageManager.Instance.currentStage.stageNumber;

        // 6 : 돌 판
        if (stageNum == 6)
            borderRenderer.sprite = borderSprites[1];

        // 1~5, 7~ : 나무판
        else 
            borderRenderer.sprite = borderSprites[0];
    }

    public void GenerateBackGround()
    {
        DestroyBackGround(); // 기존 배경 제거
        SetBackground();
        backGroundRenderer.gameObject.SetActive(true);
    }

    public void DestroyBackGround()
    {
        backGroundRenderer.sprite = null; // 스프라이트 초기화
        backGroundRenderer.gameObject.SetActive(false);
    }
    public void SetBackground()
    {

        int stageNum = StageManager.Instance.currentStage.stageNumber;
        
        // 1~2 : 평원
        if (stageNum < 2)
        {
            backGroundRenderer.sprite = backGroundSprites[0];
        }
        // 2~3 : 숲
        else if (stageNum >= 2 && stageNum < 4)
        {
            backGroundRenderer.sprite = backGroundSprites[1];
        }
        // 4~5 : 강
        else if (stageNum >= 4 && stageNum < 6)
        {
            backGroundRenderer.sprite = backGroundSprites[2];
        }
        // 6 : 사원
        else if (stageNum == 6)
        {
            backGroundRenderer.sprite = backGroundSprites[3];
        }
        // 7~ : 숲
        else
        {
            backGroundRenderer.sprite = backGroundSprites[1];
        }
    }


    public void GenerateNextBoard()
    {
        TempBoard = new Tile[boardSize, boardSizeY];

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                GameObject tileObject = Instantiate(tilePrefab, new Vector3(newBoardTransform.position.x + x, newBoardTransform.position.y + y, 0), Quaternion.identity, newBoardTransform);
                tileObject.name = $"Tile_{x}_{y}";
                Tile tile = tileObject.GetComponent<Tile>();
                tile.TileColor = TileColor.None;
                tile.Obstacle = ObstacleType.None;

                tile.GetComponent<SpriteRenderer>().color = new Color(241 / 256f, 214f / 256f, 200f / 256f);

                TempBoard[x, y] = tile;
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
        StartCoroutine(SetColortDelayed());

        // 장애물 배치 부분
        obstacleIndices = new List<ObstacleType>();
        int tileCount = boardSize * boardSize;
        int obstacleCount = Mathf.RoundToInt(tileCount * profile.obstacleDensity);
        for (int i = 0; i < tileCount - obstacleCount; i++) // 장애물이 없는 타일 추가
        {
            obstacleIndices.Add(ObstacleType.None);
        }

        // 가중치에 의한 장애물 추가를 위한 반복문
        List<ObstacleType> availableObstacleWeight = new List<ObstacleType>();
        for (int i = 0; i < profile.availableObstacle.Count; i++) // 장애물 타입을 인덱스에 추가
        {
            for (int j = 0; j < profile.availableObstacle[i].weight * 10; j++) // 가중치에 10을 곱하여 리스트에 저장
            {
                availableObstacleWeight.Add(profile.availableObstacle[i].type);
            }
        }

        // 정확한 개수에 의한 장애물 추가를 위한 반복문
        for (int i = 0; i < profile.exactObstacle.Count; i++) // 정확한 장애물 개수를 따르는 장애물의 개수
        {
            for (int j = 0; j < profile.exactObstacle[i].weight; j++) // 장애물 정확한 개수를 리스트에 저장
            {
                obstacleIndices.Add(profile.exactObstacle[i].type);
                obstacleCount--; // 장애물 개수 감소
            }
        }

        // 장애물 개수 만큼 반복
        for (int i = 0; i < obstacleCount; i++) // 장애물이 있는 타일
        {
            // 가중치에 따른 저장한 장애물 리스트들 중 랜덤으로,,, 실제 장애물 리스트에 저장
            int randIndex = Random.Range(0, availableObstacleWeight.Count);
            obstacleIndices.Add(availableObstacleWeight[randIndex]);
        }

        for (int i = obstacleIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (obstacleIndices[i], obstacleIndices[j]) = (obstacleIndices[j], obstacleIndices[i]);
        }

        // 장애물 생성

        StartCoroutine(SetObstacleDelayed());
    }

    IEnumerator SetColortDelayed()
    {
        int idx = 0;
        for (int x = 1; x < boardSize + 1; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                Board[y, x].SetTileColor(tileColors[colorIndices[idx]]);
                switch (colorIndices[idx])
                {
                    case 0: // 빨강
                        Board[y, x].TileColor = TileColor.Red;
                        break;
                    case 1: // 초록
                        Board[y, x].TileColor = TileColor.Green;
                        break;
                    case 2: // 파랑
                        Board[y, x].TileColor = TileColor.Blue;
                        break;
                    case 3: // 노랑
                        Board[y, x].TileColor = TileColor.Yellow;
                        break;
                    case 4: // 보라
                        Board[y, x].TileColor = TileColor.Purple;
                        break;
                    case 5: // 갈색
                        Board[y, x].TileColor = TileColor.Brown;
                        break;
                }
                idx++;
                yield return new WaitForSeconds(0.01f); // 색상 설정 간 약간의 지연
            }
        }

        GenerateBorder();
    }
    IEnumerator SetObstacleDelayed()
    {
        int idx = 0;
        for (int x = 1; x < boardSize + 1; x++)
        {
            for (int y = 0; y < boardSize + 0; y++)
            {
                Board[y, x].Obstacle = obstacleIndices[idx];
                // 장애물 생성
                if (obstacleIndices[idx] != ObstacleType.None)
                {
                    GameObject obstacle = Instantiate(ObstacleManager.Instance.obstaclePrefabs[obstacleIndices[idx]],
                        new Vector3(boardTransform.position.x + y, boardTransform.position.y + x, 0), Quaternion.identity, boardTransform);
                    obstacle.GetComponent<Obstacle>().obstaclePosition = new Vector2Int(y, x);
                    ObstacleManager.Instance.SetObstacle(obstacle);

                    Board[y, x].isWalkable = obstacle.GetComponent<Obstacle>().isWalkable;

                    if (obstacleIndices[idx] == ObstacleType.House)
                        ObstacleManager.Instance.AddHouseToList(obstacle);
                }
                idx++;
                yield return new WaitForSeconds(0.01f); // 장애물 설정 간 약간의 지연
            }
        }

        AddSpecialStageSetting();
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
    public bool IsInsideBoard(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < Board.GetLength(0)
            && pos.y >= 0 && pos.y < Board.GetLength(1);
    }

    public void MoveObstacle(Obstacle obstacle, Vector2Int nextPos)
    {
        Vector2Int currentPos = obstacle.obstaclePosition;
        Board[currentPos.x, currentPos.y].Obstacle = ObstacleType.None; // 현재 타일의 장애물 제거
        Board[nextPos.x, nextPos.y].Obstacle = obstacle.obstacleType; // 다음 타일에 장애물 설정
        obstacle.obstaclePosition = nextPos; // 장애물의 위치 업데이트
    }

    public void MoveObstacle(Obstacle obstacle, Vector2Int nextPos, ref ObstacleType lastObstacleType)
    {
        Vector2Int currentPos = obstacle.obstaclePosition;
        GetTile(currentPos).Obstacle = lastObstacleType; // 현재 타일의 장애물 제거
        lastObstacleType = GetTile(nextPos).Obstacle; // 마지막 장애물 타입 업데이트
        GetTile(nextPos).Obstacle = obstacle.obstacleType; // 다음 타일에 장애물 설정
        obstacle.obstaclePosition = nextPos; // 장애물의 위치 업데이트
    }

    // 주변 8칸 중 윗면과 같은 색이 몇개인지 카운팅하는 함수
    public int CountMatchingColors(Vector2Int position, TileColor targetColor, ref bool isDdongBlind)
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
                if (Board[checkPos.x, checkPos.y].Obstacle == ObstacleType.SlimeDdong)
                {
                    isDdongBlind = true;
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
                    case 5: Board[checkPos.x, checkPos.y].TileColor = TileColor.Brown; break;
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
                    case 5: Board[checkPos.x, checkPos.y].TileColor = TileColor.Brown; break;
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
            case DirectionType.ForwardOne:
                // 앞으로 한 칸
                Vector2Int forwardDir = currentPieceLastDirection;
                Vector2Int forwardPosition = gridPosition + forwardDir;
                if (IsValidPosition(forwardPosition))
                    positions.Add(forwardPosition);
                break;
        }

        return positions;
    }

    public List<Vector2Int> GetAllTilePositions()
    {

        List<Vector2Int> allList = new List<Vector2Int>();
        for (int x = 0; x <= 12; x++)
        {
            for (int y = 1; y <= 13; y++)
            {
                allList.Add(new Vector2Int(x, y));
            }
        }
        return allList;
    }

    public List<Vector2Int> GetCircleTilePositions()
    {
        List<Vector2Int> magicCircleList = new List<Vector2Int>();
        int centerX = 6;
        int centerY = 7;
        int radius = 5;
        float tolerance = 0.5f;

        // 원형 패턴
        for (int x = 0; x <= 12; x++)
        {
            for (int y = 1; y <= 13; y++)
            {
                float distance = Mathf.Sqrt(Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2));
                if (Mathf.Abs(distance - radius) < tolerance)
                {
                    magicCircleList.Add(new Vector2Int(x, y));
                }
            }
        }

        // 대각선 패턴
        for (int x = 0; x <= 12; x++)
        {
            int y1 = x + 1; // 주 대각선
            int y2 = 13 - x; // 부 대각선
            if (y1 >= 1 && y1 <= 13)
            {
                magicCircleList.Add(new Vector2Int(x, y1));
            }
            if (y2 >= 1 && y2 <= 13)
            {
                magicCircleList.Add(new Vector2Int(x, y2));
            }
        }

        // 추가: 십자형 패턴 (마법진 대칭성 강화)
        for (int x = 0; x <= 12; x++)
        {
            magicCircleList.Add(new Vector2Int(x, centerY)); // 수평선 (y = 7)
        }
        for (int y = 1; y <= 13; y++)
        {
            magicCircleList.Add(new Vector2Int(centerX, y)); // 수직선 (x = 6)
        }

        return magicCircleList;
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
                if (ObstacleManager.Instance.currentObstacles.Contains(obstacle.gameObject))
                {
                    ObstacleManager.Instance.currentObstacles.Remove(obstacle.gameObject);
                    Destroy(obstacle.gameObject);
                }
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
                if (ObstacleManager.Instance.currentObstacles.Contains(obstacle.gameObject))
                {
                    ObstacleManager.Instance.currentObstacles.Remove(obstacle.gameObject);
                    Destroy(obstacle.gameObject);
                }
                tile.Obstacle = ObstacleType.None;
            }
        }
    }

    public GameObject CreateObstacle(Vector2Int position, ObstacleType obstacleType)
    {
        if (position.x < 0 || position.x >= boardSize || position.y < 0 || position.y >= boardSizeY)
        {
            Debug.LogError("Position out of bounds");
            return null;
        }

        Tile tile = Board[position.x, position.y];
        if (tile.Obstacle != ObstacleType.None)
        {
            Debug.LogWarning($"Obstacle already exists at position ({position.x}, {position.y})");
            return null;
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

            return obstacle;
        }
        else
        {
            Debug.LogError($"No prefab found for ObstacleType: {obstacleType}");
            tile.Obstacle = ObstacleType.None; // 프리팹이 없으면 장애물 설정 취소
        }
        return null;
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

    public void ChangeGrayTiles(int count = 5)
    {
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(0, boardSize);
            int y = Random.Range(1, boardSize + 1);

            Board[x, y].SetTileColor(Color.gray);
            Board[x, y].TileColor = TileColor.Gray;
        }
    }

    public void ChangeGrayObstacles(StageData stageData)
    {
        int grayGrassCount = Random.Range(stageData.grayGrassCount.x, stageData.grayGrassCount.y + 1);
        int grayTreeCount = Random.Range(stageData.grayTreeCount.x, stageData.grayTreeCount.y + 1);
        int grayPoisonousherbCount = Random.Range(stageData.grayPoisonousherbCount.x, stageData.grayPoisonousherbCount.y + 1);

        foreach (GameObject obstacle in ObstacleManager.Instance.currentObstacles)
        {
            var obstacleComp = obstacle.GetComponent<Obstacle>();

            if (StageManager.Instance.currentStage.grayGrassCount != new Vector2Int(0, 0) &&
                obstacleComp.obstacleType == ObstacleType.Grass &&
                grayGrassCount > 0)
            {
                obstacle.GetComponent<Animator>().runtimeAnimatorController = ObstacleManager.Instance.GetGrayGrassAnimator();
                grayGrassCount--;
            }

            if (StageManager.Instance.currentStage.grayTreeCount != new Vector2Int(0, 0) &&
                obstacleComp.obstacleType == ObstacleType.Tree &&
                grayTreeCount > 0)
            {
                obstacle.GetComponent<Animator>().runtimeAnimatorController = ObstacleManager.Instance.GetGrayTreeAnimator();
                grayTreeCount--;
            }

            if (StageManager.Instance.currentStage.grayPoisonousherbCount != new Vector2Int(0, 0) &&
                obstacleComp.obstacleType == ObstacleType.PoisonousHerb &&
                grayPoisonousherbCount > 0)
            {
                obstacle.GetComponent<Animator>().runtimeAnimatorController = ObstacleManager.Instance.GetGrayPoisonousHerbAnimator();
                grayPoisonousherbCount--;
            }
        }
    }

    public void SetPawn()
    {
        for (int x = 0; x < boardSize; x += 2)
        {
            Vector2Int panwPos = new Vector2Int(x, boardSize);

            RemoveObstacleAtPosition(panwPos);
            GameObject obstacle = CreateObstacle(panwPos, ObstacleType.Pawn);

            // 미션을 위한 스테이지 상의 Pawn 리스트에 추가
            ObstacleManager.Instance.AddPawnToList(obstacle);
        }
    }
    public void SetRook(int height = 6)
    {
        // (0,6) (1,6) (0,7) (1,7) 왼쪽 룩 // 0.5 6.5
        // (11,6) (12,6) (11,7) (12,7) 오른쪽 룩 // 11.5 6.5
        int[] xPositions = { 0, 1, boardSize - 2, boardSize - 1 };

        for (int y = height; y <= height + 1; y++)
        {
            foreach (int x in xPositions)
            {
                Vector2Int rookPos = new Vector2Int(x, y);
                RemoveObstacleAtPosition(rookPos);
                CreateObstacle(rookPos, ObstacleType.Rook);

                Board[x, y].TileColor = TileColor.Gray;
                Board[x, y].SetTileColor(Color.gray);
            }
        }

        ObstacleManager.Instance.CreateVisibleRook(0.5f, 6.5f);
        ObstacleManager.Instance.CreateVisibleRook(11.5f, 6.5f);
    }

    public void SetKnight()
    {
        int count = 0;

        while (true)
        {
            int randX = Random.Range(0, boardSize);
            int randY = Random.Range(1, boardSizeY - 1);

            Vector2Int knightPos = new Vector2Int(randX, randY);

            // 이미 룩이나 나이트가 있으면 건너뛰기
            if (Board[knightPos.x, knightPos.y].Obstacle == ObstacleType.Rook ||
                Board[knightPos.x, knightPos.y].Obstacle == ObstacleType.Knight)
                continue;

            RemoveObstacleAtPosition(knightPos);
            GameObject obstacle = CreateObstacle(knightPos, ObstacleType.Knight);

            ObstacleManager.Instance.AddKnightToList(obstacle);

            ++count;

            if (count >= 2)
                break;
        }
    }

    public void SetHouse()
    {
        int count = 0;

        while (true)
        {
            int randX = Random.Range(1, boardSize - 1);
            int randY = Random.Range(3, boardSizeY - 2);

            Vector2Int housePos = new Vector2Int(randX, randY);

            // 이미 집이 있으면 스킵
            if (Board[housePos.x, housePos.y].Obstacle == ObstacleType.House)
                continue;

            RemoveObstacleAtPosition(housePos);
            GameObject obstacle = CreateObstacle(housePos, ObstacleType.House);

            ObstacleManager.Instance.AddHouseToList(obstacle);

            ++count;

            if (count >= 5)
                break;
        }
    }

    public bool IsMovementArea(Vector2Int position)
    {
        return position.y < boardSizeY - 1 && position.y > 0;
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
            case TileColor.Brown:
                tile.SetTileColor(tileColors[5]);
                break;
            case TileColor.Gray:
                tile.SetTileColor(tileColors[6]);
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


    // 보드 회전
    public void ShiftBoard()
    {
        StartCoroutine(DelectWhiteLineCoroutine());
    }

    IEnumerator DelectWhiteLineCoroutine()
    {
        for (int x = 0; x < boardSize; x++)
        {
            Board[x, 0].gameObject.SetActive(false); // 첫 줄을 비활성화
            Board[boardSize - 1 - x, boardSizeY - 1].gameObject.SetActive(false); // 마지막 줄을 비활성화
            yield return new WaitForSeconds(0.1f); // 0.1초 대기
        }

        StartCoroutine(ShiftBoardCoroutine());
    }

    IEnumerator ShiftBoardCoroutine()
    {
        float duration = 2f; // 이동 시간
        float time = 0f;
        float inflateAmount = 0.2f;
        GenerateNextBoard();

        // 새로운 보드의 맨 윗줄과 맨 아랫줄을 비활성화
        for (int x = 0; x < boardSize; x++)
        {
            TempBoard[x, 0].gameObject.SetActive(false);
            TempBoard[boardSize - 1 - x, boardSizeY - 1].gameObject.SetActive(false);
        }

        ObstacleManager.Instance.DropAlObstacles();

        Vector3 moveDir = Vector3.up;

        float boardHeight = 11f; // 보드의 높이

        newBoardTransform.localScale = Vector3.zero; // 새 보드의 초기 크기를 0으로 설정

        Vector3 contractStartPos = boardTransform.localPosition; // 기존 보드의 시작 위치를 아래로 이동
        Vector3 expandStartPos = contractStartPos + moveDir * boardHeight * 0.5f; // 새 보드의 시작 위치를 위로 이동

        newBoardTransform.localPosition = expandStartPos; // 새 보드의 시작 위치 설정

        Vector3 parentStartPos = boardPosParent.transform.localPosition;

        while (time < duration)
        {
            float t = time / duration;

            float totalScale = 1f + inflateAmount * Mathf.Sin(Mathf.PI * t);

            float contractScale = Mathf.Lerp(1f, 0f, t);
            float expandScale = totalScale - contractScale;

            newBoardTransform.localScale = new Vector3(1f, expandScale, 1f);
            boardTransform.localScale = new Vector3(1f, contractScale, 1f);

            float contractHalf = 0.5f * contractScale * boardHeight;
            float expandHalf = 0.5f * expandScale * boardHeight;

            boardTransform.localPosition = contractStartPos - moveDir * contractHalf;
            newBoardTransform.localPosition = expandStartPos - moveDir * expandHalf;

            boardPosParent.transform.localPosition = parentStartPos + moveDir * (contractHalf - expandHalf) + moveDir * (0.5f * boardHeight * t);

            time += Time.deltaTime;
            yield return null;
        }

        // 보드 위치와 크기를 최종적으로 설정
        newBoardTransform.localScale = Vector3.one; // 새 보드의 크기를 1로 설정
        newBoardTransform.localPosition = contractStartPos;
        boardPosParent.transform.localPosition = parentStartPos;

        // 새로운 보드를 기존 보드로 교체, 기존 보드 삭제
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSizeY; j++)
            {
                if (Board[i, j] != null)
                {
                    Destroy(Board[i, j].gameObject);
                }
            }
        }


        boardTransform.localPosition = contractStartPos; // 기존 보드의 위치를 새 보드의 위치로 설정
        boardTransform.localScale = Vector3.one; // 기존 보드의 크기를 1로 설정

        Board = TempBoard;
        TempBoard = new Tile[boardSize, boardSizeY]; // 새 보드 초기화

        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSizeY; j++)
            {
                if (Board[i, j] != null)
                {
                    Board[i, j].transform.SetParent(boardTransform);
                }
            }
        }

        StartCoroutine(ActivateNewBoardCoroutine());
    }

    IEnumerator ActivateNewBoardCoroutine()
    {
        for (int x = 0; x < boardSize; x++)
        {
            Board[x, 0].GetComponent<SpriteRenderer>().color = Color.white; // 첫 줄의 색상을 흰색으로 설정
            Board[x, 0].gameObject.SetActive(true);
            Board[boardSize - 1 - x, boardSizeY - 1].GetComponent<SpriteRenderer>().color = Color.white; // 첫 줄의 색상을 흰색으로 설정
            Board[boardSize - 1 - x, boardSizeY - 1].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f); // 0.1초 대기
        }

        StageManager.Instance.SetNewStage();
    }

    private void AddSpecialStageSetting()
    {
        // 특정 스테이지 미션에 따른 타일 및 장애물 세팅
        StageData stageData = StageManager.Instance.currentStage;

        if (stageData.stageNumber >= 5)
        {
            ChangeGrayTiles(stageData.grayTileCount);
            ChangeGrayObstacles(stageData);
        }
        if (stageData.stageNumber == 6)
        {
            SetPawn();
        }
        if (stageData.stageNumber == 9)
        {
            SetRook();
            SetKnight();
        }
        if (stageData.stageNumber == 10)
        {
            SetHouse();
        }
    }

    public void ClearBoard()
    {
        // 모든 타일 오브젝트 제거
        if (Board != null)
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSizeY; y++)
                {
                    if (Board[x, y] != null)
                    {
                        Destroy(Board[x, y].gameObject);
                        Board[x, y] = null;
                    }
                }
            }
        }

        // boardTransform의 모든 자식 오브젝트 제거
        if (boardTransform != null)
        {
            for (int i = boardTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(boardTransform.GetChild(i).gameObject);
            }
        }

        // 경계/배경 제거
        DestroyBorder();
        DestroyBackGround();
        Board = null;
        TempBoard = null;
    }
}
