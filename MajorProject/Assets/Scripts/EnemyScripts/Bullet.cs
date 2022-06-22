using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private float dmg;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == hitLayer.value)
        {

        }
        

        Destroy(this.gameObject);
    }
}
