using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject actionPointUI;

    public GameObject bannerPrefab;
    private GameObject bannerUI;

    [SerializeField] private GameObject stageNameUI;
    [SerializeField] private GameObject stageFailedUI;

    public void Initialize()
    {
        pauseMenuUI.SetActive(false);
        pauseMenuUI.GetComponent<PauseMenuController>().Initialize();

        bannerUI = Instantiate(bannerPrefab, transform, false);
        bannerUI.SetActive(false);
    }

    public void SetStageName(string stageName)
    {
        stageNameUI.GetComponentInChildren<TextMeshProUGUI>().text = stageName;
    }

    public void ShowBanner(int stageNumber, string stageName)
    {
        bannerUI.GetComponent<StageBannerController>().Show(stageNumber, stageName);
    }
    
    public void ShowStageFailedUI()
    {
        stageFailedUI.SetActive(true);
        stageFailedUI.GetComponent<StageFailedUI>().ShowUI();
    }
}
