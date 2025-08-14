using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singletone<UIManager>
{
    [Header("UI Prefabs")]
    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject settingUIPrefab;
    [SerializeField] private GameObject dialogueUIPrefab;

    private Canvas currentCanvas; // 현재 캔버스 참조
    private GameObject currentUIRoot; // 현재 UI 루트 오브젝트

    private GameObject settingUI;
    private GameObject dialogueUI;

    public bool IsSettingUIOpen() => settingUI != null && settingUI.activeSelf;

    protected override void Awake()
    { 
        base.Awake();
    }

    public void InitializeMainUI()
    {
        currentCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        currentUIRoot = Instantiate(mainUI, currentCanvas.transform, false);
    }

    public void InitializeGameUI()
    {
        currentCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        currentUIRoot = Instantiate(gameUI, currentCanvas.transform, false);
        currentUIRoot.GetComponent<GameUIController>().Initialize();
    }

    public void ToggleSettings(bool isOn)
    {        
        if (settingUI == null)
        {
            settingUI = Instantiate(settingUIPrefab, currentCanvas.transform, false);
            return;
        }
        settingUI.SetActive(isOn);
    }

    public void ShowDialogue()
    {
        if (dialogueUI == null)
        {
            Debug.LogWarning("[UIManager] DialogueUI가 존재하지 않습니다.");
            return;
        }
        dialogueUI.SetActive(true);
    }

    public void TogglePauseMenu()
    {
        currentUIRoot.GetComponent<GameUIController>().pauseMenuUI.GetComponent<PauseMenuController>().TogglePauseMenu();
    }

    public void SetStageName(string stageName)
    {
        currentUIRoot.GetComponent<GameUIController>().SetStageName(stageName);
    }

    public void UpdateActionPointUI()
    {
        currentUIRoot.GetComponent<GameUIController>().actionPointUI.GetComponent<ActionPointUI>().UpdateActionPointUI();
    }
    public void ShowBanner(int stageNumber, string stageName)
    {
        currentUIRoot.GetComponent<GameUIController>().ShowBanner(stageNumber, stageName);
    }

    public void HideUI()
    {
        currentUIRoot.SetActive(false);
    }
    public void ShowUI()
    {

        currentUIRoot.SetActive(true);

    }
}
