using UnityEngine;

public class HackClaw : Tower
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        damage = 5f;
        fireRate = 3f;
        range = 400f;
    }
    
}
