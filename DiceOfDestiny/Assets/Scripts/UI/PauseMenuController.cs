using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    private GameObject pauseMenu;
    private Button resumeButton, settingsButton, mainMenuButton, exitButton;

    private Image raycastBlocker;

    public bool IsPaused => pauseMenu != null && pauseMenu.activeSelf;
    private void Awake()
    {
        // 항상 자식에서 pauseMenu 찾음
        pauseMenu = transform.Find("PauseMenu")?.gameObject
            ?? GetComponentInChildren<Canvas>(true)?.transform.Find("PauseMenu")?.gameObject;

        resumeButton = pauseMenu.transform.Find("ResumeButton").GetComponent<Button>();
        settingsButton = pauseMenu.transform.Find("SettingsButton").GetComponent<Button>();
        mainMenuButton = pauseMenu.transform.Find("MainMenuButton").GetComponent<Button>();
        exitButton = pauseMenu.transform.Find("ExitButton").GetComponent<Button>();

        CreateRaycastBlocker();

        pauseMenu.SetActive(false);

        resumeButton.onClick.AddListener(ClosePause);
        settingsButton.onClick.AddListener(() => UIManager.Instance.ToggleSettings(true));
        mainMenuButton.onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene("Main"); });
        exitButton.onClick.AddListener(() => Application.Quit());
    }
    private void CreateRaycastBlocker()
    {
        // PauseMenu와 같은 부모(RectTransform) 아래에 깔아줍니다.
        var parent = pauseMenu.transform.parent as RectTransform;

        var go = new GameObject("PauseRaycastBlocker",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        raycastBlocker = go.GetComponent<Image>();
        raycastBlocker.raycastTarget = true;
        raycastBlocker.color = new Color(0f, 0f, 0f, 0f); // 완전 투명
        raycastBlocker.gameObject.SetActive(false);
    }
    public void OpenPause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;

        raycastBlocker.transform.SetAsLastSibling();
        pauseMenu.transform.SetAsLastSibling();
        raycastBlocker.gameObject.SetActive(true);
    }

    public void ClosePause()
    {
        raycastBlocker.gameObject.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (IsPaused) ClosePause();
        else OpenPause();
    }
}