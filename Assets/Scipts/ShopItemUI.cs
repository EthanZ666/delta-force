using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text priceText;

    public void Set(TowerData data)
    {
        icon.sprite = data.icon;
        priceText.text = "$" + data.price;
    }
}
