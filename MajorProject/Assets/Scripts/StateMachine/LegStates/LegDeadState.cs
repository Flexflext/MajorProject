using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegDeadState : LegState
{
    private float deathTime = 1f;
    private Transform[] targetParents;

    public LegDeadState(ProzeduralAnimationLogic _controller, LegCallback _legenterset, LegCallback _legexitreset, ProzeduralAnimationLogic.LegParams[] _legs) : base(_controller, _legenterset, _legexitreset, _legs)
    {
        targetParents = new Transform[_legs.Length];
    }

    public override IEnumerator C_MoveLegCoroutine(int _leg)
    {
        yield return null;
    }

    public override void EnterLegState(int _leg)
    {
        targetParents[_leg] = legs[_leg].ikTarget.parent;
        legs[_leg].ikTarget.parent = legController.transform;
        legController.StartCoroutine(PlayDeathAnimation(_leg));
    }

    public override void ExitLegState(int _leg) 
    {
        legs[_leg].legIKSystem.SolveIK = true;
        legs[_leg].legIKSystem.ResetBoxCollidersOnChain();
        legs[_leg].legIKSystem.ResetRigidbodysOnChain();

        legs[_leg].moveingLeg = false;
        legs[_leg].isOnMoveDelay = false;
        legs[_leg].stopLegAnimationFlag = false;

        legs[_leg].ikTarget.parent = targetParents[_leg];
    }

    private IEnumerator PlayDeathAnimation(int _leg)
    {
        legs[_leg].legIKSystem.CreateBoxCollidersOnChain();
        
        legs[_leg].stopLegAnimationFlag = true;

        Vector3 dir = legs[_leg].ikTarget.localPosition;

        dir *= 0.35f + Random.Range(-0.15f, 0.15f);
        //dir += legs[_leg].ikTarget.localPosition;

        float curTime = 0f;
        float maxTime = deathTime + Random.Range(0.1f, -0.1f);

        while (curTime < maxTime)
        {
            legs[_leg].ikTarget.localPosition = Vector3.Lerp(legs[_leg].ikTarget.localPosition, dir, curTime / maxTime);
            curTime += Time.deltaTime;
            yield return null;

        }

        legs[_leg].legIKSystem.CreateRigidbodysOnChain();
        legs[_leg].legIKSystem.SolveIK = false;
        
    }
}
