using System;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

public class ActionPointUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentState;
    [SerializeField] private TextMeshProUGUI currentTurn;
    [SerializeField] private TextMeshProUGUI Dice;
    [SerializeField] private TextMeshProUGUI AP;
    [SerializeField] private Button DiceRollButton;
    [SerializeField] private Button EndTurnButton;

    private void Start()
    {
        var apm = GameManager.Instance.actionPointManager;

        EndTurnButton.onClick.AddListener(onClickEndTurnButton);
        DiceRollButton.onClick.AddListener(onClickDiceRollButton);

        apm.OnActionPointChanged += _ => Refresh();
        apm.OnValueChanged += Refresh;

        Refresh();   // 초기 출력
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        var apm = GameManager.Instance.actionPointManager;
        apm.OnActionPointChanged -= _ => Refresh();
        apm.OnValueChanged -= Refresh;
    }
    private void onClickDiceRollButton()
    {
        if (GameManager.Instance.actionPointManager.GameState == GameState.Dice)
        {
            GameManager.Instance.actionPointManager.RollDice();
            return;
        }        
    }

    public void onClickEndTurnButton()
    {
        ObstacleManager.Instance.UpdateObstacleStep();

        GameManager.Instance.actionPointManager.EndTurn();
    }
    public void Refresh()
    {
        var apm = GameManager.Instance.actionPointManager;

        currentState.text = $"State : {apm.GameState}";
        currentTurn.text = $"Turn  : {apm.CurrentTurn}";
        Dice.text = $"Dice  : {apm.CurrentDiceValue}";
        AP.text = $"AP    : {apm.CurrentAP}";
    }
}
