using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LegLimpingHalfLegState : LegState
{
    public LegLimpingHalfLegState(ProzeduralAnimationLogic _controller, LegCallback _legenterset, LegCallback _legexitreset, ProzeduralAnimationLogic.LegParams[] _legs, UnityEvent<int, ELegStates> _onenter, UnityEvent<int> _onmove) : base(_controller, _legenterset, _legexitreset, _legs, _onenter, _onmove)
    {
    }

    public override IEnumerator C_MoveLegCoroutine(int _leg)
    {
        float maxTime = legController.LegMovementTime;
        float passedTime = 0f;

        legController.AdjustBrokenLegRotation(_leg);

        if (onMove != null)
        {
            onMove.Invoke(_leg);
        }

        //Move the Leg for the Given Time
        while (passedTime <= maxTime)
        {

            //Lerp the Target Position and add the Evaluated Curve to it
            legs[_leg].ikTarget.position = Vector3.Lerp(legs[_leg].ikTarget.position, legs[_leg].nextAnimationTargetPosition, passedTime / maxTime) + legController.LegMovementCurve.Evaluate(passedTime / maxTime) * (legController.transform.up * 0.5f);

            //Add deltaTime and Wait for next Frame
            passedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Set Position
        legs[_leg].ikTarget.position = legs[_leg].nextAnimationTargetPosition;
        legController.ResetBrokenLegRotation();


        //Reset Flag
        legs[_leg].moveingLeg = false;

        yield return new WaitForSeconds(maxTime / 2);

        legs[_leg].isOnMoveDelay = false;

    }

    public override void EnterLegState(int _leg)
    {
        if (legEnterSet != null)
        {
            legEnterSet.Invoke(_leg);
        }

        if (onEnter != null)
        {
            onEnter.Invoke(_leg, ELegStates.LS_LimpingHalfLeg);
        }
    }

    public override void ExitLegState(int _leg)
    {
        if (legExitReset != null)
        {
            legExitReset.Invoke(_leg);
        }
    }
}
