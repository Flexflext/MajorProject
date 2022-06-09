using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackWaitingPosition : MonoBehaviour
{
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
