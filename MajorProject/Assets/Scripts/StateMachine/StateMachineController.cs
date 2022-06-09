using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate bool StateMachineSwitchDelegate();

public interface IStateMachineController
{
    void InitializeStateMachine();
    void UpdateStateMachine();

}
