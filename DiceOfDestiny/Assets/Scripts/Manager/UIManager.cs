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

    public void AttachExistingMainUI(GameObject uiRoot)
    {
        if (uiRoot == null)
        {
            Debug.LogError("[UIManager] AttachExistingMainUI: uiRoot is null.");
            return;
        }

        var canvas = uiRoot.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[UIManager] 캔버스를 찾지 못했습니다. 씬에 'Canvas'가 존재하는지 확인해 주세요.");
                return;
            }
        }
        var root = uiRoot.transform.root.gameObject;
        var wasInactive = !root.activeSelf;
        if (wasInactive) root.SetActive(true);

        var uiManager = this; // 가독성용
        var uiManagerType = typeof(UIManager);
        var canvasField = uiManagerType.GetField("currentCanvas", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var rootField = uiManagerType.GetField("currentUIRoot", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        canvasField?.SetValue(uiManager, canvas);
        rootField?.SetValue(uiManager, root);
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
        if (currentCanvas == null)
        {
            currentCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
            if (currentCanvas == null) return;            
        }

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

    public void ShowStageFailedUI()
    {
        currentUIRoot.GetComponent<GameUIController>().ShowStageFailedUI();
    }

    public void InitMissionUI()
    {
        currentUIRoot.GetComponent<GameUIController>().InitMissionUI();
    }

    public void UpdateMissionUI()
    {
        currentUIRoot.GetComponent<GameUIController>().UpdateMissionUI();
    }

    public void ShowMissionUI()
    {
        currentUIRoot.GetComponent<GameUIController>().ShowMissionUI();
    }
    public void HideMissionUI()
    {
        currentUIRoot.GetComponent<GameUIController>().HideMissionUI();
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
