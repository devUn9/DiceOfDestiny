using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionPointUI : MonoBehaviour
{
    [Header("Text")]
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

    // 바인딩 안정화 설정
    [Header("Binding (Scene Transition Safe)")]
    [SerializeField] private float bindTimeout = 2.0f;          // 최대 대기 시간
    [SerializeField] private float bindRetryInterval = 0f;      // 0이면 매 프레임 재시도

    private readonly List<GameObject> apGauges = new();

    // GameManager를 건드리지 않기 위해, 바인딩된 APM을 로컬로 보관
    private ActionPointManager _boundApm = null;

    // 안전한 이벤트 해제용 캐시
    private Action<int> onAPChangedHandler;
    private Action onValueChangedHandler;

    // 표시 중인 AP(시각적 수치)
    private int displayedAP = 0;

    // 코루틴
    private Coroutine fillRoutine;
    private Coroutine bindRoutine;

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

        // 씬 전환 직후에도 안전하게: 준비될 때까지 재시도 바인딩
        if (bindRoutine != null) StopCoroutine(bindRoutine);
        bindRoutine = StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        diceRollButton.onClick.RemoveListener(onClickDiceRollButton);
        endTurnButton.onClick.RemoveListener(onClickEndTurnButton);

        if (bindRoutine != null) { StopCoroutine(bindRoutine); bindRoutine = null; }

        UnbindApm();   // 이벤트 구독 해제
    }

    private void Update()
    {
        // 런타임 중 APM이 파괴/교체된 경우를 대비해 재바인딩 시도
        if (_boundApm == null && bindRoutine == null)
        {
            bindRoutine = StartCoroutine(BindWhenReady());
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 바인딩 로직
    // ─────────────────────────────────────────────────────────────────────────────

    private IEnumerator BindWhenReady()
    {
        float elapsed = 0f;

        while (_boundApm == null && elapsed < bindTimeout)
        {
            _boundApm = TryResolveApm();
            if (_boundApm != null) break;

            float wait = Mathf.Max(0f, bindRetryInterval);
            if (wait > 0f)
                yield return new WaitForSecondsRealtime(wait);
            else
                yield return null; // 매 프레임 재시도

            elapsed += (wait > 0f ? wait : Time.unscaledDeltaTime);
        }

        if (_boundApm == null)
        {
            Debug.LogError("[ActionPointUI] ActionPointManager 바인딩 실패. 씬 세팅/활성 상태를 확인하세요.");
            bindRoutine = null;
            yield break;
        }

        // 이벤트 구독
        onAPChangedHandler = OnActionPointChanged;
        onValueChangedHandler = RefreshAll;

        _boundApm.OnActionPointChanged += onAPChangedHandler;
        _boundApm.OnValueChanged += onValueChangedHandler;

        // 최신 상태 강제 반영
        displayedAP = Mathf.Clamp(_boundApm.CurrentAP, 0, apGauges.Count);
        SetGaugesImmediate(displayedAP);
        RefreshAll();
        OnActionPointChanged(_boundApm.CurrentAP);

        bindRoutine = null;
    }

    private ActionPointManager TryResolveApm()
    {
        // 1) GameManager 캐시가 준비되어 있으면 우선 사용
        var gm = GameManager.Instance;
        if (gm != null && gm.actionPointManager != null)
            return gm.actionPointManager;

        // 2) 씬에서 직접 탐색(비활성 포함) — GameManager를 수정하지 않고도 안전하게 찾기
#if UNITY_2022_2_OR_NEWER
        var found = FindFirstObjectByType<ActionPointManager>(FindObjectsInactive.Include);
        return found;
#else
        var all = Resources.FindObjectsOfTypeAll<ActionPointManager>();
        foreach (var a in all)
        {
            if (a != null && a.gameObject.scene.IsValid())
                return a;
        }
        return null;
#endif
    }

    private void UnbindApm()
    {
        if (_boundApm != null)
        {
            if (onAPChangedHandler != null) _boundApm.OnActionPointChanged -= onAPChangedHandler;
            if (onValueChangedHandler != null) _boundApm.OnValueChanged -= onValueChangedHandler;
        }
        _boundApm = null;
        onAPChangedHandler = null;
        onValueChangedHandler = null;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 버튼 핸들러
    // ─────────────────────────────────────────────────────────────────────────────

    private void onClickDiceRollButton()
    {
        if (_boundApm == null) return;
        if (_boundApm.GameState != GameState.Dice) return;

        _boundApm.RollDice();
    }

    private void onClickEndTurnButton()
    {
        if (_boundApm == null) return;
        _boundApm.EndTurn();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // UI 갱신 / 애니메이션
    // ─────────────────────────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        if (_boundApm == null) return;

        currentState.text = $"State : {_boundApm.GameState}";
        currentTurn.text = $"Turn  : {_boundApm.CurrentTurn}";
        diceText.text = $"Dice  : {_boundApm.CurrentDiceValue}";
        apText.text = $"AP    : {_boundApm.CurrentAP}";

        diceRollButton.interactable = (_boundApm.GameState == GameState.Dice);
        endTurnButton.interactable = (_boundApm.GameState != GameState.Dice);
    }

    private void OnActionPointChanged(int ap)
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
                if (playSfxEachStep && AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(stepSfxName);
            }

            displayedAP = i + 1;
            apText.text = $"AP    : {displayedAP}";

            // 로직 변경으로 목표가 낮아졌으면 중단
            if (_boundApm != null && _boundApm.CurrentAP < displayedAP)
                break;

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
