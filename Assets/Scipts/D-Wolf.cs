using UnityEngine;

public class DWolf : Tower
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        damage = 5.5f;
        fireRate = 1.5f;
        range = 5f;
        
    }
    
}
