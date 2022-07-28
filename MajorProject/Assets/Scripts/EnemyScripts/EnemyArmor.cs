using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArmor : MonoBehaviour , IDamageable
{
    [SerializeField] private GameObject myArmorToSpawn;
    [SerializeField] private GameObject armorToTurnOff;


    public void TakeDamage(float _damage, Vector3 _knockback)
    {
        Instantiate(myArmorToSpawn, transform.position, transform.rotation);

        EnemyHealth health = GetComponentInParent<EnemyHealth>();
        health.TakeDamage(_damage, _knockback);

        //armorToTurnOff.SetActive(false);
        this.gameObject.SetActive(false);
    }


}
