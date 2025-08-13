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

    public void Initialize()
    {

    }

    public void UpdateActionPointUI()
    {
        currentState.text = $"State : {StageManager.Instance.GameState}";
        currentTurn.text = $"Turn  : {StageManager.Instance.CurrentTurn}";
        Dice.text = $"Dice  : {StageManager.Instance.DiceValue}";
        AP.text = $"AP    : {ActionPointManager.Instance.GetAP().ToString()}";
    }
}
