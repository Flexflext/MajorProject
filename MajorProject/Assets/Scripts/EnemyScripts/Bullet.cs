using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float dmg;

    private Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageObj = collision.collider.GetComponent<IDamageable>();

        if (damageObj == null)
        {
            damageObj = collision.collider.GetComponentInParent<IDamageable>();
        }

        if (damageObj != null)
        {
            damageObj.TakeDamage(dmg, transform.forward);
        }


        Destroy(this.gameObject);
    }
}
