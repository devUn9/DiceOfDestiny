using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceRollManager : Singletone<DiceRollManager>
{
    [SerializeField] private ApDiceController dicePrefab;    

    readonly List<ApDiceController> pool = new();
    private bool isRolling;

    public bool TryRoll(Action<int> OnRollEnded)
    {
        if (isRolling || this.dicePrefab == null) return false;

        var dice = GetDiceFromPool();
        dice.gameObject.SetActive(true);
        isRolling = true;

        dice.OnRollEnded += OnEnd;

        void OnEnd(int value)
        {
            dice.OnRollEnded -= OnEnd;
            isRolling = false;
            OnRollEnded?.Invoke(value);
            dice.gameObject.SetActive(false);
        }

        dice.PlayRoll();
        return true;
    }

    ApDiceController GetDiceFromPool()
    {
        foreach (var d in pool)
            if (!d.gameObject.activeSelf) return d;

        var newDice = Instantiate(dicePrefab);
        newDice.gameObject.SetActive(false);
        pool.Add(newDice);
        return newDice;
    }
}
