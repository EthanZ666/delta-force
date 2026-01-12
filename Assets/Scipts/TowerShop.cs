using UnityEngine;
using System.Collections.Generic;

public class TowerShop : MonoBehaviour
{
    public TowerDatabase database;
    public Transform shopContent;
    public GameObject shopItemPrefab;
    public int shopSize = 10;

    private List<TowerData> available = new();

    void Start()
    {
        GenerateShop();
    }

    void GenerateShop()
    {
        available.Clear();

        for (int i = 0; i < shopSize; i++)
        {
            available.Add(database.GetRandomTower());
        }

        SortByPrice();
        RenderShop();
    }

    void SortByPrice()
    {
        available.Sort((a, b) => a.price.CompareTo(b.price));
    }

    void RenderShop()
    {
        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        foreach (TowerData tower in available)
        {
            GameObject item = Instantiate(shopItemPrefab, shopContent);
            item.GetComponent<ShopItemUI>().Setup(tower);
        }
    }
}
