using UnityEngine;

[CreateAssetMenu(menuName = "Tower/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public GameObject prefab;
    public int price;
    public Sprite icon;
}
