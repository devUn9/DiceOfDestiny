using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Stage/StageData")]
public class StageData : ScriptableObject
{
    [field: SerializeField] public string StageName { get; private set; }
    public int stageNumber = 1;
    [Header("Stage Missions")]
    [SerializeReference] public List<MissionData> missions;
    public int maxTurn = 30;
    [Header("Gray Setting")]
    public Vector2Int grayGrassCount = new Vector2Int(0, 0);
    public Vector2Int grayTreeCount = new Vector2Int(0, 0);
    public Vector2Int grayPoisonousherbCount = new Vector2Int(0, 0);
    public int grayTileCount = 0;

    [Header("ColorSetting")]
    public int minimumColorEnsure;
    public int weightPower = 10; // 높을수록 가중치가 강하게 반영됨. 
    [Range(1, 20)] public int redWeight = 0;
    [Range(1, 20)] public int greenWeight = 0;
    [Range(1, 20)] public int blueWeight = 0;
    [Range(1, 20)] public int yellowWeight = 0;
    [Range(1, 20)] public int purpleWeight = 0;
    [Range(1, 20)] public int orangeWeight = 0;

    [Header("Obstacle Settings")]
    public List<ObstacleWeight> availableObstacle;
    public List<ObstacleWeight> exactObstacle;
    [Range(0, 1)] public float obstacleDensity = 0.2f;
}

[System.Serializable]
public class ObstacleWeight
{
    public ObstacleType type;
    [Range(1, 30)] public int weight;
}

public enum StormDirection
{
    Left,
    Right,
    Up,
    None
}