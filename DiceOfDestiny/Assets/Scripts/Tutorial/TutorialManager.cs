using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : Singletone<TutorialManager>
{
    [SerializeField] private GameObject[] tutorialPanels; // 각 단계별 UI 패널
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Button nextButton;

    private int step = 0;

    private void Start()
    {
        // ShowStep(step);
        // nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    public void OnNextButtonClicked()
    {
        step++;
        if (step < tutorialPanels.Length)
        {
            ShowStep(step);
        }
        else
        {
            GameManager.Instance.SetFirstRunComplete();
            SceneManager.LoadScene("MainScene");
        }
    }

    public void ShowStep(int step)
    {
        for (int i = 0; i < tutorialPanels.Length; i++)
            tutorialPanels[i].SetActive(i == step);

        switch (step)
        {
            case 0:
                tutorialText = FindAnyObjectByType<TutorialUI>()?.GetComponentInChildren<TextMeshProUGUI>();
                tutorialText.text = "주사위를 굴린 뒤 마우스로 기물을 이동해보세요.";
                break;
            case 1:
                tutorialText.text = "타일 색과 윗면 색이 같으면 액티브 스킬이 발동됩니다!";
                // 색상 매칭 예시, 스킬 발동 연출
                break;
            case 2:
                tutorialText.text = "기물 윗면의 패시브 효과는 자동으로 적용됩니다.";
                // 패시브 효과 예시, 장애물 위 이동 등
                break;
        }
    }
}