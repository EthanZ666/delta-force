using UnityEngine;

public class Vyron : Tower
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        damage = 10f;
        fireRate = 1f;
        range = 4.5f;
        
    }
    
}
