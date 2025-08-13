using UnityEngine;
using System.Collections.Generic;

public enum PieceStatus
{
    Stun,
    Disease
    // 필요하면 계속 추가
}

public class PieceStatusEffectController : MonoBehaviour
{
    private Dictionary<PieceStatus, StatusEffect> statusEffects;

    private void Awake()
    {
        statusEffects = new Dictionary<PieceStatus, StatusEffect>
        {
            { PieceStatus.Stun,    new StatusEffect("기절") },
            { PieceStatus.Disease, new StatusEffect("질병") }
        };
    }

    public void SetStatus(PieceStatus type, int turn)
    {
        if (statusEffects.TryGetValue(type, out StatusEffect effect))
        {
            effect.Set(true, turn);
        }
    }
    public int GetRemainingTurn(PieceStatus type)
    {
        if (statusEffects.TryGetValue(type, out StatusEffect effect) && effect.IsActive)
        {
            return effect.RemainingTurn;
        }

        return 0;
    }

    public bool IsStatusActive(PieceStatus type)
    {
        return statusEffects.TryGetValue(type, out StatusEffect effect) && effect.IsActive;
    }

    public void EndTurn()
    {
        foreach (var kvp in statusEffects)
        {
            if(kvp.Value.IsActive)
                kvp.Value.DecreaseTurn();
        }
    }
}

[System.Serializable]
public class StatusEffect
{
    public bool IsActive { get; private set; }
    public int RemainingTurn { get; private set; }

    private string effectName;

    public StatusEffect(string name)
    {
        effectName = name;
        IsActive = false;
        RemainingTurn = 0;
    }

    public void Set(bool active, int turn)
    {
        IsActive = active;
        RemainingTurn = turn;
    }

    public void DecreaseTurn()
    {
        RemainingTurn--;
        Debug.Log($"남은 {effectName} 턴: {RemainingTurn}");

        if (RemainingTurn <= 0)
        {
            RemainingTurn = 0;
            IsActive = false;
            Debug.Log($"{effectName}이(가) 풀렸습니다.");
        }
    }
}