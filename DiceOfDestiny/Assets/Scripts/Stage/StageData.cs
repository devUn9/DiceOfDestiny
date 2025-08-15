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
    public bool isGrayGrass = false;
    public bool isGrayTree = false;
    public bool isGrayPoisonousherb = false;

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
    [Range(0,1)] public float obstacleDensity = 0.2f;
}

[System.Serializable]
public class ObstacleWeight
{
    public ObstacleType type;
    [Range(1, 20)] public int weight;
}