using System;
using UnityEngine;

public sealed class MoneyManager : MonoBehaviour
{
    [Header("Starting Balance")]
    [SerializeField] private int startingBalance = 200;

    private int balance;

    public int Balance => balance;

    public event Action<int> BalanceChanged;

    private void Awake()
    {
        balance = Mathf.Max(0, startingBalance);
        BalanceChanged?.Invoke(balance);
    }

    public bool CanAfford(int amount)
    {
        if (amount <= 0) return true;
        return balance >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (balance < amount) return false;

        balance -= amount;
        BalanceChanged?.Invoke(balance);
        return true;
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;

        long newValue = (long)balance + amount;
        balance = (newValue > int.MaxValue) ? int.MaxValue : (int)newValue;

        BalanceChanged?.Invoke(balance);
    }
}
