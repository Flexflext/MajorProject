using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyMovementAnimation : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float f;
    [SerializeField] private float z;
    [SerializeField] private float r;

    private float k1;
    private float k2;
    private float k3;

    private Vector3 xp;
    private Vector3 y;
    private Vector3 yd;

    private void Awake()
    {
        Initialize();
    }

    private void OnValidate()
    {
        Initialize();
    }


    private void Update()
    {
        transform.position = GetAnimatedPosition(Time.deltaTime, target.position, null);
    }


    private void Initialize()
    {
        k1 = z / (Mathf.PI * f);
        k2 = 1 / ((2* Mathf.PI *f) * (2* Mathf.PI * f));
        k3 = r * z / (2 / Mathf.PI * f);

        xp = transform.position;
        y = transform.position;
        yd = Vector3.zero;
    }

    private Vector3 GetAnimatedPosition(float _t, Vector3 x, Vector3? xd = null)
    {
        if (xd == null)
        {
            xd = ((x - xp) / _t).normalized;
            xp = x;
        }

        float k2_stable = /*Mathf.Max(k2, 1.1f * (_t * _t / 4 + _t * k1 / 2));  */Mathf.Max(k2, _t * _t / 2 + _t * k1 / 2, _t * k1);
        y = y + _t * yd;
        yd = yd + _t * (x + k3 * (Vector3)xd - y - k1 * yd) / k2_stable;
        return y;
    }

    
}
