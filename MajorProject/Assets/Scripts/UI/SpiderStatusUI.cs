using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to Update Spider LEg UI
/// </summary>
public class SpiderStatusUI : MonoBehaviour
{
    [SerializeField] private SpiderSingleStatusUI[] spiderLegStatus;
    [SerializeField] private SpiderSingleStatusUI spiderBodyStatus;


    /// <summary>
    /// Set Status Indicator on UI
    /// </summary>
    /// <param name="_leg"></param>
    /// <param name="_legstate"></param>
    public void SetLegStatus(int _leg, ELegStates _legstate)
    {
        if (_leg == -1)
        {
            spiderBodyStatus.SetIndikator(_legstate);
            return;
        }

        spiderLegStatus[_leg].SetIndikator(_legstate);
    }
}
