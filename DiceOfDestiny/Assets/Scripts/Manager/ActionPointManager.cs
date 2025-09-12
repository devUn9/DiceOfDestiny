using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionPointManager : Singletone<ActionPointManager>
{
    [SerializeField] private int actionPoint;

    public void AddAP(int amount = 1)
    {
        actionPoint += amount;
        UIManager.Instance.UpdateActionPointUI();
    }

    public void RemoveAP(int amount = 1)
    {
        if (!CanUse(amount))
        {
            ToastManager.Instance.ShowToast("행동력이 없습니다.", transform, 0f);
            return;
        }
        actionPoint -= amount;
        if (actionPoint <= 0)
        {
            actionPoint = 0;
            StageManager.Instance.EndTurn();
        }
        UIManager.Instance.UpdateActionPointUI();
    }

    public bool CanUse(int amont = 1)
    {
        if (actionPoint >= amont)
        {
            return true;
        }
        return false;
    }

    public void SetZero()
    {
        actionPoint = 0;
    }
    public int GetAP()
    {
        return actionPoint;
    }
}

