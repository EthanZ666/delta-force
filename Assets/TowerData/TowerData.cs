using UnityEngine;

[CreateAssetMenu(menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public Sprite icon;
    public GameObject towerPrefab;
    public int price;

    [Header("Stats")]
    public float damage;
    public float fireRate;
    public float range;
}
