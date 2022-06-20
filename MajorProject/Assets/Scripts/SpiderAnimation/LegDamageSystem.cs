using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegDamageSystem : MonoBehaviour
{
    [SerializeField] private int legIndex;

    private ProzeduralAnimationLogic animationLogic;

    private void Start()
    {
        animationLogic = GetComponentInParent<ProzeduralAnimationLogic>();
    }

    public void TakeDamage()
    {
        if (legIndex == -1)
        {
            animationLogic.SetDeath();
            return;
        }

        animationLogic.DecreaseLegHealth(legIndex);
    }
}
