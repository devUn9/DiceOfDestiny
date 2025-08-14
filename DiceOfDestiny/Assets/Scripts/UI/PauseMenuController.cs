using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public Button resumeButton;
    public Button settingsButton;
    public Button mainMenuButton;
    public Button exitButton;

    public void Initialize()
    {
        resumeButton.onClick.AddListener(ClosePauseMenu);
        settingsButton.onClick.AddListener(() => UIManager.Instance.ToggleSettings(true)); // 설정창 띄우기
        mainMenuButton.onClick.AddListener(() => { GameManager.Instance.UnPause(); SceneManager.LoadScene("MainScene"); }); // 메인씬 이동.
        exitButton.onClick.AddListener(() => Application.Quit()); // 프로그램 종료    
    }

    public void TogglePauseMenu()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void OpenPauseMenu()
    {
        gameObject.SetActive(true);
    }

    private void ClosePauseMenu()
    {
        GameManager.Instance.UnPause();
    }

}