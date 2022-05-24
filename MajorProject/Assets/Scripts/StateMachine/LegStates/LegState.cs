using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class LegState
{
    public delegate void LegCallback(int _leg);

    protected ProzeduralAnimationLogic legController;

    protected LegCallback legEnterSet;
    protected LegCallback legExitReset;

    protected ProzeduralAnimationLogic.LegParams[] legs;
    

    public LegState(ProzeduralAnimationLogic _controller, LegCallback _legenterset, LegCallback _legexitreset, ProzeduralAnimationLogic.LegParams[] _legs)
    {
        legController = _controller;
        legEnterSet = _legenterset;
        legExitReset = _legexitreset;
        legs = _legs;
    }

    public abstract IEnumerator C_MoveLegCoroutine(int _leg);

    public abstract void EnterLegState(int _leg);

    public abstract void ExitLegState(int _leg);
}
