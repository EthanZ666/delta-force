using System.Collections.Generic;
using UnityEngine;

public class TowerDatabase : MonoBehaviour
{
    public List<TowerData> towers = new();

    public TowerData GetRandomTower()
    {
        if (towers == null || towers.Count == 0)
        {
            Debug.LogError("TowerDatabase has no towers!");
            return null;
        }

        int index = Random.Range(0, towers.Count);
        return towers[index];
    }
}
