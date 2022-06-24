using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float dmg;


    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageObj = collision.collider.GetComponent<IDamageable>();

        if (damageObj != null)
        {
            damageObj.TakeDamage(dmg);
        }
        else
        {
            damageObj = collision.collider.GetComponentInParent<IDamageable>();

            if (damageObj != null)
            {
                damageObj.TakeDamage(dmg);
            }
        }
        

        Destroy(this.gameObject);
    }
}
