using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LegBrokenState : LegState
{
    public LegBrokenState(ProzeduralAnimationLogic _controller, LegCallback _legenterset, LegCallback _legexitreset, ProzeduralAnimationLogic.LegParams[] _legs, UnityEvent<int, ELegStates> _onenter, UnityEvent<int> _onmove) : base(_controller, _legenterset, _legexitreset, _legs, _onenter, _onmove)
    {
    }


    /// <summary>
    /// Move Leg Coroutine
    /// </summary>
    /// <param name="_leg"></param>
    /// <returns></returns>
    public override IEnumerator C_MoveLegCoroutine(int _leg)
    {
        float passedTime = 0f;
        float maxTime = legController.LegMovementTime;

        //Adjust lef to be Broken
        legController.AdjustBrokenLegRotation(_leg);

        if (onMove != null)
        {
            //Set OnMove Event
            onMove.Invoke(_leg);
        }

        //Set Old Pos
        Vector3 oldPos = legController.transform.localPosition;

        //Move the Leg after the Given Time
        while (passedTime <= maxTime)
        {

            if (passedTime <= maxTime / 2)
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
        legs[_leg].ikTarget.position = legs[_leg].nextAnimationTargetPosition;
        legController.transform.localPosition = oldPos;

        //Reset Flag
        legs[_leg].moveingLeg = false;
        legController.ResetBrokenLegRotation();

        yield return new WaitForSeconds(maxTime / 2);

        legs[_leg].isOnMoveDelay = false;

    }

    public override void EnterLegState(int _leg)
    {
        //Set Enter Event to Set the Controller
        if (legEnterSet != null)
        {
            legEnterSet.Invoke(_leg);
        }

        //Invoke Enter Event
        if (onEnter != null)
        {
            onEnter.Invoke(_leg, ELegStates.LS_Broken);
        }
    }

    public override void ExitLegState(int _leg)
    {
        //Invoke Exit Set Event
        if (legExitReset != null)
        {
            legExitReset.Invoke(_leg);
        }
    }
}
