using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegDamageSystem : MonoBehaviour
{
    [SerializeField] private int legIndex;

    private ProzeduralAnimationLogic animationLogic;
    private SpiderBodyRotationController moveController;

    private void Start()
    {
        animationLogic = GetComponentInParent<ProzeduralAnimationLogic>();
        moveController = GetComponentInParent<SpiderBodyRotationController>();
    }

    public void TakeDamage(Vector3 _pos)
    {
        if (legIndex == -1)
        {
            animationLogic.SetDeath();
            moveController.AddRecoilMovement(moveController.transform.position + moveController.transform.forward);
            return;
        }

        moveController.AddRecoilMovement(_pos);
        animationLogic.DecreaseLegHealth(legIndex);
    }
}
