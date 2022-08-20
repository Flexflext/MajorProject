using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Leg State when the Spider Dies
/// </summary>
public class LegDeadState : LegState
{
    private float deathTime = 1f;
    private Vector2 timeRndAdd = Vector2.zero;
    private float deathPos = 1f;
    private Vector2 posRndAdd = Vector2.zero;


    private Transform[] targetParents;
    private Transform[] hintParents;
    private Vector3[] hintprevPos;


    public LegDeadState(ProzeduralAnimationLogic _controller, LegCallback _legenterset, LegCallback _legexitreset, ProzeduralAnimationLogic.LegParams[] _legs, float _time, Vector2 _timerndadd, float _pos, Vector2 _posrndadd, UnityEvent<int, ELegStates> _onenter, UnityEvent<int> _onmove) : base(_controller, _legenterset, _legexitreset, _legs, _onenter, _onmove)
    {
        targetParents = new Transform[_legs.Length];
        hintParents = new Transform[_legs.Length];
        hintprevPos = new Vector3[_legs.Length];

        deathTime = _time;
        deathPos = _pos;
        timeRndAdd = _timerndadd;
        posRndAdd = _posrndadd;
    }

    public override IEnumerator C_MoveLegCoroutine(int _leg)
    {
        yield return null;
    }

    /// <summary>
    /// Save Pre Death Data for Exit
    /// </summary>
    /// <param name="_leg"></param>
    public override void EnterLegState(int _leg)
    {
        targetParents[_leg] = legs[_leg].ikTarget.parent;
        hintParents[_leg] = legs[_leg].animationHint.parent;
        hintprevPos[_leg] = legs[_leg].animationHint.localPosition;
        legs[_leg].ikTarget.parent = legController.transform;
        legs[_leg].animationHint.parent = legController.transform;
        legController.StartCoroutine(PlayDeathAnimation(_leg));

        if (onEnter != null)
        {
            onEnter.Invoke(_leg, ELegStates.LS_Broken);
        }
    }

    /// <summary>
    /// Reset Pre Death Data on Legs to Reset Death State
    /// </summary>
    /// <param name="_leg"></param>
    public override void ExitLegState(int _leg) 
    {
        legController.StopCoroutine(PlayDeathAnimation(_leg));

        legs[_leg].legIKSystem.SolveIK = true;
        //legs[_leg].legIKSystem.ResetBoxCollidersOnChain();
        legs[_leg].legIKSystem.ResetRigidbodysOnChain();

        legs[_leg].moveingLeg = false;
        legs[_leg].isOnMoveDelay = false;
        legs[_leg].stopLegAnimationFlag = false;

        legs[_leg].ikTarget.parent = targetParents[_leg];
        legs[_leg].animationHint.parent = hintParents[_leg];
        legs[_leg].animationHint.localPosition = hintprevPos[_leg];
    }


    /// <summary>
    /// Coroutine to Play the Death Leg Animation -> Fold Leg
    /// </summary>
    /// <param name="_leg"></param>
    /// <returns></returns>
    private IEnumerator PlayDeathAnimation(int _leg)
    {   
        legs[_leg].stopLegAnimationFlag = true;

        Vector3 dir = legs[_leg].ikTarget.localPosition;

        dir *= deathPos + Random.Range(posRndAdd.x, posRndAdd.y);

        float curTime = 0f;
        float maxTime = deathTime + Random.Range(timeRndAdd.x, timeRndAdd.y);

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
