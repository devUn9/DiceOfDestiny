using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RulebookUI : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button ruleBookOpenCloseButton;
    [SerializeField] private Image ruleBookImage;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite clickedSprite;
    [SerializeField] private GameObject scrollView;

    [Header("Rulebook UI")]
    [SerializeField] private GameObject ClassRule;
    [SerializeField] private GameObject ObstacleRule;

    [SerializeField] private GameObject RuleTextPrefab;

    private bool isOpen = false;

    private void Start()
    {
        // 초기 상태 동기화
        SetOpen(true);

        // 버튼 리스너 연결
        if (ruleBookOpenCloseButton != null)
            ruleBookOpenCloseButton.onClick.AddListener(ToggleRuleBook);

        // Refresh 함수 구독
        EventManager.Instance.AddListener<RuleData>("ShowRule", ShowRule);
    }
    private void ToggleRuleBook()
    {
        SetOpen(!isOpen);
    }

    private void SetOpen(bool open)
    {
        isOpen = open;
        if (scrollView != null)
            scrollView.SetActive(isOpen);
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (ruleBookImage != null)
            ruleBookImage.sprite = isOpen ? defaultSprite : clickedSprite;
    }

    // 기존 onClick 메서드는 제거(혹은 아래처럼 안전하게 변경)
    public void onClickRuleBookOpenCloseButton()
    {
        ToggleRuleBook();
    }

    public void ShowRule(RuleData rule)
    {
        switch (rule.category)
        {
            case RuleCategory.Class:
                CreateRuleText(rule, ClassRule.transform);
                break;
            case RuleCategory.Obstacle:
                CreateRuleText(rule, ObstacleRule.transform);
                break;
        }
    }

    private void CreateRuleText(RuleData rule, Transform categoryTransform)
    {
        GameObject prefab = Instantiate(RuleTextPrefab, categoryTransform);
        var ruleTextUI = prefab.GetComponent<RuleText>();
        ruleTextUI.Initialize(rule);

        SortChildrenByPriority(categoryTransform);

        LayoutRebuilder.ForceRebuildLayoutImmediate(categoryTransform.GetComponent<RectTransform>());
    }

    private void SortChildrenByPriority(Transform parent)
    {
        List<Transform> children = new();

        foreach (Transform child in parent)
        {
            children.Add(child);
        }

        // displayPriority 기준으로 정렬 (작은 값이 위에 오도록)
        children.Sort((a, b) =>
        {
            var aPriority = a.GetComponent<RuleText>()?.ruleData.displayPriority ?? 0;
            var bPriority = b.GetComponent<RuleText>()?.ruleData.displayPriority ?? 0;
            return aPriority.CompareTo(bPriority);
        });

        // 정렬된 순서대로 SetSiblingIndex
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }
}