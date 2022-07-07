using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public abstract class LegState
{
    public delegate void LegCallback(int _leg);

    protected ProzeduralAnimationLogic legController;

    protected LegCallback legEnterSet;
    protected LegCallback legExitReset;
    protected UnityEvent<int, ELegStates> onEnter;
    protected UnityEvent<int> onMove;


    protected ProzeduralAnimationLogic.LegParams[] legs;


    public LegState(ProzeduralAnimationLogic _controller, LegCallback _legenterset, LegCallback _legexitreset, ProzeduralAnimationLogic.LegParams[] _legs, UnityEvent<int, ELegStates> _onenter, UnityEvent<int> _onmove)
    {
        legController = _controller;
        legEnterSet = _legenterset;
        legExitReset = _legexitreset;
        legs = _legs;
        onEnter = _onenter;
        onMove = _onmove;
    }

    public abstract IEnumerator C_MoveLegCoroutine(int _leg);

    public abstract void EnterLegState(int _leg);

    public abstract void ExitLegState(int _leg);
}
