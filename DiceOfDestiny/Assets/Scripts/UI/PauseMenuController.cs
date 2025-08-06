using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    private GameObject pauseMenu;
    private Button resumeButton, settingsButton, mainMenuButton, exitButton;
    public bool IsPaused => pauseMenu.activeSelf;
    private void Awake()
    {
        // 항상 자식에서 pauseMenu 찾음
        pauseMenu = transform.Find("PauseMenu")?.gameObject
            ?? GetComponentInChildren<Canvas>(true)?.transform.Find("PauseMenu")?.gameObject;

        resumeButton = pauseMenu.transform.Find("ResumeButton").GetComponent<Button>();
        settingsButton = pauseMenu.transform.Find("SettingsButton").GetComponent<Button>();
        mainMenuButton = pauseMenu.transform.Find("MainMenuButton").GetComponent<Button>();
        exitButton = pauseMenu.transform.Find("ExitButton").GetComponent<Button>();

        pauseMenu.SetActive(false);

        resumeButton.onClick.AddListener(ClosePause);
        settingsButton.onClick.AddListener(() => UIManager.Instance.ToggleSettings(true));
        mainMenuButton.onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene("Main"); });
        exitButton.onClick.AddListener(() => Application.Quit());
    }

    public void OpenPause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ClosePause()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (IsPaused) ClosePause();
        else OpenPause();
    }
}