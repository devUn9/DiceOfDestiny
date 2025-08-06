using TMPro;
using UnityEngine;

// RequireComponent를 통해 실수 방지
[RequireComponent(typeof(TextMeshProUGUI))]
public sealed class StageNameDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageNameText;

    private void Awake()
    {
        // Inspector 연결 깜빡했을 때 대비
        if (stageNameText == null)
        {
            stageNameText = GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        StageManager.StageLoaded += UpdateStageName;

        if (StageManager.Instance != null && StageManager.Instance.currentStage != null)
        {
            UpdateStageName(StageManager.Instance.currentStage);
        }
    }


    private void OnDisable()
    {
        StageManager.StageLoaded -= UpdateStageName;
    }

    private void UpdateStageName(StageData stageData)
    {
        stageNameText.text = stageData?.StageName ?? string.Empty;
    }
}
