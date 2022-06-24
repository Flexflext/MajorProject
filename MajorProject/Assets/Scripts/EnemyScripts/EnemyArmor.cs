using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArmor : MonoBehaviour , IDamageable
{
    [SerializeField] private GameObject myArmorToSpawn;


    public void TakeDamage(float _damage)
    {
        Instantiate(myArmorToSpawn, transform.position, transform.rotation);

        EnemyHealth health = GetComponentInParent<EnemyHealth>();
        health.TakeDamage(_damage);

        this.gameObject.SetActive(false);
    }


}
