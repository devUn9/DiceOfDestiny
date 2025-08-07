using UnityEngine;

public class ActionPoint
{
    public int Value { get; private set; }
    public int MaxValue { get; private set; }

    public ActionPoint(int initialValue = 0, int maxValue = 99)
    {
        Value = initialValue;
        MaxValue = maxValue;
    }

    public void Add(int amount)
    {
        Value = Mathf.Min(Value + amount, MaxValue);
    }

    public bool Remove(int amount)
    {
        if (Value < amount)
            return false;
            
        Value -= amount;
        return true;
    }

    public bool CanUse(int amount) => Value >= amount;

    public void Reset()
    {
        Value = 0;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}