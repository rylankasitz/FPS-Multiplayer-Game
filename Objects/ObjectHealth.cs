using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ObjectHealth : NetworkBehaviour
{
    public int Health;

    private int _currentHealth;


    void Start()
    {
        _currentHealth = Health;
    }

    void Update()
    {
        if (_currentHealth <= 0)
        {
            DestroyObj();
        }
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= (int) damage;
    }

    private void DestroyObj()
    {
        Destroy(gameObject);
        NetworkServer.Destroy(gameObject);
    }
}
