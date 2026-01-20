using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text priceText;

    private TowerData towerData;
    private Button button;
    private TowerPlacementManager placement;

    void Awake()
    {
        button = GetComponent<Button>();
        GameObject placementObj = GameObject.FindWithTag("TowerPlacementManager");
        TowerPlacementManager placement = placementObj.GetComponent<TowerPlacementManager>();

        button.onClick.AddListener(OnClick);
    }

    public void Setup(TowerData data)
    {
        towerData = data;
        icon.sprite = data.icon;
        priceText.text = data.price.ToString();
    }

    void OnClick()
    {
        if (placement == null || towerData == null) return;

        placement.BeginPlacement(towerData);
    }
}
