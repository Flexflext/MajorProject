using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate bool StateMachineSwitchDelegate();


/// <summary>
/// State Machine Interface Parent
/// </summary>
public interface IStateMachineController
{
    void InitializeStateMachine();
    void UpdateStateMachine();

}
