using System;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Hardware.DevDeviceList;

public class ActionPointUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentState;
    [SerializeField] private TextMeshProUGUI currentTurn;
    [SerializeField] private TextMeshProUGUI currentDice;
    [SerializeField] private TextMeshProUGUI currentAP;
    [SerializeField] private Button DiceRollButton;
    [SerializeField] private Button EndTurnButton;

    private Action<int> onAPChangedHandler;
    private Action onValueChangedHandler;
    private ActionPointManager apm;

    private void Start()
    {
        apm = GameManager.Instance != null ? GameManager.Instance.actionPointManager : null;


        EndTurnButton.onClick.AddListener(onClickEndTurnButton);
        DiceRollButton.onClick.AddListener(onClickDiceRollButton);

        // 이벤트 구독
        if (apm != null)
        {
            onAPChangedHandler = _ => Refresh();
            onValueChangedHandler = Refresh;

            apm.OnActionPointChanged += onAPChangedHandler;
            apm.OnValueChanged += onValueChangedHandler;

            Refresh();
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (apm != null)
        {
            if (onAPChangedHandler != null) apm.OnActionPointChanged -= onAPChangedHandler;
            if (onValueChangedHandler != null) apm.OnValueChanged -= onValueChangedHandler;
        }

        // 버튼 리스너 제거
        if (DiceRollButton != null) DiceRollButton.onClick.RemoveListener(onClickDiceRollButton);
        if (EndTurnButton != null) EndTurnButton.onClick.RemoveListener(onClickEndTurnButton);

        onAPChangedHandler = null;
        onValueChangedHandler = null;
        apm = null;
    }

    private void onClickDiceRollButton()
    {
        if (GameManager.Instance == null) return;
        var apm = GameManager.Instance.actionPointManager;
        if (apm == null) return;

        if (apm.GameState == GameState.Dice)
        {
            apm.RollDice();
            return;
        }        
    }

    public void onClickEndTurnButton()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.actionPointManager.EndTurn();
    }
    public void Refresh()
    {
        if (GameManager.Instance == null) return;
        var apm = GameManager.Instance.actionPointManager;
        if (apm == null) return;

        currentTurn.text = $"Turn   : {apm.CurrentTurn}";
        currentAP.text = $"AP    : {apm.CurrentAP}";
        currentState.text = $"State : {apm.GameState}";
        currentDice.text = $"Dice  : {apm.CurrentDiceValue}";
    }
}
