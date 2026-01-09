using UnityEngine;
using System.Collections.Generic;

public class TowerDatabase : MonoBehaviour
{
    public List<TowerData> allTowers;

    public TowerData GetRandomTower()
    {
        if (allTowers.Count == 0) return null;
        return allTowers[Random.Range(0, allTowers.Count)];
    }
}
