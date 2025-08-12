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
    [Header("Dice Settings (정보용)")]
    [SerializeField] private int[] diceFaces = new int[] { 1, 1, 2, 2, 3, 3 };

    private ActionPoint actionPoint;

    public event Action<int> OnActionPointChanged;
    public event Action OnValueChanged;

    public GameState GameState { get; private set; } = GameState.Dice;
    public int CurrentTurn { get; private set; } = 1;
    public int CurrentDiceValue { get; private set; }

    public int CurrentAP => actionPoint?.Value ?? 0;

    private void Awake()
    {
        actionPoint = new ActionPoint();
        NotifyAll();
    }

    private void Update()
    {
        switch (GameState)
        {
            case GameState.Dice:
                if (Input.GetKeyDown(KeyCode.R))
                    RollDice();
                if (Input.GetKeyDown(KeyCode.Z))
                    GameManager.Instance.actionPointManager.AddAP(100);
            break;

            case GameState.Action:
                if (Input.GetKeyDown(KeyCode.T))
                    EndTurn();
                break;

            case GameState.EndTurn:
                if (Input.GetKeyDown(KeyCode.T))
                    EndTurn();
                break;
        }
    }

    public void ResetAll()
    {
        CurrentTurn = 1;
        InitTurn();
    }

    public void InitTurn()
    {
        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.DeactivateAllDice();

        GameState = GameState.Dice;
        CurrentDiceValue = 0;
        actionPoint.Reset();
        NotifyAll();
    }

    public void RollDice()
    {
        if (GameState != GameState.Dice) return;
        if (DiceRollManager.Instance == null)
        {
            Debug.LogWarning("[APM] DiceRollManager.Instance = null");
            return;
        }

        bool started = DiceRollManager.Instance.TryRoll((value) =>
        {
            CurrentDiceValue = value;
            AddAP(value);
            Debug.Log($"주사위를 굴려서 {value}가 나왔습니다.");
            GameState = GameState.Action;
            OnValueChanged?.Invoke();
        });

        if (!started)
        {
            Debug.Log("[APM] RollDice 시작 실패(이미 굴리는 중이거나 프리팹 없음).");
            NotifyMisc();
        }
    }

    public void AddAP(int plus)
    {
        actionPoint.Add(plus);
        NotifyAll();
    }

    public void PieceAction()
    {
        RemoveAP(1);
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
        NotifyAP();

        if (!actionPoint.CanUse(1))
        {
            GameState = GameState.EndTurn;
            NotifyMisc();
        }
    }

    public bool TryUseAP() => actionPoint.CanUse(1);

    public void EndTurn()
    {
        if (GameState == GameState.Dice)
        {
            Debug.Log("먼저 주사위를 굴리세요.");
            if (ToastManager.Instance != null)
                ToastManager.Instance.ShowToast("먼저 주사위를 굴리세요.", transform);
            return;
        }

        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.DeactivateAllDice();

        if (PieceManager.Instance != null)
            PieceManager.Instance.DecreaseDebuffAllPieces();

        CurrentTurn++;
        InitTurn();
    }

    public void SetDiceFaces(int[] newFaces)
    {
        if (newFaces == null || newFaces.Length != diceFaces.Length) return;
        Array.Copy(newFaces, diceFaces, diceFaces.Length);
    }

    private void NotifyAP() => OnActionPointChanged?.Invoke(actionPoint.Value);
    private void NotifyMisc() => OnValueChanged?.Invoke();
    private void NotifyAll() { NotifyAP(); NotifyMisc(); }
}
