//using UnityEngine;

//public class TestBoardManager : Singletone<TestBoardManager>
//{
//    // public static TestBoardManager Instance { get; private set; }

//    [SerializeField] private int boardSize = 13;
//    [SerializeField] private float gridSize = 1f; // 칸 크기
//    [SerializeField] private float offsetX = -5.5f; // 보드 오프셋 X
//    [SerializeField] private float offsetY = -5.5f; // 보드 오프셋 Y
//    [SerializeField] private ColorData[] possibleColors; // 가능한 색상 목록
//    [SerializeField] private Sprite squareSprite; // 스퀘어 스프라이트
//    private ColorData[,] board; // 2D 그리드 보드
//    private SpriteRenderer[,] boardRenderers; // 각 칸의 SpriteRenderer

//    void Awake()
//    {
//        // if (Instance == null)
//        // {
//        //     Instance = this;
//        //     DontDestroyOnLoad(gameObject);
//        // }
//        // else
//        // {
//        //     Destroy(gameObject);
//        //     return;
//        // }

//        // 보드 초기화
//        board = new ColorData[boardSize, boardSize];
//        boardRenderers = new SpriteRenderer[boardSize, boardSize];
//        InitializeBoard();
//        GenerateBoardVisuals();
//    }

//    private void InitializeBoard()
//    {
//        // 가능한 색상 목록이 없으면 기본 색상 추가
//        if (possibleColors == null || possibleColors.Length == 0)
//        {
//            possibleColors = new ColorData[]
//            {
//                new ColorData { color = Color.red },
//                new ColorData { color = Color.blue },
//                new ColorData { color = Color.green },
//                new ColorData { color = Color.yellow }
//            };
//        }

//        // 보드에 무작위 색상 할당
//        for (int x = 0; x < boardSize; x++)
//        {
//            for (int y = 0; y < boardSize; y++)
//            {
//                board[x, y] = new ColorData { color = possibleColors[Random.Range(0, possibleColors.Length)].color };
//            }
//        }
//    }

//    private void GenerateBoardVisuals()
//    {
//        // 기본 스퀘어 스프라이트가 없으면 생성
//        if (squareSprite == null)
//        {
//            Debug.LogWarning("Square sprite not assigned. Creating default sprite.");
//            Texture2D texture = new Texture2D(1, 1);
//            texture.SetPixel(0, 0, Color.white);
//            texture.Apply();
//            squareSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
//        }

//        // 각 칸에 SpriteRenderer 오브젝트 생성
//        for (int x = 0; x < boardSize; x++)
//        {
//            for (int y = 0; y < boardSize; y++)
//            {
//                GameObject square = new GameObject($"BoardSquare_{x}_{y}");
//                square.transform.SetParent(transform);
//                square.transform.position = new Vector3(
//                    x * gridSize + offsetX,
//                    y * gridSize + offsetY,
//                    0f
//                );

//                SpriteRenderer renderer = square.AddComponent<SpriteRenderer>();
//                renderer.sprite = squareSprite;
//                renderer.color = board[x, y].color;
//                boardRenderers[x, y] = renderer;
//            }
//        }
//    }

//    // 주사위 위치 주변 8칸의 색상 확인
//    public int CountMatchingColors(Vector2Int position, ColorData targetColor)
//    {
//        /*
//        if (targetColor == null)
//        {
//            Debug.LogError("Target ColorData is null!");
//            return 0;
//        }
//        */

//        int matchCount = 0;
//        Vector2Int[] directions = new Vector2Int[]
//        {
//            new Vector2Int(-1, -1), // 좌상
//            new Vector2Int(-1, 0),  // 좌
//            new Vector2Int(-1, 1),  // 좌하
//            new Vector2Int(0, -1),  // 상
//            new Vector2Int(0, 1),   // 하
//            new Vector2Int(1, -1),  // 우상
//            new Vector2Int(1, 0),   // 우
//            new Vector2Int(1, 1)    // 우하
//        };

//        foreach (Vector2Int dir in directions)
//        {
//            Vector2Int checkPos = position + dir;
//            if (checkPos.x >= 0 && checkPos.x < boardSize &&
//                checkPos.y >= 0 && checkPos.y < boardSize)
//            {
//                if (board[checkPos.x, checkPos.y] != null &&
//                    board[checkPos.x, checkPos.y].color == targetColor.color)
//                {
//                    matchCount++;
//                }
//            }
//        }

//        return matchCount;
//    }

//    public ColorData GetColorAt(Vector2Int position)
//    {
//        if (position.x >= 0 && position.x < boardSize &&
//            position.y >= 0 && position.y < boardSize)
//        {
//            return board[position.x, position.y];
//        }
//        return null;
//    }
//}