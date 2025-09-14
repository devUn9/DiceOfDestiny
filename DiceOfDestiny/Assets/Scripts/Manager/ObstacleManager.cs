using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObstacleManager : Singletone<ObstacleManager>
{
    [Header("Obstacle Prefab")]
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject lionPrefab;
    [SerializeField] private GameObject puddlePrefab;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject poisonousHerbPrefab;
    [SerializeField] private GameObject grassPrefab;
    [SerializeField] private GameObject slimePrefab;
    [SerializeField] private GameObject slimeDdongPrefab;

    [SerializeField] private GameObject pawnPrefab;
    [SerializeField] private GameObject rookPrefab;
    [SerializeField] private GameObject knightPrefab;
    [SerializeField] private GameObject woodBoxPrefab;

    [SerializeField] private GameObject housePrefab;

    [SerializeField] private RuntimeAnimatorController grayGarassAnimator;
    [SerializeField] private RuntimeAnimatorController grayTreeAnimator;
    [SerializeField] private RuntimeAnimatorController grayPoisonousHerbAnimator;

    public Dictionary<ObstacleType, GameObject> obstaclePrefabs;

    public List<GameObject> currentObstacles;

    [Header("Boss Pawn")]
    private List<GameObject> pawnList = new List<GameObject>();
    public int pawnMoveIndex { get; private set; } = 0;

    [Header("Boss Rook")]
    [SerializeField] private GameObject rookVisual;
    private List<GameObject> rookVisualList = new List<GameObject>();

    [Header("Boss Knight")]
    private List<GameObject> knightList = new List<GameObject>();

    [Header("House")]
    private List<GameObject> houseList = new List<GameObject>();

    public void Initialize()
    {
        obstaclePrefabs = new Dictionary<ObstacleType, GameObject>
        {
            { ObstacleType.Zombie, zombiePrefab },
            { ObstacleType.Tree, treePrefab },
            { ObstacleType.Rock, rockPrefab },
            { ObstacleType.Lion, lionPrefab },
            { ObstacleType.Puddle, puddlePrefab },
            { ObstacleType.Chest, chestPrefab },
            { ObstacleType.PoisonousHerb, poisonousHerbPrefab },
            { ObstacleType.Grass, grassPrefab },
            { ObstacleType.Slime, slimePrefab },
            { ObstacleType.SlimeDdong, slimeDdongPrefab },
            { ObstacleType.Pawn, pawnPrefab },
            { ObstacleType.Rook, rookPrefab },
            { ObstacleType.Knight, knightPrefab },
            { ObstacleType.WoodBox, woodBoxPrefab },
            { ObstacleType.House, housePrefab },
        };

        currentObstacles = new List<GameObject>();
    }
    public void SetObstacle(GameObject obstacle)
    {
        currentObstacles.Add(obstacle);
    }

    public void RemoveAllObstacle()
    {
        foreach (GameObject obstacle in currentObstacles)
        {
            Destroy(obstacle);
        }
        currentObstacles.Clear();

        RemoveSpecialObstacle(rookVisualList);
        RemoveSpecialObstacle(knightList);
        RemoveSpecialObstacle(pawnList);
        RemoveSpecialObstacle(houseList);
    }

    public void DropAlObstacles()
    {
        foreach (GameObject obstacle in currentObstacles)
        {
            StartCoroutine(DropGameObject(obstacle));
        }
        currentObstacles.Clear();
    }

    public void UpdateObstacleStep()
    {
        if (pawnList.Count > 0)
        {
            // 폰 난수 얻기
            InOrderToMovePawn();
        }

        for (int i = currentObstacles.Count - 1; i >= 0; i--)
        {
            var behaviour = currentObstacles[i].GetComponent<IObstacleBehaviour>();

            if (behaviour != null)
            {
                behaviour.DoLogic();
            }
        }
    }

    IEnumerator DropGameObject(GameObject gameObject)
    {
        float dropTime = 4f;
        float timeElapsed = 0f;

        float speed = 0f;

        Vector3 startPosition = gameObject.transform.localPosition;

        while (timeElapsed < dropTime)
        {

            float t = timeElapsed / dropTime;

            if (t > 0.39f)
            {
                // 중력 가속도 적용
                gameObject.transform.SetParent(this.transform);
                speed += 9.8f * Time.deltaTime; // 중력 가속도 적용
                gameObject.transform.localPosition += Vector3.down * speed * Time.deltaTime; // 속도 조절            
            }
            else
            {
                gameObject.transform.localPosition = startPosition - new Vector3(0, t * 2f, 0);
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        // 드랍이 완료되면 게임 오브젝트를 제거
        Destroy(gameObject);
        yield return null;
    }

    // 보스 폰 함수들
    public void AddPawnToList(GameObject pawn)
    {
        if (pawn != null && !pawnList.Contains(pawn))
        {
            pawnList.Add(pawn);
        }
    }

    public void RemovePawnToList(GameObject pawn)
    {
        pawnList.Remove(pawn);

        MissionManager.Instance.AlivePawnCountCheck(); // 폰이 죽었을 때 미션 카운트 감소
    }

    public void DeathPawn(Vector2Int gridPos)
    {
        if (BoardManager.Instance.Board[gridPos.x, gridPos.y].Obstacle == ObstacleType.Pawn)
        {
            // 폰 리스트 상에서의 오브젝트 제거
            Obstacle pawn = BoardManager.Instance.ReturnObstacleByPosition(gridPos);
            RemovePawnToList(pawn.gameObject);

            // 현재 장애물 목록에서의 폰 삭제
            // 실제 폰 오브젝트 삭제
            // 타일에 장애물 타입 None으로 설정
            BoardManager.Instance.RemoveObstacleAtPosition(gridPos);
        }
    }

    public void HitPawn(Vector2Int gridPos)
    {
        if (BoardManager.Instance.Board[gridPos.x, gridPos.y].Obstacle == ObstacleType.Pawn)
        {
            Obstacle pawn = BoardManager.Instance.ReturnObstacleByPosition(gridPos);

            PawnBehaviour pawnBehaviour = pawn as PawnBehaviour;

            if (pawnBehaviour != null)
            {
                pawnBehaviour.TakeDamage(1);
            }
            else
            {
                Debug.LogWarning($"PawnBehaviour not found at position {gridPos}");
            }
        }
    }

    public int GetPawnListIndex(GameObject pawn)
    {
        return pawnList.IndexOf(pawn);
    }

    public void InOrderToMovePawn()
    {
        int pawnRandomIndex = Random.Range(0, pawnList.Count);
        pawnMoveIndex = pawnRandomIndex;
    }

    // 룩 4칸 그림 생성
    public void CreateVisibleRook(float _x, float _y)
    {
        GameObject rook = Instantiate(rookVisual,
            new Vector3(BoardManager.Instance.boardTransform.position.x + _x, BoardManager.Instance.boardTransform.position.y + _y, 0),
            Quaternion.identity,
            BoardManager.Instance.boardTransform);

        rookVisualList.Add(rook);
    }

    // 보스 나이트 함수들
    public void AddKnightToList(GameObject knight)
    {
        if (knight != null && !knightList.Contains(knight))
        {
            knightList.Add(knight);
        }
    }

    public void RemoveKnightToList(GameObject knight)
    {
        knightList.Remove(knight);

        MissionManager.Instance.AliveKnightCountCheck(); // 나이트가 죽었을 때 미션 카운트 감소
    }

    // 집 함수들
    public void AddHouseToList(GameObject knight)
    {
        if (knight != null && !knightList.Contains(knight))
        {
            houseList.Add(knight);
        }
    }
    public void RemoveHouseToList(GameObject pawn)
    {
        houseList.Remove(pawn);

        MissionManager.Instance.AliveHouseCountCheck(); // 집이 파괴되었을 때 미션 카운트
    }

    public void DestroyHouse(Vector2Int gridPos)
    {
        if (BoardManager.Instance.Board[gridPos.x, gridPos.y].Obstacle == ObstacleType.House)
        {
            // 집 리스트 상에서의 오브젝트 제거
            Obstacle house = BoardManager.Instance.ReturnObstacleByPosition(gridPos);
            RemoveHouseToList(house.gameObject);

            // 현재 장애물 목록에서의 집 삭제
            // 실제 집 오브젝트 삭제
            // 타일에 장애물 타입 None으로 설정
            BoardManager.Instance.RemoveObstacleAtPosition(gridPos);
        }
    }

    public void HitHouse(Vector2Int gridPos)
    {
        if (BoardManager.Instance.Board[gridPos.x, gridPos.y].Obstacle == ObstacleType.House)
        {
            Obstacle house = BoardManager.Instance.ReturnObstacleByPosition(gridPos);

            HouseBehaviour houseBehaviour = house as HouseBehaviour;

            if (houseBehaviour != null)
            {
                houseBehaviour.TakeDamage(1);
            }
            else
            {
                Debug.LogWarning($"PawnBehaviour not found at position {gridPos}");
            }
        }
    }

    public void HouseListToLogicTurn()
    {
        foreach (var house in houseList)
        {
            if (house == null)
                continue;

            house.GetComponent<HouseBehaviour>().DoLogicTurn();
        }
    }

    public void RemoveSpecialObstacle(List<GameObject> specialObstacle)
    {
        if (specialObstacle.Count == 0)
            return;

        // 특별한 장애물 리스트에 있는 모든 오브젝트 제거
        foreach (GameObject obstacle in specialObstacle)
        {
            Destroy(obstacle);
        }

        specialObstacle.Clear();
    }

    public RuntimeAnimatorController GetGrayGrassAnimator()
    {
        return grayGarassAnimator;
    }
    public RuntimeAnimatorController GetGrayTreeAnimator()
    {
        return grayTreeAnimator;
    }
    public RuntimeAnimatorController GetGrayPoisonousHerbAnimator()
    {
        return grayPoisonousHerbAnimator;
    }
}

