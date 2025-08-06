using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainUIController : MonoBehaviour
{
    private const string KeySaveExists = "SaveExists";
    private const string KeyLastScene = "LastScene";
    private const string DefaultScene = "GameScene_2.0";

    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    void Start()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        newGameButton.onClick.AddListener(OnNewGameClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(() => Application.Quit());

        bool hasSave = PlayerPrefs.GetInt("SaveExists", 0) == 1;
        continueButton.interactable = hasSave;

        UpdateContinueButtonState();
        AudioManager.Instance.PlayBGM("MainBGM");
    }
    private void UpdateContinueButtonState()
    {
        bool hasSave = PlayerPrefs.GetInt(KeySaveExists, 0) == 1;
        continueButton.interactable = hasSave;
    }

    private void OnContinueClicked()
    {
        string lastScene = PlayerPrefs.GetString(KeyLastScene, DefaultScene);
        SceneManager.LoadScene(lastScene);
    }

    private void OnNewGameClicked()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt(KeySaveExists, 1);
        PlayerPrefs.SetString(KeyLastScene, DefaultScene);
        PlayerPrefs.Save();

        SceneManager.LoadScene(DefaultScene);
    }
    private void OnSettingsClicked()
    {
        UIManager.Instance?.ToggleSettings(true);
    }

    private void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
