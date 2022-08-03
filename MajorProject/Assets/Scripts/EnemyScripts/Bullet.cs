using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bullet Script to do Dmg to any IDamageable Objects
/// </summary>
public class Bullet : MonoBehaviour
{
    [SerializeField] private float dmg;

    private void OnCollisionEnter(Collision collision)
    {
        //Check what to do Damage to
        IDamageable damageObj = collision.collider.GetComponent<IDamageable>();

        if (damageObj == null)
        {
            damageObj = collision.collider.GetComponentInParent<IDamageable>();
        }

        if (damageObj != null)
        {
            damageObj.TakeDamage(dmg, transform.forward);
        }

        //Destroy after collision
        Destroy(this.gameObject);
    }
}
