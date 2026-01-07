using UnityEngine;
using System.Collections.Generic;

public class TowerShop : MonoBehaviour
{
    public TowerDatabase database;
    public int shopSize = 10;
    public Transform content;
    public GameObject shopItemPrefab;
    

    public List<TowerData> availableTowers = new List<TowerData>();

    void Start()
    {
        GenerateShop();
        SortByPrice();
    }

    void GenerateShop()
    {
        availableTowers.Clear();
        List<TowerData> pool = new List<TowerData>(database.allTowers);

        for (int i = 0; i < shopSize && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            availableTowers.Add(pool[index]);
            pool.RemoveAt(index);
        }
    }

    public void BuyTower(int index)
    {
        availableTowers.RemoveAt(index);
        AddRandomTower();
        SortByPrice();
        RefreshUI();
    }

    void AddRandomTower()
    {
        TowerData randomTower =
            database.allTowers[Random.Range(0, database.allTowers.Count)];
        availableTowers.Add(randomTower);
    }

    void SortByPrice()
    {
        for (int i = 1; i < availableTowers.Count; i++)
        {
            TowerData key = availableTowers[i];
            int j = i - 1;

            while (j >= 0 && availableTowers[j].price > key.price)
            {
                availableTowers[j + 1] = availableTowers[j];
                j--;
            }
            availableTowers[j + 1] = key;
        }
    }

    void RefreshUI()
{
    foreach (Transform child in content)
        Destroy(child.gameObject);

    for (int i = 0; i < availableTowers.Count; i++)
    {
        GameObject item = Instantiate(shopItemPrefab, content);
        item.GetComponent<ShopItemUI>()
            .Setup(this, availableTowers[i], i);
    }
}
}
