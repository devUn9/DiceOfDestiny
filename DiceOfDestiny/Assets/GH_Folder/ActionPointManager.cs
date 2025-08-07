using System;
using UnityEngine;

public enum GameState
{
    Dice,
    Action,
    EndTurn
}

/// <summary>
/// 행동력(AP) 로직만 담당하며, 시각적 토큰 UI와는 분리되어 있습니다.
/// Token UI는 ActionPointDisplay가 OnActionPointChanged 이벤트를 구독하여 처리합니다.
/// </summary>
public class ActionPointManager : MonoBehaviour
{
    [Header("Dice Settings")]
    [SerializeField] private int[] diceFaces = new int[] { 1, 1, 2, 2, 3, 3 };

    private ActionPoint actionPoint;

    public event Action<int> OnActionPointChanged;
    public event Action OnValueChanged;

    public GameState GameState { get; private set; } = GameState.Dice;
    public int CurrentTurn { get; private set; } = 1;
    public int CurrentDiceValue { get; private set; }

    public int CurrentAP => actionPoint.Value;

    private void Awake()
    {
        actionPoint = new ActionPoint();
        NotifyChange();
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

    private void SetAP(int value)
    {
        actionPoint = new ActionPoint(value);
        OnValueChanged?.Invoke();
    }

    public void Reset()
    {
        CurrentTurn = 1;
        
        Init();
    }
    public void Init()
    {
        GameState = GameState.Dice;
        CurrentDiceValue = 0;
        SetAP(0);
        NotifyChange();
    }

    public void AddAP(int _plusAP)
    {
        actionPoint.Add(_plusAP);
        NotifyChange();
    }

    public void RemoveAP(int amount)
    {
        if (!actionPoint.CanUse(amount))
        {
            Debug.Log("행동력이 없습니다.");
            if (ToastManager.Instance != null)
                ToastManager.Instance.ShowToast("행동력이 없습니다.", transform);
            return;
        }

        actionPoint.Remove(amount);
        NotifyChange();
    }

    public bool TryUseAP() => actionPoint.CanUse(1);

    public void RollDice()
    {
        GameState = GameState.Action;
        if (DiceRollManager.Instance == null) return;
        if (!DiceRollManager.Instance.TryRoll(OnDiceResult)) return;
    }

    private void OnDiceResult(int value)
    {
        CurrentDiceValue = value;
        AddAP(value);
        Debug.Log($"주사위를 굴려서 {value}가 나왔습니다.");
        GameState = GameState.Action;
    }

    public void PieceAction()
    {
        RemoveAP(1);

        if (!TryUseAP())
        {
            GameState = GameState.EndTurn;
            NotifyChange();
        }
    }

    public void EndTurn()
    {
        if (GameState == GameState.Dice)
        {
            Debug.Log("먼저 주사위를 굴리세요.");
            if (ToastManager.Instance != null)
                ToastManager.Instance.ShowToast("먼저 주사위를 굴리세요.", transform);
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
        GameState = GameState.Dice;
        NotifyChange();
    }

    public void SetDiceFaces(int[] newFaces)
    {
        if (newFaces == null || newFaces.Length != diceFaces.Length) return;
        Array.Copy(newFaces, diceFaces, diceFaces.Length);
    }

    private void NotifyChange() => OnActionPointChanged?.Invoke(actionPoint.Value);
}
