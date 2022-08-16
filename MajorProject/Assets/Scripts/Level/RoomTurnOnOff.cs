using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTurnOnOff : MonoBehaviour
{
    [SerializeField] private LayerMask toCheck;

    [SerializeField] private GameObject midSpawnObj;
    [SerializeField] private float rangeMid;

    [SerializeField] private GameObject wholeSpawnObj;
    [SerializeField] private float rangeWhole;

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckPlayerMid();
        CheckPlayerWhole();
    }

    /// <summary>
    /// Check if Player is near to Turn on Mid Object
    /// </summary>
    private void CheckPlayerMid()
    {
        if (Physics.CheckSphere(transform.position, rangeMid, toCheck))
        {
            if (!midSpawnObj.activeSelf)
            {
                midSpawnObj.SetActive(true);
            }
        }
        else
        {
            if (midSpawnObj.activeSelf)
            {
                midSpawnObj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Check if to Turn on all Objects of the Room
    /// </summary>
    private void CheckPlayerWhole()
    {
        //Bounds bo = new Bounds(transform.position, Vector3.one * rangeWhole);

        //bo.Contains()

        if (Physics.CheckSphere(transform.position, rangeWhole, toCheck))
        {
            if (!wholeSpawnObj.activeSelf)
            {
                wholeSpawnObj.SetActive(true);
            }
        }
        else
        {
            if (wholeSpawnObj.activeSelf)
            {
                wholeSpawnObj.SetActive(false);
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangeMid);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangeWhole);
    }
}
