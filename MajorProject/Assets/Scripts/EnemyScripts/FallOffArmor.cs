using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallOffArmor : MonoBehaviour
{
    [SerializeField] private float despawnTime = 10;

    // Update is called once per frame
    void Update()
    {
        if (despawnTime <= 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            despawnTime -= Time.deltaTime;
        }
    }


}
