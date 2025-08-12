using System;
using UnityEngine;

public static class RuleEvents
{
    public static event Action<string> OnRuleTriggered;

    public static void TriggerRule(string ruleId)
    {
        OnRuleTriggered?.Invoke(ruleId);
    }
}