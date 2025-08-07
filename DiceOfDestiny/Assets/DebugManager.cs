using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    [Header("Stage Debug Settings")]
    [SerializeField] private Button reColorBoardButton;
    [SerializeField] private Button regenerateButton;
    //[SerializeField] private Button nextStepButton;


    private void Start()
    {
        reColorBoardButton.onClick.AddListener(onClickReColorBoardButton);
        //nextStepButton.onClick.AddListener(onClickNextStepButton);
    }

    public void onClickReColorBoardButton()
    {
        StageManager.Instance.StartStage();
    }

    //public void onClickNextStepButton()
    //{
    //    ObstacleManager.Instance.UpdateObstacleStep();

    //    GameManager.Instance.actionPointManager.TurnOff();
    //}
}
