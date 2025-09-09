using UnityEngine;
using System.Collections.Generic;

public class RuleRuntimeData
{
    public RuleData ruleData;
    public int currentTriggerCount;
    public bool isDiscovered;

    public RuleRuntimeData(RuleData ruleData)
    {
        this.ruleData = ruleData;
        currentTriggerCount = 0;
        isDiscovered = false;
    }
}

public class RuleManager : MonoBehaviour
{
    [SerializeField]
    private List<RuleData> allRules;

    private Dictionary<string, RuleRuntimeData> ruleRuntimeLookup = new();

    private void Awake()
    {
        // ruleId는 ScriptableObject 이름으로 사용한다고 가정
        foreach (var rule in allRules)
        {
            if (rule.isClear)
            {
                EventManager.Instance.TriggerEvent("ShowRule", rule);
                continue;
            }

            var runtimeData = new RuleRuntimeData(rule);
            ruleRuntimeLookup[rule.name] = runtimeData;
        }
    }

    private void OnEnable()
    {
        RuleEvents.OnRuleTriggered += HandleRuleTrigger;
    }

    private void OnDisable()
    {
        RuleEvents.OnRuleTriggered -= HandleRuleTrigger;
    }

    private void HandleRuleTrigger(string ruleId)
    {
        if (!ruleRuntimeLookup.TryGetValue(ruleId, out RuleRuntimeData rule))
        {
            Debug.LogWarning($"[RuleManager] 존재하지 않는 ruleId: {ruleId}");
            return;
        }

        if (ruleRuntimeLookup[ruleId].isDiscovered)
            return;

        ruleRuntimeLookup[ruleId].currentTriggerCount++;

        if (ruleRuntimeLookup[ruleId].currentTriggerCount >= rule.ruleData.triggerCount)
        {
            ruleRuntimeLookup[ruleId].isDiscovered = true;
            RevealRule(rule.ruleData);
        }
    }

    private void RevealRule(RuleData rule)
    {
        Debug.Log($"[규칙 발견] {rule.name} - {rule.ruleDescription}");
        EventManager.Instance.TriggerEvent("ShowRule", rule);

        rule.isClear = true;
    }
}