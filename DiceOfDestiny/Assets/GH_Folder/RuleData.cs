using UnityEngine;

public enum RuleCategory
{
    Class,      // 직업
    Obstacle    // 장애물
}

[CreateAssetMenu(fileName = "NewRule", menuName = "Rulebook/Rule")]
public class RuleData : ScriptableObject
{
    [TextArea]
    public string ruleDescription;

    public int displayPriority; // 우선순위

    public RuleCategory category;

    public int triggerCount = 3;

    [HideInInspector]
    public int currentTriggerCount = 0;

    public bool isDiscovered = false; // 규칙이 발견되었나
}