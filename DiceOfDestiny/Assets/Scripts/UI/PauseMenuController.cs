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
        settingButton.onClick.AddListener(); // 설정창 띄우기
        mainMenuButton.onClick.AddListener(); // 메인씬 이동.
        exitButton.onClick.AddListener(); // 프로그램 종료    
    }

    public void TogglePauseMenu()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void OpenPauseMenu()
    {
        gameObject.SetActive(true);
        raycastBlocker.SetActive(ture);
        pauseMenu.SetActive(false);
    }

    private void ClosePauseMenu()
    {
        gameObject.SetActive(false);
        GameManager.Instance.UnPause();
    }

}