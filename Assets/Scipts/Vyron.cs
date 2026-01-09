using UnityEngine;

public class Vyron : Tower
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        damage = 10f;
        fireRate = 0.8f;
        range = 4f;
        
    }
    
}
