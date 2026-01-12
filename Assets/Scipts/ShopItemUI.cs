using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text priceText;
    private TowerData towerData;

    public void Setup(TowerData data)
    {
        towerData = data;
        icon.sprite = data.icon;
        priceText.text = data.price.ToString();
    }
}
