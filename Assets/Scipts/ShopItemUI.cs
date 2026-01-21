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
        button = icon.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        var placementObj = GameObject.FindWithTag("TowerPlacementManager");
        if (placementObj == null)
        {
            return;
        }

        placement = placementObj.GetComponent<TowerPlacementManager>(); // ✅ assign the FIELD
        if (placement == null)
        {
            return;
        }

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
        Debug.Log($"Shop item clicked: {(towerData != null ? towerData.name : "NULL towerData")}");
        if (towerData == null) return;

        placement.BeginPlacement(towerData);
    }
}
