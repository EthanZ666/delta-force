using TMPro;
using UnityEngine;

public class BaseHealthTextUI : MonoBehaviour
{
    [SerializeField] private MilitaryBase militaryBase;
    [SerializeField] private TMP_Text healthText;

    [Header("Display")]
    [SerializeField] private bool showMax = true;     
    [SerializeField] private bool roundToInt = true;  

    private void Awake()
    {
        if (healthText == null)
            healthText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (militaryBase != null)
            militaryBase.HealthChanged += UpdateText;

        Refresh();
    }

    private void OnDisable()
    {
        if (militaryBase != null)
            militaryBase.HealthChanged -= UpdateText;
    }

    private void Refresh()
    {
        if (militaryBase == null || healthText == null) return;
        UpdateText(militaryBase.CurrentHealth);
    }

    private void UpdateText(float current)
    {
        if (healthText == null) return;

        if (roundToInt)
        {
            int c = Mathf.CeilToInt(current);
            if (showMax)
            {
                int m = Mathf.CeilToInt(militaryBase.MaxHealth);
                healthText.text = $"Base Health: {c}/{m}";
            }
            else
            {
                healthText.text = $"Base Health: {c}";
            }
        }
        else
        {
            if (showMax)
                healthText.text = $"Base Health: {current:0.0}/{militaryBase.MaxHealth:0.0}";
            else
                healthText.text = $"Base Health: {current:0.0}";
        }
    }
}
