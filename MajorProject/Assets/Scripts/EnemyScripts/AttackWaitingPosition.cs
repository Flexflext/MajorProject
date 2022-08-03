using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Position Class for the Attack Waiting / Hiding Positions of the Enemies
/// </summary>
public class AttackWaitingPosition : MonoBehaviour
{
    //Is Currently Used by an Enemy to Hide from the Player
    public bool inUse;

    private void Start()
    {
        EnemyManager.Instance.AttackWaitingPositionSubscribe(this);
    }

    private void OnDestroy()
    {
        EnemyManager.Instance.AttackWaitingPositionUnSubscribe(this);
    }
}
