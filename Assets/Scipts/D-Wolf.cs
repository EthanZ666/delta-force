using UnityEngine;

public class DWolf : Tower
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        damage = 7f;
        fireRate = 1.2f;
        range = 5f;
        
    }
    
}
