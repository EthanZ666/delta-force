using UnityEngine;

public class HackClaw : Tower
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        damage = 5f;
        fireRate = 0.5f;
        range = 8f;
        
    }
    
}
