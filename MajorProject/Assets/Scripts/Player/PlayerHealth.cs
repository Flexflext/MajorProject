using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Player Health Class with Basic Health Functionallity
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float healthRegenTimer;
    [SerializeField] private float healthRegenMultiplier;
    private float curHealth;
    private bool regen;
    private PersonelAudioManager audioManager;

    private void Start()
    {
        ChangeHealth(maxHealth);
        audioManager = GetComponent<PersonelAudioManager>();
    }

    private void Update()
    {
        if (regen)
        {
            ChangeHealth(curHealth + Time.deltaTime * healthRegenMultiplier);
        }
    }

    public void TakeDamage(float _damage, Vector3 _knockback)
    {
        ChangeHealth(curHealth - _damage);
        regen = false;
        audioManager.Play(EPossibleSounds.Hit, ERandomSound.Static, true);
        StopAllCoroutines();
        StartCoroutine(C_WaitForRegen());
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

    private IEnumerator C_WaitForRegen()
    {
        yield return new WaitForSeconds(healthRegenTimer);

        regen = true;
    }

    private void OnDeath()
    {
        LevelManager.Instance.SetDeathNoTimer();
    }
    
}
