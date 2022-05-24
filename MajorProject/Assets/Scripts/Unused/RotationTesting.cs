using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTesting : MonoBehaviour
{
    [SerializeField] private Transform target;

    private Vector3 localStartDirection;
    private Quaternion localStartRotation;

    private void Start()
    {
        localStartRotation = transform.localRotation;
        localStartDirection = transform.InverseTransformVector(target.position) - transform.localPosition; 
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, localStartDirection, Color.black);
        Vector3 newDirection = target.position - transform.position;

        Debug.DrawRay(transform.position, newDirection.normalized, Color.blue);

        //Debug.DrawRay(transform.position, (Quaternion.FromToRotation(localStartDirection, transform.TransformDirection(newDirection.normalized)) * transform.localRotation) * localStartDirection, Color.red);

        Quaternion change = Quaternion.FromToRotation(transform.forward, newDirection);



        transform.localRotation = localStartRotation * Quaternion.FromToRotation(localStartDirection.normalized, newDirection.normalized);
    }
}
