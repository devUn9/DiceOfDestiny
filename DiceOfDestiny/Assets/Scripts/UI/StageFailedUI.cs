using UnityEngine;

public class StageFailedUI : MonoBehaviour
{
    [SerializeField] private float displayDuration = 2.0f;

    public void ShowUI()
    {
        Invoke(nameof(GoLobby), displayDuration);
    }

    private void GoLobby()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}
