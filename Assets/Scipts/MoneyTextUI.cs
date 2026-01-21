using TMPro;
using UnityEngine;

public class MoneyTextUI : MonoBehaviour
{
    [SerializeField] private MoneyManager money;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private string prefix = "$";

    private void Awake()
    {
        if (moneyText == null)
            moneyText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (money != null)
            money.BalanceChanged += UpdateText;

        Refresh();
    }

    private void OnDisable()
    {
        if (money != null)
            money.BalanceChanged -= UpdateText;
    }

    private void Refresh()
    {
        if (money == null || moneyText == null) return;
        UpdateText(money.Balance);
    }

    private void UpdateText(int newBalance)
    {
        if (moneyText == null) return;
        moneyText.text = $"{prefix}{newBalance}";
    }
}
