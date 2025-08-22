using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    private int step = 0;

    public void Start()
    {
        ShowStep(step);
    }

    public void OnNextButtonClicked()
    {
        step++;
        if (step < 3)
        {
            ShowStep(step);
        }
        else
        {
            GameManager.Instance.SetFirstRunComplete();
            SceneManager.LoadScene("MainScene");
        }
    }

    private void ShowStep(int step)
    {
        switch (step)
        {
            case 0:
                // 기물 이동 안내 UI 활성화
                break;
            case 1:
                // 타일 색상에 따른 액티브 스킬 안내 UI 활성화
                break;
            case 2:
                // 기물 윗면 패시브 안내 UI 활성화
                break;
        }
    }
}