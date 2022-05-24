using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegLimpingHalfLegState : LegState
{
    public LegLimpingHalfLegState(ProzeduralAnimationLogic _controller, LegCallback _legenterset, LegCallback _legexitreset) : base(_controller, _legenterset, _legexitreset)
    {
    }

    public override IEnumerator C_MoveLegCoroutine(int _leg)
    {
        float passedTime = 0f;

        legController.AdjustBrokenLegRotation(_leg);

        //Move the Leg for the Given Time
        while (passedTime <= legController.LegMovementTime)
        {
            //Lerp the Target Position and add the Evaluated Curve to it
            legController.IkTargets[_leg].position = Vector3.Lerp(legController.IkTargets[_leg].position, legController.NextAnimationTargetPosition[_leg], passedTime / legController.LegMovementTime) + legController.LegMovementCurve.Evaluate(passedTime / legController.LegMovementTime) * (legController.transform.up * 0.5f);

            //Add deltaTime and Wait for next Frame
            passedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Set Position
        legController.IkTargets[_leg].position = legController.NextAnimationTargetPosition[_leg];

        //Reset Flag
        legController.MoveingLegs[_leg] = false;
        legController.ResetBrokenLegRotation();

        yield return new WaitForSeconds(legController.LegMovementTime / 2);

        legController.IsOnMoveDelay[_leg] = false;

    }

    public override void EnterLegState(int _leg)
    {
        if (legEnterSet != null)
        {
            legEnterSet.Invoke(_leg);
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
