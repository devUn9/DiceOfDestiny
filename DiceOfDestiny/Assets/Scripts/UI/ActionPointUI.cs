using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionPointUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI currentStage;
    [SerializeField] private TextMeshProUGUI currentState;
    [SerializeField] private TextMeshProUGUI currentTurn;
    [SerializeField] private TextMeshProUGUI diceText;
    [SerializeField] private TextMeshProUGUI apText;

    [Header("Buttons")]
    [SerializeField] private Button diceRollButton;
    [SerializeField] private Button endTurnButton;

    [Header("AP Slots Root")]
    [SerializeField] private Transform apSlotsRoot;
    [SerializeField] private string apGaugeChildName = "APGauge";

    [Header("AP Fill Animation")]
    [SerializeField] private bool animateFillOnIncrease = true;
    [SerializeField] private float fillStepDelay = 1.02f;
    [SerializeField] private bool playSfxEachStep = false;
    [SerializeField] private string stepSfxName = "AP_Tick";

    private readonly List<GameObject> apGauges = new();

    // 표시 중인 AP(시각적 수치)
    private int displayedAP = 0;

    // 코루틴
    private Coroutine fillRoutine;

    private void Awake()
    {
        apGauges.Clear();
        if (apSlotsRoot == null)
        {
            Debug.LogError("[ActionPointUI] apSlotsRoot 미할당");
            return;
        }

        for (int i = 0; i < apSlotsRoot.childCount; i++)
        {
            var slot = apSlotsRoot.GetChild(i);
            var gaugeTr = slot.Find(apGaugeChildName);
            if (gaugeTr == null)
            {
                Debug.LogWarning($"[ActionPointUI] '{slot.name}' 안에 '{apGaugeChildName}'가 없습니다.");
                continue;
            }
            apGauges.Add(gaugeTr.gameObject);
        }
    }

    private void OnEnable()
    {
        diceRollButton.onClick.AddListener(onClickDiceRollButton);
        endTurnButton.onClick.AddListener(onClickEndTurnButton);
    }

    private void onClickDiceRollButton()
    {
        StageManager.Instance.RollDice();
    }

    private void onClickEndTurnButton()
    {
        StageManager.Instance.EndTurn();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // UI 갱신 / 애니메이션
    // ─────────────────────────────────────────────────────────────────────────────

    public void UpdateActionPointUI()
    {
        if (StageManager.Instance == null || StageManager.Instance.currentStage == null)
        {
            currentStage.text = "스테이지 -";
            currentState.text = "State : -";
            currentTurn.text = "- / -";
            diceText.text = "Dice  : -";

            int apVal = 0;
            if (ActionPointManager.Instance != null)
                apVal = ActionPointManager.Instance.GetAP();

            apText.text = $"AP    : {apVal}";

            // 버튼 비활성 (안전상)
            if (diceRollButton != null) diceRollButton.interactable = false;
            if (endTurnButton != null) endTurnButton.interactable = false;

            // 게이지 즉시 갱신
            OnActionPointChanged(apVal);
            return;
        }
        currentStage.text = $"스테이지 {StageManager.Instance.currentStage.stageNumber}";
        currentState.text = $"State : {StageManager.Instance.GameState}";
        currentTurn.text = $"{StageManager.Instance.CurrentTurn} / {StageManager.Instance.currentStage.maxTurn}";
        diceText.text = $"Dice  : {StageManager.Instance.DiceValue}";
        apText.text = $"AP    : {ActionPointManager.Instance.GetAP()}";

        diceRollButton.interactable = (StageManager.Instance.GameState == GameState.ReadyToRoll);
        endTurnButton.interactable = (StageManager.Instance.GameState == GameState.PlayerAction);

        OnActionPointChanged(ActionPointManager.Instance.GetAP());
    }

    public void OnActionPointChanged(int ap)
    {
        int targetAP = Mathf.Clamp(ap, 0, apGauges.Count);

        // 감소 또는 애니메이션 비활성: 즉시 반영
        if (!animateFillOnIncrease || targetAP <= displayedAP)
        {
            if (fillRoutine != null)
            {
                StopCoroutine(fillRoutine);
                fillRoutine = null;
            }
            displayedAP = targetAP;
            SetGaugesImmediate(displayedAP);
            apText.text = $"AP    : {displayedAP}";
            return;
        }

        // 증가 시: 순차 점등
        if (fillRoutine != null) StopCoroutine(fillRoutine);
        fillRoutine = StartCoroutine(FillTo(targetAP));
    }

    private IEnumerator FillTo(int target)
    {
        for (int i = displayedAP; i < target; i++)
        {
            if (i < apGauges.Count && apGauges[i] != null)
            {
                apGauges[i].SetActive(true);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(stepSfxName);
            }
            displayedAP = i;
            apText.text = $"AP    : {displayedAP}";

            yield return new WaitForSeconds(fillStepDelay);
        }

        displayedAP = Mathf.Min(target, apGauges.Count);
        apText.text = $"AP    : {displayedAP}";
        fillRoutine = null;
    }

    private void SetGaugesImmediate(int countOn)
    {
        int max = apGauges.Count;
        for (int i = 0; i < max; i++)
        {
            bool active = i < countOn;
            if (apGauges[i] != null && apGauges[i].activeSelf != active)
                apGauges[i].SetActive(active);
        }
    }
}
