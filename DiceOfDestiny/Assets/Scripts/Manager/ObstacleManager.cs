using System.Collections;
using System.Collections.Generic;
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
            { ObstacleType.WoodBox, woodBoxPrefab }
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

    public void CreateVisibleRook(float _x, float _y)
    {
        Instantiate(rookVisual,
            new Vector3(BoardManager.Instance.boardTransform.position.x + _x, BoardManager.Instance.boardTransform.position.y + _y, 0),
            Quaternion.identity,
            BoardManager.Instance.boardTransform);
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
    
