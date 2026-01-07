using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text priceText;

    private TowerShop shop;
    private int index;

    public void Setup(TowerShop shop, TowerData data, int index)
    {
        this.shop = shop;
        this.index = index;

        icon.sprite = data.icon;
        priceText.text = "$" + data.price;
    }

    public void Buy()
    {
        shop.BuyTower(index);
    }
}
