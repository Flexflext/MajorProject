using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



/// <summary>
/// Parent Class for all Leg States
/// </summary>
public abstract class LegState
{
    public delegate void LegCallback(int _leg);

    protected ProzeduralAnimationLogic legController;

    // Callback Events
    protected LegCallback legEnterSet;
    protected LegCallback legExitReset;
    protected UnityEvent<int, ELegStates> onEnter;
    protected UnityEvent<int> onMove;

    /// <summary>
    /// All Legs
    /// </summary>
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

    /// <summary>
    /// Base Move Coroutine
    /// </summary>
    /// <param name="_leg"></param>
    /// <returns></returns>
    public abstract IEnumerator C_MoveLegCoroutine(int _leg);


    /// <summary>
    /// Base Enter State
    /// </summary>
    /// <param name="_leg"></param>
    public abstract void EnterLegState(int _leg);


    /// <summary>
    /// Base Exit State
    /// </summary>
    /// <param name="_leg"></param>
    public abstract void ExitLegState(int _leg);
}
