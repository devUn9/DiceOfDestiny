using UnityEditor.SceneManagement;
using UnityEngine;

public class UINextStage : MonoBehaviour
{
    public void NextStageButton()
    {
        // 다음 스테이지 보드판
        StageManager.Instance.TryLoadNextStage();

        // 게임 재개
        TempManager.Instance.ResumeGame();
    }
}
