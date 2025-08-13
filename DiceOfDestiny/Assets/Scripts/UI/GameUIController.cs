using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject actionPointUI;

    [SerializeField] private GameObject stageNameUI;

    public void Initialize()
    {
        pauseMenuUI.SetActive(false);
        pauseMenuUI.GetComponent<PauseMenuController>().Initialize();
    }

    public void SetStageName(string stageName)
    {
        stageNameUI.GetComponentInChildren<TextMeshProUGUI>().text = stageName;
    }
}
