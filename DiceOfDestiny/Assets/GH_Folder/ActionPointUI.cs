using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
        EndTurnButton.onClick.AddListener(onClickEndTurnButton);
        DiceRollButton.onClick.AddListener(onClickDiceRollButton);
    }
    private void Update()
    {
        Refresh();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.actionPointManager.OnValueChanged -= Refresh;
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
