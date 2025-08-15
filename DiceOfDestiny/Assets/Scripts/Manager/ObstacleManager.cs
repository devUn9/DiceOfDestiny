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

    public Dictionary<ObstacleType, GameObject> obstaclePrefabs;

    public List<GameObject> currentObstacles;

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
        for (int i = currentObstacles.Count - 1; i >= 0; i--)
        {
            var behaviour = currentObstacles[i].GetComponent<IObstacleBehaviour>();

            if (behaviour != null)
                behaviour.DoLogic();
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

}
    
