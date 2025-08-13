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
        settingsButton.onClick.AddListener(() => UIManager.Instance.ToggleSettings(true));
        mainMenuButton.onClick.AddListener(() => {SceneManager.LoadScene("Main"); });
        exitButton.onClick.AddListener(() => Application.Quit());
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
        gameObject.SetActive(false);
        GameManager.Instance.UnPause();
    }
}