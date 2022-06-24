using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth;
    private float curHealth;

    private System.Action onDeath;

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
        if (onDeath != null)
        {
            onDeath.Invoke();
        }
    }

    public void SubToOnDeath(System.Action _tosub)
    {
        onDeath += _tosub;
    }

    public void UnsubToOnDeath(System.Action _tounsub)
    {
        onDeath -= _tounsub;
    }
}
