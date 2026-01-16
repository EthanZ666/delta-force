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

        BubbleSortByEfficiency(available);
        RenderShop();
    }
    float CalculateEfficiency(TowerData tower)
    // Calculate the efficiency of money for tower as the damage and range in return
    {
        float efficiency = (tower.damage*10 + tower.range*5 )/ tower.price;
        return efficiency;
    }

    void BubbleSortByEfficiency(List<TowerData> list)
{
    int n = list.Count;

    for (int cursor = 0; cursor < n - 1 ; cursor++)
    {
        for (int j = 0; j < n - 1; j++)
        {
            if (CalculateEfficiency(list[j]) < CalculateEfficiency(list[j + 1]))
            {
                TowerData temp = list[j];
                list[j] = list[j + 1];
                list[j + 1] = temp;
            }
        }
    }
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
