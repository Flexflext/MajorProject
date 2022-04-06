using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRotation : MonoBehaviour
{
    [SerializeField] private Vector3 upVec;

    // Update is called once per frame
    void Update()
    {
        //this.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Lerp(this.transform.up, upVec, 20 * Time.deltaTime));

        Quaternion rot = Quaternion.FromToRotation(transform.up, upVec);

        transform.Rotate(rot.eulerAngles);
    }
}
