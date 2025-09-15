using UnityEngine;
using UnityEngine.SceneManagement;

public class StageFailedUI : MonoBehaviour
{
    [SerializeField] private float displayDuration = 2.0f;

    public void ShowUI()
    {
        Invoke(nameof(GoLobby), displayDuration);
    }

    private void GoLobby()
    {
        SceneManager.LoadScene("MainScene");
        GameManager.Instance.UnPause(); // 게임 일시정지 해제
        StageManager.Instance.ResetCurrentStage(); // 스테이지 인덱스 초기화
        ObstacleManager.Instance.RemoveAllObstacle(); // 장애물 제거
        PieceManager.Instance.ResetPieces();
        BoardSelectManager.Instance.DestroyPieceHighlightTile();
        MissionManager.Instance.ResetMission();
    }
}
