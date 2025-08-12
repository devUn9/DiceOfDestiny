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


    private void onClickDiceRollButton()
    {
        if (GameManager.Instance.ActionPointManager.GameState == GameState.Dice)
        {
            GameManager.Instance.ActionPointManager.RollDice();
            return;
        }
    }

    public void onClickEndTurnButton()
    {
        GameManager.Instance.ActionPointManager.EndTurn();
    }
    public void Refresh()
    {
        var apm = GameManager.Instance.ActionPointManager;

        currentState.text = $"State : {apm.GameState}";
        currentTurn.text = $"Turn  : {apm.CurrentTurn}";
        Dice.text = $"Dice  : {apm.CurrentDiceValue}";
        AP.text = $"AP    : {apm.CurrentAP}";
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ActionPointManager.OnValueChanged -= Refresh;
    }
}
