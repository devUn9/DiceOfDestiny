using UnityEngine;
using TMPro;

public class RuleText : MonoBehaviour
{
    public RuleData ruleData { get; private set; }
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void Initialize(RuleData rule)
    {
        ruleData = rule;
        text.text = rule.ruleDescription;
    }
}
