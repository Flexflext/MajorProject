using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth;
    private float curHealth;

    private void Awake()
    {
        ChangeHealth(maxHealth);
    }

    public void TakeDamage(float _damage)
    {
        ChangeHealth(curHealth - _damage);
    }

    private void ChangeHealth(float _newhealth)
    {
        curHealth = _newhealth;

        if (curHealth <= 0)
        {
            OnDeath();
        }
    }

    private void OnDeath()
    {
        Debug.Log("Dead Bitch");
    }
    
}
