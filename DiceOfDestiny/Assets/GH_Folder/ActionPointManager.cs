using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionPointManager : Singletone<ActionPointManager>
{
    private int actionPoint;

    public void AddAP(int amount = 1)
    {
        actionPoint += amount;
    }

    public void RemoveAP(int amount = 1)
    {
        if (CanUse(amount))
        {
            ToastManager.Instance.ShowToast("행동력이 없습니다.", transform);
        }

        actionPoint -= amount;
        if (actionPoint == 0)
        {
            StageManager.Instance.EndTurn();
        }
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

