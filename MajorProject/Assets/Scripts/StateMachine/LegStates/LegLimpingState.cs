using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegLimpingState : LegState
{
    public LegLimpingState(ProzeduralAnimationLogic _controller) : base(_controller)
    {
    }

    public override IEnumerator C_MoveLegCoroutine(int _leg)
    {
        legController.Aj(_leg);

        float passedTime = 0f;

        //Move the Leg for the Given Time
        while (passedTime <= legController.LegMovementTime)
        {
            //Lerp the Target Position and add the Evaluated Curve to it
            legController.IkTargets[_leg].position = Vector3.Lerp(legController.IkTargets[_leg].position, legController.NextAnimationTargetPosition[_leg], passedTime / legController.LegMovementTime) + legController.LegMovementCurve.Evaluate(passedTime / legController.LegMovementTime) * legController.transform.up;

            //Add deltaTime and Wait for next Frame
            passedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Set Position
        legController.IkTargets[_leg].position = legController.NextAnimationTargetPosition[_leg];

        //Reset Flag
        legController.MoveingLegs[_leg] = false;

        yield return new WaitForSeconds(legController.LegMovementTime / 2);

        legController.IsOnMoveDelay[_leg] = false;

        legController.ResetRot();
    }

    public override void EnterLegState(int _leg)
    {
        legController.AnimationRaycastOrigins[_leg].localPosition += legController.OriginLocalBack.normalized * legController.OriginBackwardsMultiplier;
        legController.AnimationHints[_leg].position += ((legController.transform.position) - (legController.AnimationHints[_leg].position)).normalized * legController.HintBackwardsMultiplier;
        legController.StartLocalPosition += Vector3.down * legController.DownAddPerBrokenLeg;
    }

    public override void ExitLegState(int _leg)
    {
        legController.AnimationHints[_leg].localPosition = legController.HintLocalStartPosition[_leg];
        legController.AnimationRaycastOrigins[_leg].localPosition = legController.OriginLocalStartPosition[_leg];
        legController.StartLocalPosition += Vector3.up * legController.DownAddPerBrokenLeg;
    }
}
