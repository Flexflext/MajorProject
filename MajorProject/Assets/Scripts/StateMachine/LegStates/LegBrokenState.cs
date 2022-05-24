using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegBrokenState : LegState
{
    public LegBrokenState(ProzeduralAnimationLogic _controller, LegCallback _legenterset, LegCallback _legexitreset) : base(_controller, _legenterset, _legexitreset)
    {
    }

    public override IEnumerator C_MoveLegCoroutine(int _leg)
    {
        float passedTime = 0f;

        legController.AdjustBrokenLegRotation(_leg);

        Vector3 oldPos = legController.transform.localPosition;

        //Move the Leg for the Given Time
        while (passedTime <= legController.LegMovementTime)
        {
            if (passedTime <= legController.LegMovementTime / 2)
            {
                legController.CurrentDownAddPerBrokenLeg += 0.0025f;
            }
            else
            {
                legController.CurrentDownAddPerBrokenLeg -= 0.0025f;
            }


            //Add deltaTime and Wait for next Frame
            passedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Set Position
        legController.IkTargets[_leg].position = legController.NextAnimationTargetPosition[_leg];
        legController.transform.localPosition = oldPos;

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
