using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegDeadState : LegState
{
    //private List<BoxCollider> addedBoxColliders;

    public LegDeadState(ProzeduralAnimationLogic _controller, LegCallback _legenterset, LegCallback _legexitreset, ProzeduralAnimationLogic.LegParams[] _legs) : base(_controller, _legenterset, _legexitreset, _legs)
    {
        //addedBoxColliders = new List<BoxCollider>();
    }

    public override IEnumerator C_MoveLegCoroutine(int _leg)
    {
        yield return null;
    }

    public override void EnterLegState(int _leg)
    {
        
        legs[_leg].legIKSystem.SolveIK = false;
        legs[_leg].legIKSystem.CreateBoxCollidersOnChain();
        legs[_leg].legIKSystem.CreateRigidbodysOnChain();

        
    }

    public override void ExitLegState(int _leg) 
    {
        legs[_leg].legIKSystem.SolveIK = true;
        legs[_leg].legIKSystem.ResetBoxCollidersOnChain();
        legs[_leg].legIKSystem.ResetRigidbodysOnChain();

        legs[_leg].moveingLeg = false;
        legs[_leg].isOnMoveDelay = false;
    }
}
