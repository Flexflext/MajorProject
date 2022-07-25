using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth;
    private float curHealth;

    private System.Action onDeath;

    private NavMeshAgent agent;
    private EnemyController controller;
    private PersonelAudioManager audioManager;

    private void Awake()
    {
        audioManager = GetComponent<PersonelAudioManager>();
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<EnemyController>();
        ChangeHealth(maxHealth);
    }

    public void TakeDamage(float _damage, Vector3 _knockback)
    {
        ChangeHealth(curHealth - _damage);
        controller.IsAgressive = true;
        StartCoroutine(C_AddKnockback(_knockback * 5));  
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

        audioManager.Play(EPossibleSounds.Die, ERandomSound.Static, true);
    }

    public void SubToOnDeath(System.Action _tosub)
    {
        onDeath += _tosub;
    }

    public void UnsubToOnDeath(System.Action _tounsub)
    {
        onDeath -= _tounsub;
    }

    private IEnumerator C_AddKnockback(Vector3 _knockback)
    {
        float curtime = 0;

        while (curtime <= 0.2f)
        {
            transform.position += _knockback * Time.deltaTime;

            curtime += Time.deltaTime;
            yield return null;
        }
    }
}
