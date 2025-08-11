using System;
using UnityEngine;

public enum GameState
{
    Dice,
    Action,
    EndTurn
}

public sealed class ActionPointManager : MonoBehaviour
{
    [Header("Dice Settings")]
    [SerializeField] private int[] diceFaces = new int[] { 1, 1, 2, 2, 3, 3 };

    private ActionPoint actionPoint;

    public event Action<int> OnActionPointChanged; // AP 수치 변경
    public event Action OnValueChanged;            // 상태/턴/주사위 등 기타 변경

    public GameState GameState { get; private set; } = GameState.Dice;
    public int CurrentTurn { get; private set; } = 1;
    public int CurrentDiceValue { get; private set; }
    public int CurrentAP => actionPoint.Value;

    private void Awake()
    {
        actionPoint = new ActionPoint();
        NotifyEverything();
    }

    private void Update()
    {
        switch (GameState)
        {
            case GameState.Dice:
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RollDice();
                    GameState = GameState.Action;
                }
                break;

            case GameState.Action:
                if (Input.GetKeyDown(KeyCode.T))
                {
                    EndTurn();
                }
                break;

            case GameState.EndTurn:
                if (Input.GetKeyDown(KeyCode.T))
                {
                    EndTurn();
                }
                break;
        }
    }

    private void SetState(GameState next)
    {
        if (GameState == next) return;
        GameState = next;
        OnValueChanged?.Invoke();
    }

    private void SetAP(int value)
    {
        actionPoint = new ActionPoint(value);
        NotifyEverything();
    }

    private void NotifyEverything()
    {
        OnActionPointChanged?.Invoke(actionPoint.Value);
        OnValueChanged?.Invoke();
    }

    public void Reset()
    {
        CurrentTurn = 1;
        Init();
    }

    public void Init()
    {
        SetState(GameState.Dice);
        CurrentDiceValue = 0;
        SetAP(0);
    }

    public void AddAP(int plus)
    {
        actionPoint.Add(plus);
        NotifyEverything();
    }

    public void RemoveAP(int amount)
    {
        if (!actionPoint.CanUse(amount))
        {
            Debug.Log("행동력이 없습니다.");
            if (ToastManager.Instance != null)
                ToastManager.Instance.ShowToast("행동력이 없습니다.", transform);

            OnValueChanged?.Invoke(); // 실패 상황도 UI 재표시 보장
            return;
        }

        actionPoint.Remove(amount);
        NotifyEverything();
    }

    public bool TryUseAP() => actionPoint.CanUse(1);

    public void RollDice()
    {
        SetState(GameState.Action);

        if (DiceRollManager.Instance == null)
        {
            SetState(GameState.Dice);
            return;
        }

        if (!DiceRollManager.Instance.TryRoll(OnDiceResult))
        {
            SetState(GameState.Dice);
            return;
        }
    }

    private void OnDiceResult(int value)
    {
        CurrentDiceValue = value;
        AddAP(value);
        Debug.Log($"주사위를 굴려서 {value}가 나왔습니다.");
        SetState(GameState.Action);
    }

    public void PieceAction()
    {
        RemoveAP(1);

        if (!TryUseAP())
        {
            SetState(GameState.EndTurn);
        }
    }

    public void EndTurn()
    {
        if (GameState == GameState.Dice)
        {
            Debug.Log("먼저 주사위를 굴리세요.");
            if (ToastManager.Instance != null)
                ToastManager.Instance.ShowToast("먼저 주사위를 굴리세요.", transform);

            OnValueChanged?.Invoke();
            return;
        }

        if (PieceManager.Instance != null)
            PieceManager.Instance.DecreaseDebuffAllPieces();

        CurrentTurn++;
        ResetTurn();
    }

    private void ResetTurn()
    {
        actionPoint.Reset();
        CurrentDiceValue = 0;
        SetState(GameState.Dice);
        NotifyEverything();
    }

    public void SetDiceFaces(int[] newFaces)
    {
        if (newFaces == null || newFaces.Length != diceFaces.Length) return;
        Array.Copy(newFaces, diceFaces, diceFaces.Length);
        OnValueChanged?.Invoke();
    }
}
