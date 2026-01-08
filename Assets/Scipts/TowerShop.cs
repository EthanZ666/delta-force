using System.Collections.Generic;
using UnityEngine;

public class TowerShop : MonoBehaviour
{
    public List<TowerData> allTowers;
    public Transform shopContent;
    public GameObject shopItemPrefab;

    private List<TowerData> currentTowers = new();

    void Start()
    {
        GenerateShop();
    }

    void GenerateShop()
    {
        currentTowers.Clear();

        while (currentTowers.Count < 10 && currentTowers.Count < allTowers.Count)
        {
            TowerData random = allTowers[Random.Range(0, allTowers.Count)];
            if (!currentTowers.Contains(random))
                currentTowers.Add(random);
        }

        SortByPrice();
        RenderShop();
    }

    void SortByPrice()
    {
        for (int i = 0; i < currentTowers.Count - 1; i++)
        {
            for (int j = 0; j < currentTowers.Count - i - 1; j++)
            {
                if (currentTowers[j].price > currentTowers[j + 1].price)
                {
                    (currentTowers[j], currentTowers[j + 1]) =
                    (currentTowers[j + 1], currentTowers[j]);
                }
            }
        }
    }

    void RenderShop()
    {
        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        foreach (TowerData tower in currentTowers)
        {
            GameObject item = Instantiate(shopItemPrefab, shopContent);
            item.GetComponent<ShopItemUI>().Set(tower);
        }
    }
}
