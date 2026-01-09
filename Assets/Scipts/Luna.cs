using UnityEngine;

public class Luna : Tower
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        damage = 8f;
        fireRate = 0.6f;
        range = 10f;
        
    }
    
}
