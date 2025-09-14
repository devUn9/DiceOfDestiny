using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSelectManager : Singletone<BoardSelectManager>
{
    [Header("이펙트 설정")]
    [SerializeField] private GameObject highlight; // 빛나는 이펙트 프리팹 (빈 타일용)
    [SerializeField] private GameObject notHighlight; // 빛나지 않는 이펙트 프리팹 (장애물 타일용)
    [SerializeField] private GameObject pieceHighlight; // 피스 선택용

    [Header("클릭된 타일")]
    [SerializeField] public Vector2Int lastClickedPosition; // 마지막 클릭된 타일 위치

    public bool isWaitingForClick { get; set; } = false; // 클릭 대기 상태, 타일 클릭 비동기적 반환

    // 장애물 타일 클릭 제한 여부 (true면 장애물 타일 클릭 불가, false면 가능)
    public bool restrictObstacle = true;

    // y값 0, 13 제한 트리거
    public bool restrictYBoundaries = true;

    private Dictionary<Vector2Int, GameObject> activeEffects; // 활성화된 이펙트 저장
    private List<GameObject> activePieceEffects = new List<GameObject>();
    private BoardManager boardManager;

    private void Start()
    {
        activeEffects = new Dictionary<Vector2Int, GameObject>();
        boardManager = BoardManager.Instance;
    }

    // 타일마다 장애물 여부에 따라 이펙트 적용 (악마 전용)
    public void HighlightTiles()
    {
        ClearAllEffects(); // 기존 이펙트 제거
        restrictObstacle = true; // 장애물 타일 클릭 제한

        for (int x = 0; x < boardManager.boardSize; x++)
        {
            for (int y = 1; y < boardManager.boardSize + 1; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                Tile tile = boardManager.GetTile(position);
                if (tile != null)
                {
                    // 하이라이트, 낫하이라이트 이미지 띄우기
                    GameObject effectPrefab = boardManager.IsEmptyTile(position) ? highlight : notHighlight;
                    // 이펙트 프리팹 인스턴스화
                    GameObject effect = Instantiate(effectPrefab,
                        new Vector3(boardManager.boardTransform.position.x + x,
                                  boardManager.boardTransform.position.y + y,
                                  -1), // z=-1로 타일 위에 렌더링
                        Quaternion.identity,
                        boardManager.boardTransform);
                    activeEffects.Add(position, effect);
                }
            }
        }
    }

    // 모든 타일에 하이라이트 이펙트 적용 (화가 전용)
    public void AllHighlightTiles()
    {
        ClearAllEffects(); // 기존 이펙트 제거
        restrictObstacle = false; // 장애물 타일 클릭 제한 해제

        for (int x = 0; x < boardManager.boardSize; x++)
        {
            for (int y = 1; y < boardManager.boardSize + 1; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                Tile tile = boardManager.GetTile(position);
                if (tile != null)
                {
                    // 모든 타일에 하이라이트 이펙트 적용
                    GameObject effectPrefab = highlight; // 항상 highlight 프리팹 사용
                                                         // 이펙트 프리팹 인스턴스화
                    GameObject effect = Instantiate(effectPrefab,
                        new Vector3(boardManager.boardTransform.position.x + x,
                                    boardManager.boardTransform.position.y + y,
                                    -1), // z=-1로 타일 위에 렌더링
                        Quaternion.identity,
                        boardManager.boardTransform);
                    activeEffects.Add(position, effect);
                }
            }
        }
    }

    // 한 타일의 주변 8칸을 하이라이트, 장애물 타일은 낫하이라이트 표시 (아기 전용)
    public void HighlightSurroundingTiles(Vector2Int centerPosition)
    {
        ClearAllEffects(); // 기존 이펙트 제거
        restrictObstacle = true; // 장애물 타일 클릭 제한
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // 중심 타일은 제외
                Vector2Int position = centerPosition + new Vector2Int(x, y);
                Tile tile = boardManager.GetTile(position);
                if (tile != null)
                {
                    // 하이라이트, 낫하이라이트 이미지 띄우기
                    GameObject effectPrefab = boardManager.IsEmptyTile(position) ? highlight : notHighlight;
                    // 이펙트 프리팹 인스턴스화
                    GameObject effect = Instantiate(effectPrefab,
                        new Vector3(boardManager.boardTransform.position.x + position.x,
                                    boardManager.boardTransform.position.y + position.y,
                                    -1), // z=-1로 타일 위에 렌더링
                        Quaternion.identity,
                        boardManager.boardTransform);
                    activeEffects.Add(position, effect);
                }
            }
        }
    }


    public void StartHighlightTiles()
    {
        ClearAllEffects(); // 기존 이펙트 제거
        restrictObstacle = true; // 장애물 타일 클릭 제한

        for (int x = 0; x < boardManager.boardSize; x++)
        {
            for (int y = 0; y < 1; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                Tile tile = boardManager.GetTile(position);
                if (tile != null)
                {
                    // 하이라이트, 낫하이라이트 이미지 띄우기
                    GameObject effectPrefab = boardManager.IsEmptyTile(position) ? highlight : notHighlight;
                    // 이펙트 프리팹 인스턴스화
                    GameObject effect = Instantiate(effectPrefab,
                        new Vector3(boardManager.boardTransform.position.x + x,
                                  boardManager.boardTransform.position.y + y,
                                  -1), // z=-1로 타일 위에 렌더링
                        Quaternion.identity,
                        boardManager.boardTransform);
                    activeEffects.Add(position, effect);
                }
            }
        }
    }

    // 피스 선택 타일 테두리 생성
    public void PieceHighlightTiles(Vector2Int pos)
    {
        DestroyPieceHighlightTile();

        GameObject pieceEffectPrefab = pieceHighlight;

        // 하이라이트 타일 생성 및 리스트에 추가
        GameObject effect = Instantiate(pieceEffectPrefab, new Vector3(boardManager.boardTransform.position.x + pos.x,
            boardManager.boardTransform.position.y + pos.y, -1),
            Quaternion.identity, boardManager.boardTransform);
        activePieceEffects.Add(effect);
    }

    // 다중 피스 선택 타일 테두리 생성
    public void PieceHighLightTilesMulty(Vector2Int pos)
    {
        GameObject pieceEffectPrefab = pieceHighlight;

        // 하이라이트 타일 생성 및 리스트에 추가
        GameObject effect = Instantiate(pieceEffectPrefab, new Vector3(boardManager.boardTransform.position.x + pos.x,
            boardManager.boardTransform.position.y + pos.y, -1),
            Quaternion.identity, boardManager.boardTransform);
        activePieceEffects.Add(effect);
    }
    public void DestroyPieceHighlightTile()
    {
        foreach (var effect in activePieceEffects)
        {
            if (effect != null)
                Destroy(effect);
        }
        activePieceEffects.Clear(); // 리스트 초기화
    }

    // 모든 이펙트 제거
    public void ClearAllEffects()
    {
        foreach (var effect in activeEffects.Values)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        activeEffects.Clear();
    }

    // 클릭된 타일의 위치를 비동기적으로 반환
    public IEnumerator WaitForTileClick()
    {
        isWaitingForClick = true;
        lastClickedPosition = new Vector2Int(-1,-1);

        while (isWaitingForClick)
        {
            yield return null;
        }

        yield return lastClickedPosition;
    }

    // 장애물 타일 클릭 제한 설정
    //public void SetRestrictToEmptyTiles(bool restrict)
    //{
    //    restrictObstacle = restrict;
    //}

    public void SetClickedTilePosition(Vector2Int position)
    {
        // restrictToEmptyTiles가 true일 때만 장애물 타일 확인
        if (restrictObstacle && !boardManager.IsEmptyTile(position))
        {
            Debug.Log($"장애물이 있는 타일({position})은 클릭할 수 없습니다.");
            // UI 피드백 호출

            return;
        }

        lastClickedPosition = position;
        isWaitingForClick = false;
        
        ClearAllEffects();
    }

    // 현재 클릭된 타일의 transform 위치를 반환
    public Transform GetClickedTileTransform()
    {
        if (lastClickedPosition == Vector2Int.zero)
        {
            Debug.LogWarning("클릭된 타일 위치가 설정되지 않았습니다.");
            return null;
        }
        Tile clickedTile = boardManager.GetTile(lastClickedPosition);
        if (clickedTile != null)
        {
            return clickedTile.transform;
        }
        else
        {
            Debug.LogWarning($"클릭된 타일({lastClickedPosition})이 존재하지 않습니다.");
            return null;
        }
    }


}