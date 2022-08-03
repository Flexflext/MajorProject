using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test-Script: Do not use
/// </summary>
public class DestroyableObject : MonoBehaviour, IDamageable
{

    public void TakeDamage(float _damage, Vector3 _knockback)
    {
        Destroy(gameObject);
    }
}
