using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth;
    private float curHealth;

    private void Start()
    {
        ChangeHealth(maxHealth);
    }

    public void TakeDamage(float _damage, Vector3 _knockback)
    {
        ChangeHealth(curHealth - _damage);
    }

    private void ChangeHealth(float _newhealth)
    {
        curHealth = _newhealth;

        if (curHealth > maxHealth) curHealth = maxHealth;
        if (curHealth > maxHealth) curHealth = maxHealth;
        if (curHealth <= 0) 
        {
            curHealth = 0;
            OnDeath();
        }

        HUD.Instance.SetPlayerHealth(curHealth / maxHealth);
    }

    private void OnDeath()
    {
        Debug.Log("Dead Bitch");
    }
    
}
