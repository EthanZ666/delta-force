using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] private int startingMoney = 200;

    public int Money { get; private set; }

    void Awake()
    {
        Money = startingMoney;
    }

    public bool CanAfford(int amount) => Money >= amount;

    public bool TrySpend(int amount)
    {
        if (!CanAfford(amount)) return false;
        Money -= amount;
        return true;
    }

    public void Add(int amount)
    {
        Money += amount;
    }
}
